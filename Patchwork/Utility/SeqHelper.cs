using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Patchwork.Collections;

namespace Patchwork.Utility {

	internal static class SeqHelper {
		public static bool RemoveWhere<T>(this ICollection<T> col, Func<T, bool> predicate) {
			var what = col.Where(predicate).ToList();
			var anyRemoved = false;
			foreach (var item in what) { anyRemoved |= col.Remove(item); }
			return anyRemoved;
		}

		public static void ForEach<T>(this IEnumerable<T> seq, Action<T> act) {
			foreach (var item in seq) { act(item); }
		}

		public static int IndexOf<T>(this IEnumerable<T> seq, Func<T, bool> pred) {
			int i = 0;
			foreach (var x in seq) {
				if (pred(x)) {
					return i;
				}
				i++;
			}
			return -1;
		}

		

		public static Func<T, TOut> Selector<T, TOut>(Func<T, TOut> selector) {
			return selector;
		}

		public static void AddRange<T>(this ICollection<T> col, IEnumerable<T> seq) {
			seq.ForEach(col.Add);
		}

		/// <summary>
		///     Extension method that, given a seq of groupings, returns the one that corresponds to the given key, or else returns
		///     an empty grouping.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="groupings">The groupings.</param>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public static IList<TValue> ByKey<TKey, TValue>(this IEnumerable<IGrouping<TKey, TValue>> groupings,
			TKey key) {
			var results =
				from grouping in groupings.ToList()
				where Equals(key, grouping.Key)
				select grouping;
			return results.SelectMany(x => x).ToList();
		}



		/// <summary>
		///     Returns all the elements in groupings with keys that match the specified type, or else have a type which is
		///     assignable to it.
		/// </summary>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="groupings">The groupings.</param>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public static IList<TValue> ByTypeKey<TValue>(this IEnumerable<IGrouping<Type, TValue>> groupings, Type key) {
			if (key == null) {
				return groupings.ByKey(null);
			}
			var result =
				from grouping in groupings.ToList()
				where grouping.Key == key || key.IsAssignableFrom(grouping.Key)
				select grouping;

			return result.SelectMany(x => x).ToList();
		}

		public static IEnumerable<TValue> ByAnyKey<TKey, TValue>(this IEnumerable<IGrouping<TKey, TValue>> groupings,
			params TKey[] keys) {
			var results =
				from grouping in groupings.ToList()
				where keys.Any(x => grouping.Key.Equals(x))
				from item in grouping
				select item;
			return results;
		}

		public static bool ContainsAny(this string str, params string[] options) {
			return options.Any(str.Contains);
		}

		public static IEnumerable<TValue> ByAnyTypeKey<TValue>(this IEnumerable<IGrouping<Type, TValue>> groupings,
			params Type[] keys) {
			return
				from grouping in groupings
				let isAssignableFromAny =
					from key in keys
					where key.IsAssignableFrom(grouping.Key)
					select key
				where isAssignableFromAny.Any()
				from item in grouping
				select item;
		}


		public static SimpleTypeLookup<T> ToSimpleTypeLookup<T>(this IEnumerable<IGrouping<Type, T>> groupings) {
			var lookup =  new SimpleTypeLookup<T>();
			lookup.AddRange(groupings);
			return lookup;
		}
		private class EmptyGroupingImpl<TKey, TValue> : IGrouping<TKey, TValue> {
			public EmptyGroupingImpl(TKey key) {
				Key = key;
			}

			public IEnumerator<TValue> GetEnumerator() {
				yield break;
			}

			IEnumerator IEnumerable.GetEnumerator() {
				return GetEnumerator();
			}

			public TKey Key { get; private set; }
		}
	}
}