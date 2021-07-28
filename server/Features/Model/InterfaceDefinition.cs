using System.Collections.Generic;

namespace Features.Model
{
	public record InterfaceDefinition
	(
		string InterfaceName,
		string SourceFile,
		IReadOnlyList<InterfaceMethod> Methods
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
}