CREATE PROCEDURE RunVectorSearchANN
	@Vector vector(1536)
AS
BEGIN

	SELECT TOP 5
		MovieJson = dbo.GetMoviesJsonUdf(mt.MovieId),
		CosineDistance = mvs.Distance
	FROM
		VECTOR_SEARCH(
			TABLE		= Movie AS mt,
			COLUMN		= Vector,
			SIMILAR_TO	= @Vector,
			METRIC		= 'cosine',
			TOP_N		= 5
		) AS mvs
		INNER JOIN Movie AS m ON m.MovieId = mt.MovieId
	ORDER BY
		CosineDistance
	
END
GO

CREATE PROCEDURE AskQuestionANN
	@Question varchar(max)
AS
BEGIN

	DECLARE @Vector vector(1536)

	EXEC VectorizeText @Question, @Vector OUTPUT

	EXEC RunVectorSearchANN @Vector

END
GO
