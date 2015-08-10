using System;
using System.Collections.Generic;
using System.Linq;

namespace Patchwork.Utility
{
	public static class StringHelper {

		public static string Replicate(this string str, int count) {
			return count == 0 ? "" : Enumerable.Repeat(str, count).Aggregate(String.Concat);
		}

		public static string Join(this IEnumerable<string> strs, string sep = "") {
			return String.Join(sep, strs);
		}
	}
}
