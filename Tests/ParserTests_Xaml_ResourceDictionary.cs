using System;
using System.IO;
using System.Linq;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

using NUnit.Framework;

using File = System.IO.File;

namespace MiKoSolutions.SemanticParsers.Xml
{
    [TestFixture]
    public class ParserTests_Xaml_ResourceDictionary
    {
        private Yaml.File _objectUnderTest;
        private Yaml.Container _root;

        [SetUp]
        public void PrepareTest()
        {
            var parentDirectory = Directory.GetParent(new Uri(GetType().Assembly.Location).LocalPath).FullName;
            var fileName = Path.Combine(parentDirectory, "Resources", "Xaml_ResourceDictionary.xml");

            // we need to adjust line breaks because Git checkout on AppVeyor (or elsewhere) will adjust the line breaks
            var originalContent = File.ReadAllText(fileName);
            File.WriteAllText(fileName, originalContent.Replace(Environment.NewLine, "\n"));

            _objectUnderTest = Parser.Parse(fileName);
            _root = _objectUnderTest.Children.Single();
        }

        [Test]
        public void File_Name_matches()
        {
            Assert.That(_objectUnderTest.Name, Does.EndWith(Path.DirectorySeparatorChar + "Xaml_ResourceDictionary.xml"));
        }

        [Test]
        public void File_LocationSpan_matches()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_objectUnderTest.LocationSpan.Start, Is.EqualTo(new LineInfo(1, 0)), "Wrong start");
                Assert.That(_objectUnderTest.LocationSpan.End, Is.EqualTo(new LineInfo(16, 25)), "Wrong end");

                Assert.That(_objectUnderTest.FooterSpan, Is.EqualTo(new CharacterSpan(0, -1)), "Wrong footer");
            });
        }

        [Test]
        public void Root_LocationSpan_matches()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_root.LocationSpan.Start, Is.EqualTo(new LineInfo(1, 1)), "Wrong start");
                Assert.That(_root.LocationSpan.End, Is.EqualTo(new LineInfo(16, 25)), "Wrong end");

                Assert.That(_root.HeaderSpan, Is.EqualTo(new CharacterSpan(0, 277)), "Wrong header");
                Assert.That(_root.FooterSpan, Is.EqualTo(new CharacterSpan(2403, 2427)), "Wrong footer");
            });
        }

        [TestCase(00,  2, 1,  2, 118,  278,  376,  384,  395)]
        [TestCase(01,  3, 1,  3, 219,  396,  599,  604,  614)]
        [TestCase(02,  4, 1,  4, 234,  615,  833,  838,  848)]
        [TestCase(03,  5, 1,  5, 232,  849, 1065, 1070, 1080)]
        [TestCase(04,  6, 1,  6, 166, 1081, 1229, 1234, 1246)]
        [TestCase(05,  7, 1,  7, 177, 1247, 1406, 1411, 1423)]
        [TestCase(06,  8, 1, 11, 183, 1424, 1582, 1601, 1612)]
        [TestCase(07, 12, 1, 12, 241, 1613, 1823, 1842, 1853)]
        [TestCase(08, 13, 1, 13, 183, 1854, 2006, 2025, 2036)]
        [TestCase(09, 14, 1, 14, 183, 2037, 2189, 2208, 2219)]
        [TestCase(10, 15, 1, 15, 183, 2220, 2372, 2391, 2402)]
        public void First_Element_matches(int index, int startLineNumber, int startLinePos, int endLineNumber, int endLinePos, int headerStartPos, int headerEndPos, int footerStartPos, int footerEndPos)
        {
            var node = _root.Children.OfType<Container>().ElementAt(index);

            Assert.Multiple(() =>
            {
                Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(startLineNumber, startLinePos)), "Wrong start");
                Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(endLineNumber, endLinePos)), "Wrong end");

                Assert.That(node.HeaderSpan, Is.EqualTo(new CharacterSpan(headerStartPos, headerEndPos)), "Wrong header");
                Assert.That(node.FooterSpan, Is.EqualTo(new CharacterSpan(footerStartPos, footerEndPos)), "Wrong footer");
            });
        }
    }
}