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
	public static class PeVerifyRunner {
		public const string DefaultPeVerifySwitches = "/il /md /hresult";

		public static string RunPeVerify(AssemblyDefinition targetAssembly, string switches = DefaultPeVerifySwitches, IEnumerable<long> ignoreErrors = null) {
			string ret;
			var matchMdTokens = new Regex(@"\[(mdToken=|token:)0x(?<token> [0-9a-f]* ) \]", RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
			
			ignoreErrors = ignoreErrors ?? new long[] {};
			var peVerifyPath = "PEVerify"; //will still be recognized as an executable, even without an extension.
			var tempPath = Guid.NewGuid().ToString();
			try {
				var callingAssembly = Path.Combine(Assembly.GetExecutingAssembly().Location, "..", "PEVerify");
				targetAssembly.Write(tempPath);
				var info = new ProcessStartInfo() {
					UseShellExecute = false,
					FileName = callingAssembly,
					RedirectStandardOutput = true,
					Arguments = $"{switches} /ignore={ignoreErrors.Select(x => x.ToString("X")).Join(",")} {tempPath}"
				};
				using (var process = Process.Start(info)) {
					ret=  process.StandardOutput.ReadToEnd().Replace(tempPath, targetAssembly.Name.Name);
				}
				var ass = AssemblyDefinition.ReadAssembly(tempPath);
				ret = matchMdTokens.Replace(ret, match => {
					//This will be uncommented when I fixed the issue with the metadata token resolution...
					var token = match.Groups["token"].Value;
					var tokenNumber = Convert.ToUInt32(token, 16);
					var tk = new MetadataToken(tokenNumber);
					var provider = (MemberReference)ass.MainModule.LookupTokenExtended(tk);
					var providerName = provider?.UserFriendlyName() ?? "";
					var informativeFormat = $@"[{providerName}, 0x{token}]";
					return informativeFormat;
					//return match.Value;
				});
				return ret;
			}
			finally {
				File.Delete(tempPath);
			}
		}
	}
}
