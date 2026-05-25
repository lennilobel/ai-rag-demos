using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rag.AIClient.Engine.Config;
using Rag.AIClient.Engine.RagProviders.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Rag.AIClient.Engine.RagProviders.Sql.SqlServer
{
	public class SqlServer2025MoviesAssistant : MoviesAssistantBase
    {
		public SqlServer2025MoviesAssistant(IRagProvider ragProvider)
			: base(ragProvider)
		{
		}

		protected override async Task<JObject[]> GetDatabaseResults(string question)
		{
			// Run a vector search in our database (Azure SQL Database via Embeddings API using a text embedding model)
			var results = await this.RunVectorSearch(question);

			return results;
		}

		private async Task<JObject[]> RunVectorSearch(string question)
        {
			var started = DateTime.Now;

			var results = new List<JObject>();

			var counter = 0;
			await SqlDataAccess.RunStoredProcedure(
				storedProcedureName: "AskQuestion",
				storedProcedureParameters:
				[
					("@Question", question),
				],
				getResult: rdr =>
				{
					counter++;
					if (DemoConfig.Instance.ShowInternalOperations && counter == 1)
					{
						ConsoleHelper.WriteHeading("SQL Server 2025 Database Vector Search Result", ConsoleHelper.SystemColor);
					}

					var resultJson = rdr["MovieJson"].ToString();
					var result = JsonConvert.DeserializeObject<JObject>(resultJson);
					results.Add(result);

					if (DemoConfig.Instance.ShowInternalOperations)
					{
						ConsoleHelper.WriteLine($"{++counter}. {result["Title"]} (distance: {rdr["CosineDistance"]})", ConsoleHelper.SystemColor);
						ConsoleHelper.WriteLine(JsonConvert.SerializeObject(result), ConsoleHelper.SystemDimColor);
					}
				},
				silent: true
			);

			base._elapsedRunVectorSearch = DateTime.Now.Subtract(started);

			return results.ToArray();
		}

	}
}
