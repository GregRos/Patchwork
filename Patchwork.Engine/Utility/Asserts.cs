using System;
using System.Diagnostics;

namespace Patchwork.Engine.Utility {
	internal static class Asserts {
		[DebuggerStepThrough]
		public static void BreakOn(this bool condition) {
			if (condition) {
				Debugger.Break();
			}
		}

		[DebuggerStepThrough]
		public static void AssertTrue(this bool condition) {
			if (!condition) {
				Debugger.Break();
				throw new Exception("Failed assertion!");
			}
		}

		[DebuggerStepThrough]
		public static void AssertFalse(this bool condition) {
			(!condition).AssertTrue();
		}
		[DebuggerStepThrough]
		public static void AssertEqual<T>(this T a, T b) {
			Equals(a, b).AssertTrue();
		}
		[DebuggerStepThrough]
		public static void AssertUnequal<T>(this T a, T b) {
			Equals(a, b).AssertFalse();
		}
	}

}