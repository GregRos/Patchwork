using System;
using System.Collections;
using System.Collections.Generic;

namespace Patchwork.Collections {
	public class SimpleTypeGroup<T> : IEnumerable<T> {

		public SimpleTypeGroup(Type key, IList<T> values) {
			Values = values;
			Key = key;
		}

		public SimpleTypeGroup(Type key) {
			Key = key;
			Values = new List<T>();
		}

		public IList<T> Values { get; private set; }

		public Type Key { get; private set; }

		public IEnumerator<T> GetEnumerator() {
			return Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return ((IEnumerable) Values).GetEnumerator();
		}
	}
}