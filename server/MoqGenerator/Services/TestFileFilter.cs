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
				if(line.Trim().StartsWith(framework))
					return true;
			}
		}

		return false;
	}
}
