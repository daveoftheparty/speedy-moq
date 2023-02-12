using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using MoqGenerator.Services;
using MoqGenerator.Model.Lsp;
using MoqGenerator.UnitTests.Utils;

namespace MoqGenerator.UnitTests;

public class CodeActionStoreTests
{
	[Test]
	public void RetrievedUnderThreeMinutesDoesNotTrim()
	{
		var now = DateTime.UtcNow.AddMinutes(-5);
		var logMock = new LoggerDouble<CodeActionStore>();

		var store = new CodeActionStore(logMock, now);
		var key = store.StoreAction(Payloads);
		store.GetAction(key);

		// assert
		Assert.False(logMock.LogEntries.Any(l => l.ToString().StartsWith("removing")));
		// should be able to get the key again:
		store.GetAction(key);
	}
	
	[Test]
	public void RetrievedOverThreeMinutesTrims()
	{
		var now = DateTime.UtcNow.AddMinutes(5);
		var logMock = new LoggerDouble<CodeActionStore>();

		var store = new CodeActionStore(logMock, now);
		var key = store.StoreAction(Payloads);
		store.GetAction(key);

		// assert
		Assert.True(logMock.LogEntries.Any(l => l.ToString().StartsWith("removing")));
		// should not be able to get the key again:
		Assert.Throws<KeyNotFoundException>(() => store.GetAction(key));
	}

	[Test]
	public void NotRetrievedUnderFifteenMinutesDoesNotTrim()
	{
		var now = DateTime.UtcNow.AddMinutes(-13);
		var logMock = new LoggerDouble<CodeActionStore>();

		var store = new CodeActionStore(logMock, now);
		var key = store.StoreAction(Payloads);

		// assert
		Assert.False(logMock.LogEntries.Any(l => l.ToString().StartsWith("removing")));
		// should be able to get the key:
		store.GetAction(key);
	}
	
	[Test]
	public void NotRetrievedOverFifteenMinutesTrims()
	{
		var now = DateTime.UtcNow.AddMinutes(16);
		var logMock = new LoggerDouble<CodeActionStore>();

		var store = new CodeActionStore(logMock, now);
		var key = store.StoreAction(Payloads);

		// assert
		Assert.True(logMock.LogEntries.Any(l => l.ToString().StartsWith("removing")));
		// should not be able to get the key:
		Assert.Throws<KeyNotFoundException>(() => store.GetAction(key));
	}

	private static Dictionary<string, IReadOnlyList<TextEdit>> Payloads = new()
	{
		{
			"foo",
			new List<TextEdit>
			{
				new TextEdit(new Model.Lsp.Range(new Position(0, 0), new Position(0, 0)), "bar")
			}
		}
	};


}