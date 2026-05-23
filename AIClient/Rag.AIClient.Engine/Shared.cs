using Azure;
using Azure.AI.OpenAI;
using Microsoft.Azure.Cosmos;
using MongoDB.Driver;
using Rag.AIClient.Engine.AIModels;
using Rag.AIClient.Engine.Config;
using System;

namespace Rag.AIClient.Engine
{
	public static class Shared
    {
        public static AppConfig AppConfig { get; set; }
		public static CosmosClient CosmosClient { get; set; }
		public static MongoClient MongoClient { get; set; }
		public static AzureOpenAIClient AzureOpenAIClient { get; set; }

		public static void Initialize(AppConfig appConfig)
		{
			AppConfig = appConfig;

			CosmosClient = new CosmosClient(
				AppConfig.CosmosDb.Endpoint,
				AppConfig.CosmosDb.MasterKey,
				new CosmosClientOptions { AllowBulkExecution = true }
			);

			MongoClient = new MongoClient(
				AppConfig.MongoDb.ConnectionString
			);

			AzureOpenAIClient = new AzureOpenAIClient(
				new Uri(AppConfig.AzureOpenAI.Endpoint),
				new AzureKeyCredential(AppConfig.AzureOpenAI.ApiKey)
			);

			AIModelsSourceFactory.OpenAIEmbeddingModelType = Shared.AppConfig.EmbeddingModelType;
		}

	}
}
