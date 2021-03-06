using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using MoqGenerator.Interfaces.Lsp;
using MoqGenerator.Model.Lsp;
using MoqGenerator.Util;

namespace MoqGenerator.Services
{
	public class Indentation : IIndentation
	{
		private readonly ILogger<Indentation> _logger;

		public Indentation(ILogger<Indentation> logger)
		{
			_logger = logger;
		}

		public IndentationConfig GetIndentationConfig(string text, uint currentLine)
		{
			var watch = new Stopwatch();
			watch.Start();

			var splitter = new TextLineSplitter();
			
			var leadingWhiteSpaceByLine = splitter
				.SplitToLines(text)
				.Select((string line, int index) => new
				{
					LineNumber = (uint)index,
					LeadingWhitespaceChars = GetLeadingWhitespaceChars(line)
				})
				.ToDictionary(pair => pair.LineNumber, pair => pair.LeadingWhitespaceChars)
				;

			// find the most tab style (tab vs. spaces) in the doc,
			// purely by how many lines use one versus the other
			var findStyle = leadingWhiteSpaceByLine
				.Values
				.Where(lwsc => lwsc.isValid && lwsc.count > 0)
				.GroupBy(tabStyle => tabStyle.tabChar)
				.Select(batch => new
				{
					Count = batch.Count(),
					TabChar = batch.Key
				})
				.OrderByDescending(lineCount => lineCount.Count)
				.ThenBy(tabsAreBetter => tabsAreBetter.TabChar) // ascii tab is 9, less than ascii space 32, so tabs win if we have a tie, because they're better
				.FirstOrDefault();

			int currentLevel = 4;
			if(findStyle == null)
			{
				watch.StopAndLogInformation(_logger, $"indent character not clear; defaulting to tab at level {currentLevel}: ");
				return new IndentationConfig(currentLevel, "\t", true);
			}

			var userTabStyle = findStyle.TabChar;

			// if the tabChar is a tab, our work here is done, because
			// this person understands that an indentation is created by
			// a tab, rather than a sequence of generated spaces
			if(userTabStyle == '\t')
			{
				currentLevel = leadingWhiteSpaceByLine[currentLine].count;
				watch.StopAndLogInformation(_logger, $"tab detected as indent character, current level {currentLevel} in: ");
				return new IndentationConfig(currentLevel, "\t", false);
			}



			// oh boy. we have one of "those guys" that went to
			// "I don't understand a tab" school. 

			var allFakeTabsForLogging = leadingWhiteSpaceByLine
				.Values
				.Where(lwsc => lwsc.isValid && lwsc.count > 0 && lwsc.tabChar == ' ')
				.Select(justCount => justCount.count)
				.GroupBy(theCount => theCount)
				.Select(stillJustTheCount => stillJustTheCount.Key)
				.OrderBy(whelpStillJustTheCountHere => whelpStillJustTheCountHere)
				.ToList()
				;
			
			_logger.LogInformation($"FakeTabCounts = ({string.Join(", ", allFakeTabsForLogging)})");

			// I originally grabbed distinct counts, think it was going to be some
			// sort of semi-complicated algorithm like if max(count) % min(count) == 0 and
			// min(count) != 1 or some nonsense, but then realized I'd have to throw out
			// outliers and still this is a whole lot of work for a person that can't 
			// indent properly, so even if I ended up with the counts {1, 4, 8, 12, 16, 17}
			// I wouldn't be able to tell without using some kinda weird analysis
			// that the 4 was the winner... so... min count == 4 should cover people who tab
			// {4, 8, 12, 16, ...} and should also cover everything else like {9, 18, 27, 36, ...}

			var fakes = CalculateFakeTabStop(allFakeTabsForLogging, leadingWhiteSpaceByLine[currentLine].count);

			var fakeTabCount = fakes.tabSize;
			currentLevel = fakes.currentIndentLevel;

			watch.StopAndLogInformation(_logger, $"spaces detected as indent 'character', current level {currentLevel} in: ");
			return new IndentationConfig(
				currentLevel,
				new string(Enumerable.Repeat(' ', fakeTabCount).ToArray()),
				true
				);
		}

		public (int tabSize, int currentIndentLevel) CalculateFakeTabStop(List<int> allFakeTabsForLogging, int currentLineSpaceCount)
		{
			if(allFakeTabsForLogging == null || allFakeTabsForLogging.Count == 0)
			{
				_logger.LogError($"invalid fakeTab list, can't calculate a proper tab stop, defaulting to a tab stop = 0 spaces and current level is 0");
				return (0, 0);
			}

			// list is pre-ordered
			List<int> workingList;
			if(allFakeTabsForLogging[0] == 1)
				workingList = allFakeTabsForLogging.Skip(1).ToList();
			else
				workingList = allFakeTabsForLogging;


			var candidates = new List<(int divisor, int matches)>();
			for(var i = 0; i < workingList.Count; i++)
			{
				var currentDivisor = workingList[i];
				var matches = 0;
				for(var k = i; k < workingList.Count; k++)
				{
					if(workingList[k] % currentDivisor == 0)
						matches++;
				}
				candidates.Add((currentDivisor, matches));
			}

			var bestMatch = candidates
				.OrderByDescending(matches => matches.matches)
				.First();

			var tabSize = bestMatch.divisor;
			var currentIndentLevel = (int)Math.Ceiling(currentLineSpaceCount / (double)tabSize);

			return
			(
				tabSize,
				currentIndentLevel
			);
		}

		private (int count, bool isValid, char tabChar) GetLeadingWhitespaceChars(string line)
		{
			var foundTabs = false;
			var foundSpaces = false;
			var foundNonWhiteSpace = false;

			var i = 0;
			for( ; i < line.Length; i++)
			{
				if(line[i] == '\t')
					foundTabs = true;
				else if(line[i] == ' ')
					foundSpaces = true;
				else
				{
					foundNonWhiteSpace = true;
					break;
				}
			}

			// this is basically here to protect against counting indents on lines that are otherwise empty:
			if(!foundTabs && !foundSpaces && !foundNonWhiteSpace)
				return (0, false, '\0');

			// explicitly ignore any lines that mix tabs and spaces as leading
			// characters, because it's already a fact that the engineer that wrote
			// that has no clue about how indentation is supposed to work
			var developerIsAnIdiot = foundTabs && foundSpaces;

			return
			(
				i,
				!developerIsAnIdiot,
				i == 0 ? '\0' : (foundTabs ? '\t' : ' ')
			);
		}
	}
}