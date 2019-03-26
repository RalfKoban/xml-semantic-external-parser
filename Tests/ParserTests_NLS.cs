using System;
using System.IO;
using System.Linq;

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
    }
}