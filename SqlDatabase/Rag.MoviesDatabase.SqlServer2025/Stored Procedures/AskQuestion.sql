CREATE PROCEDURE AskQuestion
	@Question varchar(max)
AS
BEGIN

	DECLARE @Vector vector(1536)

	SELECT @Vector = AI_GENERATE_EMBEDDINGS(@Question USE MODEL MoviesTextEmbeddingModel)

	EXEC RunVectorSearch @Vector

END
