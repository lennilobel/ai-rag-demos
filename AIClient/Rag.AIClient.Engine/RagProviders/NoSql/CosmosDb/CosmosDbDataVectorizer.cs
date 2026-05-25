using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using Rag.AIClient.Engine.AIModels;
using Rag.AIClient.Engine.Config;
using Rag.AIClient.Engine.RagProviders.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Rag.AIClient.Engine.RagProviders.NoSql.CosmosDb
{
	public class CosmosDbDataVectorizer : DataVectorizerBase
	{
		protected virtual AppConfig.CosmosDbConfig CosmosDbConfig => base.RagProvider.CosmosDbConfig;

		public int _errorCount;
		public double _ruCost;

		public CosmosDbDataVectorizer(IRagProvider ragProvider)
			: base(ragProvider)
		{
		}

		protected override async Task VectorizeEntities(int[] ids)
		{
			Debugger.Break();

			this._errorCount = 0;
			this._ruCost = 0;
			
            var itemCount = 0;
            var database = Shared.CosmosClient.GetDatabase(base.RagProvider.DatabaseName);
            var container = database.GetContainer(this.CosmosDbConfig.ContainerName);

            // Raise the throughput on the container
            await container.ReplaceThroughputAsync(ThroughputProperties.CreateAutoscaleThroughput(autoscaleMaxThroughput: 10000));

            // Query documents in the container (process results in batches)
            var sql = $"SELECT * FROM c{(ids == null ? null : $" WHERE c.id = IN({string.Join(',', ids)})")} ORDER BY c.{base.RagProvider.EntityTitleFieldName}";
            var iterator = container.GetItemQueryIterator<JObject>(
                queryText: sql,
                requestOptions: new QueryRequestOptions { MaxItemCount = 100 });

            while (iterator.HasMoreResults)
            {
                var batchStarted = DateTime.Now;

                // Retrieve the next batch of documents
                var documents = (await iterator.ReadNextAsync()).ToArray();
                foreach (var document in documents)
                {
					ConsoleHelper.WriteLine($"{++itemCount,5}: Vectorizing entity - {document[base.RagProvider.EntityTitleFieldName]} (ID {document["id"]})", ConsoleHelper.InfoDimColor);
				}

                // Generate vectors for the batch of documents
                var vectors = await this.GenerateVectors(documents);

                // Update the documents back to the container with the generated vectors
                await this.SaveVectors(container, documents, vectors);

                var batchElapsed = DateTime.Now.Subtract(batchStarted);

                ConsoleHelper.WriteLine($"Processed documents {itemCount - documents.Length + 1} - {itemCount} in {batchElapsed}", ConsoleHelper.InfoColor);
            }

            // Lower the throughput on the container
            await container.ReplaceThroughputAsync(ThroughputProperties.CreateAutoscaleThroughput(autoscaleMaxThroughput: 1000));

            ConsoleHelper.WriteLine($"Generated and embedded vectors for {itemCount} document(s) with {this._errorCount} error(s) ({this._ruCost} RUs)", ConsoleHelper.UserColor);
        }

        private async Task<float[][]> GenerateVectors(JObject[] documents)
		{
			ConsoleHelper.Write("Generating vectors... ", ConsoleHelper.SystemColor);

			// Strip meaningless properties and any previous vector from each document
			foreach (var document in documents)
			{
				document.Remove("_rid");
				document.Remove("_self");
				document.Remove("_etag");
				document.Remove("_attachments");
				document.Remove("_ts");
				document.Remove("ttl");
				document.Remove("vector");
			}

			// Generate vectors from embeddings based on the JSON string content of each document
			var input = documents.Select(d => d.ToString()).ToArray();
			var vectors = await AIModelsSourceFactory.GenerateVectorsFromEmbeddingModel(input);

			ConsoleHelper.WriteLine(vectors.Length, ConsoleHelper.SystemColor);

			return vectors;
		}

		private async Task SaveVectors(Container container, JObject[] documents, float[][] vectors)
        {
            ConsoleHelper.Write("Saving vectors... ", ConsoleHelper.SystemColor);

            // Set the vector property of each document from the generated embeddings
            for (var i = 0; i < documents.Length; i++)
            {
                var vector = JArray.FromObject(vectors[i]);
                documents[i]["vector"] = vector;
            }

            // Use bulk execution to update the documents back to the container
            var tasks = new List<Task>(documents.Length);
            foreach (JObject document in documents)
            {
                var id = document["id"].Value<string>();
                var partitionKey = document[base.RagProvider.CosmosDbConfig.PartitionKey].Value<string>();
				var task = container.ReplaceItemAsync(document, id, new PartitionKey(partitionKey));
                tasks.Add(task
                    .ContinueWith(t =>
                    {
                        if (t.Status == TaskStatus.RanToCompletion)
                        {
                            _ruCost += t.Result.RequestCharge;
                        }
                        else
                        {
                            ConsoleHelper.WriteErrorLine($"Error replacing document id='{id}'\n{t.Exception.Message}");
                            _errorCount++;
                        }
                    }));
            }

            await Task.WhenAll(tasks);

            ConsoleHelper.WriteLine(documents.Length, ConsoleHelper.SystemColor);
        }

    }
}
