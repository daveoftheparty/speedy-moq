using System.Linq;
using System.Collections.Generic;
using MoqGenerator.Interfaces.Lsp;


namespace MoqGenerator.Services;

public class TestFileFilter : ITestFileFilter
{
	private readonly HashSet<string> _testFrameworks = new()
	{
		"using NUnit.Framework;",
		"using Xunit;",
		"using Microsoft.VisualStudio.TestTools.UnitTesting;"
	};

	private readonly HashSet<string> _testAttributes = new()
	{
		#warning if possible, would it make more sense for this to be a list and be ordered by attributes MORE LIKELY to show up for faster detection?
		// see https://xunit.net/docs/comparisons for a pretty good list of attributes from all three frameworks


		// Xunit
		"[Fact", // generally seen as [Fact], but sometimes seen as [Fact(<...>

		// NUnit
		"[Test]",
		"[TestFixture]", // optional for some test classes
		"[SetUp]",
		"[TearDown]",
		"[OneTimeSetUp]",
		"[OneTimeTearDown]",
		"[TestCase(", // note no closing parens/brackets, full implementation might be [TestCase(1, true)]

		// MSTest
		"[TestMethod]",
		"[TestClass]", // REQUIRED for every test class
		"[TestInitialize]",
		"[TestCleanup]",
		"[ClassInitialize]",
		"[ClassCleanup]",
		"[TestProperty]",
		"[DataSource(",

		// multiple frameworks
		"[Theory]",
		"[Ignore",
	};

	private readonly List<string> _allTestFragments;

	public TestFileFilter()
	{
		_allTestFragments = _testFrameworks.Concat(_testAttributes).ToList();
	}

	public bool IsTestFile(IReadOnlyList<string> lines)
	{
		// return fast if an entire line exactly matches a using statement:
		if(lines.Any(l => _testFrameworks.Contains(l)))
			return true;

		// a bit slower, need to bounce every line off every test framework item:
		foreach(var line in lines)
		{
			foreach(var fragment in _allTestFragments)
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
