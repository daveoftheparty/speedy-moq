using System.Collections.Generic;

namespace Features
{
	public static class Constants
	{
		public static string LanguageId = "csharp";
		public static string DiagnosticSource = "BoilerMoq";
		public static string DiagnosticCode_CanMoq = "BoilerMoq001";

		public static Dictionary<string, string> MessagesByDiagnosticCode = new()
		{
			{
				DiagnosticCode_CanMoq,
				"We can generate basic Moq code for you from this interface name!"
			},
		};
		
	}
}