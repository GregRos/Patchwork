using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Patchwork.Engine.Utility {



	/// <summary>
	/// A special lookup table that uses types as keys.
	/// </summary>
	/// <typeparam name="T">The type of object stored in the table (usually, a supertype)</typeparam>
	public class SimpleTypeLookup<T> : IEnumerable<SimpleTypeGroup<T>> {
		private readonly Dictionary<Type, SimpleTypeGroup<T>> _lookup;

		/// <summary>
		/// Constructs an empty <see cref="SimpleTypeLookup{T}"/>.
		/// </summary>
		public SimpleTypeLookup() {
			_lookup = new Dictionary<Type, SimpleTypeGroup<T>>();
		}

		/// <summary>
		/// Returns the group keyed by the specified type.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		public SimpleTypeGroup<T> GetGroupExplicitly(Type type) {
			if (_lookup.ContainsKey(type)) {
				return _lookup[type];
			} else {
				_lookup[type] = new SimpleTypeGroup<T>(type);
				return _lookup[type];
			}
		}

		/// <summary>
		/// Returns all the elements that have at least one of the given types as a supertype.
		/// </summary>
		/// <param name="types">The supertypes.</param>
		/// <returns></returns>
		public IEnumerable<T> this[params Type[] types] {
			get { return IsAny(types); }
		}


		private IEnumerable<T> IsAny(params Type[] types) {
			var query =
				from kvp in _lookup
				where types.Any(x => x.IsAssignableFrom(kvp.Key))
				from type in kvp.Value
				select type;
			return query.ToList().AsReadOnly();
		}

		/// <summary>
		/// Returns the total number of values in the table.
		/// </summary>
		public int Count => _lookup.Values.Sum(x => x.Values.Count);

		/// <summary>
		/// Adds a value with a type key.
		/// </summary>
		/// <param name="t">The type key.</param>
		/// <param name="value">The value to add.</param>
		public void Add(Type t, T value) => this.GetGroupExplicitly(t).Values.Add(value);

		/// <summary>
		/// Removes an object.
		/// </summary>
		/// <param name="what"></param>
		public void Remove(T what) {
			var lists =
				from list in _lookup.Values
				where list.Contains(what)
				select list;

			foreach (var list in lists) {
				list.Values.Remove(what);
			}
		}

		/// <summary>
		/// Adds a sequence of values with a type key.
		/// </summary>
		/// <param name="groupings"></param>
		public void AddRange(IEnumerable<IGrouping<Type, T>> groupings) {
			foreach (var grouping in groupings) {
				var list = GetGroupExplicitly(grouping.Key);
				list.Values.AddRange(grouping);
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public IEnumerator<SimpleTypeGroup<T>> GetEnumerator() {
			foreach (var pair in _lookup) {
				yield return pair.Value;
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _lookup).GetEnumerator();
	}
}
