CREATE PROCEDURE RunVectorSearch
    @Vector vector(1536)
AS
BEGIN

    SELECT TOP 5
        MovieJson = dbo.GetMoviesJsonUdf(MovieId),
        CosineDistance = VECTOR_DISTANCE('cosine', @Vector, Vector)
    FROM
        Movie
    WHERE
        Vector IS NOT NULL
    ORDER BY
        CosineDistance

END
