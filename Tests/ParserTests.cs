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

            Assert.That(_objectUnderTest.FooterSpan, Is.EqualTo(new CharacterSpan(479, 480)));
        }

        [Test]
        public void Root_LocationSpan_matches()
        {
            Assert.That(_root.LocationSpan.Start, Is.EqualTo(new LineInfo(1, 1)));
            Assert.That(_root.LocationSpan.End, Is.EqualTo(new LineInfo(27, 12)));

            Assert.That(_root.HeaderSpan, Is.EqualTo(new CharacterSpan(0, 51)));
            Assert.That(_root.FooterSpan, Is.EqualTo(new CharacterSpan(467, 478)));
        }

        [Test]
        public void First_Comment_LocationSpan_matches()
        {
            var node = _root.Children.First(_ => _.Type == "Comment");

            Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(3, 3)));
            Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(3, 24)));
        }

        [Test]
        public void Last_Comment_LocationSpan_matches()
        {
            var node = _root.Children.Last(_ => _.Type == "Comment");

            Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(25, 3)));
            Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(25, 20)));
        }

        [Test]
        public void ProcessingInstruction_LocationSpan_matches()
        {
            var node = _root.Children.First(_ => _.Type == "ProcessingInstruction");

            Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(4, 3)));
            Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(4, 22)));
        }

        [TestCase("first", 5, 3, 5, 32, 106, 127, 128, 135)]
        [TestCase("second", 6, 3, 6, 35, 140, 162, 164, 172)]
        [TestCase("third", 7, 3, 7, 50, 177, 183, 217, 224)]
        [TestCase("forth", 8, 3, 10, 10, 229, 235, 242, 249)]
        [TestCase("fifth", 11, 3, 11, 11, 254, 262, 0, -1)]
        [TestCase("sixth", 12, 3, 18, 10, 267, 273, 350, 357)]
        [TestCase("seventh", 19, 3, 22, 23, 362, 370, 423, 432)]
        public void First_level_element_LocationSpan_matches(string name, int startLine, int startPos, int endLine, int endPos, int headerStartPos, int headerEndPos, int footerStartPos, int footerEndPos)
        {
            var node = _root.Children.OfType<Container>().First(_ => _.Name == name);

            Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(startLine, startPos)));
            Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(endLine, endPos)));

            Assert.That(node.HeaderSpan, Is.EqualTo(new CharacterSpan(headerStartPos, headerEndPos)));
            Assert.That(node.FooterSpan, Is.EqualTo(new CharacterSpan(footerStartPos, footerEndPos)));
        }

        [TestCase("third", "nested", 7, 10, 7, 42)]
        [TestCase("sixth", "nested", 13, 5, 17, 13)]
        [TestCase("seventh", "nested", 20, 5, 22, 13)]
        public void Second_level_element_LocationSpan_matches(string parentName, string name, int startLine, int startPos, int endLine, int endPos)
        {
            var node = _root.Children.OfType<Container>().First(_ => _.Name == parentName).Children.First(_ => _.Name == name);

            Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(startLine, startPos)));
            Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(endLine, endPos)));
        }

        [Test, Ignore("Just to get the string")]
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
