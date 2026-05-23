/*
	*** Create Database ***
*/

CREATE DATABASE MoviesDB
GO

EXEC sp_configure 'external rest endpoint enabled', 1
RECONFIGURE
