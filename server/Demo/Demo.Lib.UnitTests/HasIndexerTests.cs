using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;

namespace Demo.Lib.UnitTests
{
	public class HasIndexerTests
	{
		[Test]
		public void Go()
		{
			#warning: System and System.Linq.Expressions were added by MockText, but unnecessary!

			// this was generated:
			var hasIndexer = new Mock<IHasIndexer>();

			var hasIndexerStore = new Dictionary<string, string>();
			hasIndexer
				.Setup(x => x[It.IsAny<string>()])
				.Returns((string key) => hasIndexerStore[key]);
			hasIndexer
				.SetupSet(x => x[It.IsAny<string>()] = It.IsAny<string>())
				.Callback((string key, string value) => hasIndexerStore[key] = value);
			// END code generation
			
			var sut = hasIndexer.Object;
			sut["hello"] = "world";
			Assert.AreEqual("world", sut["hello"]);
		}

		[Test]
		public void GoNoSetter()
		{
			// this was generated:
			var hasIndexerNoSetter = new Mock<IHasIndexerNoSetter>();
			hasIndexerNoSetter
				.Setup(x => x[It.IsAny<string>()])
				.Returns((string key) => default);
			// END code generation

			var sut = hasIndexerNoSetter.Object;
			Assert.AreEqual(null, sut["foo"]);
		}

		[Test]
		public void GoListStringByLong()
		{
			// this was generated:
			var hasListStringByLongIndexer = new Mock<IHasListStringByLongIndexer>();

			var hasListStringByLongIndexerStore = new Dictionary<long, List<string>>();
			hasListStringByLongIndexer
				.Setup(x => x[It.IsAny<long>()])
				.Returns((long key) => hasListStringByLongIndexerStore[key]);
			hasListStringByLongIndexer
				.SetupSet(x => x[It.IsAny<long>()] = It.IsAny<List<string>>())
				.Callback((long key, List<string> value) => hasListStringByLongIndexerStore[key] = value);
			// END code generation

			var sut = hasListStringByLongIndexer.Object;
			var expected = new List<string> { "foo", "bar" };

			sut[1369] = expected;
			CollectionAssert.AreEquivalent(expected, sut[1369]);
		}
	}
}