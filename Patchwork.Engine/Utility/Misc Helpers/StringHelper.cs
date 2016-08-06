using System;
using System.Collections.Generic;
using System.Linq;

namespace Patchwork.Engine.Utility
{
	internal static class StringHelper {

		public static string Replicate(this string str, int count) {
			return count == 0 ? "" : Enumerable.Repeat(str, count).Aggregate(String.Concat);
		}

		public static string Join(this IEnumerable<string> strs, string sep = "") {
			return String.Join(sep, strs);
		}

		public static string FindLongestCommonSubstring(string a, string b, bool caseInsensitive = false) {
			var commonString = "";
			for (int i = 0; i < Math.Min(a.Length, b.Length); i++) {
				var comparisonMode =caseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
				if (!String.Equals(a[i].ToString(), b[i].ToString(), comparisonMode)) {
					return commonString;
				}
				commonString += a[i];
			}
			return commonString;
		}

		public static bool IsNullOrWhitespace(this string str) {
			return string.IsNullOrWhiteSpace(str);
		}

		private static Random _rnd = new Random();

		public static char[] CharsBetween(char start, char end) {
			var ret = new List<char>();
			for (; start <= end; start++) {
				ret.Add(start);
			}
			return ret.ToArray();
		}

		private static char[] _wordChars =
			CharsBetween('a', 'z').Concat(CharsBetween('A', 'Z')).Concat(CharsBetween('0', '9')).ToArray();

		public static string RandomWordString(int len) {
			var str = "";
			for (int i = 0; i < len; i++) {
				str += _wordChars[_rnd.Next(0, _wordChars.Length)];
			}
			return str;
		}

		public static bool EqualsIgnoreCase(this string a, string b) {
			return String.Equals(a, b, StringComparison.OrdinalIgnoreCase);
		}
	}
}
