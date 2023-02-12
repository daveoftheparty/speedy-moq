using System.Collections.Generic;

namespace MoqGenerator
{
	public static class Constants
	{

		public static string FileGlob = "**/*.cs";
		public static string LanguageId = "csharp";
		public static string DiagnosticSource = "SpeedyMoq";
		public static string DiagnosticCode_CanMoq = "SpeedyMoq001";
		public static string CodeActionFixTitle = "Generate Moq Setups";
		public static string DiagnosticMessage = "We can generate basic Moq code for you from this interface name!";
	}
}