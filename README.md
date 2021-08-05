# Speedy Moq

Code generation to speed up testing in C# using the excellent [Moq](https://www.nuget.org/packages/Moq/) library.

![Demo](docs/images/Demo.gif?raw=true "Demo")

## Usage

While in a test file, type out the name of the interface you wish to generate code for. You should be presented with a lightbulb. Using the lightbulb, or Ctrl+. you can now generate your basic Moq code by choosing `Generate Moq Setups*`

## Hints

- There are probably bugs, and lots of them. [File an issue!](https://github.com/daveoftheparty/speedy-moq/issues)

- The interface to be mocked should be referenced by your test project, or the extension can't find its definition. The most common setup would be:

```
.
└── Demo
    ├── Demo.Lib
    │   ├── Demo.Lib.csproj
    │   └── IStringAnalyzer.cs
    │
    ├── Demo.Lib.UnitTests
    │   ├── Demo.Lib.UnitTests.csproj  (references Demo.Lib)
    │   └── StringAnalyzerTests.cs     (generate Moq here)
    │
    └── Demo.sln                       (sln not necessary)
```
- If you don't get the codefix (the lightbulb), it may be one of these reasons:
	- Extension isn't ready yet. It may take up to 10 seconds to read your code and prepare suggestions from the time a .cs file is first opened in the IDE. It's in the backlog to try to make that part faster.
	- Your interface name is misspelled.
	- Your test project does not yet reference the project where the interface is declared.
	- The extension doesn't recognize the file you are editing as a test file. Currently test file detection is a simple matter of looking to see if any of these statements appear in your .cs file. If the extension is missing a test framework, please let me know.
		- `using NUnit.Framework;`
		- `using Xunit;`
		- `using Microsoft.VisualStudio.TestTools.UnitTesting;`
	- Or, there's simply a bug!! Scroll up for a link to file an issue 🤣

## Lastly

_\* I reserve the right to change the prompt from `Generate Moq Setups` to something more fun, or "on-brand"... just sayin'_

Happy Testing! ✔