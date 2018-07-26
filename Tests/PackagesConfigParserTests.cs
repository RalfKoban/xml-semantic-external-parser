using System;
using System.IO;
using System.Linq;

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
            Assert.That(_packages.Children.Any(_ => _.Name == "NUnit"), Is.True);
            Assert.That(_packages.Children.Any(_ => _.Name == "StyleCop.Analyzers"), Is.True);
            Assert.That(_packages.Children.Any(_ => _.Name == "YamlDotNet"), Is.True);
        }
    }
}