using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

using MoqGenerator.Interfaces.Lsp;

namespace OmniLsp
{
	public class CodeActionHandler : ICodeActionHandler
	{
		private readonly ILogger<CodeActionHandler> _logger;
		private readonly IWhoaCowboy _whoaCowboy;

		public CodeActionHandler(ILogger<CodeActionHandler> logger, IWhoaCowboy whoaCowboy)
		{
			_logger = logger;
			_whoaCowboy = whoaCowboy;
			_logger.LogInformation("Code action constructed...");
		}

		#region ICodeActionHandler
		public CodeActionRegistrationOptions GetRegistrationOptions(CodeActionCapability capability, ClientCapabilities clientCapabilities)
		{
			return new CodeActionRegistrationOptions
			{
				DocumentSelector = new DocumentSelector(new DocumentFilter { Pattern = MoqGenerator.Constants.FileGlob} ),
				CodeActionKinds = new Container<CodeActionKind>(CodeActionKind.QuickFix),
				ResolveProvider = false
			};
		}

		public Task<CommandOrCodeActionContainer> Handle(CodeActionParams request, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Code action firing...");
			if(!_whoaCowboy.GiddyUp)
				return Task.FromResult(new CommandOrCodeActionContainer());

			_logger.LogInformation("Code action enabled...");

			#region debugging code
			// maybe temporary, maybe not, let's log the incoming diagnostics to see if something about the Visual Studio extension 
			// and the code below with the where clauses is filtering out our actions:

			_logger.LogInformation("CADIAG begin");

			request.Context.Diagnostics?
				.Select(diagnostic => $"CADIAG \t\t\t\tSource: {diagnostic?.Source ?? "null"} Data: {diagnostic?.Data?.ToString() ?? "null"}")
				.ToList()
				.ForEach(m => _logger.LogInformation(m))
				;

			_logger.LogInformation("CADIAG end");

			#endregion debugging code

			var actions = request.Context.Diagnostics?
				.Where(diagnostic => diagnostic?.Source != null)
				.Where(diagnostic => diagnostic.Source.Equals(MoqGenerator.Constants.DiagnosticSource, StringComparison.Ordinal))
				.Where(diagnostic => !string.IsNullOrWhiteSpace(diagnostic?.Data?.ToString()))
				.Select(diagnostic => new
				{
					diagnostic,
					edits = JsonSerializer.Deserialize<IReadOnlyDictionary<string, IReadOnlyList<TextEdit>>>(diagnostic.Data.ToString())
				})
				.SelectMany(
					diagnostic => diagnostic.edits.Select(kvp =>
					{
						var title = diagnostic.edits.Count > 1
							? $"{MoqGenerator.Constants.CodeActionFixTitle} for namespace {kvp.Key}"
							: MoqGenerator.Constants.CodeActionFixTitle
							;

						return new CommandOrCodeAction
						(
							new CodeAction
							{
								Title = title,
								Diagnostics = new [] { diagnostic.diagnostic },
								Kind = CodeActionKind.QuickFix,
								Edit = new WorkspaceEdit
								{
									Changes = new Dictionary<DocumentUri, IEnumerable<TextEdit>>
									{
										{
											request.TextDocument.Uri,
											kvp.Value
										}
									}
								}
							}
						);
					})
				)
				.ToArray() ?? Array.Empty<CommandOrCodeAction>();


			actions
				.SelectMany(a => a.CodeAction.Edit.Changes.Values.SelectMany(v => v))
				.ToList()
				.ForEach(e => _logger.LogInformation($"Requesting changes to '{request.TextDocument.Uri.ToString()}' at Start ({e.Range.Start.Line}, {e.Range.Start.Character}), End ({e.Range.End.Line}, {e.Range.End.Character})"))
				;

			return Task.FromResult(new CommandOrCodeActionContainer(actions));
		}
		#endregion ICodeActionHandler
	}
}