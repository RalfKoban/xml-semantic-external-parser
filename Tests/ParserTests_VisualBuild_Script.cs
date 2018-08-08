using System;
using System.IO;
using System.Linq;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

using NUnit.Framework;

namespace MiKoSolutions.SemanticParsers.Xml
{
    [TestFixture]
    public class ParserTests_VisualBuild_Script
    {
        private Yaml.File _objectUnderTest;
        private Yaml.Container _root;

        [SetUp]
        public void PrepareTest()
        {
            var parentDirectory = Directory.GetParent(new Uri(GetType().Assembly.Location).LocalPath).FullName;
            var fileName = Path.Combine(parentDirectory, "Resources", "VisualBuild_script.xml");

            _objectUnderTest = Parser.Parse(fileName);
            _root = _objectUnderTest.Children.Single();
        }

        [Test]
        public void File_Name_matches()
        {
            Assert.That(_objectUnderTest.Name, Does.EndWith(Path.DirectorySeparatorChar + "VisualBuild_script.xml"));
        }

        [Test]
        public void File_LocationSpan_matches()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_objectUnderTest.LocationSpan.Start, Is.EqualTo(new LineInfo(1, 0)), "Wrong start");
                Assert.That(_objectUnderTest.LocationSpan.End, Is.EqualTo(new LineInfo(15, 10)), "Wrong end");

                Assert.That(_objectUnderTest.FooterSpan, Is.EqualTo(new CharacterSpan(0, -1)), "Wrong footer");
            });
        }

        [Test]
        public void Root_LocationSpan_matches()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_root.LocationSpan.Start, Is.EqualTo(new LineInfo(1, 1)), "Wrong start");
                Assert.That(_root.LocationSpan.End, Is.EqualTo(new LineInfo(15, 10)), "Wrong end");

                Assert.That(_root.HeaderSpan, Is.EqualTo(new CharacterSpan(0, 76)), "Wrong header");
                Assert.That(_root.FooterSpan, Is.EqualTo(new CharacterSpan(492, 501)), "Wrong footer");
            });
        }

        [Test]
        public void Steps_LocationSpan_matches()
        {
            var node = _root.Children.OfType<Container>().Single();

            Assert.Multiple(() =>
            {
                Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(3, 1)), "Wrong start");
                Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(14, 12)), "Wrong end");

                Assert.That(node.HeaderSpan, Is.EqualTo(new CharacterSpan(77, 96)), "Wrong header");
                Assert.That(node.FooterSpan, Is.EqualTo(new CharacterSpan(480, 491)), "Wrong footer");
            });
        }

        [Test]
        public void Step_1_LocationSpan_matches()
        {
            var node = _root.Children.OfType<Container>().Single().Children.OfType<Container>().First();

            Assert.Multiple(() =>
            {
                Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(4, 1)), "Wrong start");
                Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(9, 13)), "Wrong end");

                Assert.That(node.HeaderSpan, Is.EqualTo(new CharacterSpan(97, 127)), "Wrong header");
                Assert.That(node.FooterSpan, Is.EqualTo(new CharacterSpan(350, 362)), "Wrong footer");
            });
        }

        [Test]
        public void Step_2_LocationSpan_matches()
        {
            var node = _root.Children.OfType<Container>().Single().Children.OfType<Container>().Last();

            Assert.Multiple(() =>
            {
                Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(10, 1)), "Wrong start");
                Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(13, 13)), "Wrong end");

                Assert.That(node.HeaderSpan, Is.EqualTo(new CharacterSpan(363, 389)), "Wrong header");
                Assert.That(node.FooterSpan, Is.EqualTo(new CharacterSpan(467, 479)), "Wrong footer");
            });
        }

        [Test]
        public void Step_2_BuildFailSteps_LocationSpan_matches()
        {
            var node = _root.Children.OfType<Container>().Single().Children.OfType<Container>().Last().Children.OfType<Container>().First();

            Assert.Multiple(() =>
            {
                Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(11, 1)), "Wrong start");
                Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(11, 52)), "Wrong end");

                Assert.That(node.HeaderSpan, Is.EqualTo(new CharacterSpan(390, 421)), "Wrong header");
                Assert.That(node.FooterSpan, Is.EqualTo(new CharacterSpan(423, 441)), "Wrong footer");
            });
        }

        [Test]
        public void Step_2_BuildFailSteps_Content_LocationSpan_matches()
        {
            var node = _root.Children.OfType<Container>().Single().Children.OfType<Container>().Last().Children.OfType<Container>().First().Children.OfType<TerminalNode>().Single();

            Assert.Multiple(() =>
            {
                Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(11, 33)), "Wrong start");
                Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(11, 33)), "Wrong end");

                Assert.That(node.Span, Is.EqualTo(new CharacterSpan(422, 422)), "Wrong header");
            });
        }
    }
}