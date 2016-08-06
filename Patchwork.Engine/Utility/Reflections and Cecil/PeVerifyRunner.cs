using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Mono.Cecil;

namespace Patchwork.Engine.Utility {
	/// <summary>
	/// Represents a set of arguments with which PEVerify is invoked on an assembly.
	/// </summary>
	public class PEVerifyInput {
		/// <summary>
		/// The default switches with which PEVerify is called.
		/// </summary>
		public const string DefaultPeVerifySwitches = "/il /md /hresult";

		/// <summary>
		/// The switches with which PEVerify is called. Defaults to <see cref="DefaultPeVerifySwitches"/>.
		/// </summary>
		public string Switches {
			get;
			set;
		} = DefaultPeVerifySwitches;

		/// <summary>
		/// Tells PEVerify to ignore errors with these numbers. Used when errors of a certain kind are expected.
		/// </summary>
		public IEnumerable<long> IgnoreErrors {
			get;
			set;
		} = new List<long>();

		/// <summary>
		/// The base folder in which all requested assemblies are expected to be found. Defaults to <c>Environment.CurrentDirectory</c>
		/// </summary>
		public string AssemblyResolutionFolder {
			get;
			set;
		} = null;

		/// <summary>
		/// If true, will attempt to resolve metadata tokens referenced in the output of PEVerify.
		/// </summary>
		public bool ExpandMetadataTokens {
			get;
			set;
		} = true;
	}

	/// <summary>
	/// Represents the output of PEVerify.
	/// </summary>
	public class PEVerifyOutput {
		/// <summary>
		/// The textual output of the command, after anti post-processing phases such as metadata token expansion.
		/// </summary>
		public string Output {
			get;
			set;
		}

		/// <summary>
		/// The number of errors encountered.
		/// </summary>
		public int ErrorCount {
			get;
			set;
		}
	}

	/// <summary>
	/// A helper class used to execute and provide PEVerify-related services.
	/// </summary>
	public static class PeVerifyRunner {
		private static string PEVerifyLocation {
			get {
				var patchworkPath = typeof (PeVerifyRunner).Assembly.Location;
				var pwFolder = Path.GetDirectoryName(patchworkPath);
				var path = Path.Combine(pwFolder, "PEVerify");
				return path;
			}
		}

		/// <summary>
		/// Executes the bundled PEVerify executable on the specified assembly by first serializing it to file. PEVerify is an unmanaged, Windows-only application.
		/// </summary>
		/// <param name="targetAssembly">The assembly on which to run PEVerify.</param>
		/// <param name="input">The arguments with which to run PEVerify and process its output.</param>
		/// <returns></returns>
		public static PEVerifyOutput RunPeVerify(AssemblyDefinition targetAssembly, PEVerifyInput input) {
			var matchMdTokens = new Regex(@"\[(mdToken=|token:)0x(?<token> [0-9a-f]* ) \]", RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
			
			var tempName =  Guid.NewGuid().ToString();
			var tempPath = Path.Combine(input.AssemblyResolutionFolder, tempName);
			var matchErrorCount = new Regex($@"(?<count>\d+) Error\(s\) Verifying {tempName}", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
			try {
				
				//PEVerify is in the same folder as Patchwork, not the directory.
				targetAssembly.Write(tempPath);
				var info = new ProcessStartInfo() {
					UseShellExecute = false,
					FileName = PEVerifyLocation,
					RedirectStandardOutput = true,
					CreateNoWindow = true,
					WorkingDirectory = input.AssemblyResolutionFolder ?? Environment.CurrentDirectory,
					Arguments = $"{input.Switches} /ignore={input.IgnoreErrors.Select(x => x.ToString("X")).Join(",")} \"{tempPath}\""
				};
				string ret;
				using (var process = Process.Start(info)) {
					ret=  process.StandardOutput.ReadToEnd().Replace(tempPath, targetAssembly.Name.Name);
				}
				var ass = AssemblyCache.Default.ReadAssembly(tempPath);
				if (input.ExpandMetadataTokens) {
					ret = matchMdTokens.Replace(ret, match => {
						var token = match.Groups["token"].Value;
						var tokenNumber = Convert.ToUInt32(token, 16);
						var tk = new MetadataToken(tokenNumber);
						var provider = (MemberReference) ass.MainModule.LookupTokenExtended(tk);
						var providerName = provider?.UserFriendlyName() ?? "";
						var informativeFormat = $@"[{providerName}, 0x{token}]";
						return informativeFormat;
						//return match.Value;
					});
				}
				var countCapture = matchErrorCount.Match(ret).Groups["count"];
				int errorCount;
				if (countCapture.Success) {
					if (!int.TryParse(countCapture.Value, out errorCount)) {
						errorCount = -1;
					}
				} else {
					errorCount = 0;
				}
				return new PEVerifyOutput() {
					Output = ret,
					ErrorCount = errorCount
				};
			}
			finally {
				File.Delete(tempPath);
			}
		}
	}
}
