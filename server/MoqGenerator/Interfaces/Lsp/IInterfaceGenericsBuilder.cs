using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using MoqGenerator.Model;

namespace MoqGenerator.Interfaces.Lsp
{
	public interface IInterfaceGenericsBuilder
	{
		IReadOnlySet<InterfaceGenericsBuilderResponse> BuildSlow(string code);
		IReadOnlySet<InterfaceGenericsBuilderResponse> BuildFast(SyntaxTree tree);
	}
}
