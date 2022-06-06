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
			throw new NotImplementedException();
		}

		public IReadOnlyDictionary<string, InterfaceGenerics> BuildSlow(SyntaxTree tree)
		{
			throw new NotImplementedException();
		}

		public IReadOnlyDictionary<string, InterfaceGenerics> BuildFast(CSharpCompilation compilation, SyntaxTree tree)
		{
			throw new NotImplementedException();
		}


		// this is for InterfaceStore needs
		public IReadOnlyDictionary<string, InterfaceGenerics> BuildFastest(string interfaceName, SemanticModel model, SyntaxNode member)
		{
			throw new NotImplementedException();
		}

		private void GetGenericTypeArguments(CSharpCompilation compilation, SyntaxTree tree)
		{
			#warning DRY: there is also code in InterfaceStore.GetInterfaceTypeArguments() that has similar concerns
			var model = compilation.GetSemanticModel(tree);
			
			var results = model
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
				// .ToDictionary(pair => pair.interfaceName, pair => (IReadOnlyList<string>)pair.typeArgs)
				;
		}

	}
}