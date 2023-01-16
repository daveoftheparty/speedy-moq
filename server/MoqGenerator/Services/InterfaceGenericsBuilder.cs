using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MoqGenerator.Interfaces.Lsp;
using MoqGenerator.Model;

namespace MoqGenerator.Services
{
	public class InterfaceGenericsBuilder : IInterfaceGenericsBuilder
	{
		public IReadOnlySet<InterfaceGenericsBuilderResponse> BuildSlow(string code)
		{
			var tree = CSharpSyntaxTree.ParseText(code);
			return BuildFast(tree);
		}

		public IReadOnlySet<InterfaceGenericsBuilderResponse> BuildFast(SyntaxTree tree)
		{
			return tree
				.GetRoot()
				.DescendantNodes()
				.OfType<TypeArgumentListSyntax>()
				.Where(meth => FilterByParent(meth.Parent))
				.Select(args =>
				{
					return new InterfaceGenericsBuilderResponse
					(
						new InterfaceGenerics(
							((GenericNameSyntax)args.Parent).Identifier.Text,
							(IReadOnlyList<string>)args.Arguments.Select(a => a.ToString()).ToList()
						),
						args.Parent.Span
					);
				})
				.ToHashSet()
				;
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