using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using MoqGenerator.Model;

namespace MoqGenerator.Services
{
	public class InterfaceGenericsBuilder
	{

		#warning add XML doc to interface: all methods do the same but some are faster...
		public IReadOnlyDictionary<string, (InterfaceGenerics Generics, TextSpan Location)> BuildSlowest(string code)
		{
			var tree = CSharpSyntaxTree.ParseText(code);
			return BuildSlow(tree);
		}

		public IReadOnlyDictionary<string, (InterfaceGenerics Generics, TextSpan Location)> BuildSlow(SyntaxTree tree)
		{
			var compilation = CSharpCompilation
				.Create(null)
				.AddSyntaxTrees(tree);

			return BuildFast(compilation, tree);
		}

		public IReadOnlyDictionary<string, (InterfaceGenerics Generics, TextSpan Location)> BuildFast(CSharpCompilation compilation, SyntaxTree tree)
		{
			var model = compilation.GetSemanticModel(tree);
			
			return model
				.SyntaxTree
				.GetRoot()
				.DescendantNodes()
				.OfType<TypeArgumentListSyntax>()
				.Where(meth => FilterByParent(meth.Parent))
				.Select(args => 
				{
					return new
					{
						interfaceName = ((GenericNameSyntax)args.Parent).Identifier.Text,
						span = args.Parent.FullSpan,
						typeArgs = args.Arguments.Select(a => a.ToString()).ToList()
					};
				})
				.ToDictionary(
					pair => pair.interfaceName,
					pair => (new InterfaceGenerics(pair.interfaceName, (IReadOnlyList<string>)pair.typeArgs), pair.span)
				)
				;
		}


		// this is for InterfaceStore needs-- return type may not necessarily be the same as other methods
		public IReadOnlyDictionary<string, (InterfaceGenerics Generics, TextSpan Location)> BuildFastest(string interfaceName, SemanticModel model, SyntaxNode member)
		{
			throw new NotImplementedException();
		}

		private bool FilterByParent(SyntaxNode parent)
		{
			/*
				The first clause-- checking for GenericNameSyntax-- is to be sure we're dealing with a generic
				The second clause is necessary (checking the parent's parent) because otherwise, the following statement:

					IService<UserClassA, IReadOnlyDictionary<UserClassB>>
				
				would return this (serialized) dictionary:
					{
						"IService": {
							"InterfaceName": "IService",
							"GenericTypeArguments": [
							"UserClassA",
							"IReadOnlyDictionary<UserClassB>"
							]
						},
						"IReadOnlyDictionary": {
							"InterfaceName": "IReadOnlyDictionary",
							"GenericTypeArguments": [
							"UserClassB"
							]
						}
					}

				and, for now at least, we only want the "top level statement" because when we go to generate moqs, we simply want to
				change all occurences of TSource, TResult in this interface definition:

					public interface IService<TSource, TResult>
					{
						TResult ProcessInput(TSource data);
					}
				
				to code that looks like this:

					var mockService = new Mock<IService<UserClassA, IReadOnlyDictionary<UserClassB>>>();

				so the dictionary we're after in this class is not the "bad" example from above, but rather, just the first key/value
				pair, like so:

					{
						"IService": {
							"InterfaceName": "IService",
							"GenericTypeArguments": [
							"UserClassA",
							"IReadOnlyDictionary<UserClassB>"
							]
						}
					}
			*/

			return 
				parent?.GetType() == typeof(GenericNameSyntax) &&
				parent?.Parent?.GetType() != typeof(TypeArgumentListSyntax)
			;
		}
	}
}