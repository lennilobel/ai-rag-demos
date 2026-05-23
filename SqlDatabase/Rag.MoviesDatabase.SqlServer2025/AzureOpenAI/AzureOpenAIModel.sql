CREATE EXTERNAL MODEL MoviesTextEmbeddingModel
AUTHORIZATION dbo
WITH (
      LOCATION = 'https://lenni-openai.openai.azure.com/openai/deployments/lenni-text-embedding-3-large/embeddings?api-version=2023-03-15-preview',
      API_FORMAT = 'Azure OpenAI',
      MODEL_TYPE = EMBEDDINGS,
      MODEL = 'lenni-text-embedding-3-large',
      CREDENTIAL = [https://lenni-openai.openai.azure.com],
      PARAMETERS = '{"dimensions" : 1536}'      -- Request compressed 'Text Embedding 3 Large' vectors from 3072 (exceeds current 1998 limit) to 1536 (as defined for all vector data types throughout the database)
)
