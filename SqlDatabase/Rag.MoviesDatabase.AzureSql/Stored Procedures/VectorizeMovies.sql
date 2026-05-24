CREATE PROCEDURE VectorizeMovies
    @MovieIdsCsv varchar(max) = NULL
AS
BEGIN

    SET NOCOUNT ON

    -- The UDF returns either a single JSON object (for a single movie) or a JSON array (for multiple movies).
    DECLARE @MoviesJson varchar(max) = dbo.GetMoviesJsonUdf(@MovieIdsCsv)

    -- If a single movie is returned, wrap it as a single-element array.
    IF LEFT(@MoviesJson, 1) = '{'
        SET @MoviesJson = CONCAT('[', @MoviesJson, ']')

    -- Track processing state and batch progress.
    DECLARE @ErrorCount int = 0
    DECLARE @BatchSize int = 100
    DECLARE @CurrentPosition int = 0
    DECLARE @TotalCount int = (SELECT COUNT(*) FROM OPENJSON(@MoviesJson))
    DECLARE @Message varchar(max)

    -- Process the movies in batches
    WHILE @CurrentPosition < @TotalCount BEGIN

        BEGIN TRY

            DECLARE @MoviesBatchJson nvarchar(max)

            -- Retrieve the next batch of movie JSON objects (movies are sorted alphabetically by title for predictable processing order).
            ;WITH BatchCte AS (
                SELECT
                    [key],
                    value
                FROM
                    OPENJSON(@MoviesJson)
                ORDER BY JSON_VALUE(value, '$.Title')
                OFFSET @CurrentPosition ROWS FETCH NEXT @BatchSize ROWS ONLY
            )
            -- Rebuild the selected batch as a JSON array (preserve the original JSON array ordering so embedding indexes align correctly).
            SELECT @MoviesBatchJson =
                CONCAT(
                    '[',
                    STRING_AGG(CONVERT(varchar(max), value), ',') WITHIN GROUP (ORDER BY CONVERT(int, [key])),
                    ']'
                )
            FROM BatchCte

            -- Emit informational progress messages for each movie in the batch.
            DECLARE @MovieJson varchar(max)
            DECLARE curMovies CURSOR LOCAL FAST_FORWARD FOR SELECT value FROM OPENJSON(@MoviesBatchJson) ORDER BY CONVERT(int, [key])
            OPEN curMovies
            FETCH NEXT FROM curMovies INTO @MovieJson
            WHILE @@FETCH_STATUS = 0
            BEGIN
                SET @Message = CONCAT('Vectorizing entity - ', JSON_VALUE(@MovieJson, '$.Title'), ' (ID ', JSON_VALUE(@MovieJson, '$.MovieId'), ')')
                RAISERROR(@Message, 0, 1) WITH NOWAIT
                FETCH NEXT FROM curMovies INTO @MovieJson
            END
            CLOSE curMovies
            DEALLOCATE curMovies

            -- Vectorize the batch
            EXEC VectorizeMoviesBatch @MoviesBatchJson

        END TRY

        BEGIN CATCH

            RAISERROR('An error occurred attempting to vectorize the movie batch', 0, 1) WITH NOWAIT

            SET @Message = ERROR_MESSAGE()
            RAISERROR(@Message, 0, 1) WITH NOWAIT

            SET @ErrorCount += 1

        END CATCH

        -- Advance to the next batch.
        SET @CurrentPosition += @BatchSize

    END

    -- If any batch failed, raise a terminating error after all processing completes.
    IF @ErrorCount > 0
        THROW 50000, 'One or more errors occurred vectorizing the movies data', 1

END
