# Roslyn notes

The following debug stuff was used when trying to find namespaces in InterfaceStore, and it was hard-earned, so while this isn't meant to be a great readme, it is meant to be a breadcrumb that escapes git history to look back on if necessary:

```csharp
// debuggin:
var firstModel = models.First();
var nodes = firstModel
	.SyntaxTree
	.GetRoot()
	.DescendantNodes()
	.OfType<InterfaceDeclarationSyntax>()
	.Select(node => new
	{

		containingSymbol = firstModel.GetDeclaredSymbol(node)?.ContainingSymbol,
		containingSymbolString = firstModel.GetDeclaredSymbol(node)?.ContainingSymbol?.ToString(),

		containingNamespace = firstModel.GetDeclaredSymbol(node)?.ContainingNamespace,
		containingNamespaceString = firstModel.GetDeclaredSymbol(node)?.ContainingNamespace?.ToString(),

		containingType = firstModel.GetDeclaredSymbol(node)?.ContainingType,
		containingTypeString = firstModel.GetDeclaredSymbol(node)?.ContainingType?.ToString(),
	})
	.ToList();

// var firstNode = nodes[0];
// var firstSymbol = firstModel.GetSymbolInfo(firstNode);
// var firstDeclared = firstModel.GetDeclaredSymbol(firstNode);
// var firstMembers = firstDeclared.GetMembers();
// var firstMemberGroup = firstModel.GetMemberGroup(firstNode);

// this throws exception:
// var namespaceName = firstModel.GetTypeInfo(firstNode).Type.ContainingNamespace.Name;

/*
	this gets what we want...
	firstModel.GetDeclaredSymbol(firstNode).ContainingNamespace.Name => "Foo"
	firstModel.GetDeclaredSymbol(firstNode).Name => "IConfigProps"
	firstModel.GetDeclaredSymbol(firstNode).ToString() => "Foo.IConfigProps"
*/
```