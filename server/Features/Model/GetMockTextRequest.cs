using System.Collections.Generic;

namespace Features.Model
{
	public class GetMockTextRequest
	{
		public IEnumerable<InterfaceName> InterfaceNames { get; set; }
		public FileText FileText { get; set; }
	}
}