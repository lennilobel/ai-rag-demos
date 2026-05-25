using Rag.AIClient.Engine.RagProviders;
using System;

namespace Rag.SqlDatabasePublisher.Base
{
	public static class DatabasePublisherFactory
	{
		public static IDatabasePublisher GetDatabasePublisher(RagProviderType ragProviderType) =>
			ragProviderType switch
			{
				RagProviderType.SqlServer2025 => new SqlServer2025DatabasePublisher(),
				RagProviderType.AzureSql => new AzureSqlDatabasePublisher(),
				_ => throw new NotSupportedException($"No database publisher is implemented for RAG provider type {ragProviderType}"),
			};

	}
}
