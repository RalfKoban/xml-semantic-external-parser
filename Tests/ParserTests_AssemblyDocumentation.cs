using System;
using System.IO;
using System.Linq;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

using NUnit.Framework;

namespace MiKoSolutions.SemanticParsers.Xml
{
    [TestFixture]
    public class ParserTests_AssemblyDocumentation
    {
        private Yaml.File _objectUnderTest;
        private Yaml.Container _root;

        [SetUp]
        public void PrepareTest()
        {
            var parentDirectory = Directory.GetParent(new Uri(GetType().Assembly.Location).LocalPath).FullName;
            var fileName = Path.Combine(parentDirectory, "Resources", "AssemblyDocumentation.xml");

            _objectUnderTest = Parser.Parse(fileName);
            _root = _objectUnderTest.Children.Single();
        }

        [Test]
        public void File_Name_matches() => Assert.That(_objectUnderTest.Name, Does.EndWith(Path.DirectorySeparatorChar + "AssemblyDocumentation.xml"));

        [Test]
        public void File_LocationSpan_matches()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_objectUnderTest.LocationSpan.Start, Is.EqualTo(new LineInfo(1, 0)), "Wrong start");
                Assert.That(_objectUnderTest.LocationSpan.End, Is.EqualTo(new LineInfo(13, 6)), "Wrong end");

                Assert.That(_objectUnderTest.FooterSpan, Is.EqualTo(new CharacterSpan(0, -1)), "Wrong footer");
            });
        }

        [Test]
        public void Root_LocationSpan_matches()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_root.LocationSpan.Start, Is.EqualTo(new LineInfo(1, 1)), "Wrong start");
                Assert.That(_root.LocationSpan.End, Is.EqualTo(new LineInfo(13, 6)), "Wrong end");

                Assert.That(_root.HeaderSpan, Is.EqualTo(new CharacterSpan(0, 46)), "Wrong header");
                Assert.That(_root.FooterSpan, Is.EqualTo(new CharacterSpan(304, 309)), "Wrong footer");
            });
        }

        [TestCase("members", "member", "summary", 8, 1, 10, 18, 190, 274)]
        public void Element_matches(string groupName, string elementName, string name, int startLineNumber, int startLinePos, int endLineNumber, int endLinePos, int startPos, int endPos)
        {
            var terminalNodes = _root.Children
                                        .OfType<Container>().Where(_ => _.Type == groupName)
                                        .SelectMany(_ => _.Children).Where(_ => _.Type == elementName).OfType<Container>()
                                        .SelectMany(_ => _.Children).Where(_ => _.Type == name).OfType<TerminalNode>();

            var node = terminalNodes.FirstOrDefault();

            Assert.Multiple(() =>
            {
                Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(startLineNumber, startLinePos)), "Wrong start");
                Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(endLineNumber, endLinePos)), "Wrong end");

                Assert.That(node.Span, Is.EqualTo(new CharacterSpan(startPos, endPos)), "Wrong span");
            });
        }
    }
}