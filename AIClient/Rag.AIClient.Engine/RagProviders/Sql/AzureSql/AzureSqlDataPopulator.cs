using Rag.AIClient.Engine.RagProviders.Core;

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
