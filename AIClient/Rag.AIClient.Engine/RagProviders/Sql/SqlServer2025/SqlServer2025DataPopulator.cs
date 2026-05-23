using Rag.AIClient.Engine.AIModels;
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

			await this.LoadConfiguration();
		}

		private async Task LoadConfiguration()
		{
			ConsoleHelper.WriteHeading("Load Configuration", ConsoleHelper.UserColor);

			ConsoleHelper.WriteLine("Loading configuration", ConsoleHelper.UserColor);

			await SqlDataAccess.RunStoredProcedure(
				storedProcedureName: "LoadConfig",
				storedProcedureParameters:
				[
					("@OpenAIEndpoint", Shared.AppConfig.AzureOpenAI.Endpoint),
					("@OpenAIApiKey", Shared.AppConfig.AzureOpenAI.ApiKey),
					("@OpenAIDeploymentName", AIModelsSourceFactory.GetEmbeddingModelName()),
				]
			);
		}

	}
}
