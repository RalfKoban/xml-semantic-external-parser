using System;
using System.IO;
using System.Linq;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

using NUnit.Framework;

namespace MiKoSolutions.SemanticParsers.Xml
{
    [TestFixture]
    public class ParserTests_RegistryValue
    {
        private Yaml.File _objectUnderTest;
        private Yaml.Container _root;

        [SetUp]
        public void PrepareTest()
        {
            var parentDirectory = Directory.GetParent(new Uri(GetType().Assembly.Location).LocalPath).FullName;
            var fileName = Path.Combine(parentDirectory, "registry_value.xml");

            _objectUnderTest = Parser.Parse(fileName);
            _root = _objectUnderTest.Children.Single();
        }

        [Test]
        public void File_Name_matches()
        {
            Assert.That(_objectUnderTest.Name, Does.EndWith(Path.DirectorySeparatorChar + "registry_value.xml"));
        }

        [Test]
        public void File_LocationSpan_matches()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_objectUnderTest.LocationSpan.Start, Is.EqualTo(new LineInfo(1, 0)), "Wrong start");
                Assert.That(_objectUnderTest.LocationSpan.End, Is.EqualTo(new LineInfo(1, 110)), "Wrong end");

                Assert.That(_objectUnderTest.FooterSpan, Is.EqualTo(new CharacterSpan(0, -1)), "Wrong footer");
            });
        }

        [Test]
        public void Root_LocationSpan_matches()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_root.LocationSpan.Start, Is.EqualTo(new LineInfo(1, 1)), "Wrong start");
                Assert.That(_root.LocationSpan.End, Is.EqualTo(new LineInfo(1, 110)), "Wrong end");

                Assert.That(_root.HeaderSpan, Is.EqualTo(new CharacterSpan(0, 107)), "Wrong header");
                Assert.That(_root.FooterSpan, Is.EqualTo(new CharacterSpan(108, 109)), "Wrong footer");
            });
        }
    }
}