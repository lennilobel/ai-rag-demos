using Microsoft.Data.SqlClient;
using Rag.AIClient.Engine.Config;
using Rag.AIClient.Engine.AIModels;
using System;
using System.IO;

namespace Rag.AIClient.Engine.RagProviders.Base
{
	public abstract class RagProviderBase : IRagProvider
	{
		public abstract string ProviderName { get; }

		public abstract string DatabaseName { get; }

		public abstract string ServerName { get; }

		public virtual AppConfig.SqlConfig SqlConfig => throw new NotSupportedException($"No SQL configuration is available for RAG provider '{this.ProviderName}'");

		public virtual AppConfig.CosmosDbConfig CosmosDbConfig => throw new NotSupportedException($"No Cosmos DB configuration is available for RAG provider '{this.ProviderName}'");

		public virtual AppConfig.MongoDbConfig MongoDbConfig => throw new NotSupportedException($"No MongoDB configuration is available for RAG provider '{this.ProviderName}'");

		public string SqlConnectionString
		{
			get
			{
				var config = this.SqlConfig;

				var csb = new SqlConnectionStringBuilder
				{
					DataSource = config.ServerName,
					InitialCatalog = this.DatabaseName,
					UserID = config.Username,
					Password = config.Password,
					TrustServerCertificate = config.TrustServerCertificate
				};

				return csb.ConnectionString;
			}
		}

		public virtual bool UsesChangeEventStreaming => false;

		public virtual bool UsesDatabaseConfiguration => false;

		public virtual bool UsesDatabasePublisher => false;

		public abstract string EntityTitleFieldName { get; }

		public virtual string GetDataFilePath(string filename) => this.GetDataFileLocalPath(filename);

		public virtual string GetDataFileLocalPath(string filename) => new FileInfo($@"Data\{filename}").FullName;

		public abstract IDataPopulator GetDataPopulator();

		public abstract IDataVectorizer GetDataVectorizer();

		public abstract IAIAssistant GetAIAssistant();

		protected string GetDatabaseNameSuffix() =>
			AIModelsSourceFactory.AIModelsSourceType switch
			{
				AIModelsSourceType.AzureOpenAI =>
					AIModelsSourceFactory.OpenAIEmbeddingModelType switch
					{
						OpenAIEmbeddingModelType.Default => string.Empty,
						OpenAIEmbeddingModelType.TextEmbedding3Large => "-3l",
						OpenAIEmbeddingModelType.TextEmbedding3Small => "-3s",
						OpenAIEmbeddingModelType.TextEmbeddingAda002 => "-ada",
						_ => throw new NotSupportedException($"No database name suffix is implemented for embedding model type {AIModelsSourceFactory.OpenAIEmbeddingModelType}"),
					},
				AIModelsSourceType.LocalAI =>
					string.Empty,
				_ =>
					throw new NotSupportedException($"No database name suffix is implemented"),
			};

	}
}
