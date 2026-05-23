/*
	*** Cleanup ***
*/

USE MoviesDB
GO

DROP PROCEDURE IF EXISTS VectorSearch
DROP PROCEDURE IF EXISTS VectorizeText
DROP TABLE IF EXISTS Movie

EXEC sp_configure 'external rest endpoint enabled', 0
RECONFIGURE
