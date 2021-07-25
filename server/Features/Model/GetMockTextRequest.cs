using System.Collections.Generic;
using Features.Model.Lsp;

namespace Features.Model
{
	public class GetMockTextRequest
	{
		public IEnumerable<InterfaceName> InterfaceNames { get; set; }
		public TextDocumentItem TextDocument { get; set; }
	}
}