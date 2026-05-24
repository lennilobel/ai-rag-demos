using Rag.AIClient.Engine.AIModels;
using Rag.AIClient.Engine.RagProviders.Base;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Rag.AIClient.Engine.RagProviders.Sql
{
	public abstract class SqlDataPopulatorBase : DataPopulatorBase
	{
		public SqlDataPopulatorBase(IRagProvider ragProvider)
			: base(ragProvider)
		{
		}

		public override async Task InitializeData()
		{
			Debugger.Break();

			var started = DateTime.Now;

			ConsoleHelper.WriteHeading("Load Data", ConsoleHelper.UserColor);

			await this.DisableChangeEventStreaming();

			ConsoleHelper.WriteLine("Deleting all data", ConsoleHelper.UserColor);
			await SqlDataAccess.RunStoredProcedure("DeleteAllData");

			var filename = base.RagProvider.GetDataFilePath(base.RagProvider.SqlConfig.JsonInitialDataFilename);
			await this.LoadDataFromJsonFile(filename);

			await this.EnableChangeEventStreaming();

			var elapsed = DateTime.Now.Subtract(started);
			ConsoleHelper.WriteLine();
			ConsoleHelper.WriteLine($"Data loaded in {elapsed}", ConsoleHelper.UserColor);
		}

		protected async Task LoadDataFromJsonFile(string filename)
		{
			ConsoleHelper.WriteLine();
			ConsoleHelper.WriteLine($"Loading data from {filename}", ConsoleHelper.UserColor);

			await SqlDataAccess.RunStoredProcedure(
				storedProcedureName: "LoadMovies",
				storedProcedureParameters:
				[
					("@Filename", filename)
				]
			);
		}

		protected async Task LoadConfiguration()
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
			await this.LoadDataFromJsonFile(remoteFilename);

			var elapsed = DateTime.Now.Subtract(started);
			ConsoleHelper.WriteLine();
			ConsoleHelper.WriteLine($"Data updated in {elapsed}", ConsoleHelper.InfoColor);
		}

		public override async Task ResetData()
		{
			Debugger.Break();

			ConsoleHelper.WriteHeading("Reset Data", ConsoleHelper.UserColor);

			await this.DisableChangeEventStreaming();
			await SqlDataAccess.RunStoredProcedure("DeleteStarWarsTrilogy");
			await this.EnableChangeEventStreaming();
		}

		private async Task DisableChangeEventStreaming()
		{
			if (base.RagProvider.UsesChangeEventStreaming)
			{
				ConsoleHelper.WriteLine("Disabling Change Event Streaming", ConsoleHelper.UserColor);
				await SqlDataAccess.RunStoredProcedure("EnableDisableCES", [("Action", "Disable")]);
			}
		}

		private async Task EnableChangeEventStreaming()
		{
			if (base.RagProvider.UsesChangeEventStreaming)
			{
				ConsoleHelper.WriteLine("Enabling Change Event Streaming", ConsoleHelper.UserColor);
				await SqlDataAccess.RunStoredProcedure("EnableDisableCES", [("Action", "Enable")]);
			}
		}

	}
}
