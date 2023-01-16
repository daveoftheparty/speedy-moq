using System.Linq;
using System.Collections.Generic;
using MoqGenerator.Interfaces.Lsp;


namespace MoqGenerator.Services;

public class TestFileFilter : ITestFileFilter
{
	private enum Framework
	{
		Xunit,
		NUnit,
		MSTest
	}

	private record TestAttributeFragments
	(
		string Fragment,
		int Priority, // start with 0 for most likely to be found in a file, 2 for least likely, and 1 for everything in between
		Framework[] Frameworks,
		string SampleOrComment = null
	);

	private readonly HashSet<string> _testFrameworks = new()
	{
		"using Xunit;",
		"using NUnit.Framework;",
		"using Microsoft.VisualStudio.TestTools.UnitTesting;" // Framework.MSTest
	};

	
	// we'll sort these in constructor by Priority, List Index
	private readonly List<TestAttributeFragments> _fragments = new()
	{
		// see https://xunit.net/docs/comparisons for a pretty good list of attributes from all three frameworks
		new TestAttributeFragments("[Fact", 0, new[] { Framework.Xunit }, "generally seen as [Fact], but sometimes seen as [Fact(<...> like [Fact(Skip=\"reason\")]"),
		new TestAttributeFragments("[Test]", 0, new[] { Framework.NUnit }),
		new TestAttributeFragments("[TestClass]", 0, new[] { Framework.MSTest }, "REQUIRED for every test class"),
		new TestAttributeFragments("[Theory]", 0, new[] { Framework.Xunit, Framework.NUnit }),
		new TestAttributeFragments("[TestFixture]", 0, new[] { Framework.NUnit }, "no longer required but very likely to appear"),


		// data driven (XUnit data driven Theory already present above with higher priority)
		new TestAttributeFragments("[TestCase(", 1, new[] { Framework.NUnit }, "note no closing parens/brackets, full implementation might be [TestCase(1, true)]"),
		new TestAttributeFragments("[DataSource(", 1, new[] { Framework.MSTest }, "note no closing parens/brackets"),

		// setups/teardowns
		new TestAttributeFragments("[SetUp]", 1, new[] { Framework.NUnit }),
		new TestAttributeFragments("[TearDown]", 1, new[] { Framework.NUnit }),
		new TestAttributeFragments("[OneTimeSetUp]", 1, new[] { Framework.NUnit }),
		new TestAttributeFragments("[OneTimeTearDown]", 1, new[] { Framework.NUnit }),

		new TestAttributeFragments("[ClassInitialize]", 1, new[] { Framework.MSTest }),
		new TestAttributeFragments("[ClassCleanup]", 1, new[] { Framework.MSTest }),
		new TestAttributeFragments("[TestInitialize]", 1, new[] { Framework.MSTest }),
		new TestAttributeFragments("[TestCleanup]", 1, new[] { Framework.MSTest }),

		// lowest priority
		new TestAttributeFragments("[Ignore", 2, new[] { Framework.NUnit, Framework.MSTest }, "NUnit: [Ignore(\"reason\")], MSTest: [Ignore]"),
		new TestAttributeFragments("[TestProperty]", 2, new[] { Framework.MSTest }),

		new TestAttributeFragments("[TestMethod]", 2, new[] { Framework.MSTest }, "required for every test method, but since TestClass also required, let's evaluate this last"),
	};


	private readonly List<string> _allFragments;

	public TestFileFilter()
	{
		_allFragments = _testFrameworks
			.Concat(_fragments
				.Select((fragment, index) => new { fragment, index})
				.OrderBy(p => p.fragment.Priority)
				.ThenBy(sort => sort.index)
				.Select(s => s.fragment.Fragment)
			)
			.ToList();
	}

	public bool IsTestFile(IReadOnlyList<string> lines)
	{
		// return fast if an entire line exactly matches a using statement:
		if(lines.Any(l => _testFrameworks.Contains(l)))
			return true;

		// a bit slower, need to bounce every line off every test framework/attribute item:
		foreach(var line in lines)
		{
			foreach(var fragment in _allFragments)
			{
				/*
				well, this is awkward, because these are easy enough to test for:
					using Xunit; // see this trailing comment?
					using System;using Xunit;
				
				and so is this, make sure there isn't an in-line comment preceding:
					// using System;usingXunit;

				but I really don't feel like coding for a block comment
				
					/<imagine a *>
					...
					using System;usingXunit;
					...
					<imagine a *>/

				and really, is it the end of the world if we THINK it's a test file and it's not?
				Diagnoser will just output codegen suggestions for non-test files...
				*/

				if(line.IndexOf(fragment) >= 0)
					return true;
			}
		}

		return false;
	}
}
