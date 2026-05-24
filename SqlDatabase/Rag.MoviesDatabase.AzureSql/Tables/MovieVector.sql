CREATE TABLE MovieVector
(
	MovieId	int PRIMARY KEY,
	Vector		vector(1536),

	CONSTRAINT FK_MovieVector_Movie FOREIGN KEY (MovieId) REFERENCES Movie (MovieId),
)
