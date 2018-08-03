using System;
using System.IO;
using System.Linq;
using System.Text;

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
            var fileName = Path.Combine(parentDirectory, "Resources", "packages.xml");

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