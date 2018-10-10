using System;
using System.IO;
using System.Linq;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

using NUnit.Framework;

namespace MiKoSolutions.SemanticParsers.Xml
{
    [TestFixture]
    public class ParserTests_EdmxV3
    {
        private Yaml.File _objectUnderTest;
        private Yaml.Container _root;

        [SetUp]
        public void PrepareTest()
        {
            var parentDirectory = Directory.GetParent(new Uri(GetType().Assembly.Location).LocalPath).FullName;
            var fileName = Path.Combine(parentDirectory, "Resources", "EdmxV3.xml");

            _objectUnderTest = Parser.Parse(fileName);
            _root = _objectUnderTest.Children.Single();
        }

        [Test]
        public void File_Name_matches()
        {
            Assert.That(_objectUnderTest.Name, Does.EndWith(Path.DirectorySeparatorChar + "EdmxV3.xml"));
        }

        [Test]
        public void File_LocationSpan_matches()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_objectUnderTest.LocationSpan.Start, Is.EqualTo(new LineInfo(1, 0)), "Wrong start");
                Assert.That(_objectUnderTest.LocationSpan.End, Is.EqualTo(new LineInfo(18, 12)), "Wrong end");

                Assert.That(_objectUnderTest.FooterSpan, Is.EqualTo(new CharacterSpan(0, -1)), "Wrong footer");
            });
        }

        [Test]
        public void Root_LocationSpan_matches()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_root.LocationSpan.Start, Is.EqualTo(new LineInfo(1, 1)), "Wrong start");
                Assert.That(_root.LocationSpan.End, Is.EqualTo(new LineInfo(18, 12)), "Wrong end");

                Assert.That(_root.HeaderSpan, Is.EqualTo(new CharacterSpan(0, 125)), "Wrong header");
                Assert.That(_root.FooterSpan, Is.EqualTo(new CharacterSpan(947, 958)), "Wrong footer");
            });
        }

        [Test]
        public void RuntimeElement_found()
        {
            var node = (Container)_root.Children.Single();
            Assert.Multiple(() =>
            {
                Assert.That(node.Type, Is.EqualTo("edmx:Runtime"));

                Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(3, 1)), "Wrong start");
                Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(17, 19)), "Wrong end");

                Assert.That(node.HeaderSpan, Is.EqualTo(new CharacterSpan(126, 174)), "Wrong header");
                Assert.That(node.FooterSpan, Is.EqualTo(new CharacterSpan(928, 946)), "Wrong footer");
            });
        }

        [Test]
        public void Schema_has_namespace()
        {
            var runtime = _root.Children.OfType<Container>().Single();
            var storageModels = runtime.Children.OfType<Container>().First();
            var node = storageModels.Children.OfType<Container>().First();

            Assert.Multiple(() =>
            {
                Assert.That(node.Type, Is.EqualTo("Schema"));
                Assert.That(node.Name, Is.EqualTo("Model.Store"));

                Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(7, 1)), "Wrong start");
                Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(15, 17)), "Wrong end");

                Assert.That(node.HeaderSpan, Is.EqualTo(new CharacterSpan(228, 573)), "Wrong header");
                Assert.That(node.FooterSpan, Is.EqualTo(new CharacterSpan(884, 900)), "Wrong footer");
            });
        }
    }
}
