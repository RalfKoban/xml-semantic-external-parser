using System;
using System.IO;
using System.Linq;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

using NUnit.Framework;

namespace MiKoSolutions.SemanticParsers.Xml
{
    [TestFixture]
    public class ParserTests_WrongEncoding
    {
        private Yaml.File _objectUnderTest;
        private Yaml.Container _root;

        [SetUp]
        public void PrepareTest()
        {
            var parentDirectory = Directory.GetParent(new Uri(GetType().Assembly.Location).LocalPath).FullName;
            var fileName = Path.Combine(parentDirectory, "Resources", "test_with_umlaut_and_wrong_encoding.xml");

            _objectUnderTest = Parser.Parse(fileName);
            _root = _objectUnderTest.Children.Single();
        }

        [Test]
        public void File_Name_matches()
        {
            Assert.That(_objectUnderTest.Name, Does.EndWith(Path.DirectorySeparatorChar + "test_with_umlaut_and_wrong_encoding.xml"));
        }

        [Test]
        public void File_LocationSpan_matches()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_objectUnderTest.LocationSpan.Start, Is.EqualTo(new LineInfo(1, 0)), "Wrong start");
                Assert.That(_objectUnderTest.LocationSpan.End, Is.EqualTo(new LineInfo(5, 0)), "Wrong end");

                Assert.That(_objectUnderTest.FooterSpan, Is.EqualTo(new CharacterSpan(82, 83)), "Wrong footer");
            });
        }

        [Test]
        public void Root_LocationSpan_matches()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_root.LocationSpan.Start, Is.EqualTo(new LineInfo(1, 1)), "Wrong start");
                Assert.That(_root.LocationSpan.End, Is.EqualTo(new LineInfo(4, 7)), "Wrong end");

                Assert.That(_root.HeaderSpan, Is.EqualTo(new CharacterSpan(0, 48)), "Wrong header");
                Assert.That(_root.FooterSpan, Is.EqualTo(new CharacterSpan(75, 81)), "Wrong footer");
            });
        }

        [Test]
        public void Element_matches()
        {
            var node = _root.Children.OfType<Container>().First();

            Assert.Multiple(() =>
            {
                Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(3, 1)), "Wrong start");
                Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(3, 26)), "Wrong end");

                Assert.That(node.HeaderSpan, Is.EqualTo(new CharacterSpan(49, 59)), "Wrong header");
                Assert.That(node.FooterSpan, Is.EqualTo(new CharacterSpan(63, 74)), "Wrong footer");
            });
        }
    }
}