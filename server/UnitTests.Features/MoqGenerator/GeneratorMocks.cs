using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Features.Interfaces;
using Moq;

namespace UnitTests.Features.MoqGenerator
{
	public class GeneratorMocks
	{
		public static (Mock<IFileHandler> mock, Expression<Func<IFileHandler, Task<bool>>> hasFileChanged)
		GetFileHandlerMock(bool returns)
		{
			var fileHandler = new Mock<IFileHandler>();
			Expression<Func<IFileHandler, Task<bool>>> hasFileChanged = x => x.HasFileChangedAsync(It.IsAny<string>(), It.IsAny<string>());
			fileHandler
				.Setup(hasFileChanged)
				.Returns(Task.FromResult(returns));

			return (fileHandler, hasFileChanged);
		}
	}
}