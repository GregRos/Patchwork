using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace Patchwork.Utility {
	internal static class PeVerifyRunner {
		
		public static string RunPeVerify(AssemblyDefinition targetAssembly, string switches = "/il /md /verbose /hresult /nologo", IEnumerable<string> ignoreErrors = null) {
			ignoreErrors = ignoreErrors ?? new string[] {};
			var peVerifyPath = "PEVerify"; //will still be recognized as an executable, even without an extension.
			var tempPath = Guid.NewGuid().ToString();
			targetAssembly.Write(tempPath);
			var info = new ProcessStartInfo() {
				UseShellExecute = false,
				FileName = "cmd",
				RedirectStandardOutput = true,
				Arguments = string.Format($"/c \"\"{peVerifyPath}\" {switches} /ignore={ignoreErrors.Join(",")} \"{tempPath}\"\"")
			};
			using (var process = Process.Start(info)) {
				return process.StandardOutput.ReadToEnd();
			}
		}
	}
}
