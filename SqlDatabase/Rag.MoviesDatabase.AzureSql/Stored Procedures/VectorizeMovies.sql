CREATE PROCEDURE VectorizeMovies
    @MovieIdsCsv varchar(max) = NULL
AS
BEGIN

    SET NOCOUNT ON

    -- The UDF returns either a single JSON object (for a single movie) or a JSON array (for multiple movies).
    DECLARE @MoviesJson json = dbo.GetMoviesJsonUdf(@MovieIdsCsv)

    -- If a single movie is returned, wrap it as a single-element array.
    IF JSON_PATH_EXISTS(@MoviesJson, '$.MovieId') = 1
        SET @MoviesJson = JSON_ARRAY(@MoviesJson RETURNING json)

    -- Track processing state and batch progress.
    DECLARE @ErrorCount int = 0
    DECLARE @BatchSize int = 100
    DECLARE @CurrentPosition int = 0
    DECLARE @TotalCount int = (SELECT COUNT(*) FROM OPENJSON(@MoviesJson))
    DECLARE @Message varchar(max)

    -- Process the movies in batches.
    WHILE @CurrentPosition < @TotalCount
    BEGIN

        BEGIN TRY

            DECLARE @MoviesBatchJson json

            -- Retrieve the next batch of movie JSON objects
            -- and rebuild them as a native JSON array.
            ;WITH BatchCte AS (
                SELECT
                    [key],
                    value
                FROM
                    OPENJSON(@MoviesJson)
                ORDER BY
                    JSON_VALUE(value, '$.Title')
                OFFSET @CurrentPosition ROWS
                FETCH NEXT @BatchSize ROWS ONLY
            )
            SELECT
                @MoviesBatchJson =
                    JSON_ARRAYAGG(
                        CONVERT(json, value)
                        ORDER BY CONVERT(int, [key])
                        RETURNING json
                    )
            FROM
                BatchCte

            -- Emit informational progress messages for each movie in the batch.
            DECLARE @MovieJson json

            DECLARE curMovies CURSOR LOCAL FAST_FORWARD FOR
                SELECT
                    CONVERT(json, value)
                FROM
                    OPENJSON(@MoviesBatchJson)
                ORDER BY
                    CONVERT(int, [key])

            OPEN curMovies
            FETCH NEXT FROM curMovies INTO @MovieJson

            WHILE @@FETCH_STATUS = 0
            BEGIN

                SET @Message =
                    CONCAT(
                        'Vectorizing entity - ',
                        JSON_VALUE(@MovieJson, '$.Title'),
                        ' (ID ',
                        JSON_VALUE(@MovieJson, '$.MovieId'),
                        ')'
                    )

                RAISERROR(@Message, 0, 1) WITH NOWAIT

                FETCH NEXT FROM curMovies INTO @MovieJson

            END

            CLOSE curMovies
            DEALLOCATE curMovies

            -- Vectorize the batch.
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
