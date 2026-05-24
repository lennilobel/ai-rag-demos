IF NOT EXISTS (SELECT 1 FROM sys.database_scoped_credentials WHERE name = N'MoviesCesCredential') BEGIN

    ALTER DATABASE SCOPED CONFIGURATION SET PREVIEW_FEATURES = ON

    CREATE DATABASE SCOPED CREDENTIAL MoviesCesCredential
    WITH
	    IDENTITY = 'SHARED ACCESS SIGNATURE',
	    SECRET = 'SharedAccessSignature sr=https%3a%2f%2fces-namespace.servicebus.windows.net%2fces-hub&sig=Wy7Y0M%2fHsk8LHW6SnlC1QpEujrCESGaC7whhNJqy5AM%3d&se=2095209230&skn=ces-policy'

    EXEC sys.sp_enable_event_stream

    EXEC sys.sp_create_event_stream_group
        @stream_group_name      = N'MoviesCesGroup',
        @destination_type       = N'AzureEventHubsAmqp',
        @destination_location   = N'ces-namespace.servicebus.windows.net/ces-hub',
        @destination_credential = MoviesCesCredential,
        @max_message_size_kb    = 1024,
        @partition_key_scheme   = N'StreamGroup'

    EXEC sys.sp_add_object_to_event_stream_group 'MoviesCesGroup', 'Movie'
    EXEC sys.sp_add_object_to_event_stream_group 'MoviesCesGroup', 'MovieGenre'
    EXEC sys.sp_add_object_to_event_stream_group 'MoviesCesGroup', 'MovieProductionCompany'
    EXEC sys.sp_add_object_to_event_stream_group 'MoviesCesGroup', 'MovieProductionCountry'
    EXEC sys.sp_add_object_to_event_stream_group 'MoviesCesGroup', 'MovieSpokenLanguage'

END
