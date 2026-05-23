/*
	*** Setup ***
*/

USE MoviesDB
GO

-- Create a table to hold movie titles and associated vectors
CREATE TABLE Movie (
	MovieId int IDENTITY,
	Title varchar(50),
	Vector vector(1536)
)

-- Populate four movie titles
INSERT INTO Movie (Title) VALUES
	('Return of the Jedi'),
	('The Godfather'),
	('Animal House'),
	('The Two Towers')
GO

-- Create a stored procedure that can call Azure OpenAI to vectorize any text
CREATE PROCEDURE VectorizeText
	@Text varchar(max),
	@Vector vector(1536) OUTPUT
AS
BEGIN

	DECLARE @OpenAIEndpoint varchar(max) = 'https://lenni-openai.openai.azure.com/'		-- Your Azure OpenAI endpoint
	DECLARE @OpenAIApiKey varchar(max) = '1e981882b329481ebe4b2bfa261f8dce'				-- Your Azure OpenAI API key
	DECLARE @OpenAIDeploymentName varchar(max) = 'lenni-text-embedding-3-small'			-- The 'Text Embedding 3 Small' model yields 1536 components (floating point values) per vector

	DECLARE @Url varchar(max) = CONCAT(@OpenAIEndpoint, 'openai/deployments/', @OpenAIDeploymentName, '/embeddings?api-version=2023-03-15-preview')
	DECLARE @Headers varchar(max) = JSON_OBJECT('api-key': @OpenAIApiKey)
	DECLARE @Payload varchar(max) = JSON_OBJECT('input': @Text)
	DECLARE @Response nvarchar(max)
	DECLARE @ReturnValue int

	-- Call Azure OpenAI via sp_invoke_external_rest_endpoint to vectorize the text
	EXEC @ReturnValue = sp_invoke_external_rest_endpoint
		@url = @Url,
		@method = 'POST',
		@headers = @Headers,
		@payload = @Payload,
		@response = @Response OUTPUT

	IF @ReturnValue != 0
		THROW 50000, @Response, 1

	DECLARE @VectorJson nvarchar(max) = JSON_QUERY(@Response, '$.result.data[0].embedding')

	SET @Vector = CONVERT(vector(1536), @VectorJson)

END
GO

-- Create a stored procedure to run a vector search using the Cosine Distance metric
CREATE PROCEDURE VectorSearch
	@Question varchar(max)
AS
BEGIN

	-- Prepare a vector variable to capture the question vector components returned from Azure OpenAI
	DECLARE @QuestionVector vector(1536)

	-- Vectorize the question, and store the question vector components in the table variable
	EXEC VectorizeText
		@Question,
		@QuestionVector OUTPUT

	SELECT TOP 1
		Question = @Question,
		Answer = Title,
		CosineDistance = VECTOR_DISTANCE('cosine', @QuestionVector, Vector)
	FROM
		Movie
	ORDER BY
		CosineDistance

END
GO

-- View the movie titles with no vectors
SELECT * FROM Movie
