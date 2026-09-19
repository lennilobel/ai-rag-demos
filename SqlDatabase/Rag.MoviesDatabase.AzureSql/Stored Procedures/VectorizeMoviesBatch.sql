CREATE PROCEDURE VectorizeMoviesBatch
    @MoviesBatchJson json
AS
BEGIN

    -- Convert the batch of movie JSON objects into an array of JSON strings
    -- Preserve the original array ordering so the returned embedding indexes align with the source movies
    -- Explicitly request 1536 dimensions so the result fits into SQL Server's vector(1536) data type
    DECLARE @MoviesPayloadJson json

    SELECT @MoviesPayloadJson =
        JSON_OBJECT(
            'input': JSON_ARRAYAGG(value ORDER BY CONVERT(int, [key]) RETURNING json),
            'dimensions': 1536
            RETURNING json
        )
    FROM OPENJSON(@MoviesBatchJson)

    -- Retrieve Azure OpenAI configuration
    DECLARE @OpenAIEndpoint varchar(max)        = (SELECT ConfigValue FROM AppConfig WHERE ConfigKey = 'OpenAIEndpoint')
    DECLARE @OpenAIApiKey varchar(max)          = (SELECT ConfigValue FROM AppConfig WHERE ConfigKey = 'OpenAIApiKey')
    DECLARE @OpenAIDeploymentName varchar(max)  = (SELECT ConfigValue FROM AppConfig WHERE ConfigKey = 'OpenAIDeploymentName')

    -- Build the embeddings endpoint URL and request headers
    DECLARE @Url varchar(max) = @OpenAIEndpoint || 'openai/deployments/' || @OpenAIDeploymentName || '/embeddings?api-version=2023-03-15-preview'

    DECLARE @HeadersJson json =
        JSON_OBJECT(
            'api-key': @OpenAIApiKey
            RETURNING json
        )

    -- sp_invoke_external_rest_endpoint requires text parameters, so convert from native JSON only at the REST boundary
    DECLARE @MoviesPayloadText nvarchar(max) = CONVERT(nvarchar(max), @MoviesPayloadJson)
    DECLARE @HeadersText nvarchar(max) = CONVERT(nvarchar(max), @HeadersJson)

    DECLARE @ResponseText nvarchar(max)
    DECLARE @ReturnValue int

    -- Invoke the Azure OpenAI REST API to generate embeddings for the movie batch
    EXEC @ReturnValue = sp_invoke_external_rest_endpoint
        @url = @Url,
        @method = 'POST',
        @headers = @HeadersText,
        @payload = @MoviesPayloadText,
        @response = @ResponseText OUTPUT

    -- If the REST call failed, throw the returned response payload as the error message
    IF @ReturnValue != 0
        THROW 50000, @ResponseText, 1

    -- Convert the successful REST text response into native JSON
    DECLARE @ResponseJson json = CONVERT(json, @ResponseText)

    -- Extract MovieId values from the source batch and assign a zero-based index
    -- The index must match the embedding index returned by the Azure OpenAI response
    ;WITH MoviesCte AS (
        SELECT
            MovieId = CONVERT(int, JSON_VALUE(value, '$.MovieId')),
            MovieIndex = ROW_NUMBER() OVER (ORDER BY CONVERT(int, [key])) - 1
        FROM
            OPENJSON(@MoviesBatchJson)
    ),
    -- Extract each embedding vector from the response payload and cast it to SQL Server's vector(1536) type
    EmbeddingsCte AS (
        SELECT
            MovieIndex,
            Vector = CAST(Embedding AS vector(1536))
        FROM
            OPENJSON(@ResponseJson, '$.result.data')
        WITH (
            MovieIndex int '$.index',
            Embedding nvarchar(max) '$.embedding' AS JSON
        )
    ),
    -- Join the source movies to the returned embeddings using the shared positional index
    VectorsCte AS (
        SELECT
            mb.MovieId,
            e.Vector
        FROM
            MoviesCte AS mb
            INNER JOIN EmbeddingsCte AS e ON e.MovieIndex = mb.MovieIndex
    )
    -- Insert/update the MovieVector table with the generated vectors
    MERGE MovieVector AS t
        USING VectorsCte AS s
        ON t.MovieId = s.MovieId
    WHEN MATCHED THEN
        UPDATE SET Vector = s.Vector
    WHEN NOT MATCHED THEN
        INSERT (MovieId, Vector)
        VALUES (s.MovieId, s.Vector)
    ;

END
