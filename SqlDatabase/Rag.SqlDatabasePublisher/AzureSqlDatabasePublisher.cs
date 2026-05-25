using Microsoft.SqlServer.Dac;
using Rag.SqlDatabasePublisher.Core;
using System.IO;

namespace Rag.SqlDatabasePublisher
{
	public class AzureSqlDatabasePublisher : DatabasePublisherBase
	{
		public override string SqlProjectFile => Path.GetFullPath(@"..\..\..\..\..\SqlDatabase\Rag.MoviesDatabase.AzureSql\Rag.MoviesDatabase.AzureSql.sqlproj");

		protected override void SetPublishOptions(PublishOptions publishOptions, DatabasePublisherConfig config)
		{
			publishOptions.DeployOptions.SqlCommandVariableValues["CesSasToken"] = config.SqlCommandVariables["CesSasToken"];
			publishOptions.DeployOptions.SqlCommandVariableValues["StorageSasToken"] = config.SqlCommandVariables["StorageSasToken"];
		}
	}
}
