using Microsoft.SqlServer.Dac;
using Rag.SqlDatabasePublisher.Core;
using System.IO;

namespace Rag.SqlDatabasePublisher
{
	public class SqlServer2025DatabasePublisher : DatabasePublisherBase
	{
		public override string SqlProjectFile => Path.GetFullPath(@"..\..\..\..\..\SqlDatabase\Rag.MoviesDatabase.SqlServer2025\Rag.MoviesDatabase.SqlServer2025.sqlproj");

		protected override void SetPublishOptions(PublishOptions publishOptions, DatabasePublisherConfig config)
		{
			publishOptions.DeployOptions.SqlCommandVariableValues["OpenAIApiKey"] = config.SqlCommandVariables["OpenAIApiKey"];
		}
	}
}
