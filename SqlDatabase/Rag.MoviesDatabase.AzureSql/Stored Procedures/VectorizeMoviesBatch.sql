CREATE PROCEDURE VectorizeMoviesBatch
    @MoviesBatchJson varchar(max)
AS
BEGIN

    DECLARE @MoviesPayload nvarchar(max)

    SELECT @MoviesPayload =
        JSON_OBJECT(
            'input': JSON_QUERY(
                '[' + STRING_AGG(
                    '"' + STRING_ESCAPE(CONVERT(nvarchar(max), value), 'json') + '"',
                    ','
                ) + ']'
            )
        )
    FROM OPENJSON(@MoviesBatchJson)

    DECLARE @OpenAIEndpoint varchar(max)        = (SELECT ConfigValue FROM AppConfig WHERE ConfigKey = 'OpenAIEndpoint')
    DECLARE @OpenAIApiKey varchar(max)          = (SELECT ConfigValue FROM AppConfig WHERE ConfigKey = 'OpenAIApiKey')
    DECLARE @OpenAIDeploymentName varchar(max)  = (SELECT ConfigValue FROM AppConfig WHERE ConfigKey = 'OpenAIDeploymentName')

    DECLARE @Url varchar(max) = CONCAT(@OpenAIEndpoint, 'openai/deployments/', @OpenAIDeploymentName, '/embeddings?api-version=2023-03-15-preview')
    DECLARE @Headers varchar(max) = JSON_OBJECT('api-key': @OpenAIApiKey)
    DECLARE @Response nvarchar(max)
    DECLARE @ReturnValue int

    EXEC @ReturnValue = sp_invoke_external_rest_endpoint
        @url = @Url,
        @method = 'POST',
        @headers = @Headers,
        @payload = @MoviesPayload,
        @response = @Response OUTPUT

    IF @ReturnValue != 0
        THROW 50000, @Response, 1

    ;WITH MoviesCte AS (
        SELECT
            MovieId = CONVERT(int, JSON_VALUE(value, '$.MovieId')),
            MovieIndex = ROW_NUMBER() OVER (ORDER BY CONVERT(int, [key])) - 1
        FROM
            OPENJSON(@MoviesBatchJson)
    ),
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
    UPDATE m
        SET Vector = e.Vector
    FROM
        Movie AS m
        INNER JOIN MoviesCte AS mb ON mb.MovieId = m.MovieId
        INNER JOIN EmbeddingsCte AS e ON e.MovieIndex = mb.MovieIndex
END
