using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Patchwork.Tests.Patch
{
	[NewType]
	public static class Asserts
	{
		public static void AssertTrue(this bool b, [CallerMemberName] string caller = null, [CallerLineNumber] int lineNumber = -1, [CallerFilePath] string path = null)
		{
			var id = string.Format("{0}, [{1}\t ln#{2}]", caller, path, lineNumber);
			if (b)
			{
				Console.WriteLine("Passed: {0}", id);
				return;
			}
			path = Path.GetFileName(path);
			throw new Exception(string.Format("Failed assertion: {0}, {1}", caller, id));
		}

		public static void AssertFalse(this bool b, [CallerMemberName] string callerName = null, [CallerLineNumber] int lineNumber = -1, [CallerFilePath] string path = null)
		{
			(!b).AssertTrue(callerName, lineNumber, path);
		}

		public static void AssertEqual<T>(this T a, T b, [CallerMemberName] string callerName = null, [CallerLineNumber] int lineNumber = -1, [CallerFilePath] string path = null)
		{
			EqualityComparer<T>.Default.Equals(a, b).AssertTrue(callerName, lineNumber, path);
		}

		public static void AssertUnequal<T>(this T a, T b, [CallerMemberName] string callerName = null,
			[CallerLineNumber] int lineNumber = -1, [CallerFilePath] string path = null)
		{
			EqualityComparer<T>.Default.Equals(a, b).AssertFalse(callerName, lineNumber, path);
		}
	}
}
