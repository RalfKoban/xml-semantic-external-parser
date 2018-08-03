using System;
using System.IO;
using System.Linq;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

using NUnit.Framework;

namespace MiKoSolutions.SemanticParsers.Xml
{
    [TestFixture("test_with_Unix_LineEnd.dat")]
    [TestFixture("test_with_Macintosh_LineEnd.dat")]
    public class ParserTests_LineEnds
    {
        private readonly string _fileName;
        private Yaml.File _objectUnderTest;
        private Yaml.Container _root;

        public ParserTests_LineEnds(string fileName) => _fileName = fileName;

        [SetUp]
        public void PrepareTest()
        {
            var parentDirectory = Directory.GetParent(new Uri(GetType().Assembly.Location).LocalPath).FullName;
            var fileName = Path.Combine(parentDirectory, "Resources", _fileName);

            _objectUnderTest = Parser.Parse(fileName);
            _root = _objectUnderTest.Children.Single();
        }

        [Test]
        public void File_Name_matches()
        {
            Assert.That(_objectUnderTest.Name, Does.EndWith(Path.DirectorySeparatorChar + _fileName));
        }

        [Test]
        public void File_LocationSpan_matches()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_objectUnderTest.LocationSpan.Start, Is.EqualTo(new LineInfo(1, 0)), "Wrong start");
                Assert.That(_objectUnderTest.LocationSpan.End, Is.EqualTo(new LineInfo(5, 0)), "Wrong end");

                Assert.That(_objectUnderTest.FooterSpan, Is.EqualTo(new CharacterSpan(68, 68)), "Wrong footer");
            });
        }

        [Test]
        public void Root_LocationSpan_matches()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_root.LocationSpan.Start, Is.EqualTo(new LineInfo(1, 1)), "Wrong start");
                Assert.That(_root.LocationSpan.End, Is.EqualTo(new LineInfo(4, 7)), "Wrong end");

                Assert.That(_root.HeaderSpan, Is.EqualTo(new CharacterSpan(0, 46)), "Wrong header");
                Assert.That(_root.FooterSpan, Is.EqualTo(new CharacterSpan(61, 67)), "Wrong footer");
            });
        }

        [Test]
        public void FirstChild_LocationSpan_matches()
        {
            var node = _root.Children.OfType<Container>().First();

            Assert.Multiple(() =>
            {
                Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(3, 1)), "Wrong start");
                Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(3, 14)), "Wrong end");

                Assert.That(node.HeaderSpan, Is.EqualTo(new CharacterSpan(47, 57)), "Wrong header");
                Assert.That(node.FooterSpan, Is.EqualTo(new CharacterSpan(58, 60)), "Wrong footer");
            });
        }
    }
}