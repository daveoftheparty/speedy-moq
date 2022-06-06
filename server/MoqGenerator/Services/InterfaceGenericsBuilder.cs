using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MoqGenerator.Model;

namespace MoqGenerator.Services
{
	public class InterfaceGenericsBuilder
	{

		#warning add XML doc to interface: all methods do the same but some are faster...
		public IReadOnlyDictionary<string, InterfaceGenerics> BuildSlowest(string code)
		{
			var tree = CSharpSyntaxTree.ParseText(code);
			return BuildSlow(tree);
		}

		public IReadOnlyDictionary<string, InterfaceGenerics> BuildSlow(SyntaxTree tree)
		{
			var compilation = CSharpCompilation
				.Create(null)
				.AddSyntaxTrees(tree);

			return BuildFast(compilation, tree);
		}

		public IReadOnlyDictionary<string, InterfaceGenerics> BuildFast(CSharpCompilation compilation, SyntaxTree tree)
		{
			var model = compilation.GetSemanticModel(tree);
			
			return model
				.SyntaxTree
				.GetRoot()
				.DescendantNodes()
				.OfType<TypeArgumentListSyntax>()
				.Where(meth => meth.Parent.GetType() == typeof(GenericNameSyntax))
				.Select(args => new
				{
					interfaceName = ((GenericNameSyntax)args.Parent).Identifier.Text,
					typeArgs = args.Arguments.Select(a => a.ToString()).ToList()
				})
				.ToDictionary(
					pair => pair.interfaceName,
					pair => new InterfaceGenerics(pair.interfaceName, (IReadOnlyList<string>)pair.typeArgs)
				)
				;
		}



		// this is for InterfaceStore needs-- return type may not necessarily be the same as other methods
		public IReadOnlyDictionary<string, InterfaceGenerics> BuildFastest(string interfaceName, SemanticModel model, SyntaxNode member)
		{
			throw new NotImplementedException();
		}
	}
}