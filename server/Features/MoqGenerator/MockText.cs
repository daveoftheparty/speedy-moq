using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Features.Interfaces;
using Features.Model;
using Features.Model.Lsp;

namespace Features.MoqGenerator
{
	public class MockText : IMockText
	{
		// private readonly Dictionary<TextDocumentIdentifier, HashSet<InterfaceName>> _alreadyMocked = new();

		// public async Task<GetMockTextResponse> GetMockTextAsync(TextDocumentItem item)
		// {
		// 	return await Task.FromResult(new GetMockTextResponse());
		// }

		public async Task<string> GetMockTextAsync(TextDocumentIdentifier textDocumentIdentifier, DiagnosticData data)
		{
			return await Task.FromResult("hey, how YOU doin??");
		}
		

		#region IMockText

		// public async Task<GetInterfaceNamesResponse> GetInterfaceNamesAsync(FileText fileText)
		// {
		// 	/*
		// 	this is going to be the hardest to implement; will need to scan/compile the whole project to find the interface
		// 	not only that, it might be in a related project, and not the project our file is even in...
		// 	don't just hack at this for a real implementation, google how to do this with Roslyn... it's a solved problem
		// 	*/
		// 	throw new System.NotImplementedException();
		// }

		// public async Task<GetMockTextResponse> GetMockTextAsync(GetMockTextRequest request)
		// {
		// 	/*
		// 		check our internal dict to see if already mocked, if not in our dict, calc
		// 		whether or not the mock body has already been implemented / generated

		// 		can be as simple as does the text
		// 			var _ = new Mock<ourInterfaceName>();
		// 		exist for now
		// 	*/
		// 	var result = new GetMockTextResponse();


		// 	if(!_alreadyMocked.TryGetValue(request.TextDocument.Identifier, out var mocks))
		// 	{
		// 		_alreadyMocked.Add(request.TextDocument.Identifier, mocks);
		// 	}

		// 	result.Replacements = request
		// 		.InterfaceNames
		// 		.Where(i => !mocks.Contains(i))
		// 		.Select(g => GenerateAndAddToDict(mocks, g, request.TextDocument.Text))
		// 		;

		// 	throw new System.NotImplementedException();
		// }

		#endregion IMockText

		// private MockTextReplacement GenerateAndAddToDict(HashSet<InterfaceName> mocks, InterfaceName interfaceName, string text)
		// {
		// 	mocks.Add(interfaceName);

		// 	/*
		// 		GetInterfaceDefinition() // this will return a DTO
		// 		generate text
		// 	*/
		// 	return new MockTextReplacement { InterfaceName = interfaceName, NewText = text };
		// }
	}
}