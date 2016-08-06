using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using Patchwork;

namespace OpenAssemblyCreator {
	class Program {

		private static readonly string Logo =
			$@"
Patchwork OAC Open Assembly Creator, Version: {PatchworkInfo.Version}.
=================";

		private static readonly string Usage =
			$@"USAGE: OpenAssemblyCreator.exe ""SOURCE ASSEMBLY"" ""TARGET LOCATION""
SOURCE ASSEMBLY - The input assembly to be modified.
TARGET LOCATION - The location to which the modified assembly should be written.";

		const string Description =
			@"-----------------
This is the open assembly creator. It is a command-line tool for modifying assemblies 
so that you can reference them from your IDE and write Patchwork patch assemblies against them.

The tool takes an input assembly and does the following things:
1. Changes all fields, methods, and and types to public.
2. Makes all types un-sealed.
3. Renames any events that have properties or fields with the same name by adding 'Event' to the end. 

The modified assembly is written to the target location.

The purpose of these changes is a bit complicated and is explained in the Patchwork guide. 

Note that changes 1 and 2 can lead to generating IL that is invalid at the point of injection.
However, PEVerify will pick up on these and warn you.

In the future, this tool may be improved to provide some information about 
the original state of the assembly.
";
		/// <summary>
		///     Makes an assembly 'open', whic7h means that everything is public and nothing is sealed. Ideal for writing a patching
		///     assembly.
		/// </summary>
		/// <param name="assembly">The assembly.</param>
		/// <param name="modifyEvents">if set to <c>true</c> [modify events].</param>
		public static void MakeOpenAssembly(AssemblyDefinition assembly, bool modifyEvents) {
			var allTypes = assembly.MainModule.GetAllTypes();
			allTypes = allTypes.ToList();
			foreach (var type in allTypes) {
				foreach (var field in type.Fields) {
					field.IsPublic = true;
					field.IsInitOnly = false;
				}
				foreach (var method in type.Methods) {
					method.IsPublic = true;
				}
				if (modifyEvents) {
					foreach (var vent in type.Events) {
						if (type.Fields.Any(x => x.Name == vent.Name) || type.Properties.Any(x => x.Name == vent.Name)) {
							vent.Name += "Event";
						}
					}
				}
				type.IsSealed = false;
				if (type.IsNested) {
					type.IsNestedPublic = true;
				} else {
					type.IsPublic = true;
				}
			}
		}
		static int Main(string[] args) {
			Console.WriteLine(Logo);
			if (args.Length != 2) {
				var isInvalidCall = args.Length != 0 && (args.Length != 1 || args[0] != "/?");
				if (isInvalidCall) {
					Console.WriteLine("ERROR: Expected 2 arguments.\r\n");
				}
				Console.WriteLine(Usage);
				if (!isInvalidCall) {
					Console.WriteLine(Description);
				}
				return isInvalidCall ? 1 : 0;
			}
			var input = args[0];
			var dest = args[1];
			if (!File.Exists(input)) {
				Console.WriteLine($"ERROR: The file '{input}' does not exist.");
				return 1;
			}
			AssemblyDefinition ass;
			try {
				ass = AssemblyDefinition.ReadAssembly(input);
			}
			catch (Exception ex) {
				Console.WriteLine($"ERROR: Could not read the assembly '{input}'.");
				Console.WriteLine(ex);
				return 1;
			}
			MakeOpenAssembly(ass, true);
			try {
				ass.Write(dest);
			}
			catch (Exception ex) {
				Console.WriteLine($"ERROR: Could not write resulting assembly to '{dest}'.");
				Console.WriteLine(ex);
				return 1;
			}
			Console.WriteLine($"SUCCESS: Opened assembly at '{input}' and wrote the result to '{dest}'");
			return 0;
		}
	}
}
