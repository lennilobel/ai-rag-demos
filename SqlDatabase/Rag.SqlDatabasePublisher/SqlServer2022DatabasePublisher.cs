using Rag.SqlDatabasePublisher.Core;
using System.IO;

namespace Rag.SqlDatabasePublisher
{
	public class SqlServer2022DatabasePublisher : DatabasePublisherBase
	{
		public override string SqlProjectFile => Path.GetFullPath(@"..\..\..\..\..\SqlDatabase\Rag.MoviesDatabase.SqlServer2022\Rag.MoviesDatabase.SqlServer2022.sqlproj");
	}
}
