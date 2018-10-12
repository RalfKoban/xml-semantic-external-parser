using System;
using System.IO;
using System.Linq;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

using NUnit.Framework;

namespace MiKoSolutions.SemanticParsers.Xml
{
    public class ParserTests_NuSpec
    {
        private Yaml.File _objectUnderTest;
        private Yaml.Container _root;

        [SetUp]
        public void PrepareTest()
        {
            var parentDirectory = Directory.GetParent(new Uri(GetType().Assembly.Location).LocalPath).FullName;
            var fileName = Path.Combine(parentDirectory, "Resources", "NuSpec.xml");

            _objectUnderTest = Parser.Parse(fileName);
            _root = _objectUnderTest.Children.Single();
        }

        [Test]
        public void File_Name_matches()
        {
            Assert.That(_objectUnderTest.Name, Does.EndWith(Path.DirectorySeparatorChar + "NuSpec.xml"));
        }

        [Test]
        public void File_LocationSpan_matches()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_objectUnderTest.LocationSpan.Start, Is.EqualTo(new LineInfo(1, 0)), "Wrong start");
                Assert.That(_objectUnderTest.LocationSpan.End, Is.EqualTo(new LineInfo(16, 10)), "Wrong end");

                Assert.That(_objectUnderTest.FooterSpan, Is.EqualTo(CharacterSpan.None), "Wrong footer");
            });
        }

        [Test]
        public void Root_LocationSpan_matches()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_root.LocationSpan.Start, Is.EqualTo(new LineInfo(1, 1)), "Wrong start");
                Assert.That(_root.LocationSpan.End, Is.EqualTo(new LineInfo(16, 10)), "Wrong end");

                Assert.That(_root.HeaderSpan, Is.EqualTo(new CharacterSpan(0, 279)), "Wrong header");
                Assert.That(_root.FooterSpan, Is.EqualTo(new CharacterSpan(703, 712)), "Wrong footer");
            });
        }

        [Test]
        public void MetaData_matches()
        {
            var node = _root.Children.OfType<Container>().FirstOrDefault(_ => _.Type == "metadata");

            Assert.Multiple(() =>
            {
                Assert.That(node.Name, Is.EqualTo("My package"));

                Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(5, 1)), "Wrong start");
                Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(12, 15)), "Wrong end");

                Assert.That(node.HeaderSpan, Is.EqualTo(new CharacterSpan(280, 293)), "Wrong header");
                Assert.That(node.FooterSpan, Is.EqualTo(new CharacterSpan(607, 621)), "Wrong footer");
            });
        }

        [Test]
        public void MetaData_ID_matches()
        {
            var node = _root.Children.OfType<Container>().Where(_ => _.Type == "metadata").SelectMany(_ => _.Children).OfType<TerminalNode>().FirstOrDefault(_ => _.Type == "id");

            Assert.Multiple(() =>
            {
                Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(6, 1)), "Wrong start");
                Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(6, 25)), "Wrong end");

                Assert.That(node.Span, Is.EqualTo(new CharacterSpan(294, 318)), "Wrong span");
            });
        }
    }
}