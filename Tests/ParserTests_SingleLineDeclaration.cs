using System;
using System.IO;
using System.Linq;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

using NUnit.Framework;

namespace MiKoSolutions.SemanticParsers.Xml
{
    [TestFixture]
    public class ParserTests_SingleLineDeclaration
    {
        private Yaml.File _objectUnderTest;
        private Yaml.Container _root;

        [SetUp]
        public void PrepareTest()
        {
            var parentDirectory = Directory.GetParent(new Uri(GetType().Assembly.Location).LocalPath).FullName;
            var fileName = Path.Combine(parentDirectory, "single_line_description.xml");

            _objectUnderTest = Parser.Parse(fileName);
            _root = _objectUnderTest.Children.Single();
        }

        [Test]
        public void File_Name_matches()
        {
            Assert.That(_objectUnderTest.Name, Does.EndWith(Path.DirectorySeparatorChar + "single_line_description.xml"));
        }

        [Test]
        public void File_LocationSpan_matches()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_objectUnderTest.LocationSpan.Start, Is.EqualTo(new LineInfo(1, 0)));
                Assert.That(_objectUnderTest.LocationSpan.End, Is.EqualTo(new LineInfo(2, 0)));

                Assert.That(_objectUnderTest.FooterSpan, Is.EqualTo(new CharacterSpan(274, 275)));
            });
        }

        [Test]
        public void Root_LocationSpan_matches()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_root.LocationSpan.Start, Is.EqualTo(new LineInfo(1, 9)));
                Assert.That(_root.LocationSpan.End, Is.EqualTo(new LineInfo(1, 274)));

                Assert.That(_root.HeaderSpan, Is.EqualTo(new CharacterSpan(8, 41)));
                Assert.That(_root.FooterSpan, Is.EqualTo(new CharacterSpan(260, 273)));
            });
        }
    }
}