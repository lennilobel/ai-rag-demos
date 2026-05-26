using Microsoft.SqlServer.Dac;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Rag.SqlDatabasePublisher.Core
{
	public abstract class DatabasePublisherBase : IDatabasePublisher
	{
		public abstract string SqlProjectFile { get; }

		public async Task Publish(DatabasePublisherConfig config)
		{
			Debugger.Break();

			var dacpacFile = this.BuildDatabaseProject();

			this.DeployDacpacFile(dacpacFile, config);
		}

		private string BuildDatabaseProject()
		{
			var sqlprojFile = this.SqlProjectFile;

			if (!File.Exists(sqlprojFile))
			{
				throw new FileNotFoundException($"Database project file '{sqlprojFile}' not found");
			}

			Console.WriteLine($"Building {sqlprojFile}");

			var msbuildExe = this.GetMSBuildPath();
			var msbuildArgs = $"\"{sqlprojFile}\" /restore /p:Configuration=Debug /v:minimal";

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
			var dacpacFile = Path.Combine(projectDir, "bin\\Debug", $"{projectName}.dacpac");

			if (!File.Exists(dacpacFile))
			{
				throw new FileNotFoundException($"Expected dacpac file '{dacpacFile}' was not produced.");
			}

			Console.WriteLine("Build succeeded");
			Console.WriteLine();

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
			});

			var output = process.StandardOutput.ReadToEnd().Trim();
			process.WaitForExit();

			if (process.ExitCode != 0 || string.IsNullOrWhiteSpace(output))
			{
				throw new FileNotFoundException("MSBuild.exe not found.");
			}

			return output.Split(Environment.NewLine)[0];
		}

		private void DeployDacpacFile(string dacpacFile, DatabasePublisherConfig config)
		{
			Console.WriteLine($"Deploying {dacpacFile}");

			var publishOptions = new PublishOptions
			{
				DeployOptions = new()
				{
					BlockOnPossibleDataLoss = false
				},
				GenerateDeploymentReport = true
			};

			this.SetPublishOptions(publishOptions, config);

			var dacServices = new DacServices(config.SqlConnectionString);

			dacServices.Message += (_, e) =>
			{
				Console.WriteLine(e.Message.Message);
			};

			using var dacpac = DacPackage.Load(dacpacFile);

			dacServices.Publish(dacpac, config.DatabaseName, publishOptions);

			Console.WriteLine("Deployment succeeded");
			Console.WriteLine();
		}

		protected virtual void SetPublishOptions(PublishOptions publishOptions, DatabasePublisherConfig config)
		{
			// Subclasses can override this method to customize publish options (e.g., set SQLCMD variable values) based on the config
		}

	}
}
