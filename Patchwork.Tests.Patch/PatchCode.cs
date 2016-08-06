using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Patchwork.Tests.Target;

namespace Patchwork.Tests.Patch
{
	[NewType]
	public class EventClass : IInterfaceWithEvent {
		public event Action<int> IntEvent;

		public void RaiseEvent(int arg) {
			if (IntEvent != null) { IntEvent(arg); }
		}
	}

	[NewType]
	public class New_NewTestObject : ExistingTestObject
	{
		public New_NewTestObject InstanceMethod(New_NewTestObject o)
		{
			var x = new New_NewTestObject();
			return new New_NewTestObject();
		}

		public static New_NewTestObject StaticMethod(New_NewTestObject o)
		{
			return new New_NewTestObject();
		}

		public New_NewTestObject InstanceField;

		public static New_NewTestObject StaticField;

		public ExistingTestObject InstanceMethod2()
		{
			return new New_NewTestObject();
		}

	}

	[ModifiesType]
	public class Mod_ExistingTestObject : ExistingTestObject
	{

		[ModifiesMember]
		public new ExistingTestObject ExistingInstanceField;

		[ModifiesMember]
		public new static ExistingTestObject ExistingStaticField;

		[NewMember]
		public ExistingTestObject NewInstanceField;

		[NewMember]
		public ExistingTestObject NewInstanceMethod()
		{
			var x = new List<int>();
			return new ExistingTestObject();
		}

		[NewMember]
		public ExistingTestObject NewInstanceProperty
		{
			[NewMember]
			get;
			[NewMember]
			set;
		}


		[ModifiesMember]
		public new ExistingTestObject ExistingInstanceMethod(ExistingTestObject o)
		{
			return new ExistingTestObject();
		}
		[ModifiesMember]
		public static new ExistingTestObject ExistingStaticMethod(ExistingTestObject o)
		{
			return new ExistingTestObject();
		}

		[ModifiesMember]
		public new ExistingTestObject ExistingInstanceProperty_NoSet
		{
			[ModifiesMember]
			get;
			[NewMember]
			set;
		}

		public new static ExistingTestObject ExistingStaticProperty
		{
			[ModifiesMember]
			get;
			[ModifiesMember]
			set;
		}

		[ModifiesMember]
		public new TOut ExistingGenericInstanceMethod<T, TOut>(T o)
		where TOut : new()
		{
			o.ToString().Length.AssertUnequal(0);
			return new TOut();
		}

		[NewMember]
		public TOut NewGenericInstanceMethod<T, TOut>(T o)
		{
			return default(TOut);
		}
	}


	[ModifiesType("Patchwork.Tests.Target.EntryPoint")]
	public class TestClass
	{

		[NewMember]
		public static IEnumerable<T> IterateOverThese<T>(params T[] args)
		{
			foreach (var item in args)
			{
				yield return item;
				yield return item;
			}
		}

		[ModifiesMember]
		public static string OtherTests()
		{
			var list = IterateOverThese(1, 2, 3, 4, 5, 6, 10, 1).ToList();
			return list.ToString();
		}

		[ModifiesMember]
		public static string StandardTests()
		{

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

			var asModified = (Mod_ExistingTestObject)o;

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

			var eventClass = new EventClass();
			eventClass.IntEvent += n => {
				n.AssertEqual(5);
			};
			eventClass.RaiseEvent(5);
			newObject.InstanceMethod2().AssertUnequal(null);
			newObject.ExistingGenericInstanceMethod<int, long>(100).AssertEqual(0);
			o.ExistingGenericInstanceMethod<Type, int>(typeof(int)).AssertEqual(0);
			asModified.NewGenericInstanceMethod<int, long>(0).AssertEqual(0);
			
			return "Success";
		}
	}

}
