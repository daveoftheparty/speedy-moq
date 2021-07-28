namespace Features.Model.Lsp
{
	
	public record Diagnostic
	(
		Range Range,
		DiagnosticSeverity Severity,
		string Code,
		string Source,
		string Message,
		string Data // anything extra we want to send to/process in code action provider, could format as json and deserialize as necessary if more than one value
	);

	public enum DiagnosticSeverity
	{
		Error = 1,
		Warning = 2,
		Information = 3,
		Hint = 4
	}
}