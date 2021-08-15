using System;
using System.Collections.Generic;

namespace MoqGenerator.UnitTests.Utils
{
	public class DictionaryDumper
	{
		public static void DumpDictionaries(IReadOnlyDictionary<string, string> expected, IReadOnlyDictionary<string, string> actual)
		{
			Console.WriteLine("");
			Console.WriteLine("READABLE FAILURES:");
			Console.WriteLine("");

			foreach(var singleExpected in expected)
			{
				Console.WriteLine("expected " + singleExpected.Key);
				Console.WriteLine("========================");
				Console.WriteLine(singleExpected.Value);
				Console.WriteLine("========================");
			}

			foreach(var singleActual in actual)
			{
				Console.WriteLine("actual " + singleActual.Key);
				Console.WriteLine("========================");
				Console.WriteLine(singleActual.Value);
				Console.WriteLine("========================");
			}
		}
	}
}