using Rag.AIClient.Engine.AIModels;
using Rag.AIClient.Engine.RagProviders.Core;
using System;
using System.Drawing;

namespace Rag.AIClient.Engine
{
	public static class ConsoleHelper
	{
		public static bool IsDarkMode => true;
		public static Color ForegroundColor => IsDarkMode ? Color.White : Color.Black;
		public static Color DefaultColor => IsDarkMode ? Color.DarkGray : Color.Gray;
		public static Color UserColor => IsDarkMode ? Color.Yellow : Color.SaddleBrown;
		public static Color InfoColor => IsDarkMode ? Color.Cyan : Color.DarkCyan;
		public static Color InfoDimColor => IsDarkMode ? Color.DarkCyan : Color.DarkCyan;
		public static Color ErrorColor => Color.Red;
		public static Color SystemColor => IsDarkMode ? Color.LightGreen : Color.Green;
		public static Color SystemDimColor => IsDarkMode ? Color.Green : Color.FromArgb(0, 196, 0);

		public static void Clear()
		{
			ResetColor();
			Console.Clear();
		}

		public static void ResetColor()
		{
			Console.ResetColor();
			Console.BackgroundColor = IsDarkMode ? ConsoleColor.Black : ConsoleColor.White;
			Console.ForegroundColor = IsDarkMode ? ConsoleColor.White : ConsoleColor.Black;
		}

		public static void SetForegroundColor(Color? color)
		{
			var c = color ?? DefaultColor;
			Console.Write($"\u001b[38;2;{c.R};{c.G};{c.B}m");
		}

		public static void Write(object text, Color? color = null) =>
			WriteLine(text, color, suppressLineFeed: true);

		public static void WriteLine(object text = null, Color? color = null, bool suppressLineFeed = false)
		{
			SetForegroundColor(color);

			Console.Write(text);

			if (!suppressLineFeed)
			{
				Console.Write(Environment.NewLine);
			}

			ResetColor();
		}

		public static void WriteErrorLine(string text) =>
			WriteLine(text, Color.Red);

		public static void WriteHeading(string text, Color color)
		{
			var width = Console.WindowWidth;
			Console.WriteLine();
			SetForegroundColor(color);
			Console.WriteLine($"    ╔{new string('═', text.Length + 2)}╗");
			Console.WriteLine($"    ║ {text} ║");
			Console.WriteLine($"════╝{new string(' ', text.Length + 2)}╚{new string('═', width - text.Length - 9)}");
			ResetColor();
			Console.WriteLine();
		}

		public static void WriteEnvironmentInfo()
		{
			var provider = RagProviderFactory.GetRagProvider();

			Console.WriteLine(@$"   Provider Name:     {provider.ProviderName}");
			Console.WriteLine(@$"   Database Name:     {provider.DatabaseName}");
			Console.WriteLine(@$"   Database Server:   {provider.ServerName}");
			Console.WriteLine(@$"   AI Models Source:  {AIModelsSourceFactory.AIModelsSourceType}");
			Console.WriteLine(@$"   Embedding Model:   {AIModelsSourceFactory.GetEmbeddingModelName()}");
		}

	}
}
