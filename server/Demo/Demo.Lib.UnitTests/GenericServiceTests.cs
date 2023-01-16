using System;
using System.Linq.Expressions;
using Moq;
using System.Collections.Generic;

using NUnit.Framework;
using Demo.Lib;

using TSource=System.Int32;
using TResult=System.Int32;

namespace Demo.Lib.UnitTests
{
	public class SUT 
	{
		private readonly IGenericService<char, int> _genericService;

		public SUT(IGenericService<char, int> genericService)
		{
			_genericService = genericService;
		}

		public void Go()
		{
			var results = _genericService.TransformSource(new[] { 'a', 'b', 'c'} );
			_genericService.Increment("foo", 13);
		}
	}

	public class GenericServiceTests
	{
		[Test]
		public void Go()
		{
			#region generated code
			var genericService = new Mock<IGenericService<char, int>>();

			Expression<Func<IGenericService<char, int>, IEnumerable<int>>> transformSource = x =>
				x.TransformSource(It.IsAny<IEnumerable<char>>());

			genericService
				.Setup(transformSource)
				.Returns((IEnumerable<char> items) =>
				{
					return default;
				});

			Expression<Action<IGenericService<char, int>>> increment = x =>
				x.Increment(It.IsAny<string>(), It.IsAny<int>());

			genericService
				.Setup(increment)
				.Callback((string name, int value) =>
				{
					return;
				});
			#endregion generated code

			var sut = new SUT(genericService.Object);
			sut.Go();

			#region generated code
			genericService.Verify(transformSource, Times.Once);
			genericService.Verify(increment, Times.Once);
			#endregion generated code
		}
	}
}