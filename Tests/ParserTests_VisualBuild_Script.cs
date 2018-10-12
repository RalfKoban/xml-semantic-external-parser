using System;
using System.IO;
using System.Linq;
using System.Text;

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
                Assert.That(_objectUnderTest.LocationSpan.End, Is.EqualTo(new LineInfo(18, 10)), "Wrong end");

                Assert.That(_objectUnderTest.FooterSpan, Is.EqualTo(CharacterSpan.None), "Wrong footer");
            });
        }

        [Test]
        public void Root_LocationSpan_matches()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_root.LocationSpan.Start, Is.EqualTo(new LineInfo(1, 1)), "Wrong start");
                Assert.That(_root.LocationSpan.End, Is.EqualTo(new LineInfo(18, 10)), "Wrong end");

                Assert.That(_root.HeaderSpan, Is.EqualTo(new CharacterSpan(0, 76)), "Wrong header");
                Assert.That(_root.FooterSpan, Is.EqualTo(new CharacterSpan(556, 565)), "Wrong footer");
            });
        }

        [Test]
        public void Steps_LocationSpan_matches()
        {
            var node = _root.Children.OfType<Container>().First();

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
            var node = _root.Children.OfType<Container>().First().Children.OfType<TerminalNode>().First(_ => _.Type.StartsWith("step "));

            Assert.Multiple(() =>
            {
                Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(4, 1)), "Wrong start");
                Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(9, 13)), "Wrong end");

                Assert.That(node.Span, Is.EqualTo(new CharacterSpan(97, 362)), "Wrong span");
            });
        }

        [Test]
        public void Step_2_LocationSpan_matches()
        {
            var node = _root.Children.OfType<Container>().First().Children.OfType<TerminalNode>().Last(_ => _.Type.StartsWith("step "));

            Assert.Multiple(() =>
            {
                Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(10, 1)), "Wrong start");
                Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(13, 13)), "Wrong end");

                Assert.That(node.Span, Is.EqualTo(new CharacterSpan(363, 479)), "Wrong span");
            });
        }

        [Test]
        public void Macro_1_LocationSpan_matches()
        {
            var container = _root.Children.OfType<Container>().Last();
            Assert.That(container.Name, Is.EqualTo("macros"));

            var node = container.Children.OfType<TerminalNode>().First(_ => _.Type.StartsWith("macro"));

            Assert.Multiple(() =>
            {
                Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(16, 1)), "Wrong start");
                Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(16, 39)), "Wrong end");

                Assert.That(node.Span, Is.EqualTo(new CharacterSpan(504, 542)), "Wrong span");
            });
        }

        [Test]
        public void RoundTrip_does_not_report_parsing_errors()
        {
            var builder = new StringBuilder();
            using (var writer = new StringWriter(builder))
            {
                YamlWriter.Write(writer, _objectUnderTest);
            }

            Assert.That(builder.ToString(), Does.Contain("parsingErrorsDetected: false"));
        }
    }
}