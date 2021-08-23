using System.Threading;

using Moq;
using NUnit.Framework;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using MoqGenerator.UnitTests.Utils;
using MoqGenerator.Interfaces.Lsp;

using OmniRange = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace MoqGenerator.Lsp.UnitTests
{
	public class CodeActionHandlerTests
	{
		[Test]
		public void NullDiagnosticSourceShouldNotThrow()
		{
			var request = new CodeActionParams
			{
				TextDocument = new TextDocumentIdentifier
				{
					Uri = "file:///c%3A/src/daveoftheparty/speedy-moq/server/MoqGenerator.UnitTests/ThrowExceptionLocally.cs"
				},
				Range = new OmniRange
				{
					Start = new Position(9, 0),
					End = new Position(9, 10000)
				},
				Context = new CodeActionContext
				{
					Diagnostics = new[] {
						new Diagnostic
						{
							Range = new OmniRange
							{
								Start = new Position(9, 0),
								End = new Position(9, 10000)
							},
							Message = "  String lengths are both 5. Strings differ at index 0.\r\n  Expected: \"howdy\"\r\n  But was:  \"doody\"\r\n  -----------^\r\n",
							Severity = DiagnosticSeverity.Error
						}
					},
					Only = new Container<CodeActionKind>(new CodeActionKind(CodeActionKind.QuickFix))
				}
			};

			var logger = new LoggerDouble<CodeActionHandler>();
			var whoaCowboy = new Mock<IWhoaCowboy>();
			whoaCowboy.SetupGet(x => x.GiddyUp).Returns(true);

			var handler = new CodeActionHandler(logger, whoaCowboy.Object);
			Assert.DoesNotThrowAsync(async () => await handler.Handle(request, CancellationToken.None));
		}
	}
}