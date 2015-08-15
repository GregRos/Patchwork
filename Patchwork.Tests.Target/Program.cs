using System;

namespace Patchwork.Tests.Target
{

	public interface IInterfaceWithEvent {
		event Action<int> IntEvent;
	}

	public class ExistingTestObject {
		public ExistingTestObject ExistingInstanceField;
		public static ExistingTestObject ExistingStaticField;

		public virtual ExistingTestObject ExistingInstanceProperty_NoSet {
			get { return null; }
		}

		public static ExistingTestObject ExistingStaticProperty {
			get { return null; }
			set { }
		}

		public virtual ExistingTestObject ExistingInstanceMethod(ExistingTestObject o) {
			return null;
		}

		public static ExistingTestObject ExistingStaticMethod(ExistingTestObject o) {
			return null;
		}

		public TOut ExistingGenericInstanceMethod<T, TOut>(T input)
			where TOut : new() {
			return new TOut();
		}

	}
	public class EntryPoint
	{
		public static string OtherTests() {
			return "Failure";
		}

		public static string StandardTests() {
			return "Failure";
		}
	}
}
