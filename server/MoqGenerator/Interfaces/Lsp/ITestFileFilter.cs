using System.Collections.Generic;

namespace MoqGenerator.Interfaces.Lsp;
public interface ITestFileFilter
{
	bool IsTestFile(IReadOnlyList<string> lines);
}