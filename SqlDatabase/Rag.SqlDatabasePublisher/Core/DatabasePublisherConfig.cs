using System.Collections.Generic;

namespace Rag.SqlDatabasePublisher.Core
{
	public class DatabasePublisherConfig
	{
		public DatabasePublisherType DatabasePublisherType { get; init; }
		public string SqlConnectionString { get; init; }
		public string DatabaseName { get; init; }
		public Dictionary<string, string> SqlCommandVariables { get; init; }
	}
}
