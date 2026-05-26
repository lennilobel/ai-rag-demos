using Rag.AIClient.Engine.RagProviders.NoSql.CosmosDb;
using Rag.AIClient.Engine.RagProviders.NoSql.MongoDb;
using Rag.AIClient.Engine.RagProviders.Sql.AzureSql;
using Rag.AIClient.Engine.RagProviders.Sql.SqlServer;
using Rag.AIClient.Engine.RagProviders.Sql.SqlServer2025;
using System;
using System.Linq;
using System.Reflection;

namespace Rag.AIClient.Engine.RagProviders.Core
{
	public static class RagProviderFactory
	{
		public static RagProviderType RagProviderType { get; set; }
		public static string ExternalRagProviderType { get; set; }

		static RagProviderFactory()
		{
			var args = Environment.GetCommandLineArgs();

			RagProviderType = args.Length > 1
				? (RagProviderType)Enum.Parse(typeof(RagProviderType), args[1], ignoreCase: true)
				: Shared.AppConfig.RagProviderType;

			ExternalRagProviderType = args.Length > 2
				? args[2]
				: Shared.AppConfig.ExternalRagProviderType;
		}

		public static IRagProvider GetRagProvider()
		{
			if (RagProviderType == RagProviderType.External)
			{
				return GetExternalRagProvider();
			}

			var expectedTypeName = $"{RagProviderType}RagProvider";

			var providerType = AppDomain.CurrentDomain
				.GetAssemblies()
				.SelectMany(a => a.GetTypes())
				.SingleOrDefault(t =>
					typeof(IRagProvider).IsAssignableFrom(t) &&
					!t.IsAbstract &&
					string.Equals(t.Name, expectedTypeName, StringComparison.Ordinal))
				?? throw new NotSupportedException($"No provider is implemented for RAG provider type {RagProviderType}");
			
			var ragProvider = (IRagProvider)Activator.CreateInstance(providerType);

			return ragProvider;
		}

		private static IRagProvider GetExternalRagProvider()
		{
			var externalRagProvider = Shared.AppConfig.ExternalRagProviders.FirstOrDefault(erp => string.Equals(erp.ExternalRagProviderType, ExternalRagProviderType, StringComparison.OrdinalIgnoreCase))
				?? throw new NotSupportedException($"No external provider exists for external RAG provider type {ExternalRagProviderType}");

			var assemblyPath = externalRagProvider.ExternalRagProviderAssemblyPath;
			var assembly = Assembly.LoadFrom(assemblyPath);

			var className = externalRagProvider.ExternalRagProviderClassName;
			var type = Type.GetType(className);

			if (type == null || !typeof(IRagProvider).IsAssignableFrom(type))
			{
				throw new InvalidOperationException($"The external provider class '{className}' does not exist or does not implement IRagProvider.");
			}

			var provider = (IRagProvider)Activator.CreateInstance(type);

			return provider;
		}

	}
}
