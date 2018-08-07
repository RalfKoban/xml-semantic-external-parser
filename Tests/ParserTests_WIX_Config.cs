using System;
using System.IO;
using System.Linq;

using NUnit.Framework;

namespace MiKoSolutions.SemanticParsers.Xml
{
    [TestFixture]
    public class ParserTests_WIX_Config
    {
        private Yaml.File _objectUnderTest;
        private Yaml.Container _root;

        [SetUp]
        public void PrepareTest()
        {
            var parentDirectory = Directory.GetParent(new Uri(GetType().Assembly.Location).LocalPath).FullName;
            var fileName = Path.Combine(parentDirectory, "Resources", "WIX_config.xml");

            _objectUnderTest = Parser.Parse(fileName);
            _root = _objectUnderTest.Children.Single();
        }

        [Test]
        public void Root_has_correct_amount_of_children()
        {
            Assert.That(_root.Children, Has.Count.EqualTo(2));
        }

        [Test]
        public void Defines_have_correct_names()
        {
            var names = _root.Children.Select(_ => _.Name).ToList();
            var foundNames = string.Join(",", names);

            Assert.That(names.Contains("define 'Something'"), foundNames);
            Assert.That(names.Contains("define 'Whatever'"), foundNames);
        }
    }
}