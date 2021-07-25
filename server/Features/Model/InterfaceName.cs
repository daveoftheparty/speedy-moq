namespace Features.Model
{
	public record InterfaceName(
		string Name,
		object Position // where in the text does this InterfaceName appear? (may be more than one place)
	);
}