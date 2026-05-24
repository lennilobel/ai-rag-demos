CREATE PROCEDURE EnableDisableCES
	@Action varchar(max)
AS
BEGIN

	IF @Action = 'Enable' BEGIN

		EXEC sys.sp_enable_event_stream

		EXEC sys.sp_create_event_stream_group
			@stream_group_name      = 'MoviesCesGroup',
			@destination_location   = 'ces-namespace.servicebus.windows.net/ces-hub',
			@destination_credential = MoviesCesCredential,
			@destination_type       = 'AzureEventHubsAmqp',
			@max_message_size_kb    = 1024,
			@partition_key_scheme   = 'StreamGroup'

		EXEC sys.sp_add_object_to_event_stream_group 'MoviesCesGroup', 'Movie'
		EXEC sys.sp_add_object_to_event_stream_group 'MoviesCesGroup', 'MovieGenre'
		EXEC sys.sp_add_object_to_event_stream_group 'MoviesCesGroup', 'MovieProductionCompany'
		EXEC sys.sp_add_object_to_event_stream_group 'MoviesCesGroup', 'MovieProductionCountry'
		EXEC sys.sp_add_object_to_event_stream_group 'MoviesCesGroup', 'MovieSpokenLanguage'

		RAISERROR('CES has been enabled', 0, 1) WITH NOWAIT

	END ELSE IF @Action = 'Disable' BEGIN

		EXEC sys.sp_remove_object_from_event_stream_group 'MoviesCesGroup', 'Movie'
		EXEC sys.sp_remove_object_from_event_stream_group 'MoviesCesGroup', 'MovieGenre'
		EXEC sys.sp_remove_object_from_event_stream_group 'MoviesCesGroup', 'MovieProductionCompany'
		EXEC sys.sp_remove_object_from_event_stream_group 'MoviesCesGroup', 'MovieProductionCountry'
		EXEC sys.sp_remove_object_from_event_stream_group 'MoviesCesGroup', 'MovieSpokenLanguage'

		EXEC sys.sp_drop_event_stream_group 'MoviesCesGroup'
		
		EXEC sys.sp_disable_event_stream

		RAISERROR('CES has been disabled', 0, 1) WITH NOWAIT

	END ELSE
		THROW 50000, '@Action parameter must be specified as either ''Enable'' or ''Disable''', 1

END
