using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using MoqGenerator.Model;

#warning elevate interfaces up one level before? done with this branch, no need for everyting to be in Interfaces.Lsp -- or do a separate branch just for that
namespace MoqGenerator.Interfaces.Lsp
{
	public interface IInterfaceGenericsBuilder
	{
		#warning did I really need a dictionary here? I may have, for InterfaceStore, but not for Diagnoser...
		IReadOnlyDictionary<string, (InterfaceGenerics Generics, TextSpan Location)> BuildSlowest(string code);
		IReadOnlyDictionary<string, (InterfaceGenerics Generics, TextSpan Location)> BuildSlow(SyntaxTree tree);
		IReadOnlyDictionary<string, (InterfaceGenerics Generics, TextSpan Location)> BuildFast(CSharpCompilation compilation, SyntaxTree tree);
		InterfaceGenerics BuildFastest(SemanticModel model, SyntaxNode member);
	}
}
