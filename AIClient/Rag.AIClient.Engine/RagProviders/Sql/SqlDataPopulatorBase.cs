using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rag.AIClient.Engine.AIModels;
using Rag.AIClient.Engine.RagProviders.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

			await this.LoadConfiguration();

			await this.EnableChangeEventStreaming();

			var elapsed = DateTime.Now.Subtract(started);
			ConsoleHelper.WriteLine();
			ConsoleHelper.WriteLine($"Data loaded in {elapsed}", ConsoleHelper.UserColor);
		}

		private async Task LoadDataFromJsonFile(string filename)
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

		private async Task LoadConfiguration()
		{
			if (!base.RagProvider.UsesDatabaseConfiguration)
			{
				return;
			}

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

			if (!base.RagProvider.UsesChangeEventStreaming)
			{
				var localFilename = base.RagProvider.GetDataFileLocalPath(base.RagProvider.SqlConfig.JsonUpdateDataFilename);
				var documents = JsonConvert.DeserializeObject<JArray>(File.ReadAllText(localFilename));
				var movieIds = documents.Select(d => ((JObject)d)["id"].Value<int>()).ToArray();

				var vectorizer = base.RagProvider.GetDataVectorizer();
				await vectorizer.VectorizeData(movieIds);
			}

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
			if (!base.RagProvider.UsesChangeEventStreaming)
			{
				return;
			}

			ConsoleHelper.WriteLine("Disabling Change Event Streaming", ConsoleHelper.UserColor);
			await SqlDataAccess.RunStoredProcedure("EnableDisableCES", [("Action", "Disable")]);
		}

		private async Task EnableChangeEventStreaming()
		{
			if (!base.RagProvider.UsesChangeEventStreaming)
			{
				return;
			}

			ConsoleHelper.WriteLine("Enabling Change Event Streaming", ConsoleHelper.UserColor);
			await SqlDataAccess.RunStoredProcedure("EnableDisableCES", [("Action", "Enable")]);
		}

	}
}
