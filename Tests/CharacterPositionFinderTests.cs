using System;
using System.IO;
using MiKoSolutions.SemanticParsers.Xml.Yaml;
using NUnit.Framework;

namespace MiKoSolutions.SemanticParsers.Xml
{
    [TestFixture]
    public sealed class CharacterPositionFinderTests
    {
        private CharacterPositionFinder _objectUnderTest;

        [SetUp]
        public void PrepareTest()
        {
            var parentDirectory = Directory.GetParent(new Uri(GetType().Assembly.Location).LocalPath).FullName;
            var fileName = Path.Combine(parentDirectory, "test.xml");

            _objectUnderTest = CharacterPositionFinder.CreateFrom(fileName);
        }

        [TestCase(3, 10, ExpectedResult = 63)]
        [TestCase(18, 12, ExpectedResult = 359)]
        public int GetCharacterPosition(int lineNumber, int linePosition) => _objectUnderTest.GetCharacterPosition(new LineInfo(lineNumber, linePosition));

        [TestCase(3, 10, ExpectedResult = 26)]
        [TestCase(18, 12, ExpectedResult = 12)]
        public int GetLineLength(int lineNumber, int linePosition) => _objectUnderTest.GetLineLength(new LineInfo(lineNumber, linePosition));
    }
}