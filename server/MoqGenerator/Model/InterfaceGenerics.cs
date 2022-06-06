using System.Collections.Generic;

namespace MoqGenerator.Model
{
	public record InterfaceGenerics
	(
		string InterfaceName,
		IReadOnlyList<string> GenericTypeArguments
	);
}