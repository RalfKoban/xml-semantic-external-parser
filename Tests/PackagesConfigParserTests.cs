using System;
using System.IO;
using System.Linq;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

using NUnit.Framework;

namespace MiKoSolutions.SemanticParsers.Xml
{
    [TestFixture]
    public class PackagesConfigParserTests
    {
        private Yaml.File _objectUnderTest;
        private Yaml.Container _packages;

        [SetUp]
        public void PrepareTest()
        {
            var parentDirectory = Directory.GetParent(new Uri(GetType().Assembly.Location).LocalPath).FullName;
            var fileName = Path.Combine(parentDirectory, "packages.xml");

            _objectUnderTest = Parser.Parse(fileName);
            _packages = _objectUnderTest.Children.Single();
        }

        [Test]
        public void Packages_has_correct_amount_of_children()
        {
            Assert.That(_packages.Children, Has.Count.EqualTo(3));
        }

        [Test]
        public void Packages_have_correct_names()
        {
            var names = _packages.Children.Select(_ => _.Name).ToList();
            var foundNames = string.Join(",", names);

            Assert.That(names.Contains("NUnit"), foundNames);
            Assert.That(names.Contains("StyleCop.Analyzers"), foundNames);
            Assert.That(names.Contains("YamlDotNet"), foundNames);
        }

        [TestCase("NUnit", "id=\"NUnit\"")]
        public void Packages_IDs_have_correct_names(string elementName, string idName)
        {
            var element = _packages.Children.OfType<Container>().First(_ => _.Name == elementName);
            var id = element.Children.First(_ => _.Type == NodeType.Attribute);

            Assert.That(id.Name, Is.EqualTo(idName));
        }
    }
}