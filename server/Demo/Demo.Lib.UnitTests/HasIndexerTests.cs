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
			/*
			// this is what was generated:
			var hasIndexer = new Mock<IHasIndexer>();
			hasIndexer
				.Setup(x => x.[It.IsAny<string>()])
				.Returns((string key) => return default);
			hasIndexer
				.SetupSet(x => x.[It.IsAny<string>()] = It.IsAny<string>())
				.Callback((string key, string value) => return);
			*/

			#warning: System and System.Linq.Expressions were added by MockText, but unnecessary!

			

			/*
			// this is legal c# and should be what we generate, maybe:

			var hasIndexer = new Mock<IHasIndexer>();
			hasIndexer
				.Setup(x => x[It.IsAny<string>()])
				.Returns((string key) => default);
			hasIndexer
				.SetupSet(x => x[It.IsAny<string>()] = It.IsAny<string>())
				.Callback((string key, string value) => {});
			
			// BUT!!! should we go ahead and if it has get and set, produce a backing dictionary?? might be cool...
			*/

			var store = new Dictionary<string, string>();
			
			var hasIndexer = new Mock<IHasIndexer>();
			hasIndexer
				.Setup(x => x[It.IsAny<string>()])
				.Returns((string key) => store[key]);
			hasIndexer
				.SetupSet(x => x[It.IsAny<string>()] = It.IsAny<string>())
				.Callback((string key, string value) => store[key] = value);

			var sut = hasIndexer.Object;
			sut["hello"] = "world";
			Assert.AreEqual("world", sut["hello"]);
		}
	}
}