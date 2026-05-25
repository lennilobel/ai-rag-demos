EXEC sp_configure 'show advanced options', 1
RECONFIGURE
GO

EXEC sp_configure 'external rest endpoint enabled', 1
RECONFIGURE
GO

IF NOT EXISTS (SELECT 1 FROM sys.symmetric_keys WHERE name = '##MS_DatabaseMasterKey##')
    CREATE MASTER KEY ENCRYPTION BY PASSWORD = 'Str0ngP@$$w0rd'
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_scoped_credentials WHERE name = N'https://lenni-openai.openai.azure.com')
    CREATE DATABASE SCOPED CREDENTIAL [https://lenni-openai.openai.azure.com]
    WITH
        IDENTITY = 'HTTPEndpointHeaders',
        SECRET = '{"api-key":"$(OpenAIApiKey)"}'
GO

IF EXISTS (SELECT 1 FROM sys.external_models WHERE name = N'MoviesTextEmbeddingModel')
    DROP EXTERNAL MODEL MoviesTextEmbeddingModel
GO

CREATE EXTERNAL MODEL MoviesTextEmbeddingModel
AUTHORIZATION dbo
WITH (
      LOCATION = 'https://lenni-openai.openai.azure.com/openai/deployments/lenni-text-embedding-3-large/embeddings?api-version=2023-03-15-preview',
      API_FORMAT = 'Azure OpenAI',
      MODEL_TYPE = EMBEDDINGS,
      MODEL = 'lenni-text-embedding-3-large',
      CREDENTIAL = [https://lenni-openai.openai.azure.com],
      PARAMETERS = '{"dimensions" : 1536}'
)
GO
