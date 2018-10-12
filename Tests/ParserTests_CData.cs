using System;
using System.IO;
using System.Linq;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

using NUnit.Framework;

namespace MiKoSolutions.SemanticParsers.Xml
{
    [TestFixture]
    public class ParserTests_CData
    {
        private Yaml.File _objectUnderTest;
        private Yaml.Container _root;

        [SetUp]
        public void PrepareTest()
        {
            var parentDirectory = Directory.GetParent(new Uri(GetType().Assembly.Location).LocalPath).FullName;
            var fileName = Path.Combine(parentDirectory, "Resources", "test_with_CDATA.xml");

            _objectUnderTest = Parser.Parse(fileName);
            _root = _objectUnderTest.Children.Single();
        }

        [Test]
        public void File_Name_matches() => Assert.That(_objectUnderTest.Name, Does.EndWith(Path.DirectorySeparatorChar + "test_with_CDATA.xml"));

        [Test]
        public void File_LocationSpan_matches()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_objectUnderTest.LocationSpan.Start, Is.EqualTo(new LineInfo(1, 0)), "Wrong start");
                Assert.That(_objectUnderTest.LocationSpan.End, Is.EqualTo(new LineInfo(11, 7)), "Wrong end");

                Assert.That(_objectUnderTest.FooterSpan, Is.EqualTo(CharacterSpan.None), "Wrong footer");
            });
        }

        [Test]
        public void Root_LocationSpan_matches()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_root.LocationSpan.Start, Is.EqualTo(new LineInfo(1, 1)), "Wrong start");
                Assert.That(_root.LocationSpan.End, Is.EqualTo(new LineInfo(11, 7)), "Wrong end");

                Assert.That(_root.HeaderSpan, Is.EqualTo(new CharacterSpan(0, 48)), "Wrong header");
                Assert.That(_root.FooterSpan, Is.EqualTo(new CharacterSpan(215, 221)), "Wrong footer");
            });
        }

        [Test]
        public void First_Level1_matches()
        {
            var node = _root.Children.First(_ => _.Name == "level1") as Container;

            Assert.Multiple(() =>
            {
                Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(3, 1)), "Wrong start");
                Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(5, 13)), "Wrong end");

                Assert.That(node.HeaderSpan, Is.EqualTo(new CharacterSpan(49, 60)), "Wrong header");
                Assert.That(node.FooterSpan, Is.EqualTo(new CharacterSpan(99, 111)), "Wrong footer");
            });
        }

        [Test]
        public void First_CData_matches()
        {
            var node = _root.Children.OfType<Container>().SelectMany(_ => _.Children).First(_ => _.Type == NodeType.CDATA) as TerminalNode;

            Assert.Multiple(() =>
            {
                Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(4, 1)), "Wrong start");
                Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(4, 38)), "Wrong end");

                Assert.That(node.Span, Is.EqualTo(new CharacterSpan(61, 98)), "Wrong span");
            });
        }

        [Test]
        public void Second_Level1_matches()
        {
            var node = _root.Children.Last(_ => _.Name == "level1") as Container;

            Assert.Multiple(() =>
            {
                Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(6, 1)), "Wrong start");
                Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(10, 13)), "Wrong end");

                Assert.That(node.HeaderSpan, Is.EqualTo(new CharacterSpan(112, 123)), "Wrong header");
                Assert.That(node.FooterSpan, Is.EqualTo(new CharacterSpan(202, 214)), "Wrong footer");
            });
        }

        [Test]
        public void First_Level2_matches()
        {
            var node = _root.Children.OfType<Container>().SelectMany(_ => _.Children).First(_ => _.Name == "level2") as Container;

            Assert.Multiple(() =>
            {
                Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(7, 1)), "Wrong start");
                Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(9, 18)), "Wrong end");

                Assert.That(node.HeaderSpan, Is.EqualTo(new CharacterSpan(124, 135)), "Wrong header");
                Assert.That(node.FooterSpan, Is.EqualTo(new CharacterSpan(191, 201)), "Wrong footer");
            });
        }

        [Test]
        public void Last_CData_matches()
        {
            var node = _root.Children.OfType<Container>().SelectMany(_ => _.Children).OfType<Container>().SelectMany(_ => _.Children).First(_ => _.Type == NodeType.CDATA) as TerminalNode;

            Assert.Multiple(() =>
            {
                Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(7, 13)), "Wrong start");
                Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(9, 7)), "Wrong end");

                Assert.That(node.Span, Is.EqualTo(new CharacterSpan(136, 190)), "Wrong span");
            });
        }
    }
}