using System;
using System.IO;
using System.Linq;
using System.Text;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

using NUnit.Framework;

namespace MiKoSolutions.SemanticParsers.Xml
{
    [TestFixture]
    public class ParserTests_SettingsFile
    {
        private Yaml.File _objectUnderTest;
        private Yaml.Container _root;

        [SetUp]
        public void PrepareTest()
        {
            var parentDirectory = Directory.GetParent(new Uri(GetType().Assembly.Location).LocalPath).FullName;
            var fileName = Path.Combine(parentDirectory, "Resources", "Settings_file.xml");

            _objectUnderTest = Parser.Parse(fileName);
            _root = _objectUnderTest.Children.Single();
        }

        [Test]
        public void SettingsFile_has_correct_amount_of_children()
        {
            Assert.That(_root.Children, Has.Count.EqualTo(2));
        }

        [Test]
        public void Settings_have_correct_names()
        {
            var names = _root.Children
                                    .OfType<Container>().Where(_ => _.Type == "Settings")
                                    .SelectMany(_ => _.Children)
                                    .Select(_ => _.Name)
                                    .ToList();

            var foundNames = string.Join(",", names);

            Assert.That(names.Contains("Some setting"), foundNames);
            Assert.That(names.Contains("Some other setting"), foundNames);
        }

        [Test]
        public void Profiles_have_correct_names()
        {
            var names = _root.Children
                                    .OfType<Container>().Where(_ => _.Type == "Profiles")
                                    .SelectMany(_ => _.Children)
                                    .Select(_ => _.Name)
                                    .ToList();

            var foundNames = string.Join(",", names);

            Assert.That(names.Contains("(Default)"), foundNames);
            Assert.That(names.Contains("yet another profile"), foundNames);
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