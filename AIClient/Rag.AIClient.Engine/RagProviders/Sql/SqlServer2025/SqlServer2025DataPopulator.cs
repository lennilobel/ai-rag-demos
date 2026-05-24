using Rag.AIClient.Engine.RagProviders.Base;
using System.Threading.Tasks;

namespace Rag.AIClient.Engine.RagProviders.Sql
{
	public class SqlServer2025DataPopulator : SqlDataPopulatorBase
	{
		public SqlServer2025DataPopulator(IRagProvider ragProvider)
			: base(ragProvider)
		{
		}

		public override async Task InitializeData()
		{
			await base.InitializeData();

			await base.LoadConfiguration();
		}

	}
}
