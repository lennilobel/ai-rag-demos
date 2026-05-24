using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rag.AIClient.Engine.RagProviders.Base;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Rag.AIClient.Engine.RagProviders.Sql
{
	public class SqlServer2022DataPopulator : SqlDataPopulatorBase
	{
		public SqlServer2022DataPopulator(IRagProvider ragProvider)
			: base(ragProvider)
		{
		}

		public override async Task UpdateData()
		{
			Debugger.Break();

			await base.UpdateData();

			var started = DateTime.Now;

			ConsoleHelper.WriteHeading("Vectorize Updated Data", ConsoleHelper.UserColor);

			var localFilename = base.RagProvider.GetDataFileLocalPath(base.RagProvider.SqlConfig.JsonUpdateDataFilename);
			var documents = JsonConvert.DeserializeObject<JArray>(File.ReadAllText(localFilename));
			var movieIds = documents.Select(d => ((JObject)d)["id"].Value<int>()).ToArray();

			var vectorizer = base.RagProvider.GetDataVectorizer();
			await vectorizer.VectorizeData(movieIds);

			var elapsed = DateTime.Now.Subtract(started);
			ConsoleHelper.WriteLine();
			ConsoleHelper.WriteLine($"Updated data vectorized in {elapsed}", ConsoleHelper.InfoColor);
		}

	}
}
