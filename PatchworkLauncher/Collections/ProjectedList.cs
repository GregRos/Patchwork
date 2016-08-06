using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Patchwork.Engine.Utility;

namespace Patchwork.Utility.Binding
{
	class ProjectedList<TIn, TOut> : IList<TOut> {
		readonly Dictionary<TOut, TIn> _conversionHistory = new Dictionary<TOut, TIn>();


		public ProjectedList(IList<TIn> inner, Func<TOut, TIn> convertIn) {
			Inner = inner;
			ConvertIn = v => {
				var value = convertIn(v);
				_conversionHistory[v] = value;
				return value;
			};
		}

		private TOut ConvertOut(TIn v) {
			return _conversionHistory.FindValue(v).Value.Key;
		}

		private Func<TOut, TIn> ConvertIn {
			get;
		}

		public IList<TIn> Inner {
			get;
		}


		public IEnumerator<TOut> GetEnumerator() {
			foreach (var item in Inner) {
				yield return ConvertOut(item);
			}
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		public void Add(TOut item) => Inner.Add(ConvertIn(item));

		public void Clear() {
			_conversionHistory.Clear();
			Inner.Clear();
		}

		public bool Contains(TOut item) => _conversionHistory.ContainsKey(item);

		public void CopyTo(TOut[] array, int arrayIndex) {
			var arr = new TIn[Count];
			Inner.CopyTo(arr, 0);
			for (int i = 0; i < Count; i++) {
				array[i + arrayIndex] = ConvertOut(arr[i]);
			}
		}

		public bool Remove(TOut item) {
			if (_conversionHistory.ContainsKey(item)) {
				var r =  Inner.Remove(_conversionHistory[item]);
				_conversionHistory.Remove(item);
				return r;
			}
			return false;

		}

		public int Count => Inner.Count;

		public bool IsReadOnly => Inner.IsReadOnly;

		public int IndexOf(TOut item) {
			if (!_conversionHistory.ContainsKey(item)) {
				return -1;
			}
			return Inner.IndexOf(_conversionHistory[item]);
		}

		public void Insert(int index, TOut item) {
			var inner = ConvertIn(item);
			Inner.Insert(index, inner);
		}

		public void RemoveAt(int index) {
			var v = Inner[index];
			_conversionHistory.Remove(ConvertOut(v));
			Inner.RemoveAt(index);
		}

		public TOut this[int index] {
			get {
				return ConvertOut(Inner[index]);
			}
			set {
				_conversionHistory.Remove(this[index]);
				Inner[index] = ConvertIn(value);
			}
		}
	}
}
