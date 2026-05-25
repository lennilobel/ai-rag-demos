using Rag.AIClient.Engine.Config;
using Rag.AIClient.Engine.RagProviders.Core;

namespace Rag.AIClient.Engine.RagProviders.NoSql.CosmosDb
{
    public class CosmosDbRagProvider : RagProviderBase
    {
        public override string ProviderName => "Azure Cosmos DB for NoSQL";

        public override string DatabaseName => CosmosDbConfig.DatabaseName + GetDatabaseNameSuffix();

		public override string ServerName => CosmosDbConfig.Endpoint;
		
        public override AppConfig.CosmosDbConfig CosmosDbConfig => Shared.AppConfig.CosmosDb;

		public override string EntityTitleFieldName => "title";

		public override IDataPopulator GetDataPopulator() => new CosmosDbDataPopulator(this);

        public override IDataVectorizer GetDataVectorizer() => new CosmosDbDataVectorizer(this);

        public override IAIAssistant GetAIAssistant() => new CosmosDbMoviesAssistant(this);
    }
}
