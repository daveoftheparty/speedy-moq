using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using MoqGenerator.Model;

#warning elevate interfaces up one level before? done with this branch, no need for everyting to be in Interfaces.Lsp -- or do a separate branch just for that
namespace MoqGenerator.Interfaces.Lsp
{
	public interface IInterfaceGenericsBuilder
	{
		IReadOnlySet<InterfaceGenericsBuilderResponse> BuildSlow(string code);
		IReadOnlySet<InterfaceGenericsBuilderResponse> BuildFast(SyntaxTree tree);
	}
}
