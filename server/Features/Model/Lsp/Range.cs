namespace Features.Model.Lsp
{
	public record Range(Position start, Position end);
	public record Position(uint line, uint character);
}