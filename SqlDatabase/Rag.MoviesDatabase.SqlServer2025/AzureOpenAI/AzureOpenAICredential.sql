CREATE MASTER KEY ENCRYPTION BY PASSWORD = 'Str0ngP@$$w0rd'
GO

CREATE DATABASE SCOPED CREDENTIAL [https://lenni-openai.openai.azure.com] WITH
	IDENTITY = 'HTTPEndpointHeaders',
	SECRET = '{"api-key": "1e981882b329481ebe4b2bfa261f8dce"}'
GO
