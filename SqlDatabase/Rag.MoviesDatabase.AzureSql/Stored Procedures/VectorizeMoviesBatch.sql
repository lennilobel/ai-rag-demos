CREATE PROCEDURE VectorizeMoviesBatch
    @MoviesBatchJson varchar(max)
AS
BEGIN

    -- Build the JSON payload expected by the Azure OpenAI embeddings endpoint.
    -- The "input" property must contain a JSON array of strings.
    DECLARE @MoviesPayload varchar(max)

    -- Convert the batch of movie JSON objects into an array of escaped strings.
    -- Preserve the original array ordering so the returned embedding indexes align with the source movies.
    -- Explicitly request 1536 dimensions so the result fits into SQL Server's vector(1536) data type.
    SELECT @MoviesPayload =
        JSON_OBJECT(
            'input': JSON_QUERY(
                '[' + STRING_AGG(
                    '"' + STRING_ESCAPE(CONVERT(varchar(max), value), 'json') + '"',
                    ','
                ) WITHIN GROUP (ORDER BY CONVERT(int, [key])) + ']'
            ),
            'dimensions': 1536
        )
    FROM OPENJSON(@MoviesBatchJson)

    -- Retrieve Azure OpenAI configuration values from the application configuration table.
    DECLARE @OpenAIEndpoint varchar(max)        = (SELECT ConfigValue FROM AppConfig WHERE ConfigKey = 'OpenAIEndpoint')
    DECLARE @OpenAIApiKey varchar(max)          = (SELECT ConfigValue FROM AppConfig WHERE ConfigKey = 'OpenAIApiKey')
    DECLARE @OpenAIDeploymentName varchar(max)  = (SELECT ConfigValue FROM AppConfig WHERE ConfigKey = 'OpenAIDeploymentName')

    -- Build the embeddings endpoint URL and request headers.
    DECLARE @Url varchar(max) = CONCAT(@OpenAIEndpoint, 'openai/deployments/', @OpenAIDeploymentName, '/embeddings?api-version=2023-03-15-preview')
    DECLARE @Headers varchar(max) = JSON_OBJECT('api-key': @OpenAIApiKey)
    DECLARE @Response nvarchar(max)
    DECLARE @ReturnValue int

    -- Invoke the Azure OpenAI REST API to generate embeddings for the movie batch.
    EXEC @ReturnValue = sp_invoke_external_rest_endpoint
        @url = @Url,
        @method = 'POST',
        @headers = @Headers,
        @payload = @MoviesPayload,
        @response = @Response OUTPUT

    -- If the REST call failed, throw the returned response payload as the error message.
    IF @ReturnValue != 0
        THROW 50000, @Response, 1

    -- Extract MovieId values from the source batch and assign a zero-based index.
    -- The index must match the embedding index returned by the Azure OpenAI response.
    ;WITH MoviesCte AS (
        SELECT
            MovieId = CONVERT(int, JSON_VALUE(value, '$.MovieId')),
            MovieIndex = ROW_NUMBER() OVER (ORDER BY CONVERT(int, [key])) - 1
        FROM
            OPENJSON(@MoviesBatchJson)
    ),
    -- Extract each embedding vector from the response payload and cast it to SQL Server's vector(1536) type.
    EmbeddingsCte AS (
        SELECT
            MovieIndex,
            Vector = CAST(Embedding AS vector(1536))
        FROM
            OPENJSON(@Response, '$.result.data')                    -- each movie is an element in the result's data array
        WITH (
            MovieIndex int '$.index',
            Embedding nvarchar(max) '$.embedding' AS JSON           -- each movie's vector is retrieved from the embedding array in the data array of each result
        )
    )
    -- Update the Movie table with the generated vectors.
    -- Join the source movies to the returned embeddings using the shared positional index.
    UPDATE m
        SET Vector = e.Vector
    FROM
        Movie AS m
        INNER JOIN MoviesCte AS mb ON mb.MovieId = m.MovieId
        INNER JOIN EmbeddingsCte AS e ON e.MovieIndex = mb.MovieIndex

END