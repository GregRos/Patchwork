using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patchwork.Collections {
	public class CachingDictionary<TKey, T> : IDictionary<TKey, T> {
		private readonly IDictionary<TKey, T> _inner;
		private readonly Func<TKey, T> _create;

		public CachingDictionary(Func<TKey, T> create)
		: this(new Dictionary<TKey, T>(), create){
			
		} 

		public CachingDictionary(IDictionary<TKey, T> inner, Func<TKey, T> create) {
			_inner = inner;
			_create = create;
		}

		public IEnumerator<KeyValuePair<TKey, T>> GetEnumerator() {
			return _inner.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return ((IEnumerable) _inner).GetEnumerator();
		}

		public void Add(KeyValuePair<TKey, T> item) {
			_inner.Add(item);
		}

		public void Clear() {
			_inner.Clear();
		}

		public bool Contains(KeyValuePair<TKey, T> item) {
			return _inner.Contains(item);
		}

		public void CopyTo(KeyValuePair<TKey, T>[] array, int arrayIndex) {
			_inner.CopyTo(array, arrayIndex);
		}

		public bool Remove(KeyValuePair<TKey, T> item) {
			return _inner.Remove(item);
		}

		public int Count {
			get {
				return _inner.Count;
			}
		}

		public bool IsReadOnly {
			get {
				return _inner.IsReadOnly;
			}
		}

		public bool ContainsKey(TKey key) {
			return _inner.ContainsKey(key);
		}

		public void Add(TKey key, T value) {
			_inner.Add(key, value);
		}

		public bool Remove(TKey key) {
			return _inner.Remove(key);
		}

		public bool TryGetValue(TKey key, out T value) {
			return _inner.TryGetValue(key, out value);
		}

		public T this[TKey key] {
			get {
				if (_inner.ContainsKey(key)) {
					return _inner[key];
				}
				return _inner[key] = _create(key);
			}
			set {
				_inner[key] = value;
			}
		}

		public ICollection<TKey> Keys {
			get {
				return _inner.Keys;
			}
		}

		public ICollection<T> Values {
			get {
				return _inner.Values;
			}
		}
	}
}
