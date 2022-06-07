using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using MoqGenerator.Model;

namespace MoqGenerator.Interfaces.Lsp
{
	public interface IInterfaceGenericsBuilder
	{
		IReadOnlyDictionary<string, (InterfaceGenerics Generics, TextSpan Location)> BuildSlowest(string code);
		IReadOnlyDictionary<string, (InterfaceGenerics Generics, TextSpan Location)> BuildSlow(SyntaxTree tree);
		IReadOnlyDictionary<string, (InterfaceGenerics Generics, TextSpan Location)> BuildFast(CSharpCompilation compilation, SyntaxTree tree);
		IReadOnlyDictionary<string, (InterfaceGenerics Generics, TextSpan Location)> BuildFastest(string interfaceName, SemanticModel model, SyntaxNode member);
	}
}
