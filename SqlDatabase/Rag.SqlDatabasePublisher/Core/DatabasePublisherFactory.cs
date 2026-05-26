using System;
using System.Linq;

namespace Rag.SqlDatabasePublisher.Core
{
	public static class DatabasePublisherFactory
	{
		public static IDatabasePublisher GetDatabasePublisher(DatabasePublisherType databasePublisherType)
		{
			var publisherTypeName = $"Rag.SqlDatabasePublisher.{databasePublisherType}DatabasePublisher";

			var publisherType = AppDomain.CurrentDomain
				.GetAssemblies()
				.Select(assembly => assembly.GetType(publisherTypeName, throwOnError: false))
				.SingleOrDefault(type => type is not null)
					?? throw new NotSupportedException($"No publisher is implemented for database publisher type {databasePublisherType}");
			
			return (IDatabasePublisher)Activator.CreateInstance(publisherType)!;
		}

	}
}
