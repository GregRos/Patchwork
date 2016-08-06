using System;
using System.Collections;
using System.Collections.Generic;

namespace Patchwork.Engine.Utility {
	/// <summary>
	/// A group of objects having the same type.
	/// </summary>
	/// <typeparam name="T">The type of the object.</typeparam>
	public class SimpleTypeGroup<T> : IEnumerable<T> {

		/// <summary>
		/// Constructs a new instance of the <see cref="SimpleTypeGroup{T}"/> containing a list of types.
		/// </summary>
		/// <param name="key">The type keying this group.</param>
		/// <param name="values">A list of objects sharing that type.</param>
		public SimpleTypeGroup(Type key, IList<T> values) {
			Values = values;
			Key = key;
		}
		/// <summary>
		/// Constructs a new instance of the <see cref="SimpleTypeGroup{T}"/> with an empty list of types.
		/// </summary>
		/// <param name="key">The type keying this group.</param>
		public SimpleTypeGroup(Type key) {
			Key = key;
			Values = new List<T>();
		}

		/// <summary>
		/// Returns the list of types.
		/// </summary>
		public IList<T> Values { get; private set; }

		/// <summary>
		/// Returns the type key.
		/// </summary>
		public Type Key { get; private set; }
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public IEnumerator<T> GetEnumerator() {
			return Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return ((IEnumerable) Values).GetEnumerator();
		}
	}
}