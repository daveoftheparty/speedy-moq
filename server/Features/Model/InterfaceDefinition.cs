using System.Collections.Generic;

namespace Features.Model
{
	public record InterfaceDefinition
	(
			string SourceFile,
			string InterfaceName,
			string ReturnType,
			string MethodName,
			IEnumerable<InterfaceDefinitionParameters> Parameters
	);

	public record InterfaceDefinitionParameters
	(
		string ParameterType,
		string ParameterName,
		string ParameterDefinition
	);
}