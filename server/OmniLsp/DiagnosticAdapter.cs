using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace OmniLsp
{
	public class DiagnosticAdapter
	{
		public static OmniSharp.Extensions.LanguageServer.Protocol.Models.Diagnostic From(Features.Model.Lsp.Diagnostic diagnostic)
		{
			DiagnosticSeverity severity = DiagnosticSeverity.Error;
			switch(diagnostic.Severity)
			{
				case Features.Model.Lsp.DiagnosticSeverity.Error:
					severity = DiagnosticSeverity.Error;
					break;
				case Features.Model.Lsp.DiagnosticSeverity.Warning:
					severity = DiagnosticSeverity.Warning;
					break;
				case Features.Model.Lsp.DiagnosticSeverity.Information:
					severity = DiagnosticSeverity.Information;
					break;
				case Features.Model.Lsp.DiagnosticSeverity.Hint:
					severity = DiagnosticSeverity.Hint;
					break;
			}

			return new OmniSharp.Extensions.LanguageServer.Protocol.Models.Diagnostic
			{
				Range = new OmniSharp.Extensions.LanguageServer.Protocol.Models.Range
				(
					new Position((int)diagnostic.Range.start.line, (int)diagnostic.Range.start.character),
					new Position((int)diagnostic.Range.end.line, (int)diagnostic.Range.end.character)
				),
				Severity = severity,
				Code = diagnostic.Code,
				Source = diagnostic.Source,
				Message = diagnostic.Message,
				Data = diagnostic.Data
			};
		}
	}
}