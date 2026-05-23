namespace Rag.AIClient.Engine.AIModels
{
	public enum AIModelsSourceType
	{
		AzureOpenAI,
		LocalAI,
	}

	public enum OpenAIEmbeddingModelType
	{
		Default,				// text-embedding-3-large, no database suffix
		TextEmbedding3Large,	// text-embedding-3-large, database suffix -3l
		TextEmbedding3Small,    // text-embedding-3-small, database suffix -3s
		TextEmbeddingAda002,    // text-embedding-ada-002, database suffix -ada
	}

}
