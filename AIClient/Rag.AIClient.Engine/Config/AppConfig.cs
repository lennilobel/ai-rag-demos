using Rag.AIClient.Engine.AIModels;
using Rag.AIClient.Engine.RagProviders;
using System.Linq;

namespace Rag.AIClient.Engine.Config
{
	// This class is mapped to appsettings.json
	public class AppConfig
    {
        public RagProviderType RagProviderType { get; set; }
		public string ExternalRagProviderType { get; set; }
		public OpenAIEmbeddingModelType EmbeddingModelType { get; set; }

		public SqlConfig SqlServer2022 { get; set; }
		public SqlConfig SqlServer2025 { get; set; }
		public SqlConfig AzureSql { get; set; }
		public class SqlConfig
		{
			public string ServerName { get; set; }
			public string DatabaseName { get; set; }
			public string Username { get; set; }
			public string Password { get; set; }
			public bool TrustServerCertificate { get; set; }
			public string JsonInitialDataFilename { get; set; }
			public string JsonUpdateDataFilename { get; set; }
		}

		public CosmosDbConfig CosmosDb { get; set; }
		public class CosmosDbConfig
		{
			public string Endpoint { get; set; }
			public string MasterKey { get; set; }
			public string DatabaseName { get; set; }
			public string ContainerName { get; set; }
			public string PartitionKey { get; set; }
			public string JsonInitialDataFilename { get; set; }
			public string JsonUpdateDataFilename { get; set; }
		}

		public MongoDbConfig MongoDb { get; set; }
		public class MongoDbConfig
		{
			public string ConnectionString { get; set; }
			public string DatabaseName { get; set; }
			public string CollectionName{ get; set; }
			public string JsonInitialDataFilename { get; set; }
			public string JsonUpdateDataFilename { get; set; }
		}

		public ExternalRagProviderTypeConfig[] ExternalRagProviders { get; set; }
		public class ExternalRagProviderTypeConfig
		{
			public string ExternalRagProviderType { get; set; }
			public string ExternalRagProviderAssemblyPath { get; set; }
			public string ExternalRagProviderClassName { get; set; }
			public SqlConfig SqlServer { get; set; }
			public SqlConfig AzureSql { get; set; }
			public SqlConfig AzureSqlPreview { get; set; }
			public CosmosDbConfig CosmosDb { get; set; }
			public MongoDbConfig MongoDb { get; set; }
		}

		public AIModelsSourceType AIModelsSourceType { get; set; }

		public AzureOpenAIConfig AzureOpenAI { get; set; }
		public class AzureOpenAIConfig
		{
			public string Endpoint { get; set; }
			public string ApiKey { get; set; }
			public EmbeddingDeploymentNamesConfig EmbeddingDeploymentNames { get; set; }
			public class EmbeddingDeploymentNamesConfig
			{
				public string Default { get; set; }
				public string TextEmbedding3Large { get; set; }
				public string TextEmbedding3Small { get; set; }
				public string TextEmbeddingAda002 { get; set; }
			}
			public string CompletionDeploymentName { get; set; }
			public string DalleDeploymentName { get; set; }
		}

		public ChangeEventStreamingConfig ChangeEventStreaming { get; set; }
		public class ChangeEventStreamingConfig
		{
			public string CesSasToken { get; set; }
			public string StorageSasToken { get; set; }
		}

		public LocalAIConfig LocalAI { get; set; }
		public class LocalAIConfig
		{
			public EmbeddingConfig Embedding { get; set; }
			public class EmbeddingConfig
			{
				public string Endpoint { get; set; }
				public string ModelName { get; set; }
			}
			public CompletionConfig Completion { get; set; }
			public class CompletionConfig
			{
				public string Endpoint { get; set; }
				public string ModelName { get; set; }
			}
		}

		public ExternalRagProviderTypeConfig GetExternalRagProvider(string externalRagProviderType) =>
			Shared.AppConfig.ExternalRagProviders.First(erp => erp.ExternalRagProviderType == externalRagProviderType);

	}
}
