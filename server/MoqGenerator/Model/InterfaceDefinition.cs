using System.Collections.Generic;

namespace MoqGenerator.Model
{
	public record InterfaceDefinition
	(
		string InterfaceName,
		InterfaceGenerics Generics,
		string SourceFile,
		IReadOnlyList<InterfaceMethod> Methods,
		IReadOnlyList<string> Properties,
		InterfaceIndexer Indexer
	);

	public record InterfaceMethod
	(
		string MethodName,
		string ReturnType,
		IReadOnlyList<InterfaceMethodParameter> Parameters
	);

	public record InterfaceMethodParameter
	(
		string ParameterType,
		string ParameterName,
		string ParameterDefinition
	);

	public record InterfaceIndexer
	(
		string KeyType,
		string ReturnType,
		bool HasGet,
		bool HasSet
	);
}