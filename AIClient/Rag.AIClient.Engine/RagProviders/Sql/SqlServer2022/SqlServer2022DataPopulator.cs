using Rag.AIClient.Engine.RagProviders.Base;

namespace Rag.AIClient.Engine.RagProviders.Sql
{
	public class SqlServer2022DataPopulator : SqlDataPopulatorBase
	{
		public SqlServer2022DataPopulator(IRagProvider ragProvider)
			: base(ragProvider)
		{
		}

	}
}
