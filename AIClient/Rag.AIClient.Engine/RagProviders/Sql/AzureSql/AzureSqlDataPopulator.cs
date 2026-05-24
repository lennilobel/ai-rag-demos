using Rag.AIClient.Engine.RagProviders.Base;
using System.Threading.Tasks;

namespace Rag.AIClient.Engine.RagProviders.Sql
{
	public class AzureSqlDataPopulator : SqlDataPopulatorBase
	{
		public AzureSqlDataPopulator(IRagProvider ragProvider)
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
