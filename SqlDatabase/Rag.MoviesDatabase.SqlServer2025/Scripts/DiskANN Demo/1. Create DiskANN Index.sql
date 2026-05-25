-- 1m 10s with MAXDOP = 8 on Surface Pro 8
CREATE VECTOR INDEX MoviesVectorDiskANNIndex 
	ON Movie (Vector)
	WITH (
		METRIC = 'cosine',
		TYPE = 'diskann',
		MAXDOP = 8
	)
GO

-- Fails... the table is readonly once it has a vector index on it
UPDATE Movie SET Budget += 100 WHERE MovieId = 16
