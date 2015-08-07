using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Patchwork.Utility;

namespace Patchwork.Collections {

	/// <summary>
	/// This is kind of similar to an ILookup, with types as keys.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal class SimpleTypeLookup<T> : IEnumerable<SimpleTypeGroup<T>> {
		private readonly Dictionary<Type, SimpleTypeGroup<T>> _lookup;

		public SimpleTypeLookup() {
			_lookup = new Dictionary<Type, SimpleTypeGroup<T>>();
		}

		public SimpleTypeGroup<T> GetGroup(Type type) {
			if (_lookup.ContainsKey(type)) {
				return _lookup[type];
			} else {
				_lookup[type] = new SimpleTypeGroup<T>(type);
				return _lookup[type];
			}
		}

		public IEnumerable<T> this[params Type[] types] {
			get { return IsAny(types); }
		} 

		

		public IEnumerable<T> IsAny(params Type[] types) {
			var query =
				from kvp in _lookup
				where types.Any(x => x.IsAssignableFrom(kvp.Key))
				from type in kvp.Value
				select type;
			return query.ToList().AsReadOnly();
		}

		public int Count { get { return _lookup.Values.Sum(x => x.Values.Count); } }
		public void Add(Type t, T value) {
			this.GetGroup(t).Values.Add(value);
		}

		public void Remove(T what) {
			var lists =
				from list in _lookup.Values
				where list.Contains(what)
				select list;

			foreach (var list in lists) {
				list.Values.Remove(what);
			}
		}

		public void AddRange(IEnumerable<IGrouping<Type, T>> groupings) {
			foreach (var grouping in groupings) {
				var list = GetGroup(grouping.Key);
				list.Values.AddRange(grouping);
			}
		}

		public IEnumerator<SimpleTypeGroup<T>> GetEnumerator() {
			foreach (var pair in _lookup) {
				yield return pair.Value;
			}
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return ((IEnumerable) _lookup).GetEnumerator();
		}
	}
}
