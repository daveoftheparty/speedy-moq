# Speedy Moq

Code generation to speed up testing in C# using the excellent [Moq](https://www.nuget.org/packages/Moq/) library.

_Demo GIF blurry? Scroll down for a text based demo._

![Demo](docs/images/Demo.gif?raw=true "Demo")

## Given:
```csharp
namespace Demo.Lib
{
	public interface IStringAnalyzer
	{
		int CharOccurs(string text, char charToCount);
	}
}
```

## And:
```csharp
using NUnit.Framework; // or Xunit, Microsoft.VisualStudio.TestTools

namespace Demo.Lib.UnitTests
{
	public class StringAnalyzerTests
	{
		[Test]
		public void Go()
		{
			IStringAnalyzer
		}
	}
}
```

## When: User clicks the lightbulb and chooses `Generate Moq Setups`

## Then:
```csharp
using System;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;

namespace Demo.Lib.UnitTests
{
	public class StringAnalyzerTests
	{
		[Test]
		public void Go()
		{
			var stringAnalyzer = new Mock<IStringAnalyzer>();
			Expression<Func<IStringAnalyzer, int>> charOccurs = x =>
				x.CharOccurs(It.IsAny<string>(), It.IsAny<char>());
			
			stringAnalyzer
				.Setup(charOccurs)
				.Returns((string text, char charToCount) =>
				{
					return default;
				});

			stringAnalyzer.Verify(charOccurs, Times.Once);
		}
	}
}
```



## Usage

While in a test file, type out the name of the interface you wish to generate code for--on its own line. You should be presented with a lightbulb. Using the lightbulb, or the keyboard sequence `Ctrl`+`.` you can now generate your basic Moq code by choosing `Generate Moq Setups*`

*Note that for interfaces with generic type arguments, you will need to enter the type arguments in angle brackets before you get code generation.*

### Example Generic Usage

```csharp
using Xunit;

namespace Demo;

public interface IGenericService<TSource, TResult>
{
	IEnumerable<TResult> TransformSource(IEnumerable<TSource> items);
}

public class ServiceConsumerTests
{
	[Test]
	public void Go()
	{
		// enter this to get code generation/moq setups:
		IGenericService<string, int>
	}
}
```

## Hints

There are probably bugs, and lots of them. [File an issue!](https://github.com/daveoftheparty/speedy-moq/issues)

If you don't get the codefix (the lightbulb), it may be one of these reasons:
- Extension isn't ready yet. It may take 3-5 seconds to read your code and prepare suggestions from the time a .cs file is first opened in the IDE.
- Your interface name is misspelled. Code completion should help avoid that. _Your interface name must be the only text on a line, other than whitespace._
- Your test project does not yet reference the project where the interface is declared.
- The extension doesn't recognize the file you are editing as a test file. Currently test file detection is a simple matter of looking to see if any of these statements appear in your .cs file. If the extension is missing a test framework, please let me know.
	- `using NUnit.Framework;`
	- `using Xunit;`
	- `using Microsoft.VisualStudio.TestTools.UnitTesting;`
- Or, there's simply a bug!! Scroll up for a link to file an issue 🤣

## Lastly

_\* I reserve the right to change the prompt from `Generate Moq Setups` to something more fun, or "on-brand"... just sayin'_

✔ Happy Testing!