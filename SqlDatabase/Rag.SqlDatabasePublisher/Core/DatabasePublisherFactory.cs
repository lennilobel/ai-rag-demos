using System;

namespace Rag.SqlDatabasePublisher.Core
{
	public static class DatabasePublisherFactory
	{
		public static IDatabasePublisher GetDatabasePublisher(DatabasePublisherType databasePublisherType) =>
			databasePublisherType switch
			{
				DatabasePublisherType.SqlServer2025 => new SqlServer2025DatabasePublisher(),
				DatabasePublisherType.AzureSql => new AzureSqlDatabasePublisher(),
				_ => throw new NotSupportedException($"No publisher is implemented for database publisher type {databasePublisherType}"),
			};

	}
}
