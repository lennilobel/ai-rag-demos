# This runs Azurite (an Azure Storage Emulator) in a Docker container, as an alternative to using a real Azure Storage account for checkpointing Azure Event Hubs.
docker run -p 10000:10000 -p 10001:10001 -p 10002:10002 mcr.microsoft.com/azure-storage/azurite
