using System.Collections.Generic;

namespace MoqGenerator.Model
{
	public sealed record InterfaceGenerics
	(
		string InterfaceName,
		IReadOnlyList<string> GenericTypeArguments
	)
	{
		public string InterfaceNameKey { get; init; } = $"{InterfaceName};{GenericTypeArguments?.Count}";

		public bool Equals(InterfaceGenerics other)
		{
			return InterfaceNameKey.Equals(other?.InterfaceNameKey);
		}
		
		public override int GetHashCode() => InterfaceNameKey.GetHashCode();

		public override string ToString()
		{
			return $"{{ InterfaceName = {InterfaceName}, GenericTypeArguments = {string.Join(", ", GenericTypeArguments)} }}";
		}
	}
}