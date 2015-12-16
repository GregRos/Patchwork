using System;

namespace Patchwork.Utility.Binding {
	public class ValidationRule<T> {
		public ValidationRule(Func<T, bool> validate, string name) {
			Name = name;
			Validate = validate;
		}

		public string Name {
			get;
		}

		public Func<T, bool> Validate {
			get;
		}
	}
}