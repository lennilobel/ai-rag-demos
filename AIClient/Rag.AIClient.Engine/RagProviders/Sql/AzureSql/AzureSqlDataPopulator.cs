using Rag.AIClient.Engine.AIModels;
using Rag.AIClient.Engine.RagProviders.Base;
using System;
using System.Diagnostics;
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

		public override async Task UpdateData()
		{
			Debugger.Break();

			var started = DateTime.Now;

			ConsoleHelper.WriteHeading("Update Data", ConsoleHelper.UserColor);

			var remoteFilename = base.RagProvider.GetDataFilePath(base.RagProvider.SqlConfig.JsonUpdateDataFilename);
			await base.LoadDataFromJsonFile(remoteFilename);

			var elapsed = DateTime.Now.Subtract(started);
			ConsoleHelper.WriteLine();
			ConsoleHelper.WriteLine($"Data updated in {elapsed}", ConsoleHelper.InfoColor);
		}

	}
}
