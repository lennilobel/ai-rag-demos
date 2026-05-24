using Rag.AIClient.Engine.Config;
using Rag.AIClient.Engine.RagProviders.Base;

namespace Rag.AIClient.Engine.RagProviders.Sql.SqlServer
{
    public class SqlServer2025RagProvider : RagProviderBase
    {
        public override string ProviderName => "SQL Server 2025";

        public override string DatabaseName => SqlConfig.DatabaseName + GetDatabaseNameSuffix();

		public override string ServerName => SqlConfig.ServerName;
		
        public override AppConfig.SqlConfig SqlConfig => Shared.AppConfig.SqlServer2025;

		public override bool UsesChangeEventStreaming => true;

		public override string EntityTitleFieldName => "Title";

        public override IDataPopulator GetDataPopulator() => new SqlServer2025DataPopulator(this);

        public override IDataVectorizer GetDataVectorizer() => new SqlServer2025DataVectorizer(this);

        public override IAIAssistant GetAIAssistant() => new SqlServer2025MoviesAssistant(this);
    }

}
