using System;
using System.IO;
using System.Linq;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

using NUnit.Framework;

namespace MiKoSolutions.SemanticParsers.Xml
{
    [TestFixture]
    public sealed class ParserTests_Xsl
    {
        private Yaml.File _objectUnderTest;
        private Yaml.Container _root;

        [SetUp]
        public void PrepareTest()
        {
            var parentDirectory = Directory.GetParent(new Uri(GetType().Assembly.Location).LocalPath).FullName;
            var fileName = Path.Combine(parentDirectory, "Resources", "StyleSheet.xslt");

            _objectUnderTest = Parser.Parse(fileName);
            _root = _objectUnderTest.Children.Single();
        }

        [Test]
        public void File_Name_matches()
        {
            Assert.That(_objectUnderTest.Name, Does.EndWith(Path.DirectorySeparatorChar + "StyleSheet.xslt"));
        }

        [Test]
        public void File_LocationSpan_matches()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_objectUnderTest.LocationSpan.Start, Is.EqualTo(new LineInfo(1, 0)), "Wrong start");
                Assert.That(_objectUnderTest.LocationSpan.End, Is.EqualTo(new LineInfo(13, 0)), "Wrong end");

                Assert.That(_objectUnderTest.FooterSpan, Is.EqualTo(new CharacterSpan(428, 429)), "Wrong footer");
            });
        }

        [Test]
        public void Root_LocationSpan_matches()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_root.LocationSpan.Start, Is.EqualTo(new LineInfo(1, 1)), "Wrong start");
                Assert.That(_root.LocationSpan.End, Is.EqualTo(new LineInfo(12, 17)), "Wrong end");

                Assert.That(_root.HeaderSpan, Is.EqualTo(new CharacterSpan(0, 204)), "Wrong header");
                Assert.That(_root.FooterSpan, Is.EqualTo(new CharacterSpan(411, 427)), "Wrong footer");
            });
        }

        [Test]
        public void Output_element_matches()
        {
            var node = _root.Children.OfType<TerminalNode>().First();

            Assert.Multiple(() =>
            {
                Assert.That(node.Type, Is.EqualTo("xsl:output"));
                Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(5, 1)), "Wrong start");
                Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(5, 45)), "Wrong end");

                Assert.That(node.Span, Is.EqualTo(new CharacterSpan(205, 249)), "Wrong span");
            });
        }

        [Test]
        public void Template_element_matches()
        {
            var node = _root.Children.OfType<TerminalNode>().Last();

            Assert.Multiple(() =>
            {
                Assert.That(node.Type, Is.EqualTo("xsl:template"));
                Assert.That(node.Name, Is.EqualTo("@* | node()"));

                Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(6, 1)), "Wrong start");
                Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(11, 21)), "Wrong end");

                Assert.That(node.Span, Is.EqualTo(new CharacterSpan(250, 410)), "Wrong span");
            });
        }
    }
}
