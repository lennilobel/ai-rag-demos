IF NOT EXISTS (SELECT 1 FROM sys.symmetric_keys WHERE name = '##MS_DatabaseMasterKey##')
    CREATE MASTER KEY ENCRYPTION BY PASSWORD = 'Str0ngP@$$w0rd'
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_scoped_credentials WHERE name = N'BlobStorageCredential')
    CREATE DATABASE SCOPED CREDENTIAL BlobStorageCredential
    WITH
        IDENTITY = 'SHARED ACCESS SIGNATURE',   -- SAS token for Blob, Object, Read access (expires 5/24/2046)
        SECRET = 'sv=2026-02-06&ss=b&srt=o&sp=r&se=2046-05-24T07:57:47Z&st=2026-05-23T23:42:47Z&spr=https&sig=McQy0WYUcBJUuqIVr7QHLpTvA8hkfrvxxv%2FQxrD%2B470%3D'
GO

IF NOT EXISTS (SELECT 1 FROM sys.external_data_sources WHERE name = N'BlobStorageContainer')
    CREATE EXTERNAL DATA SOURCE BlobStorageContainer
    WITH (
	    TYPE = BLOB_STORAGE,
	    LOCATION = 'https://lennidemo.blob.core.windows.net/datasets',
	    CREDENTIAL = BlobStorageCredential
    )
GO
