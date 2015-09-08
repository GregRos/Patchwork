# Patchwork<a href="https://gitter.im/GregRos/Patchwork"><img style="float: right" src="https://badges.gitter.im/Join%20Chat.svg"/></a>
**License:** [MIT License](http://opensource.org/licenses/MIT) 

**Latest Version:** 0.7.0

**Patchwork** is a library for integrating your own code into existing .NET assemblies ("patching" them). It allows you to edit, create, or replace things such as types, properties, and methods in a simple, straight-forward, and declarative way, using attributes.

The library lets you basically rewrite entire programs, such as games, according to your whims (as long as they're written in a .NET language of course). Little in the code is beyond your control, and you can write it all using the same tools as the original developers. 

You write code in C# or another language, and that code is injected into the target assembly according to your patching declarations. It is minimally transformed, fixing references to such things as types and methods, so that it remains valid at the point of injection.

The library was written with game modding in mind, specifically, for [Pillars of Eternity](http://eternity.obsidian.net/), though you can use it for any purpose. It is based, in principle, on the [IE modding framework](https://bitbucket.org/Bester/poe-modding-framework) for Pillars of Eternity, though it doesn't share any code with it at this point.

The library is mostly documented, including the non-public members. I'd welcome any help you could give in improving it, as there is a lot that could be done.

## Moddable Games
Like I said above, the library was written with game modding in mind. In general, you can mod two kinds of games with it: i
### .NET/XNA
Games that run on .NET/XNA. You can mod pretty much anything in this case. However, there aren't many popular XNA titles.

### Unity/.NET
Games that run on Unity and use .NET for their game logic (mainly C#, but some also use other languages). 

Luckily, the majority of popular Unity titles do primarily use .NET.

Modding in this case is somewhat more limited, as you can only mod the game logic in the scripts, but from experience, you still have vastly more power than typical official modding tools would give you.

## Usage Guide
You need to know some C# or another .NET language, depending on the scope of your modifications, and have a decent IDE.

You can reference this library from NuGet or you can just add this source to your project. Personally I'd recommend adding the source, as the library is still in its early stages, doesn't have complete error reporting, and you may also encounter bugs when using the more advanced features.

Patchwork consists of two assemblies, `Patchwork` and `Patchwork.Attributes`.

The `Patchwork.Attributes` assembly is meant to be referenced by your patch assembly, and is compiled with framework version 2.0 to improve compatibility. It contains the attributes that serve as patching instructions. It has no dependencies.

The `Patchwork` assembly is the one that actually does the patching. 

### Finding what to Patch
Before you start patching, you need to find what you want to patch first. This involves decompiling the target assembly. See *Recommended Decompilers* below for more information. 


Also, take note of the target framework version of the assembly, as for the most reliable results you'd want your patch to be built against the same framework version.

### Creating an Open Assembly
Before you begin patching an assembly, you need to have an "open" version of it, in which all the members are public and unsealed. This allows the compiler to correctly resolve references to members that wouldn't normally be accessible in the assembly where you write your code, but would be accessible at the point of injection. 

Currently, there isn't a way to fully automate the process of creating this assembly. Instead, you'll just have to use `CecilHelper.MakeOpenAssembly`. Save the result, and reference it in your patch assembly.

### Writing the Patch Assembly
You patch an existing target assembly by writing a *patch assembly* (probably with the target assembly referenced), and load it as input to patch a "target" assembly. This assembly contains attributes that are used as patching instructions. 

You need to specify the `PatchAssembly` attribute on any such patch assembly.

Patch assemblies consist of patch types, which are new types or just sets of modifications to an existing type. Here is a simple example:

	[ModifiesType] //means the class modifies another class
	class AttackMod : Attack {
		//note that by default it modifies its parent class, if any, 
		//so this class modifies Attack.
		
		[NewMember] //this means a new method will be injected into the type
		public Hit() {
			//whatever you want to do
		}
		
		[ModifiesMember("ExistingMethod")]
		public void ExistingMethodRevised() {
			//your instructions will replace those of ExistingMethod,
			//as defined in the modified type
		}
	}
	
	[NewType]
	public class MyNewType {
		//No need to specify attributes here.
		private readonly string _myField;
		
		public int MyMethod(int x) { ... }
	}
You can add any members to the type, and attach the right attributes to them, depending on whether you want them to modify existing members or create new ones. (If you create a new type, there are no existing members to modify, of course).

Compiler-generated members are imported as new members by default, even if they don't have a patching attribute.

### Patching
Once you're done with all that, you create an instance of `Patchwork.AssemblyPatcher`. Each such instance represents a single "editing session" on a single assembly. You can patch it with one or more assemblies, and then write it to file.

		//+ Creating patcher
		var patcher = new AssemblyPatcher(originalDllPath,
			ImplicitImportSetting.OnlyCompilerGenerated, Log);

		//+ Patching assemblies
		patcher.PatchAssembly(typeof (IEModType).Assembly.Location);
		
		//+ Writing to file
		patcher.WriteTo(copyToPath);
			
Note that the `Cecil.AssemblyDefinition` of the assembly you're modifying is exposed, so you can also make your own modifications between patching, or write the assembly in your own way.

In some cases, one of the operations can throw an exception. In that case, that `AssemblyPatcher` instance has been corrupted and shouldn't be used.

### Example
A very extensive example of modifying an assembly using Patchwork (and modding a game) is found in the [IEMod.pw](https://github.com/GregRos/IEMod.pw) project (link pending), which is a mod for Pillars of Eternity. 

### Logging 
`AssemblyPatcher` accepts a `Serilog.ILogger` argument. This is a log (from the open source library [Serilog](https://github.com/serilog/serilog)) to which the patcher will print important information, so you should have it visible while the patching takes place. For example, the log can tell you that patching a member has failed because a duplicate exists.

[Here](https://github.com/serilog/serilog/wiki/Configuration-Basics) is a simple tutorial on what you need to do to configure this log. As this library evolves, you won't have to deal with configuring the log yourself.

Only completely unexpected or obviously fatal errors throw an exception. 

## Available Attributes
These attributes are located in the `Patchwork.Attributes` namespace. Note that this isn't necessarily a full list.

### PatchingAssembly
You *must* add this attribute to your assembly (using `[assembly: PatchingAssembly]`) for it to be recognized as an assembly that contains patching types.

### ModifiesType(name)
Says that your type modifies another type in the game. Allows you to use `ModifiesMember` within that type.

You can specify the full name of the type you want to modify, or let PW infer it.

### ReplacesType(name)
Alternative version of the above attribute. Removes all the members of the type, overwriting it with your own members. Currently implemented only on enums. `ModifiesMember` attributes are invalid, since they have no meaning.

### ModifiesMember(name,scope)
Modifies the member, such as its accessibility, body, and maybe other things. scope controls the scope of the modification.

### ModifiesAccessibility(name)
Restricted form of the last attribute. Modifies just the accessibility to be identical to your member. 

Provided for convenience.

### NewMember
Introduce this as a new member to the patched type.

### DuplicatesBody(methodName, declaringType)
Put this on a method marked with NewMember or ModifiesMember to insert the body of another method into it. Optionally, you can provide the type that declares the method; otherwise, it defaults to the type being modified.
You can use it to call original members in the modified type, as it takes the body from the original assembly.

### NewType
Put this attribute on a type to denote it is a new type that will be introduced into the assembly.

The name of the type will be the same as it is in your assembly, including namespaces and so forth. You cannot change this name.

You can create any kind of type you like, whether interface, struct, or class. You can have inheritance, generic type parameters, put constraints on those parameters, etc. Anything goes.

You don't need to use creation attributes on any of your type's members, except for other types. They will be considered to have the `NewMember` attribute. 

You can put `ModifiesType` on a nested type inside a `NewType`, but not `ModifiesMember`. 

### RemoveThisMember

Removes a member of the same name from the modified type. Added for the sake of completeness.

After using it, it's wise to mark the member using the `[Obsolete]` attribute so you don't invoke it by accident.

PW will not check if this action causes an error, but errors may still come up in the patching process later on.

It is not possible to remove types.

### DisablePatching
Disables the patching of this element and all child elements, including nested types. 

Modifications will not be performed, and new types will not be created.

### MemberAlias(memberName, declaringType, aliasCallMode)
This attribute lets you create an alias for another member. When Patchwork encounters a reference to the alias member in your code, it will replace that reference with the aliased member.

It is useful for making explicit calls to things such as base class constructors. If `aliasCallMode == AliasCallMode.NonVirtual`, a call to the member is translated to a non-virtual call, bypassing any overrides. This will allow you to inject `base.OverriddenMethod()` sorts of calls into the methods you modify.

### PatchworkDebugRegister(memberName, declaringType)
This is a special attribute for debugging purposes.  You can specify a static string member that will be used as a debug register for the current method. It will be updated with information about which line number is going to be executed next. It lets you find the line number at which an exception was thrown (or something else happened), when the exception does not contain this information. 

For example, the register can contain the following after an exception is thrown and is caught in the same method:

	10 ⇒ 11 ⇒ 45 ⇒ 46 ⇒ 47 ⇒ 251 ⇒ 252

If the catch clause was at line 251, then line 47 is the one that threw the exception.


## Modifying Specific Elements

### About Overloading
When you put an attribute on a code element, the framework will usually use that element's name (or an alternative name you supply) and, in the case of methods and properties, their parameters, to find what to modify.

To modify one of several overloaded methods, you just need to duplicate that method's parameter types exactly.

Note that return types of existing methods cannot be modified.

### Modifying/Creating Properties
Note that to modify a property's `get` and `set` accessors, you need to put `ModifiesMember` *on the accessor you want to modify*, not on the property deceleration. The accessors are actually methods, and it's those the framework modifies.

However, you can choose to put `NewMember` on the property, in which case the accessors will be created automatically.

This also applies to the property's accessibility. In the IL, only get/set methods have accessibility, so if you want to modify it you have to put the attribute on the accessor, possibly on both.

You might need to use the explicit name of the property accessor to modify it (if your property is named differently). Accessor names are normally `get_PROPERTY` and `set_PROPERTY`.

Pretty much *the only* time you'd want to use `ModifiesMember` attribute on a property itself is when you want to create a brand new accessor for the property. In this case, the property data must be modified. You'll still put the `NewMember` attribute on the new accessor.  

### Modifying Constructors
You can't create constructors for existing types, but you can modify existing ones. Constructors are just methods called `.ctor`. You just need to duplicate their signature in a normal method, and change the modified member name in the attribute. Every object has a default `.ctor`.

Static constructors are called `.cctor`. Not all types have static constructors.

Note that constructors also contain the type's initializers, so you may need to copy those or the class might not work correctly. 

Instance constructors normally contain explicit calls to a base class constructor (e.g. `base::.ctor()`). It is best practice to add this call. This can be achieved by using the `MemberAlias` attribute. For example:

	[MemberAlias(".ctor", typeof(object))]
	private void object_ctor() {
		//this is an alias for object::.ctor()
	}
	
	[ModifiesMember(".ctor")]
	public void CtorNew() {
		object_ctor();
		IEModOptions.LoadFromPrefs();
	}

Static constructors do not contain such a call.

### Nested Types

You can have your nested types modify other types, or you can modify other nested types, without regard to the nesting level. The location of your nested type doesn't matter, and using `ModifiesType` behaves the same way. 

To modify a type by name (rather than having PW infer it), you have to give the *full name* of the type, without regard to where the attribute appears.

In the IL and in Mono.Cecil, as well as in this framework, you use `/` to indicate nesting. E.g. `Namespace.ContainerClass/Nested/NestedNested`.

You can also have a new nested type inside a modification to an existing type. In this case, the nested type will be moved to the modified type.

### Modifying Explicitly Implemented Methods
These are regular methods with different actual names. The names are `[INTERFACE_FULL_NAME].Method`. For example, if `IEnumerable<T>.GetEnumerator()` were explicitly implemented, you'd set the member name to:

		System.Collections.Generic.IEnumerable<T>.GetEnumerator

The dots are actually part of the name of the method, just like the dot in `.ctor` is. IL doesn't follow C# naming rules.

## Limitations
In this section I'll list the limitations of the library, in terms of the code that it can deal with at this stage, and what it *can't* allow you to do. This section will be updated as more features get worked in.

### Assemblies
1. Multi-module assemblies won't work properly (either as patches or patch targets). Note that few IDEs (if any) can naturally produce such assemblies, though they can be the result of tools such as ILMerge.
2. Inter-dependencies between multiple patch assemblies will most likely work, but they haven't been sufficiently tested.

### Members
2. You can't add new constructors or finalizers to existing types.
3. Existing declarations can only be modified in limited ways. For example, you can't un-seal a sealed class, change type parameters and their constraints, etc. New members can still be sealed or unsealed, etc, as you prefer.
4. You can't add new members with the same name as existing members. This can sometimes be an issue for compiler-generated members that are implicitly created, the names of which are generated automatically and cannot be changed. However, it doesn't come up very often at all. 
3. Field initializers don't work in modifying types, except for const fields. This is unlikely to be fixed anytime soon, as it requires pretty tricky IL injection.

### Language Features
1. `unsafe` context features, like pointers and pinned variables, probably won't work.
2. Various exotic and undocumented (in C#) constructs cannot be used, such as `__arglist`.


### Other .NET Languages
This library is for transforming IL, not transforming source code, so it doesn't actually care what language you write in. As long as you put attributes on things that are recognizable in the IL as properties, methods, and classes, it will work correctly.

That said, you could experience more problems if you write in languages other than C#, simply because they can be compiled to very different IL, and the different input could reveal flaws I never encountered during testing. 

However, don't take this as me discouraging you from using other languages. 

## Recommended Decompilers
I've tried a number of decompilers. 

1. **[Telerik JustDecompile](http://www.telerik.com/download/justdecompile)**: Probably the best overall decompiler I've tested. It has great search functions, can produce IL as well as C#, good decompilation ability, and has a great interface. Decompilation isn't perfect, as it can't decompile such things as iterators. Tends to handle errors fairly decently.
2. [**ILSpy:**](http://ilspy.net/) This one generates the best source code *by far* from the decompilers I've tested. It can decompile iterators, lambdas, you name it. Unfortunately, it has no search function that deserves the name and handles errors very badly, even when set to IL. The interface is also inconvenient.
3. [**dotPeek**](https://www.jetbrains.com/decompiler): It has a good interface and decent search, with the very helpful ability of finding related (e.g. derived) types, but isn't very good at decompiling compiler-generated code. It can't even handle things like auto-properties.

## Dependencies

1. [Mono.Cecil](http://www.mono-project.com/docs/tools+libraries/libraries/Mono.Cecil/). The source code for Cecil is included in this repository, mostly because it's much easier to debug with it in full view.
2. [Serilog](http://serilog.net/), used for logging.