using System;
using System.IO;
using System.Linq;
using System.Text;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

using NUnit.Framework;

namespace MiKoSolutions.SemanticParsers.Xml
{
    [TestFixture]
    public class ParserTests_RuleSet
    {
        private Yaml.File _objectUnderTest;
        private Yaml.Container _root;

        [SetUp]
        public void PrepareTest()
        {
            var parentDirectory = Directory.GetParent(new Uri(GetType().Assembly.Location).LocalPath).FullName;
            var fileName = Path.Combine(parentDirectory, "Resources", "RuleSet.xml");

            _objectUnderTest = Parser.Parse(fileName);
            _root = _objectUnderTest.Children.Single();
        }

        [Test]
        public void Root_has_correct_name() => Assert.That(_root.Name, Is.EqualTo("Microsoft Managed Recommended Rules"));

        [TestCase("Rules", "Microsoft.Analyzers.ManagedCodeAnalysis")]
        [TestCase("Rules", "StyleCop.Analyzers")]
        public void Rules_group_is_found_properly(string groupType, string name)
        {
            var item = _root.Children.OfType<Container>().Where(_ => _.Type == groupType).Any(_ => _.Name == name);

            Assert.That(item, Is.True);
        }

        [TestCase("Rules", "Microsoft.Analyzers.ManagedCodeAnalysis", "CA1001")]
        [TestCase("Rules", "Microsoft.Analyzers.ManagedCodeAnalysis", "CA1009")]
        [TestCase("Rules", "StyleCop.Analyzers", "SA1120")]
        public void Rule_is_found_properly(string groupType, string name, string ruleName)
        {
            var item = _root.Children.OfType<Container>().Where(_ => _.Type == groupType && _.Name == name).SelectMany(_ => _.Children).Any(_ => _.Name == ruleName);

            Assert.That(item, Is.True);
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