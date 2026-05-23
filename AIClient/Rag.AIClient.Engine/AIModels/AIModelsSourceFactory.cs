using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Rag.AIClient.Engine.AIModels
{
	public static class AIModelsSourceFactory
	{
		public static AIModelsSourceType AIModelsSourceType { get; set; }
		public static OpenAIEmbeddingModelType OpenAIEmbeddingModelType { get; set; }

		static AIModelsSourceFactory()
		{
			var args = Environment.GetCommandLineArgs();

			AIModelsSourceType = args.Length > 2
				? (AIModelsSourceType)Enum.Parse(typeof(AIModelsSourceType), args[2], ignoreCase: true)
				: Shared.AppConfig.AIModelsSourceType;
		}

		public static AIModelsSourceType GetModelsSource() =>
			Shared.AppConfig.AIModelsSourceType;

		public static async Task<float[][]> GenerateVectorsFromEmbeddingModel(string[] input) =>
			Shared.AppConfig.AIModelsSourceType switch
			{
				AIModelsSourceType.AzureOpenAI => await GenerateVectorsFromAzureOpenAIEmbeddingModel(input),
				AIModelsSourceType.LocalAI => await GenerateVectorsFromLocalAIEmbeddingModel(input),
				_ => null,
			};

		private static async Task<float[][]> GenerateVectorsFromAzureOpenAIEmbeddingModel(string[] input)
		{
			var embeddingClient = Shared.AzureOpenAIClient.GetEmbeddingClient(GetEmbeddingModelName());
			var embeddings = (await embeddingClient.GenerateEmbeddingsAsync(input)).Value.ToArray();
			var vectors = embeddings
				.Select(e => e.ToFloats().ToArray())
				.ToArray();

			return vectors;
		}

		private static async Task<float[][]> GenerateVectorsFromLocalAIEmbeddingModel(string[] input)
		{
			var httpClient = new HttpClient();

			var tasks = input.Select(async text =>
			{
				var request = new { text };
				var response = await httpClient.PostAsJsonAsync(Shared.AppConfig.LocalAI.Embedding.Endpoint, request);
				response.EnsureSuccessStatusCode();

				var result = await response.Content.ReadFromJsonAsync<JsonElement>();

				return result.GetProperty("vector").Deserialize<float[]>();
			});

			return await Task.WhenAll(tasks);
		}

		public static string GetEmbeddingModelName() =>
			Shared.AppConfig.AIModelsSourceType switch
			{
				AIModelsSourceType.AzureOpenAI => GetAzureOpenAIEmbeddingModelName().Split(':')[0],
				AIModelsSourceType.LocalAI => GetLocalAIEmbeddingModelName().Split(':')[0],
				_ => null,
			};

		private static string GetAzureOpenAIEmbeddingModelName() =>
			OpenAIEmbeddingModelType.ToString().Split(':')[0] switch     // expected colon delimeter using format "deployment-name:vector-size"
			{
				nameof(OpenAIEmbeddingModelType.Default) =>
					Shared.AppConfig.AzureOpenAI.EmbeddingDeploymentNames.Default,

				nameof(OpenAIEmbeddingModelType.TextEmbedding3Large) =>
					Shared.AppConfig.AzureOpenAI.EmbeddingDeploymentNames.TextEmbedding3Large,

				nameof(OpenAIEmbeddingModelType.TextEmbedding3Small) =>
					Shared.AppConfig.AzureOpenAI.EmbeddingDeploymentNames.TextEmbedding3Small,

				nameof(OpenAIEmbeddingModelType.TextEmbeddingAda002) =>
					Shared.AppConfig.AzureOpenAI.EmbeddingDeploymentNames.TextEmbeddingAda002,

				_ =>
					throw new NotSupportedException($"No deployment name is implemented for Azure OpenAI embedding model type {OpenAIEmbeddingModelType}"),
			};

		private static string GetLocalAIEmbeddingModelName() =>
			Shared.AppConfig.LocalAI.Embedding.ModelName;

		public static int GetVectorSize() =>
			Shared.AppConfig.AIModelsSourceType switch
			{
				AIModelsSourceType.AzureOpenAI => GetVectorSizeFromAzureOpenAIEmbeddingModel(),
				AIModelsSourceType.LocalAI => GetVectorSizeFromLocalAIEmbeddingModel(),
				_ => 0,
			};

		private static int GetVectorSizeFromAzureOpenAIEmbeddingModel()
		{
			var parts = OpenAIEmbeddingModelType.ToString().Split(':');
			return parts.Length == 2 && int.TryParse(parts[1], out var size) ? size : 3072;
		}

		private static int GetVectorSizeFromLocalAIEmbeddingModel() =>
			int.Parse(Shared.AppConfig.LocalAI.Embedding.ModelName.ToString().Split(':')[1]);   // expected colon delimeter using format "deployment-name:vector-size"

	}
}
