using System.Threading.Tasks;
using Features.Model;

namespace Features.Interfaces
{
	public interface IMockText
	{
		Task<GetInterfaceNamesResponse> GetInterfaceNamesAsync(FileText fileText);
		// Task<bool> AlreadyMockedAsync(GetAlreadyMockedRequest request);
		Task<GetMockTextResponse> GetMockTextAsync(GetMockTextRequest request);
	}
}