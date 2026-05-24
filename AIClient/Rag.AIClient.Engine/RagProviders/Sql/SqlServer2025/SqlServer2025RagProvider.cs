using Rag.AIClient.Engine.Config;
using Rag.AIClient.Engine.RagProviders.Base;
using Rag.AIClient.Engine.RagProviders.Sql.SqlServer;

namespace Rag.AIClient.Engine.RagProviders.Sql.SqlServer2025
{
    public class SqlServer2025RagProvider : SqlRagProviderBase
    {
        public override string ProviderName => "SQL Server 2025";

        public override AppConfig.SqlConfig SqlConfig => Shared.AppConfig.SqlServer2025;

		public override bool UsesDatabaseConfiguration => true;

		public override string EntityTitleFieldName => "Title";

        public override IDataPopulator GetDataPopulator() => new SqlServer2025DataPopulator(this);

        public override IDataVectorizer GetDataVectorizer() => new SqlServer2025DataVectorizer(this);

        public override IAIAssistant GetAIAssistant() => new SqlServer2025MoviesAssistant(this);
    }

}
