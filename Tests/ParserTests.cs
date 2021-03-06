﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

using NUnit.Framework;

using File = System.IO.File;

namespace MiKoSolutions.SemanticParsers.Xml
{
    [TestFixture]
    public class ParserTests
    {
        private Yaml.File _objectUnderTest;
        private Yaml.Container _root;

        [SetUp]
        public void PrepareTest()
        {
            var parentDirectory = Directory.GetParent(new Uri(GetType().Assembly.Location).LocalPath).FullName;
            var fileName = Path.Combine(parentDirectory, "Resources", "test.xml");

            _objectUnderTest = Parser.Parse(fileName);
            _root = _objectUnderTest.Children.Single();
        }

        [Test]
        public void File_Name_matches()
        {
            Assert.That(_objectUnderTest.Name, Does.EndWith(Path.DirectorySeparatorChar + "test.xml"));
        }

        [Test]
        public void File_LocationSpan_matches()
        {
            Assert.That(_objectUnderTest.LocationSpan.Start, Is.EqualTo(new LineInfo(1, 0)), "Wrong start");
            Assert.That(_objectUnderTest.LocationSpan.End, Is.EqualTo(new LineInfo(29, 0)), "Wrong end");

            Assert.That(_objectUnderTest.FooterSpan, Is.EqualTo(new CharacterSpan(536, 537)), "Wrong footer");
        }

        [Test]
        public void Root_LocationSpan_matches()
        {
            Assert.That(_root.LocationSpan.Start, Is.EqualTo(new LineInfo(1, 1)), "Wrong start");
            Assert.That(_root.LocationSpan.End, Is.EqualTo(new LineInfo(28, 12)), "Wrong end");

            Assert.That(_root.HeaderSpan, Is.EqualTo(new CharacterSpan(0, 53)), "Wrong header");
            Assert.That(_root.FooterSpan, Is.EqualTo(new CharacterSpan(498, 535)), "Wrong footer");
        }

        [TestCase("some",  3, 1,  4, 24,  54, 103)]
        [TestCase("last", 23, 1, 24, 59, 437, 497)]
        public void ProcessingInstruction_LocationSpan_and_Span_matches(string name, int startLine, int startPos, int endLine, int endPos, int spanStartPos, int spanEndPos)
        {
            Assert.Multiple(() =>
            {
                var node = _root.Children.OfType<TerminalNode>().Where(_ => _.Type == "ProcessingInstruction").First(_ => _.Name == name);

                Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(startLine, startPos)), "Wrong start for {0}", name);
                Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(endLine, endPos)), "Wrong end for {0}", name);
                Assert.That(node.Span, Is.EqualTo(new CharacterSpan(spanStartPos, spanEndPos)), "wrong span for {0}", name);
            });
        }

        [TestCase("first",    5, 1,  5, 34, 104, 127, 128, 137)]
        [TestCase("second",   6, 1,  6, 37, 138, 163, 164, 174)]
        [TestCase("third",    7, 1,  7, 54, 175, 183, 219, 228)]
        [TestCase("forth",    8, 1, 10, 12, 229, 239, 240, 253)]
        [TestCase("fifth",   11, 1, 11, 13, 254, 262, 263, 266)]
        [TestCase("sixth",   12, 1, 18, 12, 267, 277, 350, 361)]
        [TestCase("seventh", 19, 1, 22, 25, 362, 374, 425, 436)]
        public void First_level_element_LocationSpan_and_Span_matches(string name, int startLine, int startPos, int endLine, int endPos, int headerStartPos, int headerEndPos, int footerStartPos, int footerEndPos)
        {
            Assert.Multiple(() =>
            {
                var node = _root.Children.OfType<Container>().First(_ => _.Name == name);

                Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(startLine, startPos)), "Wrong start for {0}", name);
                Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(endLine, endPos)), "Wrong end for {0}", name);

                Assert.That(node.HeaderSpan, Is.EqualTo(new CharacterSpan(headerStartPos, headerEndPos)), "wrong header span for {0}", name);
                Assert.That(node.FooterSpan, Is.EqualTo(new CharacterSpan(footerStartPos, footerEndPos)), "wrong footer span for {0}", name);
            });
        }

        [TestCase("third",   "nested",  7, 10,  7, 44, 184, 208, 209, 218)]
        [TestCase("sixth",   "nested", 13,  1, 17, 15, 278, 334, 335, 349)]
        [TestCase("seventh", "nested", 20,  1, 22, 13, 375, 411, 412, 424)]
        public void Second_level_element_LocationSpan_and_Span_matches(string parentName, string name, int startLine, int startPos, int endLine, int endPos, int headerStartPos, int headerEndPos, int footerStartPos, int footerEndPos)
        {
            Assert.Multiple(() =>
            {
                var parent = _root.Children.OfType<Container>().First(_ => _.Name == parentName);
                var node = parent.Children.First(_ => _.Name == name);

                Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(startLine, startPos)), "Wrong start for {0}", name);
                Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(endLine, endPos)), "Wrong end for {0}", name);

                if (node is Container cNode)
                {
                    Assert.That(cNode.HeaderSpan, Is.EqualTo(new CharacterSpan(headerStartPos, headerEndPos)), "wrong header span for {0}", name);
                    Assert.That(cNode.FooterSpan, Is.EqualTo(new CharacterSpan(footerStartPos, footerEndPos)), "wrong footer span for {0}", name);
                }
                else if (node is TerminalNode tNode)
                {
                    Assert.That(tNode.Span, Is.EqualTo(new CharacterSpan(headerStartPos, footerEndPos)), "wrong span for {0}", name);
                }
            });
        }

        [Ignore("Parsing of Attributes is currently disabled")]
        [TestCase("first",  "element", 5, 10, 5, 23, 113, 126)]
        [TestCase("second", "element", 6, 11, 6, 24, 148, 161)]
        public void First_level_element_attribute_LocationSpan_and_Span_matches(string parentName, string name, int startLine, int startPos, int endLine, int endPos, int spanStartPos, int spanEndPos)
        {
            Assert.Multiple(() =>
            {
                var parent = _root.Children.OfType<Container>().First(_ => _.Name == parentName);
                var node = parent.Children.OfType<TerminalNode>().First(_ => _.Name == name);

                Assert.That(node.LocationSpan.Start, Is.EqualTo(new LineInfo(startLine, startPos)), "Wrong start for {0}", name);
                Assert.That(node.LocationSpan.End, Is.EqualTo(new LineInfo(endLine, endPos)), "Wrong end for {0}", name);
                Assert.That(node.Span, Is.EqualTo(new CharacterSpan(spanStartPos, spanEndPos)), "wrong span for {0}", name);
            });
        }

        [Test]
        public void All_characters_are_found()
        {
            var chars = Enumerable.Range(0, 482).ToHashSet();

            RemoveSpan(chars, _objectUnderTest.FooterSpan);

            foreach (var child in _objectUnderTest.Children)
            {
                RemoveChars(chars, child);
            }

            Assert.That(chars.Count, Is.EqualTo(0), "Still left are: " + string.Join(",", chars));
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

        [Test]
        public void Xml_without_declaration_can_be_read()
        {
            var parentDirectory = Directory.GetParent(new Uri(GetType().Assembly.Location).LocalPath).FullName;
            var fileName = Path.Combine(parentDirectory, "Resources", "test_without_declaration.xml");

            _objectUnderTest = Parser.Parse(fileName);
            _root = _objectUnderTest.Children.Single();

            Assert.That(_objectUnderTest.LocationSpan.Start, Is.EqualTo(new LineInfo(1, 0)), "Wrong start");
            Assert.That(_objectUnderTest.LocationSpan.End, Is.EqualTo(new LineInfo(28, 0)), "Wrong end");

            Assert.That(_objectUnderTest.FooterSpan, Is.EqualTo(new CharacterSpan(495, 496)), "Wrong footer");
        }

        [Test]
        public void Empty_file_can_be_read_without_parsing_errors()
        {
            var fileName = Path.GetTempFileName();
            try
            {
                _objectUnderTest = Parser.Parse(fileName);
            }
            finally
            {
                File.Delete(fileName);
            }

            Assert.That(_objectUnderTest.LocationSpan.Start, Is.EqualTo(LineInfo.None), "Wrong start");
            Assert.That(_objectUnderTest.LocationSpan.End, Is.EqualTo(LineInfo.None), "Wrong end");

            Assert.That(_objectUnderTest.FooterSpan, Is.EqualTo(CharacterSpan.None), "Wrong footer");
        }

        private static void RemoveChars(HashSet<int> chars, Container node)
        {
            RemoveSpan(chars, node.HeaderSpan);
            RemoveSpan(chars, node.FooterSpan);

            foreach (var child in node.Children)
            {
                if (child is Container c)
                {
                    RemoveChars(chars, c);
                }
                else if (child is TerminalNode t)
                {
                    RemoveChars(chars, t);
                }
            }
        }

        private static void RemoveChars(HashSet<int> chars, TerminalNode node)
        {
            RemoveSpan(chars, node.Span);
        }

        private static void RemoveSpan(HashSet<int> chars, CharacterSpan span)
        {
            for (var i = span.Start; i <= span.End; i++)
            {
                chars.Remove(i);
            }
        }
    }
}
