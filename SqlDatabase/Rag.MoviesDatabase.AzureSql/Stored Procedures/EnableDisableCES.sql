CREATE PROCEDURE EnableDisableCES
	@Action varchar(max)
AS
BEGIN

	IF @Action = 'Enable' BEGIN

		EXEC sys.sp_enable_change_event_stream

		EXEC sys.sp_create_change_event_stream_group
			@stream_group_name      = 'MoviesCesGroup',
			@destination_type       = 'AzureEventHubs',
			@destination_location   = 'ces-namespace.servicebus.windows.net:9093/ces-hub',
			@destination_credential = MoviesCesCredential,
			@max_message_size_kb    = 1024,
			@partition_key_scheme   = 'StreamGroup'

		EXEC sys.sp_add_object_to_change_event_stream_group 'MoviesCesGroup', 'dbo.Movie'
		EXEC sys.sp_add_object_to_change_event_stream_group 'MoviesCesGroup', 'dbo.MovieGenre'
		EXEC sys.sp_add_object_to_change_event_stream_group 'MoviesCesGroup', 'dbo.MovieProductionCompany'
		EXEC sys.sp_add_object_to_change_event_stream_group 'MoviesCesGroup', 'dbo.MovieProductionCountry'
		EXEC sys.sp_add_object_to_change_event_stream_group 'MoviesCesGroup', 'dbo.MovieSpokenLanguage'

		RAISERROR('CES has been enabled', 0, 1) WITH NOWAIT

	END ELSE IF @Action = 'Disable' BEGIN

		EXEC sys.sp_remove_object_from_change_event_stream_group 'MoviesCesGroup', 'dbo.Movie'
		EXEC sys.sp_remove_object_from_change_event_stream_group 'MoviesCesGroup', 'dbo.MovieGenre'
		EXEC sys.sp_remove_object_from_change_event_stream_group 'MoviesCesGroup', 'dbo.MovieProductionCompany'
		EXEC sys.sp_remove_object_from_change_event_stream_group 'MoviesCesGroup', 'dbo.MovieProductionCountry'
		EXEC sys.sp_remove_object_from_change_event_stream_group 'MoviesCesGroup', 'dbo.MovieSpokenLanguage'

		EXEC sys.sp_drop_change_event_stream_group 'MoviesCesGroup'
		
		EXEC sys.sp_disable_change_event_stream

		RAISERROR('CES has been disabled', 0, 1) WITH NOWAIT

	END ELSE
		THROW 50000, '@Action parameter must be specified as either ''Enable'' or ''Disable''', 1

END
