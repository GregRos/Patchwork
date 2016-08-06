using System;
using System.Collections;
using System.Collections.Generic;

namespace Patchwork.Utility.Binding
{
	public static class B {

		public static IList<T> CastList<T>(this IList objList) {
			return new CastList<T>(objList);
		}

		public static IList<TOut> ProjectList<TIn, TOut>(this IList<TIn> list, Func<TOut, TIn> projection) {
			return new ProjectedList<TIn,TOut>(list, projection);
		}

	}

	internal class CastList<T> : IList<T> {
		public CastList(IList inner) {
			Inner = inner;
		}

		IList Inner {
			get;
		}

		public IEnumerator<T> GetEnumerator() {
			foreach (var item in Inner) {
				yield return (T)item;
			}
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		public void Add(T item) => Inner.Add(item);

		public void Clear() => Inner.Clear();

		public bool Contains(T item) => Inner.Contains(item);

		public void CopyTo(T[] array, int arrayIndex) => Inner.CopyTo(array, arrayIndex);

		public bool Remove(T item) {
			var orig = Count;
			Inner.Remove(item);
			return Count < orig;
		}

		public int Count => Inner.Count;

		public bool IsReadOnly => Inner.IsReadOnly;

		public int IndexOf(T item) => Inner.IndexOf(item);

		public void Insert(int index, T item) => Inner.Insert(index, item);

		public void RemoveAt(int index) => Inner.RemoveAt(index);

		public T this[int index] {
			get {
				return (T) Inner[index];
			}
			set {
				Inner[index] = value;
			}
		}
	}
}
