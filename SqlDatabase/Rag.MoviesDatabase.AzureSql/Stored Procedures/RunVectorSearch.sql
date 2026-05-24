CREATE PROCEDURE RunVectorSearch
    @Vector vector(1536)
AS
BEGIN

    SELECT TOP 5
        MovieJson = dbo.GetMoviesJsonUdf(m.MovieId),
        CosineDistance = VECTOR_DISTANCE('cosine', @Vector, mv.Vector)
    FROM
        MovieVector AS mv
        INNER JOIN Movie AS m ON mv.MovieId = m.MovieId
    ORDER BY
        CosineDistance

END
