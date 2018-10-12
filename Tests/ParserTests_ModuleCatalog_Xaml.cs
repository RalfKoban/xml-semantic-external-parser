using System;
using System.IO;
using System.Linq;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

using NUnit.Framework;

namespace MiKoSolutions.SemanticParsers.Xml
{
    [TestFixture]
    public class ParserTests_ModuleCatalog_Xaml
    {
        private Yaml.File _objectUnderTest;
        private Yaml.Container _root;

        [SetUp]
        public void PrepareTest()
        {
            var parentDirectory = Directory.GetParent(new Uri(GetType().Assembly.Location).LocalPath).FullName;
            var fileName = Path.Combine(parentDirectory, "Resources", "ModuleCatalog.xml");

            _objectUnderTest = Parser.Parse(fileName);
            _root = _objectUnderTest.Children.Single();
        }

        [Test]
        public void File_Name_matches() => Assert.That(_objectUnderTest.Name, Does.EndWith(Path.DirectorySeparatorChar + "ModuleCatalog.xml"));

        [Test]
        public void File_LocationSpan_matches()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_objectUnderTest.LocationSpan.Start, Is.EqualTo(new LineInfo(1, 0)), "Wrong start");
                Assert.That(_objectUnderTest.LocationSpan.End, Is.EqualTo(new LineInfo(12, 27)), "Wrong end");

                Assert.That(_objectUnderTest.FooterSpan, Is.EqualTo(CharacterSpan.None), "Wrong footer");
            });
        }

        [Test]
        public void Root_LocationSpan_matches()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_root.LocationSpan.Start, Is.EqualTo(new LineInfo(1, 1)), "Wrong start");
                Assert.That(_root.LocationSpan.End, Is.EqualTo(new LineInfo(12, 27)), "Wrong end");

                Assert.That(_root.HeaderSpan, Is.EqualTo(new CharacterSpan(0, 395)), "Wrong header");
                Assert.That(_root.FooterSpan, Is.EqualTo(new CharacterSpan(660, 730)), "Wrong footer (should include comment at the end)");
            });
        }

        [TestCase("ModuleOne", 5, 1, 7,  68, 396, 542)]
        [TestCase("ModuleTwo", 8, 1, 8, 117, 543, 659)]
        public void Element_matches(string name, int startLineNumber, int startLinePos, int endLineNumber, int endLinePos, int startPos, int endPos)
        {
            var node = _root.Children.Where(_ => _.Type != NodeType.Attribute).OfType<TerminalNode>().FirstOrDefault(_ => _.Name == name);

            Assert.Multiple(() =>
            {
                Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(startLineNumber, startLinePos)), "Wrong start");
                Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(endLineNumber, endLinePos)), "Wrong end");

                Assert.That(node.Span, Is.EqualTo(new CharacterSpan(startPos, endPos)), "Wrong span");
            });
        }

        [Test]
        public void No_attributes_are_reported_to_allow_semantic_merge()
        {
            var hasAttributes = _root.Children.Any(_ => _.Type == NodeType.Attribute);

            Assert.That(hasAttributes, Is.False);
        }
    }
}