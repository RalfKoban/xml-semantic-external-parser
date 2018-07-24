using System;
using System.IO;
using System.Linq;
using System.Text;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

using NUnit.Framework;

namespace MiKoSolutions.SemanticParsers.Xml
{
    [TestFixture]
    public class ParserTests
    {
        private Yaml.File _objectUnderTest;
        private Yaml.Container _root;

        [SetUp]
        public void PrepareTest()
        {
            var parentDirectory = Directory.GetParent(new Uri(GetType().Assembly.Location).LocalPath).FullName;
            var fileName = Path.Combine(parentDirectory, "test.xml");

            _objectUnderTest = Parser.Parse(fileName);
            _root = _objectUnderTest.Children.Single();
        }

        [Test]
        public void File_Name_matches()
        {
            Assert.That(_objectUnderTest.Name, Does.EndWith(Path.DirectorySeparatorChar + "test.xml"));
        }

        [Test]
        public void File_LocationSpan_matches()
        {
            Assert.That(_objectUnderTest.LocationSpan.Start, Is.EqualTo(new LineInfo(1, 0)));
            Assert.That(_objectUnderTest.LocationSpan.End, Is.EqualTo(new LineInfo(28, 0)));

            Assert.That(_objectUnderTest.FooterSpan, Is.EqualTo(new CharacterSpan(481, 482)));
        }

        [Test]
        public void Root_LocationSpan_matches()
        {
            Assert.That(_root.LocationSpan.Start, Is.EqualTo(new LineInfo(1, 1)));
            Assert.That(_root.LocationSpan.End, Is.EqualTo(new LineInfo(27, 12)));

            Assert.That(_root.HeaderSpan, Is.EqualTo(new CharacterSpan(0, 53)));
            Assert.That(_root.FooterSpan, Is.EqualTo(new CharacterSpan(469, 480)));
        }

        [Test]
        public void First_Comment_LocationSpan_and_Span_matches()
        {
            var node = _root.Children.OfType<TerminalNode>().First(_ => _.Type == "Comment");

            Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(3, 1)));
            Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(3, 26)));

            Assert.That(node.Span, Is.EqualTo(new CharacterSpan(54, 79)));
        }

        [Test]
        public void Last_Comment_LocationSpan_and_Span_matches()
        {
            var node = _root.Children.OfType<TerminalNode>().Last(_ => _.Type == "Comment");

            Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(23, 1)));
            Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(26, 2)));

            Assert.That(node.Span, Is.EqualTo(new CharacterSpan(437, 468)));
        }

        [Test]
        public void ProcessingInstruction_LocationSpan_matches()
        {
            var node = _root.Children.OfType<TerminalNode>().First(_ => _.Type == "ProcessingInstruction");

            Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(4, 1)));
            Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(4, 24)));

            Assert.That(node.Span, Is.EqualTo(new CharacterSpan(80, 103)));
        }

        [TestCase("first",    5, 1,  5, 34, 104, 127, 128, 137)]
        [TestCase("second",   6, 1,  6, 37, 138, 163, 164, 174)]
        [TestCase("third",    7, 1,  7, 54, 175, 183, 219, 228)]
        [TestCase("forth",    8, 1, 10, 12, 229, 239, 240, 253)]
        [TestCase("fifth",   11, 1, 11, 13, 254, 262, 263, 266)]
        [TestCase("sixth",   12, 1, 18, 12, 267, 277, 350, 361)]
        [TestCase("seventh", 19, 1, 22, 25, 362, 374, 425, 436)]
        public void First_level_element_LocationSpan_matches(string name, int startLine, int startPos, int endLine, int endPos, int headerStartPos, int headerEndPos, int footerStartPos, int footerEndPos)
        {
            var node = _root.Children.OfType<Container>().First(_ => _.Name == name);

            Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(startLine, startPos)), "Wrong start");
            Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(endLine, endPos)), "Wrong end");

            Assert.That(node.HeaderSpan, Is.EqualTo(new CharacterSpan(headerStartPos, headerEndPos)), "wrong header span");
            Assert.That(node.FooterSpan, Is.EqualTo(new CharacterSpan(footerStartPos, footerEndPos)), "wrong footer span");
        }

        [TestCase("third",   "nested",  7, 10,  7, 44, 184, 192, 209, 218)]
        [TestCase("sixth",   "nested", 13,  1, 17, 15, 278, 291, 335, 349)]
        [TestCase("seventh", "nested", 20,  1, 22, 13, 375, 388, 412, 424)]
        public void Second_level_element_LocationSpan_matches(string parentName, string name, int startLine, int startPos, int endLine, int endPos, int headerStartPos, int headerEndPos, int footerStartPos, int footerEndPos)
        {
            var node = _root.Children.OfType<Container>().First(_ => _.Name == parentName).Children.First(_ => _.Name == name);

            Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(startLine, startPos)));
            Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(endLine, endPos)));

            if (node is Container cNode)
            {
                Assert.That(cNode.HeaderSpan, Is.EqualTo(new CharacterSpan(headerStartPos, headerEndPos)), "wrong header span");
                Assert.That(cNode.FooterSpan, Is.EqualTo(new CharacterSpan(footerStartPos, footerEndPos)), "wrong footer span");
            }
            else if (node is TerminalNode tNode)
            {
                Assert.That(tNode.Span, Is.EqualTo(new CharacterSpan(headerStartPos, footerEndPos)), "wrong span");
            }
        }

        [Test, Explicit]
        public void RoundTrip()
        {
            var builder = new StringBuilder();
            using (var writer = new StringWriter(builder))
            {
                YamlWriter.Write(writer, _objectUnderTest);
            }

            Assert.Fail(builder.ToString());
        }
    }
}
