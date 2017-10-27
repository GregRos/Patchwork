# Patchwork

<span style="font-size:18">[MIT License](http://opensource.org/licenses/MIT) |  [Latest Version: 0.9.1](https://github.com/GregRos/Patchwork/releases/latest)</span>

----

## User's Guide
Patchwork is a program that allows you to mod certain games using mod files made by others.

You will need to download Patchwork from a mod site or a similar source, because the version that can be downloaded from this site is only suitable for developers.

### Requirements
Patchwork is a Mono/.NET application and so needs the .NET Framework or Mono to run.

* **Windows:** [.NET Framework 4.5+](https://www.microsoft.com/en-us/download/details.aspx?id=30653) 
* **Linux and Mac:** [Mono 4.2.1.102+](http://www.mono-project.com/download/)

### Instructions

Using the program is straight-forward:

1. Extract it into an **empty** folder.

2. Launch the program (`PatchworkLauncher.exe`)
   
   **Note:** On Linux, you may need to open the program using  `mono` explicitly (see instructions on running Mono applications in your distribution).

3. Specify your game folder in the dialog box or type it in the textbox.

   **Note:** The dialog box will not display hidden files or folders.

4. Go to the *Active Mods* menu and add the mod file(s) (usually ending with `.pw.dll`) to the list of mods, checking those you want enabled.
 
   **Note:** Mod files so chosen will normally be copied to the `Mods` folder.

5. Use *Launch with Mods* and *Launch without Mods* to start the game.

## Developer's Guide
**Patchwork** is a framework for integrating your own code into existing .NET assemblies ("patching" them). It allows you to edit, create, or replace things such as types, properties, and methods in a simple, straight-forward, and declarative way, using attributes.

The framework lets you basically rewrite entire programs, such as games, according to your whims (as long as they're written in a .NET language of course). Little in the code is beyond your control, and you can write it all using the same tools as the original developers. 

You write code in C# or another language, and that code is injected into the target assembly according to your patching declarations. It is minimally transformed, fixing references to such things as types and methods, so that it remains valid at the point of injection.

The framework was written with game modding in mind, but can be used for any purpose.

The framework is mostly documented, including the non-public members.

### Moddable Games
Like I said above, the library was written with game modding in mind. In general, you can mod two kinds of games with it: i
#### .NET/XNA
Games that run on .NET/XNA. You can mod pretty much anything in this case. However, there aren't many popular XNA titles.

#### Unity/.NET
Games that run on Unity and use .NET for their game logic (mainly C#, but some also use other languages). 

Luckily, the majority of popular Unity titles do primarily use .NET.

Modding in this case is somewhat more limited, as you can only mod the game logic in the scripts, but from experience, you still have vastly more power than typical official modding tools would give you.

### Components 
The framework consists of several separate components. 

#### PatchworkLauncher (PWL)
This is the end-user patching GUI. Users use this program to apply your modifications to their games or applications. It is most convenient to download it from the [Releases](https://github.com/GregRos/Patchwork/releases) section.

You also use this program to test your patch assembly for yourself. 

##### OpenAssemblyCreator (OAC)
This is a command-line tool packaged with PWL. It allows you to create an open assembly to reference in your patch assembly. More on this later.

#### Patchwork.Attributes (PWA)
The `Patchwork.Attributes` assembly is meant to be referenced by your patch assembly, and is compiled with framework version 2.0 to improve compatibility. It contains the attributes that serve as patching instructions. It has no dependencies.

You can conveniently get this package from [NuGet: Patchwork.Attributes](https://www.nuget.org/packages/Patchwork.Attributes/) and reference it in your patch assembly.

#### Patchwork.Engine
This is the library that actually does the patching. It can be used separately from the GUI. However, you generally wouldn't want to do so unless you wanted to write a new front-end, some kind of script, or... had some other reason.

Naturally, it's packaged with the launcher.

### Getting Started
First, get the latest PWL release from the releases section. 

Unless someone else already wrote it, you'll need to write an `AppInfo.dll` file for the game you want to patch. See the section on the launcher for more information.

Next, create a new project (preferably with a name ending with `.pw` for reasons explained later). 

Reference the `Patchwork.Attributes` assembly from that project.

Since you probably don't want to copy files around manually, you should take advantage of the `DontCopyFiles` option in the `Preferences` section of the `preferences.pw.xml` file. It lets you add files to your mod list without copying them to a mods folder, so you can just add the result of your build directly.

That's... it. You're ready to go. Good luck out there. 

By the way, patching using the launcher creates a dependency on `Patchwork.Attributes` in the target (patched) assembly.

### Overview
There are a few stages to writing a patch.

#### Finding What to Patch
Before you start writing your patch assembly, you need to find what you want to patch. This involves decompiling the target assembly. See *Recommended Decompilers* below for more information. 

Also, take note of the target framework version of the assembly, as for the most reliable results you'd want your patch to be built against the same framework version.

#### Creating an Open Assembly
You also need to have an "open" version of the assembly you want to patch. "Open" here means that all of its members are public and non-sealed. 

To see why this is required, imagine you have a class like:

	class Player {
		private int _hitpoints;
		
		public void GetHit() {
			_hitpoints--;
		}
	}
	
And you want to overwrite the `GetHit` method to perform `_hitpoints++` instead. 

The problem is that `_hitpoints` can only be accessed from inside the class `Player`. But you have to write the code `_hitpoints++` in your own assembly. The framework will inject it into the correct spot, but your C# compiler doesn't know that and won't let you access the member.

Changing all members to public allows you to access the member. However, it can cause problems too. You might make a mistake and illegally refer to a non-public member, for example.

On the brighter side, the `PEVerify` tool (which executed automatically) will catch any accessibility issues and warn you about them.

You create an open assembly using the command-line tool `OpenAssemblyCreator.exe`, also called OAC. Using it is straight-forward. 
#### Creating the Patch Assembly
You patch an existing target assembly by writing a *patch assembly* (probably with the target assembly referenced), and load it as input to patch a "target" assembly. This assembly contains attributes that are used as patching instructions. 

You need to specify the `PatchAssembly` attribute on any such patch assembly.

##### Writing It
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

##### Format and Additional Info
The recommended extension for patch assemblies is `pw.dll`.

In order for a patch assembly to work with the Patchwork launcher, it must define a class with the following requirements.

1. It implements `Patchwork.Attributes.IPatchInfo`
2. It is decorated with `Patchwork.Attributes.PatchInfoAttribute`.
3. It has a default constructor.

There must be only one such class in the assembly.

The class should not contain any references to any types not found in the GAC or in `Patchwork.Attributes`.

It is instantiated in a separate AppDomain from the rest of the application, and this AppDomain is unloaded if the user removes it from the patch list.

The `PatchInfo` is needed to tell the launcher what file to patch. The class can find the file based on the operating system and other information.

#### Patching
The patch is mostly viewed as data by the patchwork launcher. To patch another assembly with your patch assembly, you must load the `pw.dll` file into the launcher and then start the game.

That's it.


### Patchwork Launcher
Each launcher executable is meant to work with one game. To work with a game, someone must write an `AppInfo.dll` assembly (as described below) and put it in the launcher directory. The launcher loads it on startup and uses it to get information about the game. The launcher should be distributed with this file.

The launcher allows users to manage patches for the chosen game, as well as change the order in which they are applied. It keeps the original game files on disk and switches them with modded files when the user launches the game. It only patches the files when necessary. It stays in the background, and once the game is exited, the launcher switches to the original files once more. 

Once it starts up, the launcher also checks the state of the files and fixes them if necessary, in case it was terminated unexpectedly.

The launcher works on Mono and is written in Windows Forms for that purpose.

#### AppInfo.dll
This file is required for the launcher to work correctly with a game. It's an assembly (the name doesn't matter) containing a type that:

1. Inherits from `Patchwork.Attributes.AppInfoFactory`.
2. Is decorated with `Patchwork.Attributes.AppInfoFactoryAttribute`.
3. Has a default constructor.

This type has a `CreateInfo` method that returns an `Pathcwork.Attributes.AppInfo` object which provides information about the game.

There must be only one such class in the assembly.

This class is instantiated during runtime, in the same AppDomain as the original application. The launcher can't start if the file is invalid or not found.

If this file is not found, an error message is generated and the launcher doesn't work properly.

### Available Attributes
These attributes are located in the `Patchwork.Attributes` namespace. Note that this isn't necessarily a full list.

#### Note about attribute constructors
Attributes that require types as parameters invariably have an `object` parameter instead of a `Type` parameter.

This is a necessary workaround. You still use `typeof(T)` to specify the type.

#### PatchingAssembly
You *must* add this attribute to your assembly (using `[assembly: PatchingAssembly]`) for it to be recognized as an assembly that contains patching types.

#### ModifiesType(name)
Says that your type modifies another type in the game. Allows you to use `ModifiesMember` within that type.

You can specify the full name of the type you want to modify, or let PW infer it.

#### ReplacesType(name)
Alternative version of the above attribute. Removes all the members of the type, overwriting it with your own members. Currently implemented only on enums. `ModifiesMember` attributes are invalid, since they have no meaning.

#### ModifiesMember(name,scope)
Modifies the member, such as its accessibility, body, and maybe other things. scope controls the scope of the modification.

#### ModifiesAccessibility(name)
Restricted form of the last attribute. Modifies just the accessibility to be identical to your member. 

Provided for convenience.

#### NewMember(altName)
Introduce this as a new member to the patched type. If you specify `altName`, the member will be introduced under this name instead.

If the member collides with an existing member, its name will be suffixed with `_$pw$_RANDOM`, e.g. `_$pw$_dfRff`.  A warning will be emitted.

#### DuplicatesBody(methodName, declaringType)
Put this on a method marked with NewMember or ModifiesMember to insert the body of another method into it. Optionally, you can provide the type that declares the method; otherwise, it defaults to the type being modified.
You can use it to call original members in the modified type, as it takes the body from the original assembly.

#### NewType(altName, altNamespace)
Put this attribute on a type to denote it is a new type that will be introduced into the assembly.

The name of the type will normally be the same as it is in your assembly, including namespaces and so forth. However, `altName` and `altNamespace` allow you to specify an alternative name/namespace.

In case of a collision, the type name will be suffixed with `_$pw$_RANDOM`, e.g. `_$pw$_dffERr`. A warning will be emitted.

You can create any kind of type you like, whether interface, struct, or class. You can have inheritance, generic type parameters, put constraints on those parameters, etc. Anything goes.

You don't need to use creation attributes on any of your type's members, except for other types. They will be considered to have the `NewMember` attribute. 

You can put `ModifiesType` on a nested type inside a `NewType`, but not `ModifiesMember`. 


#### RemoveThisMember

Removes a member of the same name from the modified type. Added for the sake of completeness.

After using it, it's wise to mark the member using the `[Obsolete]` attribute so you don't invoke it by accident.

PW will not check if this action causes an error, but errors may still come up in the patching process later on.

It is not possible to remove types.

#### DisablePatching
Disables the patching of this element and all child elements, including nested types. 

Modifications will not be performed, and new types will not be created.

#### MemberAlias(memberName, declaringType, aliasCallMode)
This attribute lets you create an alias for another member. When Patchwork encounters a reference to the alias member in your code, it will replace that reference with the aliased member.

It is useful for making explicit calls to things such as base class constructors. If `aliasCallMode == AliasCallMode.NonVirtual`, a call to the member is translated to a non-virtual call, bypassing any overrides. This will allow you to inject `base.OverriddenMethod()` sorts of calls into the methods you modify.

#### PatchworkDebugRegister(memberName, declaringType)
This is a special attribute for debugging purposes.  You can specify a static string member that will be used as a debug register for the current method. It will be updated with information about which line number is going to be executed next. It lets you find the line number at which an exception was thrown (or something else happened), when the exception does not contain this information. 

For example, the register can contain the following after an exception is thrown and is caught in the same method:

	10 ⇒ 11 ⇒ 45 ⇒ 46 ⇒ 47 ⇒ 251 ⇒ 252

If the catch clause was at line 251, then line 47 is the one that threw the exception.

This is a hack, but it can be quite useful.

#### ToggleFieldAttributes(fieldAttributes)
This custom attribute toggles (XORs) the intrinsic deceleration attributes of the patched field with the input attributes. It must be used with an action attribute, such as `ModifiesMember`.

This attribute allows you to change accessibility, as well as more arcane things. Using it incautiously can cause runtime errors.

#### ToggleMethodAttributes(methodAttributes)
This custom attribute toggles (XORs) the intrinsic deceleration attributes of the patched method with the input attributes. It must be used with an action attribute, such as `ModifiesMember`.

This attribute allows you to change accessibility, add/remove the `sealed` qualifier, and perform other, more arcane tasks. Using it incautiously can cause runtime errors.

### Naming Conventions
It is best practice to follow certain naming conventions when writing your patch assembly.

You should prefix each code element according to the action the framework is expected to perform on it. That way, you will be able to tell what sort of member it is just by glancing at the name, and your code will be more readable to others.

| Function                                                   | Form            | Related Attribute            |
|---------------------------------------------------------   |-----------------|------------------------------|
| Modification of `Name` type/member from the original assembly | `mod_Name`        | `ModifiesType`, `ModifiesMember` |
| Duplicate of `Name` in the modified type                  | `orig_Name`       | `DuplicatesBody`               |
| Duplicate of `Name` in type `Type` in the original assembly | `orig_Type_Name` | `DuplicatesBody`               |
| Alias of `Name` in type `Type`                              | `alias_Type_Name` | `MemberAlias`                  |
| New type or member                                      | (none)          | `NewMember`, `NewType`           

For instance constructors, use the name `ctor` and for static ones use `cctor`. 

(none) means that you should not prefix the name with anything.



### Specific Issues

#### About Overloading
When you put an attribute on a code element, the framework will usually use that element's name (or an alternative name you supply) and, in the case of methods and properties, their parameters, to find what to modify.

To modify one of several overloaded methods, you just need to duplicate that method's parameter types exactly.

Note that return types of existing methods cannot be modified.

#### Modifying/Creating Properties
Note that to modify a property's `get` and `set` accessors, you need to put `ModifiesMember` *on the accessor you want to modify*, not on the property deceleration. The accessors are actually methods, and it's those the framework modifies.

However, you can choose to put `NewMember` on the property, in which case the accessors will be created automatically.

This also applies to the property's accessibility. In the IL, only get/set methods have accessibility, so if you want to modify it you have to put the attribute on the accessor, possibly on both.

You might need to use the explicit name of the property accessor to modify it (if your property is named differently). Accessor names are normally `get_{Property}` and `set_{Property}`.

Pretty much *the only* time you'd want to use `ModifiesMember` attribute on a property itself is when you want to create a brand new accessor for the property. In this case, the property data must be modified. You'll still put the `NewMember` attribute on the new accessor.  

#### Modifying Constructors
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

#### Nested Types

You can have your nested types modify other types, or you can modify other nested types, without regard to the nesting level. The location of your nested type doesn't matter, and using `ModifiesType` behaves the same way. 

To modify a type by name (rather than having PW infer it), you have to give the *full name* of the type, without regard to where the attribute appears.

In the IL and in Mono.Cecil, as well as in this framework, you use `/` to indicate nesting. E.g. `Namespace.ContainerClass/Nested/NestedNested`.

You can also have a new nested type inside a modification to an existing type. In this case, the nested type will be moved to the modified type.

#### Modifying Explicitly Implemented Methods
These are regular methods with different actual names. The names are `[INTERFACE_FULL_NAME].Method`. For example, if `IEnumerable<T>.GetEnumerator()` were explicitly implemented, you'd set the member name to:

		System.Collections.Generic.IEnumerable<T>.GetEnumerator

The dots are actually part of the name of the method, just like the dot in `.ctor` is. IL doesn't follow C# naming rules.

### Patching History
The launcher embeds various attributes in the target assembly that let you see what Patchwork did to it. These are called history attributes. The following are embedded.

Also, the patching attributes you use also get embedded, except for `PatchAssembly`.

#### PatchingHistoryAttribute
Abstract parent of all history attributes.

#### PatchedByAssemblyAttribute
Contains information about the patch assembly, the original assembly (before patching was performed), and the Patchwork assembly that performed patching.

#### PatchedByMemberAttribute
Indicates the member in the patch assembly that contained the patching instruction to patch this member.

#### PatchedByTypeAttribute
Indicates the type in the patch assembly that contained the patching instruction to patch this type.

### Limitations
In this section I'll list the limitations of the library, in terms of the code that it can deal with at this stage, and what it *can't* allow you to do. This section will be updated as more features get worked in.

#### Assemblies
1. Multi-module assemblies won't work properly (either as patches or patch targets). Note that few IDEs (if any) can naturally produce such assemblies, though they can be the result of tools such as ILMerge.
2. Inter-dependencies between multiple patch assemblies haven't been tested.

#### Members
2. You can't add new constructors or finalizers to existing types.
3. Existing declarations can only be modified in limited ways. For example, you can't un-seal a sealed class, change type parameters and their constraints, etc. New members can still be sealed or unsealed, etc, as you prefer. 
3. Field initializers don't work in modifying types, except for const fields. This is unlikely to be fixed anytime soon, as it requires pretty tricky IL injection.

#### Language Features
1. `unsafe` context features, like pointers and pinned variables, probably won't work.
2. Various exotic and undocumented (in C#) constructs cannot be used, such as `__arglist`.

#### Other .NET Languages
This library is for transforming IL, not transforming source code, so it doesn't actually care what language you write in. As long as you put attributes on things that are recognizable in the IL as properties, methods, and classes, it will probably work correctly.

That said, you could experience more problems if you write in languages other than C#, simply because they can be compiled to very different IL, and the different input could reveal flaws I never encountered during testing. 

However, don't take this as me discouraging you from using other languages. 



### Recommended Decompilers
I've tried a number of decompilers. 

1. **[Telerik JustDecompile](http://www.telerik.com/download/justdecompile)**: Probably the best overall decompiler I've tested. It has great search functions, can produce IL as well as C#, good decompilation ability, and has a great interface. Decompilation isn't perfect, as it can't decompile such things as iterators. Tends to handle errors fairly decently.
2. [**ILSpy:**](http://ilspy.net/) This one generates the best source code *by far* from the decompilers I've tested. It can decompile iterators, lambdas, you name it. Unfortunately, it has no search function that deserves the name and handles errors very badly, even when set to IL. The interface is also inconvenient.
3. [**dotPeek**](https://www.jetbrains.com/decompiler): It has a good interface and decent search, with the very helpful ability of finding related (e.g. derived) types, but isn't very good at decompiling compiler-generated code. It can't even handle things like auto-properties.

### Dependencies

1. [Mono.Cecil](http://www.mono-project.com/docs/tools+libraries/libraries/Mono.Cecil/), without which none of this would have been possible.
2. [Serilog](http://serilog.net/), used for logging.
