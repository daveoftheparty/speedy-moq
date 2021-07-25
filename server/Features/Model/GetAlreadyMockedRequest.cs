using System.Collections.Generic;
using Features.Model.Lsp;

namespace Features.Model
{
	#warning this class might be deprecated....
	public class GetAlreadyMockedRequest
	{
		public IEnumerable<InterfaceName> InterfaceNames { get; set; }
		public TextDocumentItem TextDocument { get; set; }
	}
}