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
		return lines.Any(l => _testFrameworks.Contains(l));
	}
}
