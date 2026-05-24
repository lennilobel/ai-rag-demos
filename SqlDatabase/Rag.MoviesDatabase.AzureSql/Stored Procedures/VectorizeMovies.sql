CREATE PROCEDURE VectorizeMovies
    @MovieIdsCsv varchar(max) = NULL
AS
BEGIN

    SET NOCOUNT ON

    DECLARE @MoviesJson varchar(max) = dbo.GetMoviesJsonUdf(@MovieIdsCsv)

    IF LEFT(@MoviesJson, 1) = '{'
        SET @MoviesJson = CONCAT('[', @MoviesJson, ']')

    DECLARE @ErrorCount int = 0
    DECLARE @BatchSize int = 100
    DECLARE @CurrentPosition int = 0
    DECLARE @TotalCount int = (SELECT COUNT(*) FROM OPENJSON(@MoviesJson))
    DECLARE @Message varchar(max)

    WHILE @CurrentPosition < @TotalCount BEGIN

        BEGIN TRY

            DECLARE @MoviesBatchJson nvarchar(max)

            ;WITH BatchCte AS (
                SELECT
                    [key],
                    value
                FROM
                    OPENJSON(@MoviesJson)
                ORDER BY JSON_VALUE(value, '$.Title')
                OFFSET @CurrentPosition ROWS FETCH NEXT @BatchSize ROWS ONLY
            )
            SELECT @MoviesBatchJson =
                CONCAT(
                    '[',
                    STRING_AGG(CONVERT(varchar(max), value), ',') WITHIN GROUP (ORDER BY CONVERT(int, [key])),
                    ']'
                )
            FROM BatchCte

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

            EXEC VectorizeMoviesBatch @MoviesBatchJson

        END TRY

        BEGIN CATCH

            RAISERROR('An error occurred attempting to vectorize the movie batch', 0, 1) WITH NOWAIT

            SET @Message = ERROR_MESSAGE()
            RAISERROR(@Message, 0, 1) WITH NOWAIT

            SET @ErrorCount += 1

        END CATCH

        SET @CurrentPosition += @BatchSize

    END

    IF @ErrorCount > 0
        THROW 50000, 'One or more errors occurred vectorizing the movies data', 1

END
