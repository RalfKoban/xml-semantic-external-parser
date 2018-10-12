using System;
using System.IO;
using System.Linq;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

using NUnit.Framework;

namespace MiKoSolutions.SemanticParsers.Xml
{
    [TestFixture]
    public class ParserTests_FxCop
    {
        private Yaml.File _objectUnderTest;
        private Yaml.Container _root;

        [SetUp]
        public void PrepareTest()
        {
            var parentDirectory = Directory.GetParent(new Uri(GetType().Assembly.Location).LocalPath).FullName;
            var fileName = Path.Combine(parentDirectory, "Resources", "FxCop_CustomDictionary.xml");

            _objectUnderTest = Parser.Parse(fileName);
            _root = _objectUnderTest.Children.Single();
        }

        [Test]
        public void File_Name_matches()
        {
            Assert.That(_objectUnderTest.Name, Does.EndWith(Path.DirectorySeparatorChar + "FxCop_CustomDictionary.xml"));
        }

        [Test]
        public void File_LocationSpan_matches()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_objectUnderTest.LocationSpan.Start, Is.EqualTo(new LineInfo(1, 0)), "Wrong start");
                Assert.That(_objectUnderTest.LocationSpan.End, Is.EqualTo(new LineInfo(28, 13)), "Wrong end");

                Assert.That(_objectUnderTest.FooterSpan, Is.EqualTo(CharacterSpan.None), "Wrong footer");
            });
        }

        [Test]
        public void Root_LocationSpan_matches()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_root.Name, Is.EqualTo("Dictionary"));

                Assert.That(_root.LocationSpan.Start, Is.EqualTo(new LineInfo(1, 1)), "Wrong start");
                Assert.That(_root.LocationSpan.End, Is.EqualTo(new LineInfo(28, 13)), "Wrong end");

                Assert.That(_root.HeaderSpan, Is.EqualTo(new CharacterSpan(0, 13)), "Wrong header");
                Assert.That(_root.FooterSpan, Is.EqualTo(new CharacterSpan(617, 629)), "Wrong footer");
            });
        }

        [Test]
        public void Acronyms_LocationSpan_matches()
        {
            var node = _root.Children.OfType<Container>().First(_ => _.Name == "Acronyms");

            Assert.Multiple(() =>
            {
                Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(2, 1)), "Wrong start");
                Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(6, 15)), "Wrong end");

                Assert.That(node.HeaderSpan, Is.EqualTo(new CharacterSpan(14, 27)), "Wrong header");
                Assert.That(node.FooterSpan, Is.EqualTo(new CharacterSpan(107, 121)), "Wrong footer");
            });
        }

        [Test]
        public void Words_LocationSpan_matches()
        {
            var node = _root.Children.OfType<Container>().First(_ => _.Name == "Words");

            Assert.Multiple(() =>
            {
                Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(7, 1)), "Wrong start");
                Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(27, 12)), "Wrong end");

                Assert.That(node.HeaderSpan, Is.EqualTo(new CharacterSpan(122, 132)), "Wrong header");
                Assert.That(node.FooterSpan, Is.EqualTo(new CharacterSpan(605, 616)), "Wrong footer");
            });
        }

        [Test]
        public void Unrecognized_Words_LocationSpan_matches()
        {
            var node = _root.Children.OfType<Container>().First(_ => _.Name == "Words").Children.OfType<Container>().First(_ => _.Name == "Unrecognized");

            Assert.Multiple(() =>
            {
                Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(8, 1)), "Wrong start");
                Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(10, 21)), "Wrong end");

                Assert.That(node.HeaderSpan, Is.EqualTo(new CharacterSpan(133, 152)), "Wrong header");
                Assert.That(node.FooterSpan, Is.EqualTo(new CharacterSpan(179, 199)), "Wrong footer");
            });
        }

        [Test]
        public void Word_in_Unrecognized_Words_LocationSpan_matches()
        {
            var node = _root.Children.OfType<Container>().First(_ => _.Name == "Words").Children.OfType<Container>().First(_ => _.Name == "Unrecognized").Children.Single() as TerminalNode;

            Assert.Multiple(() =>
            {
                Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(9, 1)), "Wrong start");
                Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(9, 26)), "Wrong end");

                Assert.That(node.Span, Is.EqualTo(new CharacterSpan(153, 178)), "Wrong span");
            });
        }
    }
}