using System.Collections.Generic;

namespace MoqGenerator.Model
{
	public record InterfaceGenerics
	(
		string InterfaceName,
		List<string> GenricTypeArguments
	);
}