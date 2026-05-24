using Rag.AIClient.Engine.RagProviders.Base;

namespace Rag.AIClient.Engine.RagProviders.Sql.AzureSql
{
	public class AzureSqlDataPopulator : SqlDataPopulatorBase
	{
		public AzureSqlDataPopulator(IRagProvider ragProvider)
			: base(ragProvider)
		{
		}

	}
}
