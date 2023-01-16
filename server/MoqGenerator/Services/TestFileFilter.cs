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

	public bool IsTestFile(IReadOnlyList<string> lines)
	{
		// return fast if an entire line exactly matches a using statement:
		if(lines.Any(l => _testFrameworks.Contains(l)))
			return true;

		// a bit slower, need to bounce every line off every test framework item:
		foreach(var line in lines)
		{
			foreach(var framework in _testFrameworks)
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

				if(line.IndexOf(framework) >= 0)
					return true;
			}
		}

		return false;
	}
}
