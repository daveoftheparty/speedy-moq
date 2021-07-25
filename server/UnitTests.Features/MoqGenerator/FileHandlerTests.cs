using NUnit.Framework;
using Features.MoqGenerator;
using System.Threading.Tasks;

namespace UnitTests.Features.MoqGenerator
{
	public class FileHandlerTests
	{
		[Test]
		public async Task FirstFileReturnsTrue()
		{
			var handler = new FileHandler();
			Assert.IsTrue(await handler.HasFileChangedAsync("somefile.txt", "abc"));
		}

		[Test]
		public async Task SameFileReturnsFalse()
		{
			var handler = new FileHandler();
			Assert.IsTrue(await handler.HasFileChangedAsync("somefile.txt", "abc"));
			Assert.IsFalse(await handler.HasFileChangedAsync("somefile.txt", "abc"));
		}

		[Test]
		public async Task DifferentFileSameTextReturnsTrue()
		{
			var handler = new FileHandler();
			Assert.IsTrue(await handler.HasFileChangedAsync("somefile.txt", "abc"));
			Assert.IsTrue(await handler.HasFileChangedAsync("other.txt", "abc"));
		}

		[Test]
		public async Task NewTextReturnsTrue()
		{
			var handler = new FileHandler();
			Assert.IsTrue(await handler.HasFileChangedAsync("somefile.txt", "abc"));
			Assert.IsTrue(await handler.HasFileChangedAsync("somefile.txt", "abc defg"));
		}
	}
}