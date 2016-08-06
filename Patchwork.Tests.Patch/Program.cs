using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Patchwork.Tests.Target;
using Serilog;
using Serilog.Events;
using Patchwork;
using Patchwork.Engine;

namespace Patchwork.Tests.Patch
{
	class Program
	{
		static ILogger Log { get; set; }

		static StreamWriter LogFile { get; set; }

		private static void DoSetup()
		{
			//+ Logging-related
			//log messages can get pretty big, so it's nice to have a lot of space to view them:
			Console.WindowWidth = (int)(Console.LargestWindowWidth * 0.75);
			Console.BufferWidth = 300; //for extra long messages
			Console.WindowHeight = (int)(Console.LargestWindowHeight * 0.75);
			Console.BufferHeight = 2000; //so everything is visible

			LogFile = new StreamWriter(File.Open("log.txt", FileMode.Create));

			var log =
				new LoggerConfiguration()
				.MinimumLevel.Verbose()
				.WriteTo.ColoredConsole(LogEventLevel.Debug)
				.WriteTo.TextWriter(LogFile).CreateLogger();

			//note: if you're going to be looking at this a lot, better set your console font to something snazzy
			//(right click on titlebar, Default)
			Log = log;
		}

		private static void PatchTest()
		{

			var targetPath = @"..\..\..\Patchwork.Tests.Target\bin\debug\Patchwork.Tests.Target.dll";;
			
			var dir = Path.GetDirectoryName(targetPath);
			var fn = Path.GetFileNameWithoutExtension(targetPath);
			var ext = Path.GetExtension(targetPath);
			var newFileName = string.Format("{0}.patched{1}", fn, ext);
			var newTarget = Path.Combine(dir, newFileName);
			File.Copy(targetPath, newTarget, true);
			var patcher = new AssemblyPatcher(newTarget, log: Log);
			var patchPath = typeof(Patchwork.Tests.Patch.TestClass).Assembly.Location;
			var maker = new ManifestCreator();
			var manifest = maker.CreateManifest(patchPath);
			patcher.PatchManifest(manifest, null);

	

			patcher.WriteTo(newTarget);
			Log.Information("Loading assembly into memory...");
			var loaded = Assembly.LoadFrom(newTarget);
			Log.Information("Invoking method");
			var module = loaded.GetModules()[0];
			var types = module.FindTypes((typ, o) => typ.Name.Contains("EntryPoint"), null);
			var foundType = types.Single();
			var method = foundType.GetMethod("StandardTests");
			try
			{
				var ret = method.Invoke(null, null);
				Log.Information("Result: {@Result}", ret);
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException;
			}
			Console.ReadKey();
		}

		[STAThread]
		private static void Main(string[] args)
		{
			DoSetup();
			PatchTest();
			LogFile.Flush();
			LogFile.Close();
		}
	}
}