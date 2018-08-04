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
            var fileName = Path.Combine(parentDirectory, "Resources", "test.xml");

            _objectUnderTest = CharacterPositionFinder.CreateFrom(fileName);
        }

        [TestCase(3, 10, ExpectedResult = 63)]
        [TestCase(18, 12, ExpectedResult = 361)]
        public int GetCharacterPosition(int lineNumber, int linePosition) => _objectUnderTest.GetCharacterPosition(new LineInfo(lineNumber, linePosition));

        [TestCase(3, 10, ExpectedResult = 63)]
        [TestCase(18, 12, ExpectedResult = 361)]
        public int GetCharacterPosition_raw(int lineNumber, int linePosition) => _objectUnderTest.GetCharacterPosition(lineNumber, linePosition);

        [TestCase(3, ExpectedResult = 26)]
        [TestCase(18, ExpectedResult = 12)]
        public int GetLineLength(int lineNumber) => _objectUnderTest.GetLineLength(lineNumber);

        [TestCase(3, 10, ExpectedResult = 26)]
        [TestCase(18, 12, ExpectedResult = 12)]
        [TestCase(28, 5, ExpectedResult = 14)]
        [TestCase(29, 1, ExpectedResult = 0)]
        public int GetLineLength(int lineNumber, int linePosition) => _objectUnderTest.GetLineLength(new LineInfo(lineNumber, linePosition));

        [TestCase(3, 10, 63)]
        [TestCase(18, 12, 361)]
        public void GetLineInfo(int lineNumber, int linePosition, int characterPosition) => Assert.That(_objectUnderTest.GetLineInfo(characterPosition), Is.EqualTo(new LineInfo(lineNumber, linePosition)));
    }
}