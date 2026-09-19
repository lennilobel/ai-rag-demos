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

	-- Open a cursor to process one movie at a time
	DECLARE @MovieJson varchar(max)
	DECLARE @ErrorCount int = 0

	DECLARE curMovies CURSOR FOR
		SELECT value FROM OPENJSON(@MoviesJson) ORDER BY JSON_VALUE(value, '$.Title')

	OPEN curMovies
	FETCH NEXT FROM curMovies INTO @MovieJson

	WHILE @@FETCH_STATUS = 0
	BEGIN

		-- Echo current movie title and ID using RAISERROR (will display on console)
		DECLARE @MovieId int = JSON_VALUE(@MovieJson, '$.MovieId')
		DECLARE @Title varchar(max) = JSON_VALUE(@MovieJson, '$.Title')

		DECLARE @Message varchar(max) = CONCAT('Vectorizing movie: ', @Title, ' (', @MovieId, ')')
		RAISERROR(@Message, 0, 1) WITH NOWAIT

		-- Generate a vector from the movie JSON using the configured embedding model
		DECLARE @MovieVector vector(1536)

		BEGIN TRY

			SELECT @MovieVector = AI_GENERATE_EMBEDDINGS(@MovieJson USE MODEL MoviesTextEmbeddingModel)

		END TRY

		-- Handle vectorization error
		BEGIN CATCH

			RAISERROR('An error occurred attempting to vectorize the movie', 0, 1) WITH NOWAIT

			SET @Message = ERROR_MESSAGE()
			RAISERROR(@Message, 0, 1) WITH NOWAIT

			SET @ErrorCount +=1

		END CATCH

		-- Insert the vector for a newly vectorized movie, or replace the existing vector if one already exists
		MERGE MovieVector AS t
			USING (
				SELECT
					MovieId = @MovieId,
					Vector = @MovieVector
			) AS s ON t.MovieId = s.MovieId
			WHEN MATCHED THEN
				UPDATE SET Vector = s.Vector
			WHEN NOT MATCHED THEN
				INSERT (MovieId, Vector)
				VALUES (s.MovieId, s.Vector)
		;

		FETCH NEXT FROM curMovies INTO @MovieJson

	END

	CLOSE curMovies
	DEALLOCATE curMovies

	IF @ErrorCount > 0
		THROW 50000, 'One or more errors occurred vectorizing the movies data', 1

END
