using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

using NUnit.Framework;
using Moq;

using Features.Interfaces;
using Features.Model;
using Features.MoqGenerator;

namespace UnitTests.Features.MoqGenerator
{
	public class GeneratorLogicTests
	{
		[Test]
		public async Task NoFileChange()
		{
			var fileHandler = GeneratorMocks.GetFileHandlerMock(false);

			var generator = new Generator(fileHandler.mock.Object);
			var result = await generator.GetReplacementAsync("a.txt", "abc");
			
			Assert.IsFalse(result.IsChanged);
			fileHandler.mock.Verify(fileHandler.hasFileChanged, Times.Once);
		}
	}
}