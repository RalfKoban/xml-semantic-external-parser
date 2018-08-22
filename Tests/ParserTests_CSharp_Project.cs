using System;
using System.IO;
using System.Linq;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

using NUnit.Framework;

namespace MiKoSolutions.SemanticParsers.Xml
{
    [TestFixture]
    public class ParserTests_CSharp_Project
    {
        private Yaml.File _objectUnderTest;
        private Yaml.Container _root;

        [SetUp]
        public void PrepareTest()
        {
            var parentDirectory = Directory.GetParent(new Uri(GetType().Assembly.Location).LocalPath).FullName;
            var fileName = Path.Combine(parentDirectory, "Resources", "CSharp_Project.xml");

            _objectUnderTest = Parser.Parse(fileName);
            _root = _objectUnderTest.Children.Single();
        }

        [Test]
        public void File_Name_matches() => Assert.That(_objectUnderTest.Name, Does.EndWith(Path.DirectorySeparatorChar + "CSharp_Project.xml"));

        [TestCase("Reference", "YamlDotNet")]
        [TestCase("Compile", "LocationSpanConverter.cs")]
        [TestCase("Analyzer", "StyleCop.Analyzers.dll")]
        [TestCase("ProjectReference", "Common.csproj")]
        public void Item_is_found_and_truncated_properly(string type, string name)
        {
            var item = _root.Children.OfType<Container>().SelectMany(_ => _.Children).Where(_ => _.Type == type).Any(_ => _.Name == name);

            Assert.That(item, Is.True);
        }
    }
}