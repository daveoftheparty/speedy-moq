using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using MoqGenerator.Interfaces.Lsp;

namespace OmniLsp.Adapters
{
	public class DiagnosticAdapter
	{
		public static OmniSharp.Extensions.LanguageServer.Protocol.Models.Diagnostic From(MoqGenerator.Model.Lsp.Diagnostic diagnostic, IClientAbilities clientAbilities)
		{
			DiagnosticSeverity severity = DiagnosticSeverity.Error;
			switch(diagnostic.Severity)
			{
				case MoqGenerator.Model.Lsp.DiagnosticSeverity.Error:
					severity = DiagnosticSeverity.Error;
					break;
				case MoqGenerator.Model.Lsp.DiagnosticSeverity.Warning:
					severity = DiagnosticSeverity.Warning;
					break;
				case MoqGenerator.Model.Lsp.DiagnosticSeverity.Information:
					severity = DiagnosticSeverity.Information;
					break;
				case MoqGenerator.Model.Lsp.DiagnosticSeverity.Hint:
					severity = DiagnosticSeverity.Hint;
					break;
			}

			Dictionary<string, IEnumerable<TextEdit>> omniData = null;
			if(clientAbilities.CanReceiveDiagnosticData)
			{
				// have to convert our TextEdit to Omni's version:
				omniData = diagnostic
					.Data
					.Select(teByNamespace => new
					{
						NamespaceName = teByNamespace.Key,
						Edits = teByNamespace
							.Value
							.Select(TextEditAdapter.From)
						}
					)
					.ToDictionary(pair => pair.NamespaceName, pair => pair.Edits);
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
				Data = omniData != null ? JsonSerializer.Serialize(omniData) : null
			};
		}

		// well, I wrote this method thinking I needed it. And as of now, I don't, but I don't want to write it again:
		public static MoqGenerator.Model.Lsp.Diagnostic From(OmniSharp.Extensions.LanguageServer.Protocol.Models.Diagnostic diagnostic, IClientAbilities clientAbilities)
		{
			MoqGenerator.Model.Lsp.DiagnosticSeverity severity = MoqGenerator.Model.Lsp.DiagnosticSeverity.Error;
			switch(diagnostic.Severity)
			{
				case DiagnosticSeverity.Error:
					severity = MoqGenerator.Model.Lsp.DiagnosticSeverity.Error;
					break;
				case DiagnosticSeverity.Warning:
					severity = MoqGenerator.Model.Lsp.DiagnosticSeverity.Warning;
					break;
				case DiagnosticSeverity.Information:
					severity = MoqGenerator.Model.Lsp.DiagnosticSeverity.Information;
					break;
				case DiagnosticSeverity.Hint:
					severity = MoqGenerator.Model.Lsp.DiagnosticSeverity.Hint;
					break;
			}

			return new MoqGenerator.Model.Lsp.Diagnostic
			(
				new MoqGenerator.Model.Lsp.Range
				(
					new MoqGenerator.Model.Lsp.Position((uint)diagnostic.Range.Start.Line, (uint)diagnostic.Range.Start.Character),
					new MoqGenerator.Model.Lsp.Position((uint)diagnostic.Range.End.Line, (uint)diagnostic.Range.End.Character)
				),
				severity,
				diagnostic.Code,
				diagnostic.Source,
				diagnostic.Message,
				clientAbilities.CanReceiveDiagnosticData ? JsonSerializer.Deserialize<IReadOnlyDictionary<string, IReadOnlyList<MoqGenerator.Model.Lsp.TextEdit>>>(diagnostic.Data.ToString()) : null
			);
		}
	}
}