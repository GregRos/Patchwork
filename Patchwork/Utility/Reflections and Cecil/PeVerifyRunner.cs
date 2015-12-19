using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Mono.Cecil;

namespace Patchwork.Utility {
	public class PEVerifyInput {
		public const string DefaultPeVerifySwitches = "/il /md /hresult";

		public string Switches {
			get;
			set;
		} = DefaultPeVerifySwitches;

		public IEnumerable<long> IgnoreErrors {
			get;
			set;
		} = new List<long>();

		/// <summary>
		/// null means Environment.CurrentDirectory
		/// </summary>
		public string AssemblyResolutionFolder {
			get;
			set;
		} = null;

		public bool ExpandMetadataTokens {
			get;
			set;
		} = true;
	}

	public class PEVerifyOutput {
		public string Raw {
			get;
			set;
		}

		public int ErrorCount {
			get;
			set;
		}
	}



	public static class PeVerifyRunner {


		private static string PEVerifyLocation {
			get {
				var patchworkPath = typeof (PeVerifyRunner).Assembly.Location;
				var pwFolder = Path.GetDirectoryName(patchworkPath);
				var path = Path.Combine(pwFolder, "PEVerify");
				return path;
			}
		}

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
					Raw = ret,
					ErrorCount = errorCount
				};
			}
			finally {
				File.Delete(tempPath);
			}
		}
	}
}
