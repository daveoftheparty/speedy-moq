using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Reflection;

using NUnit.Framework;
using MoqGenerator.UnitTests.Utils;

using MoqGenerator.Services;

namespace MoqGenerator.UnitTests
{
	public class ProjectHandlerTests
	{
		[TestCaseSource(nameof(GetTestInputs))]
		public void Go((string testIdMessage, string rootCsProj, IReadOnlyList<string> expectedProjects) tests)
		{
			var logger = new LoggerDouble<ProjectHandler>();
			var handler = new ProjectHandler(logger);

			var testProjectsPath = GetTestProjectsPath();
			CollectionAssert.AreEquivalent(
				tests.expectedProjects.Select(e => testProjectsPath + e),
				handler.GetProjectAndProjectReferences(tests.rootCsProj),
				tests.testIdMessage);
		}

		public static string GetTestProjectsPath()
		{
			var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			// basePath should be something like **/server/MoqGenerator.UnitTests/** where the last glob is maybe bin/Debug/net5.0 or whatever

			var tokens = basePath.Split(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
			var unitTestPath = tokens.TakeUntil(t => t == "MoqGenerator.UnitTests");

			return
				string.Join(Path.DirectorySeparatorChar, unitTestPath) +
				Path.DirectorySeparatorChar +
				"TestProjects" + 
				Path.DirectorySeparatorChar;
		}

		public static IEnumerable<(string testIdMessage, string rootCsProj, IReadOnlyList<string> expectedProjects)> GetTestInputs()
		{
			/*
				need to build an absolute path to the TestProjects path,
				then read folder-by-folder,
				expecting a root.txt and expected.json in each top level folder,
				where root.txt contains the filename only of the csproj that starts the project ref recursion

				because different clones of this repo will have different base paths, even though the outcome
				of IProjectHandler.GetProjectAndProjectReferences() is absolute paths, what's in root.txt and expected.json
				is actually relative paths
			*/
			var testProjectsPath = GetTestProjectsPath();

			var result = Directory
				.EnumerateDirectories(testProjectsPath)
				// .Where(dir => dir.Contains("LegacyFrameworkProjectWithReference"))
				// .Where(dir => dir.Contains("FlatReferences"))
				.ToList();

			foreach(var dir in result)
			{

				var testIdMessage = new DirectoryInfo(dir).Name;
				var fileReadPath = dir + Path.DirectorySeparatorChar;

				var rootProj = fileReadPath + File.ReadAllText(fileReadPath + "root.txt").Trim();
				var expected = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(fileReadPath + "expected.json"));
				yield return
				(
					testIdMessage,
					rootProj,
					expected
				);
			}
		}
	}
}