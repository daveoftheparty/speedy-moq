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

		// well, I wrote this method thinking I needed it. And as of now, I don't, but I don't want to write it again:
		public static Features.Model.Lsp.Diagnostic From(OmniSharp.Extensions.LanguageServer.Protocol.Models.Diagnostic diagnostic)
		{
			Features.Model.Lsp.DiagnosticSeverity severity = Features.Model.Lsp.DiagnosticSeverity.Error;
			switch(diagnostic.Severity)
			{
				case DiagnosticSeverity.Error:
					severity = Features.Model.Lsp.DiagnosticSeverity.Error;
					break;
				case DiagnosticSeverity.Warning:
					severity = Features.Model.Lsp.DiagnosticSeverity.Warning;
					break;
				case DiagnosticSeverity.Information:
					severity = Features.Model.Lsp.DiagnosticSeverity.Information;
					break;
				case DiagnosticSeverity.Hint:
					severity = Features.Model.Lsp.DiagnosticSeverity.Hint;
					break;
			}

			return new Features.Model.Lsp.Diagnostic
			(
				new Features.Model.Lsp.Range
				(
					new Features.Model.Lsp.Position((uint)diagnostic.Range.Start.Line, (uint)diagnostic.Range.Start.Character),
					new Features.Model.Lsp.Position((uint)diagnostic.Range.End.Line, (uint)diagnostic.Range.End.Character)
				),
				severity,
				diagnostic.Code,
				diagnostic.Source,
				diagnostic.Message,
				diagnostic.Data.ToString()
			);
		}
	}
}