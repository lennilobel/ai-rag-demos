IF NOT EXISTS (SELECT 1 FROM sys.database_scoped_credentials WHERE name = N'MoviesCesCredential') BEGIN

    CREATE DATABASE SCOPED CREDENTIAL MoviesCesCredential
    WITH
	    IDENTITY = 'SHARED ACCESS SIGNATURE',
        SECRET = '$(CesSasToken)'

    EXEC sys.sp_enable_event_stream

    EXEC sys.sp_create_event_stream_group
        @stream_group_name      = 'MoviesCesGroup',
        @destination_type       = 'AzureEventHubsAmqp',
        @destination_location   = 'ces-namespace.servicebus.windows.net/ces-hub',
        @destination_credential = MoviesCesCredential,
        @max_message_size_kb    = 1024,
        @partition_key_scheme   = 'StreamGroup'

    EXEC sys.sp_add_object_to_event_stream_group 'MoviesCesGroup', 'Movie'
    EXEC sys.sp_add_object_to_event_stream_group 'MoviesCesGroup', 'MovieGenre'
    EXEC sys.sp_add_object_to_event_stream_group 'MoviesCesGroup', 'MovieProductionCompany'
    EXEC sys.sp_add_object_to_event_stream_group 'MoviesCesGroup', 'MovieProductionCountry'
    EXEC sys.sp_add_object_to_event_stream_group 'MoviesCesGroup', 'MovieSpokenLanguage'

END
