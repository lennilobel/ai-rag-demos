using Rag.AIClient.Engine.RagProviders.Base;

namespace Rag.AIClient.Engine.RagProviders.Sql
{
	public abstract class SqlRagProviderBase : RagProviderBase
	{
		public override string DatabaseName => SqlConfig.DatabaseName + base.GetDatabaseNameSuffix();

		public override string ServerName => SqlConfig.ServerName;

		public override string EntityTitleFieldName => "Title";

	}
}
