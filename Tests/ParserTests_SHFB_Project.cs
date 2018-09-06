using System;
using System.IO;
using System.Linq;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

using NUnit.Framework;

namespace MiKoSolutions.SemanticParsers.Xml
{
    [TestFixture]
    public class ParserTests_SHFB_Project
    {
        private Yaml.File _objectUnderTest;
        private Yaml.Container _root;

        [SetUp]
        public void PrepareTest()
        {
            var parentDirectory = Directory.GetParent(new Uri(GetType().Assembly.Location).LocalPath).FullName;
            var fileName = Path.Combine(parentDirectory, "Resources", "SFHB_Project.xml");

            _objectUnderTest = Parser.Parse(fileName);
            _root = _objectUnderTest.Children.Single();
        }

        [Test]
        public void File_Name_matches() => Assert.That(_objectUnderTest.Name, Does.EndWith(Path.DirectorySeparatorChar + "SFHB_Project.xml"));

        [TestCase("NamespaceSummaries", "NamespaceSummaryItem", "MiKoSolutions.SemanticParsers.Xml")]
        [TestCase("DocumentationSources", "DocumentationSource", "my_assembly.xml")]
        [TestCase("PlugInConfigurations", "PlugInConfig", "Hierarchical Table of Contents")]
        [TestCase("ComponentConfigurations", "ComponentConfig", "Code Block Component")]
        [TestCase("ApiFilter", "Filter 'Namespace'", "MiKoSolutions.SemanticParsers.Xml")]
        public void Item_is_found_(string groupType, string type, string name)
        {
            var properties = _root.Children.Where(_ => _.Type == "PropertyGroup").OfType<Container>().SelectMany(_ => _.Children);
            var nodes = properties.Where(_ => _.Type == groupType).OfType<Container>().SelectMany(_ => _.Children);
            var item = nodes.Any(_ => _.Type == type && _.Name == name);

            Assert.That(item, Is.True);
        }

        [TestCase("NamespaceSummaries", "NamespaceSummaries")]
        [TestCase("ApiFilter", "ApiFilter")]
        public void Group_is_found_(string groupType, string name)
        {
            var properties = _root.Children.Where(_ => _.Type == "PropertyGroup").OfType<Container>().SelectMany(_ => _.Children);
            var item = properties.Where(_ => _.Type == groupType).Any(_ => _.Name == name);

            Assert.That(item, Is.True);
        }
    }
}