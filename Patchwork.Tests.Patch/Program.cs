using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using Patchwork.Attributes;
using Patchwork.Tests.Target;

namespace Patchwork.Tests.Patch {

	[NewType]
	public static class Asserts {
		public static void AssertTrue(this bool b, [CallerMemberName] string caller = null, [CallerLineNumber] int lineNumber = -1, [CallerFilePath] string path = null) {
			var id = string.Format("{0}, [{1}\t ln#{2}]", caller, path, lineNumber);
			if (b) {
				Console.WriteLine("Passed: {0}", id);
				return;
			};
			path = Path.GetFileName(path);
			throw new Exception(string.Format("Failed assertion: {0}, {1}", caller, id));
		}

		public static void AssertFalse(this bool b, [CallerMemberName] string callerName = null, [CallerLineNumber] int lineNumber = -1, [CallerFilePath] string path = null) {
			(!b).AssertTrue(callerName, lineNumber, path);
		}

		public static void AssertEqual<T>(this T a, T b, [CallerMemberName] string callerName = null, [CallerLineNumber] int lineNumber = -1, [CallerFilePath] string path = null) {
			Equals(a, b).AssertTrue(callerName, lineNumber, path);
		}

		public static void AssertUnequal<T>(this T a, T b, [CallerMemberName] string callerName = null,
			[CallerLineNumber] int lineNumber = -1, [CallerFilePath] string path = null) {
			Equals(a, b).AssertFalse(callerName, lineNumber, path);
		}
	}

	[NewType]
	public class New_NewTestObject : ExistingTestObject {
		public New_NewTestObject InstanceMethod(New_NewTestObject o) {
			var x = new New_NewTestObject();
			return new New_NewTestObject();
		}

		public static New_NewTestObject StaticMethod(New_NewTestObject o) {
			return new New_NewTestObject();
		}

		public New_NewTestObject InstanceField;

		public static New_NewTestObject StaticField;

		public ExistingTestObject InstanceMethod2() {
			return new New_NewTestObject();
		}

		public XDocument GenericMethod<T>(T o) {
			return null;
		}
	}

	[ModifiesType]
	public class Mod_ExistingTestObject : ExistingTestObject {

		[ModifiesMember]
		public new ExistingTestObject ExistingInstanceField;

		[ModifiesMember]
		public new static ExistingTestObject ExistingStaticField;

		[NewMember]
		public ExistingTestObject NewInstanceField;

		[NewMember]
		public ExistingTestObject NewInstanceMethod() {
			var x = new List<int>();
			return new ExistingTestObject();
		}

		[NewMember]
		public ExistingTestObject NewInstanceProperty {
			[NewMember]
			get;
			[NewMember]
			set;
		}


		[ModifiesMember]
		public new ExistingTestObject ExistingInstanceMethod(ExistingTestObject o) {
			return new ExistingTestObject(); 
		}
		[ModifiesMember]
		public static new ExistingTestObject ExistingStaticMethod(ExistingTestObject o) {
			return new ExistingTestObject();
		}

		[ModifiesMember]
		public new ExistingTestObject ExistingInstanceProperty_NoSet {
			[ModifiesMember]
			get;
			[NewMember]
			set;
		}

		public new static ExistingTestObject ExistingStaticProperty {
			[ModifiesMember]
			get;
			[ModifiesMember]
			set;
		}

		[ModifiesMember]
		public new TOut ExistingGenericInstanceMethod<T, TOut>(T o)
		where TOut : new() {
			o.ToString().Length.AssertUnequal(0);
			return new TOut();
		}
		
		[NewMember]
		public TOut NewGenericInstanceMethod<T, TOut>(T o) {
			return default(TOut);
		}
	} 


	[ModifiesType("Patchwork.Tests.Target.EntryPoint")]
	public class TestClass {

		[NewMember]
		public static IEnumerable<T> IterateOverThese<T>(params T[] args) {
			foreach (var item in args) {
				yield return item;
				yield return item;
			}
		}

		[ModifiesMember]
		public static string OtherTests() {
			var list = IterateOverThese(1, 2, 3, 4, 5, 6, 10, 1).ToList();
			return list.ToString();
		}

		[ModifiesMember]
		public static string StandardTests() {
			
			var o = new ExistingTestObject();
			
			ExistingTestObject.ExistingStaticMethod(o).AssertUnequal(null);

			ExistingTestObject.ExistingStaticProperty.AssertEqual(null);
			ExistingTestObject.ExistingStaticProperty = new ExistingTestObject();
			ExistingTestObject.ExistingStaticProperty.AssertUnequal(null);

			ExistingTestObject.ExistingStaticField.AssertEqual(null);
			ExistingTestObject.ExistingStaticField = new ExistingTestObject();
			ExistingTestObject.ExistingStaticField.AssertUnequal(null);

			o.ExistingInstanceMethod(new ExistingTestObject()).AssertUnequal(null);

			o.ExistingInstanceField.AssertEqual(null);
			o.ExistingInstanceField = new ExistingTestObject();
			o.ExistingInstanceField.AssertUnequal(null);

			o.ExistingInstanceProperty_NoSet.AssertEqual(null);

			var asModified = (Mod_ExistingTestObject) o;

			asModified.ExistingInstanceProperty_NoSet = new ExistingTestObject();
			o.ExistingInstanceProperty_NoSet.AssertUnequal(null);

			asModified.NewInstanceProperty.AssertEqual(null);
			asModified.NewInstanceProperty = new ExistingTestObject();
			asModified.AssertUnequal(null);

			asModified.NewInstanceMethod().AssertUnequal(null);
			asModified.NewInstanceField.AssertEqual(null);
			asModified.NewInstanceField = new ExistingTestObject();
			asModified.NewInstanceField.AssertUnequal(null);

			var newObject = new New_NewTestObject();
			newObject.InstanceMethod(new New_NewTestObject()).AssertUnequal(null);
			New_NewTestObject.StaticMethod(newObject).AssertUnequal(null);

			newObject.InstanceField.AssertEqual(null);
			newObject.InstanceField = new New_NewTestObject();
			newObject.InstanceField.AssertUnequal(null);

			New_NewTestObject.StaticField.AssertEqual(null);
			New_NewTestObject.StaticField = new New_NewTestObject();
			New_NewTestObject.StaticField.AssertUnequal(null);

			newObject.InstanceMethod2().AssertUnequal(null);
			newObject.ExistingGenericInstanceMethod<int, long>(100).AssertEqual(0);
			newObject.GenericMethod<New_NewTestObject>(new New_NewTestObject()).AssertEqual(null);
			o.ExistingGenericInstanceMethod<Type, int>(typeof(int)).AssertEqual(0);
			asModified.NewGenericInstanceMethod<int, long>(0).AssertEqual(0);
			return "Success";
		}
	}

	internal class Program {
		private static void Main(string[] args) {


		}
	}
}