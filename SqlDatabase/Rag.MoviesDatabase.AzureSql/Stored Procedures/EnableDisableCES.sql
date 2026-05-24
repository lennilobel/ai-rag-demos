CREATE PROCEDURE EnableDisableCES
	@Action varchar(max)
AS
BEGIN

	IF @Action = 'Enable' BEGIN

		IF EXISTS (SELECT 1 FROM sys.database_scoped_credentials WHERE name = 'MoviesCesCredential')
			THROW 50000, 'CES is already enabled', 1

		CREATE DATABASE SCOPED CREDENTIAL MoviesCesCredential
		WITH
		  IDENTITY = 'SHARED ACCESS SIGNATURE',
		  SECRET = 'SharedAccessSignature sr=https%3a%2f%2fces-namespace.servicebus.windows.net%2fces-hub&sig=U18owfRqU%2fnyahIv29HeRiVVcICE0TUt4SKL%2fcWCkcU%3d&se=1767293868&skn=ces-policy'

		EXEC sys.sp_enable_event_stream

		EXEC sys.sp_create_event_stream_group
		  @stream_group_name      = 'MoviesCesGroup',
		  @destination_location   = 'ces-namespace.servicebus.windows.net/ces-hub',
		  @destination_credential = MoviesCesCredential,
		  @destination_type       = 'AzureEventHubsAmqp',
		  @max_message_size_bytes = 10000000,
		  @partition_key_scheme   = 'StreamGroup'

		EXEC sys.sp_add_object_to_event_stream_group 'MoviesCesGroup', 'Movie'
		EXEC sys.sp_add_object_to_event_stream_group 'MoviesCesGroup', 'MovieGenre'
		EXEC sys.sp_add_object_to_event_stream_group 'MoviesCesGroup', 'MovieProductionCompany'
		EXEC sys.sp_add_object_to_event_stream_group 'MoviesCesGroup', 'MovieProductionCountry'
		EXEC sys.sp_add_object_to_event_stream_group 'MoviesCesGroup', 'MovieSpokenLanguage'

		RAISERROR('CES has been enabled', 0, 1) WITH NOWAIT

	END ELSE IF @Action = 'Disable' BEGIN

		IF NOT EXISTS (SELECT 1 FROM sys.database_scoped_credentials WHERE name = 'MoviesCesCredential')
			THROW 50000, 'CES is already disabled', 1

		EXEC sys.sp_remove_object_from_event_stream_group 'MoviesCesGroup', 'Movie'
		EXEC sys.sp_remove_object_from_event_stream_group 'MoviesCesGroup', 'MovieGenre'
		EXEC sys.sp_remove_object_from_event_stream_group 'MoviesCesGroup', 'MovieProductionCompany'
		EXEC sys.sp_remove_object_from_event_stream_group 'MoviesCesGroup', 'MovieProductionCountry'
		EXEC sys.sp_remove_object_from_event_stream_group 'MoviesCesGroup', 'MovieSpokenLanguage'

		EXEC sys.sp_drop_event_stream_group 'MoviesCesGroup'
		
		EXEC sys.sp_disable_event_stream

		DROP DATABASE SCOPED CREDENTIAL MoviesCesCredential

		RAISERROR('CES has been disabled', 0, 1) WITH NOWAIT

	END ELSE
		THROW 50000, '@Action parameter must be specified as either ''Enable'' or ''Disable''', 1

END
