using System.Threading.Tasks;
using Features.Model;
using Features.Model.Lsp;

namespace Features.Interfaces
{
	public interface IMockText
	{
		Task<GetMockTextResponse> GetMockTextAsync(TextDocumentItem item);
	}
}