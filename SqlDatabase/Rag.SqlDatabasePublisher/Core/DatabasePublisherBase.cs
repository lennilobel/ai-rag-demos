using Microsoft.SqlServer.Dac;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Rag.SqlDatabasePublisher.Core
{
	public abstract class DatabasePublisherBase : IDatabasePublisher
	{
		public string SqlProjectFile => Path.GetFullPath(@"..\..\..\..\..\SqlDatabase\Rag.MoviesDatabase.AzureSql\Rag.MoviesDatabase.AzureSql.sqlproj");

		public async Task Publish(DatabasePublisherConfig config)
		{
			Debugger.Break();

			var dacpacPath = this.BuildDatabaseProject();
			var publishOptions = new PublishOptions
			{
				DeployOptions = new()
				{
					BlockOnPossibleDataLoss = false
				},
				GenerateDeploymentReport = true
			};

			publishOptions.DeployOptions.SqlCommandVariableValues["CesSasToken"] = config.SqlCommandVariables["CesSasToken"];
			publishOptions.DeployOptions.SqlCommandVariableValues["StorageSasToken"] = config.SqlCommandVariables["StorageSasToken"];

			var dacServices = new DacServices(config.SqlConnectionString);

			dacServices.Message += (_, e) =>
			{
				Console.WriteLine(e.Message.Message);
			};

			using var dacpac = DacPackage.Load(dacpacPath);

			dacServices.Publish(dacpac, config.DatabaseName, publishOptions);

			Console.WriteLine("Publish complete.");
		}

		private string BuildDatabaseProject()
		{
			var sqlprojFile = this.SqlProjectFile;

			if (!File.Exists(sqlprojFile))
			{
				throw new FileNotFoundException($"Database project file '{sqlprojFile}' not found");
			}

			var msbuildExe = this.GetMSBuildPath();
			var configuration = "Debug";
			var msbuildArgs = $"\"{sqlprojFile}\" /restore /p:Configuration={configuration} /v:minimal";

			var build = Process.Start(new ProcessStartInfo
			{
				FileName = msbuildExe,
				Arguments = msbuildArgs,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			}) ?? throw new InvalidOperationException("Failed to start MSBuild.");

			var stdout = build.StandardOutput.ReadToEnd();
			var stderr = build.StandardError.ReadToEnd();

			build.WaitForExit();

			if (build.ExitCode != 0)
			{
				throw new Exception($"""
SQL project build failed.

Command:
"{msbuildExe}" {msbuildArgs}

Exit code:
{build.ExitCode}

STDOUT:
{stdout}

STDERR:
{stderr}
"""
				);
			}

			var projectDir = Path.GetDirectoryName(sqlprojFile)!;
			var projectName = Path.GetFileNameWithoutExtension(sqlprojFile);

			var dacpacFile = Path.Combine(
				projectDir,
				"bin",
				configuration,
				$"{projectName}.dacpac");

			if (!File.Exists(dacpacFile))
			{
				throw new FileNotFoundException($"Expected dacpac file '{dacpacFile}' was not produced.");
			}

			return dacpacFile;
		}

		private string GetMSBuildPath()
		{
			var vswhere = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
				@"Microsoft Visual Studio\Installer\vswhere.exe");

			var process = Process.Start(new ProcessStartInfo
			{
				FileName = vswhere,
				Arguments = "-latest -requires Microsoft.Component.MSBuild -find MSBuild\\**\\Bin\\MSBuild.exe",
				RedirectStandardOutput = true,
				UseShellExecute = false,
				CreateNoWindow = true
			})!;

			var output = process.StandardOutput.ReadToEnd().Trim();
			process.WaitForExit();

			if (process.ExitCode != 0 || string.IsNullOrWhiteSpace(output))
			{
				throw new FileNotFoundException("MSBuild.exe not found.");
			}

			return output.Split(Environment.NewLine)[0];
		}

	}
}
