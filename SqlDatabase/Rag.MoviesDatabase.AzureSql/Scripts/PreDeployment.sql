IF NOT EXISTS (SELECT 1 FROM sys.symmetric_keys WHERE name = '##MS_DatabaseMasterKey##')
    CREATE MASTER KEY ENCRYPTION BY PASSWORD = 'Str0ngP@$$w0rd'
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_scoped_credentials WHERE name = N'BlobStorageCredential')
    CREATE DATABASE SCOPED CREDENTIAL BlobStorageCredential
    WITH
        IDENTITY = 'SHARED ACCESS SIGNATURE',
        SECRET = '$(StorageSasToken)'
GO

IF NOT EXISTS (SELECT 1 FROM sys.external_data_sources WHERE name = N'BlobStorageContainer')
    CREATE EXTERNAL DATA SOURCE BlobStorageContainer
    WITH (
	    TYPE = BLOB_STORAGE,
	    LOCATION = 'https://lennidemo.blob.core.windows.net/datasets',
	    CREDENTIAL = BlobStorageCredential
    )
GO
