IF NOT EXISTS (SELECT 1 FROM sys.database_scoped_credentials WHERE name = N'MoviesCesCredential') BEGIN

    CREATE DATABASE SCOPED CREDENTIAL MoviesCesCredential
    WITH
	    IDENTITY = 'ces-policy',
        SECRET = '$(CesPolicyKey)'

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

END
