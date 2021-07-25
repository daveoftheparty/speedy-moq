using NUnit.Framework;
using Features.MoqGenerator;

namespace UnitTests.Features.MoqGenerator
{
	public class FileHandlerTests
	{
		[Test]
		public void FirstFileReturnsTrue()
		{
			var handler = new FileHandler();
			Assert.IsTrue(handler.HasFileChanged("somefile.txt", "abc"));
		}

		[Test]
		public void SameFileReturnsFalse()
		{
			var handler = new FileHandler();
			Assert.IsTrue(handler.HasFileChanged("somefile.txt", "abc"));
			Assert.IsFalse(handler.HasFileChanged("somefile.txt", "abc"));
		}

		[Test]
		public void DifferentFileSameTextReturnsTrue()
		{
			var handler = new FileHandler();
			Assert.IsTrue(handler.HasFileChanged("somefile.txt", "abc"));
			Assert.IsTrue(handler.HasFileChanged("other.txt", "abc"));
		}

		[Test]
		public void NewTextReturnsTrue()
		{
			var handler = new FileHandler();
			Assert.IsTrue(handler.HasFileChanged("somefile.txt", "abc"));
			Assert.IsTrue(handler.HasFileChanged("somefile.txt", "abc defg"));
		}
	}
}