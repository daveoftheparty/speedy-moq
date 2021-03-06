## (next, see next heading for format)
- added support for mocking indexers, this code:
```csharp
public interface IHasIndexer
{
	string this[string key] { get; set; }
}
```
produces this setup:
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


## 0.0.9 (Aug 23, 2021)
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

## 0.0.8 (Aug 14, 2021)
- improved readme
- logging improvements
- formatting change: wrap the Linq Expression for better readability for methods with long params. Went from:
```csharp
Expression<Func<IStringAnalyzer, int>> charOccurs = x => x.CharOccurs(It.IsAny<string>(), It.IsAny<char>());
```
to
```csharp
Expression<Func<IStringAnalyzer, int>> charOccurs = x =>
	x.CharOccurs(It.IsAny<string>(), It.IsAny<char>());
```
- more testing around tabs vs. spaces detection for proper output formatting
- update demo gif, hoping for better display (less blurry)
- thin dark outline around extension icon for light theme display
- developer/testing quality of life stuff around test inputs

## 0.0.7 (Aug 10, 2021)
- tiny performance improvement, won't keep trolling the file system to find .csproj files for a .cs file that has previously been resolved
- fixes a bug introduced with 0.0.6 where some namespace calculations can blow over/remove previous interface definitions. (As a user: you might have seen the same interface defined in two or more namespaces randomly offer to Generate Moq Setups for all namespaces, then later, for only one of them.)

## 0.0.6 (Aug 9, 2021)

Now handles multiple namespaces -- given an interface `IDealio` that exists in both namespace `Two.Lib.Version1` _**and**_ `Two.Lib.Version2`, you will now be presented with the options:
- Generate Moq setups for namespace Two.Lib.Version1
- Generate Moq setups for namespace Two.Lib.Version2

## 0.0.5 (Aug 7, 2021)
- Cut compile time (time until a user sees the lightbulb to generate moq) in half by reading .csproj files directly instead of trying to build them
- Fixed a bug where other extensions would send diagnostics without a Source, this extension was doing string matching on Source (and assuming it was not null)

## 0.0.4 (Aug 5, 2021)

Initial launch to Marketplace