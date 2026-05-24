using Rag.AIClient.Engine.RagProviders.Base;

namespace Rag.AIClient.Engine.RagProviders.Sql.SqlServer2025
{
	public class SqlServer2025DataPopulator : SqlDataPopulatorBase
	{
		public SqlServer2025DataPopulator(IRagProvider ragProvider)
			: base(ragProvider)
		{
		}

	}
}
