namespace Features.Model.Lsp
{
	
	public record Diagnostic
	(
		Range Range,
		DiagnosticSeverity Severity,
		string Code,
		string Source,
		string Message,
		string Data // should be valid json, anything extra we want to send to/process in code action provider
	);

	public enum DiagnosticSeverity
	{
		Error = 1,
		Warning = 2,
		Information = 3,
		Hint = 4
	}
}