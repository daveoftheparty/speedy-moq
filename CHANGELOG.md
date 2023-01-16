Change Log


- [0.1.0 (Jan 15, 2023)](#010-jan-15-2023)
	- [Changes](#changes)
	- [Examples](#examples)
		- [Mocking Indexers](#mocking-indexers)
		- [Better Generics](#better-generics)
- [0.0.9 (Aug 23, 2021)](#009-aug-23-2021)
- [0.0.8 (Aug 14, 2021)](#008-aug-14-2021)
	- [Changes](#changes-1)
	- [Examples](#examples-1)
		- [Linq Expression Wrapping](#linq-expression-wrapping)
- [0.0.7 (Aug 10, 2021)](#007-aug-10-2021)
- [0.0.6 (Aug 9, 2021)](#006-aug-9-2021)
- [0.0.5 (Aug 7, 2021)](#005-aug-7-2021)
- [0.0.4 (Aug 5, 2021)](#004-aug-5-2021)



# 0.1.0 (Jan 15, 2023)

***see next heading for format -- if next upgrades to net6.0, maybe version should be 0.1.0***

## Changes
- added support for mocking indexers
- greatly increased usability when dealing with generics.
- (for extension contributors) Architecture diagrams added, see [CONTRIBUTING.md](CONTRIBUTING.md)


## Examples
### Mocking Indexers

this code:
```csharp
public interface IHasIndexer
{
	string this[string key] { get; set; }
}
```
will now produce this setup:
```csharp
var hasIndexer = new Mock<IHasIndexer>();

var hasIndexerStore = new Dictionary<string, string>();
hasIndexer
	.Setup(x => x[It.IsAny<string>()])
	.Returns((string key) => hasIndexerStore[key]);
hasIndexer
	.SetupSet(x => x[It.IsAny<string>()] = It.IsAny<string>())
	.Callback((string key, string value) => hasIndexerStore[key] = value);
```

### Better Generics

BEFORE this release, when you entered the generic interface name, you got a lame version of the setups with, in the following example, TSource, TResult-- and would have to edit the code in several places:
```csharp
public interface IGenericService<TSource, TResult>
{
	IEnumerable<TResult> TransformSource(IEnumerable<TSource> items);
}

// before moq setups applied:
public class GenericServiceTests
{
	[Test]
	public void Go()
	{
		// you would type this on a single line:
		IGenericService
	}
}

// class after Generating Moq Setups:
public class GenericServiceTests
{
	[Test]
	public void Go()
	{
		// YUCK!!! 🤢🤮
		var genericService = new Mock<IGenericService<TSource, TResult>>();

		Expression<Func<IGenericService<TSource, TResult>, IEnumerable<TResult>>> transformSource = x =>
			x.TransformSource(It.IsAny<IEnumerable<TSource>>());

		genericService
			.Setup(transformSource)
			.Returns((IEnumerable<TSource> items) =>
			{
				return default;
			});

		genericService.Verify(transformSource, Times.Once);
	}
}
```

AFTER this release, enter the generic interface __*and the type arguments you wish to use*__ and you get a much better result:
```csharp
public interface IGenericService<TSource, TResult>
{
	IEnumerable<TResult> TransformSource(IEnumerable<TSource> items);
}

// before moq setups applied:
public class GenericServiceTests
{
	[Test]
	public void Go()
	{
		// you would type this on a single line:
		IGenericService<char, int>
	}
}

// class after Generating Moq Setups:
public class GenericServiceTests
{
	[Test]
	public void Go()
	{
		// oooh, so PRETTY! So usable! 😎😍
		var genericService = new Mock<IGenericService<char, int>>();

		Expression<Func<IGenericService<char, int>, IEnumerable<int>>> transformSource = x =>
			x.TransformSource(It.IsAny<IEnumerable<char>>());

		genericService
			.Setup(transformSource)
			.Returns((IEnumerable<char> items) =>
			{
				return default;
			});

		genericService.Verify(transformSource, Times.Once);
	}
}
```

# 0.0.9 (Aug 23, 2021)
Bugfix with generating mock for a generic. Before fix, this interface:
```csharp
public interface IService<T>
{
	T GetSomething();
}
```
generated:
```csharp
var service = new Mock<IService>();
Expression<Func<IService, T>> getSomething = x =>
	x.GetSomething();
```
which is a compilation error, and this library tries to avoid generating code with compilation errors.

Now, the interface above will generate:
```csharp
var service = new Mock<IService<T>>();
Expression<Func<IService<T>, T>> getSomething = x =>
	x.GetSomething();
```
which, while the T isn't super useful, is at least, syntactically correct... just replace T with _*whatever*_

# 0.0.8 (Aug 14, 2021)
## Changes
- improved readme
- logging improvements
- Linq Expression Wrapping (see below)
- more testing around tabs vs. spaces detection for proper output formatting
- update demo gif, hoping for better display (less blurry)
- thin dark outline around extension icon for light theme display
- (for extension contributors) quality of life stuff around test inputs

## Examples
### Linq Expression Wrapping
Just a formatting change, but wrap the Linq Expression for better readability, especially helpful for methods with long lists of parameters. 

In this simple example, the formatting BEFORE:
```csharp
Expression<Func<IStringAnalyzer, int>> charOccurs = x => x.CharOccurs(It.IsAny<string>(), It.IsAny<char>());
```
and AFTER:
```csharp
Expression<Func<IStringAnalyzer, int>> charOccurs = x =>
	x.CharOccurs(It.IsAny<string>(), It.IsAny<char>());
```


# 0.0.7 (Aug 10, 2021)
- tiny performance improvement, won't keep trolling the file system to find .csproj files for a .cs file that has previously been resolved
- fixes a bug introduced with 0.0.6 where some namespace calculations can blow over/remove previous interface definitions. (As a user: you might have seen the same interface defined in two or more namespaces randomly offer to Generate Moq Setups for all namespaces, then later, for only one of them.)

# 0.0.6 (Aug 9, 2021)

Now handles multiple namespaces -- given an interface `IDealio` that exists in both namespace `Two.Lib.Version1` _**and**_ `Two.Lib.Version2`, you will now be presented with the options:
- Generate Moq setups for namespace Two.Lib.Version1
- Generate Moq setups for namespace Two.Lib.Version2

# 0.0.5 (Aug 7, 2021)
- Cut compile time (time until a user sees the lightbulb to generate moq) in half by reading .csproj files directly instead of trying to build them
- Fixed a bug where other extensions would send diagnostics without a Source, this extension was doing string matching on Source (and assuming it was not null)

# 0.0.4 (Aug 5, 2021)

Initial launch to Marketplace