namespace Rag.AIClient.Engine.RagProviders.Core
{
    public enum RagProviderType
    {
        SqlServer2022,      // SQL Server 2022
		SqlServer2025,      // SQL Server 2025
		AzureSql,           // Azure SQL Database
		CosmosDb,           // Azure Cosmos DB for NoSQL
        MongoDb,            // Azure Cosmos DB for MongoDB vCore
        External,           // An external provider based on any of the above provider types
    }

}
