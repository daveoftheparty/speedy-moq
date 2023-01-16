using Microsoft.CodeAnalysis.Text;

namespace MoqGenerator.Model
{
	public sealed record InterfaceGenericsBuilderResponse
	(
		InterfaceGenerics Generics,
		TextSpan Location
	)
	{
		private string key = string.Join(';',
			Generics,
			Location.IsEmpty,
			Location.Start,
			Location.End,
			Location.Length
		);

		public bool Equals(InterfaceGenericsBuilderResponse other)
		{
			return key.Equals(other?.key);
		}
		
		public override int GetHashCode() => key.GetHashCode();
	}
}