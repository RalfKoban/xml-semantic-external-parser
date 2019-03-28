using System;
using System.IO;
using System.Linq;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

using NUnit.Framework;

namespace MiKoSolutions.SemanticParsers.Xml
{
    [TestFixture]
    public sealed class ParserTests_NLS
    {
        private Yaml.File _objectUnderTest;
        private Yaml.Container _root;

        [SetUp]
        public void PrepareTest()
        {
            var parentDirectory = Directory.GetParent(new Uri(GetType().Assembly.Location).LocalPath).FullName;
            var fileName = Path.Combine(parentDirectory, "Resources", "NLS1.xml");

            _objectUnderTest = Parser.Parse(fileName);
            _root = _objectUnderTest.Children.Single();
        }

        [Test]
        public void File_Name_matches()
        {
            Assert.That(_objectUnderTest.Name, Does.EndWith(Path.DirectorySeparatorChar + "NLS1.xml"));
        }

        [Test]
        public void Category_string_matches()
        {
            var node = _objectUnderTest.Children.First(_ => _.Type == "localization").Children.OfType<Container>().First(_ => _.Type == "descriptions").Children.First(_ => _.Type == "string");
            Assert.That(node.Name, Is.EqualTo("ID_CATEGORY_OF_DEVICE"));
        }
    }
}