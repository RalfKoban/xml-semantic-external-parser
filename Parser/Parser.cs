﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Strategies;
using MiKoSolutions.SemanticParsers.Xml.Yaml;

using Container = MiKoSolutions.SemanticParsers.Xml.Yaml.Container;

namespace MiKoSolutions.SemanticParsers.Xml
{
    public static class Parser
    {
        public static File Parse(string filePath)
        {
            var finder = CharacterPositionFinder.CreateFrom(filePath);
            var strategy = XmlStrategyFinder.Find(filePath);

            Trace.WriteLine($"Using {strategy.GetType().Name} Strategy for '{filePath}'.", "RKN Semantic");

            var file = ParseCore(filePath, finder, strategy);

            Resorter.Resort(file);
            GapFiller.Fill(file, finder);
            CommentCleaner.Clean(file);

            return file;
        }

        public static File ParseCore(string filePath, CharacterPositionFinder finder, IXmlStrategy strategy)
        {
            using (var reader = new XmlTextReader(filePath))
            {
                var file = new File
                               {
                                   Name = filePath,
                                   FooterSpan = new CharacterSpan(0, -1), // there is no footer
                               };

                var fileBegin = new LineInfo(reader.LineNumber + 1, reader.LinePosition);

                try
                {
                    var dummyRoot = new Container();

                    // Parse the XML and display the text content of each of the elements.
                    while (reader.Read())
                    {
                        Parse(reader, dummyRoot, finder, strategy);
                    }

                    var root = dummyRoot.Children.OfType<Container>().Last();
                    var rootEnd = IncludeXmlDeclarationInRoot(root, dummyRoot);

                    var fileEnd = new LineInfo(reader.LineNumber, reader.LinePosition - 1);

                    var positionAfterLastElement = new LineInfo(rootEnd.LineNumber, rootEnd.LinePosition + 1); // we calculate the next one (either a new line character or a regular one)

                    file.LocationSpan = new LocationSpan(fileBegin, fileEnd);

                    if (positionAfterLastElement < fileEnd)
                    {
                        file.FooterSpan = GetCharacterSpan(new LocationSpan(positionAfterLastElement, fileEnd), finder);
                    }

                    file.Children.Add(root);
                }
                catch (XmlException ex)
                {
                    file.ParsingErrors.Add(new ParsingError
                                               {
                                                   ErrorMessage = ex.Message,
                                                   Location = new LineInfo(ex.LineNumber, ex.LinePosition),
                                               });

                    var lines = System.IO.File.ReadLines(filePath).Count();

                    // try to adjust location span to include full file content
                    file.LocationSpan = lines == 0
                                        ? new LocationSpan(new LineInfo(0, -1), new LineInfo(0, -1))
                                        : new LocationSpan(new LineInfo(1, 0), new LineInfo(lines + 1, 0));
                }

                return file;
            }
        }

        private static XmlNodeType Parse(XmlTextReader reader, Container parent, CharacterPositionFinder finder, IXmlStrategy strategy)
        {
            var nodeType = reader.NodeType;
            switch (nodeType)
            {
                case XmlNodeType.Element:
                {
                    ParseElement(reader, parent, finder, strategy);
                    break;
                }

                case XmlNodeType.ProcessingInstruction:
                case XmlNodeType.Comment:
                case XmlNodeType.XmlDeclaration:
                case XmlNodeType.CDATA:
                case XmlNodeType.Text:
                {
                    ParseTerminalNode(reader, parent, finder, strategy);
                    break;
                }
            }

            return nodeType;
        }

        private static void ParseElement(XmlTextReader reader, Container parent, CharacterPositionFinder finder, IXmlStrategy strategy)
        {
            var name = strategy.GetName(reader);
            var type = strategy.GetType(reader);

            var container = new Container
                                {
                                    Type = type,
                                    Name = name,
                                };

            var isEmpty = reader.IsEmptyElement;

            var startingSpan = GetLocationSpanWithParseAttributes(reader, container, finder, strategy);
            var headerSpan = GetCharacterSpan(startingSpan, finder);

            if (isEmpty)
            {
                // there is no content, so we have to get away of the footer by just using the '/>' characters as footer
                var headerSpanCorrected = new CharacterSpan(headerSpan.Start, Math.Max(headerSpan.Start, headerSpan.End - 2));
                var footerSpanCorrected = new CharacterSpan(Math.Max(0, headerSpan.End - 1), headerSpan.End);

                container.LocationSpan = startingSpan;
                container.HeaderSpan = headerSpanCorrected;
                container.FooterSpan = footerSpanCorrected;
            }
            else
            {
                while (reader.NodeType != XmlNodeType.EndElement)
                {
                    var nodeType = Parse(reader, container, finder, strategy);

                    // we had a side effect (reading further on stream to get the location span), so we have to check whether we found already an end element
                    if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        break;
                    }

                    if (reader.NodeType != nodeType)
                    {
                        continue;
                    }

                    if (!reader.Read())
                    {
                        break;
                    }
                }

                var endingSpan = GetLocationSpan(reader);
                var footerSpan = GetCharacterSpan(endingSpan, finder);

                container.LocationSpan = new LocationSpan(startingSpan.Start, endingSpan.End);
                container.HeaderSpan = headerSpan;
                container.FooterSpan = footerSpan;
            }

            // check whether we can use a terminal node instead
            var nodeToAdd = strategy.ShallBeTerminalNode(container)
                            ? (ContainerOrTerminalNode)container.ToTerminalNode()
                            : container;
            parent.Children.Add(nodeToAdd);
        }

        private static void ParseAttributes(XmlTextReader reader, Container parent, CharacterPositionFinder finder, IXmlStrategy strategy)
        {
            if (strategy.ParseAttributesEnabled)
            {
                reader.MoveToFirstAttribute();

                // read all attributes
                do
                {
                    ParseAttribute(reader, parent, finder, strategy);
                }
                while (reader.MoveToNextAttribute());
            }
        }

        private static void ParseAttribute(XmlTextReader reader, Container parent, CharacterPositionFinder finder, IXmlStrategy strategy)
        {
            var attributeStartPos = new LineInfo(reader.LineNumber, reader.LinePosition);

            var name = strategy.GetName(reader);
            var type = strategy.GetType(reader);

            reader.ReadAttributeValue();
            var value = reader.Value;

            var attributeEndPos = new LineInfo(reader.LineNumber, reader.LinePosition + value.Length);

            var startPos = finder.GetCharacterPosition(attributeStartPos);
            var endPos = finder.GetCharacterPosition(attributeEndPos);

            var locationSpan = new LocationSpan(attributeStartPos, attributeEndPos);
            var span = new CharacterSpan(startPos, endPos);

            AddTerminalNode(parent, type, name, locationSpan, span);
        }

        private static void ParseTerminalNode(XmlTextReader reader, Container parent, CharacterPositionFinder finder, IXmlStrategy strategy)
        {
            var name = strategy.GetName(reader);
            var type = strategy.GetType(reader);

            var locationSpan = GetLocationSpan(reader);
            var span = GetCharacterSpan(locationSpan, finder);

            AddTerminalNode(parent, type, name, locationSpan, span);
        }

        private static void AddTerminalNode(Container parent, string type, string name, LocationSpan locationSpan, CharacterSpan span)
        {
            parent.Children.Add(new TerminalNode
                                    {
                                        Type = type,
                                        Name = name,
                                        LocationSpan = locationSpan,
                                        Span = span,
                                    });
        }

        private static int GetPositionCorrection(XmlReader reader) => GetPositionCorrection(reader.NodeType);

        private static int GetPositionCorrection(XmlNodeType type)
        {
            switch (type)
            {
                case XmlNodeType.Comment: return 4;               // 4 is length of <!--
                case XmlNodeType.ProcessingInstruction: return 2; // 2 is length of <?
                case XmlNodeType.Element: return 1;               // 1 is length of <
                case XmlNodeType.EndElement: return 2;            // 2 is length of </
                case XmlNodeType.XmlDeclaration: return 2;        // 2 is length of <?
                default: return 0;
            }
        }

        private static LineInfo GetStartLine(XmlTextReader reader)
        {
            var startCorrection = GetPositionCorrection(reader);
            return new LineInfo(reader.LineNumber, reader.LinePosition - startCorrection);
        }

        private static LineInfo GetEndLine(XmlTextReader reader)
        {
            var endCorrection = GetPositionCorrection(reader);
            return new LineInfo(reader.LineNumber, reader.LinePosition - endCorrection - 1); // previous character needed
        }

        //// ATTENTION !!!! SIDE EFFECT AS WE READ FURTHER !!!!
        private static LocationSpan GetLocationSpan(XmlTextReader reader)
        {
            var start = GetStartLine(reader);
            var end = reader.Read()
                      ? GetEndLine(reader)
                      : start;

            return new LocationSpan(start, end);
        }

        //// ATTENTION !!!! SIDE EFFECT AS WE READ FURTHER !!!!
        private static LocationSpan GetLocationSpanWithParseAttributes(XmlTextReader reader, Container container, CharacterPositionFinder finder, IXmlStrategy strategy)
        {
            var start = GetStartLine(reader);

            if (reader.HasAttributes)
            {
                // we have to read the attributes
                ParseAttributes(reader, container, finder, strategy);
            }

            // read to end of character
            reader.Read();

            var end = GetEndLine(reader);

            return new LocationSpan(start, end);
        }

        private static CharacterSpan GetCharacterSpan(LocationSpan locationSpan, CharacterPositionFinder finder)
        {
            var startPos = finder.GetCharacterPosition(locationSpan.Start);
            var endPos = finder.GetCharacterPosition(locationSpan.End);

            return new CharacterSpan(startPos, endPos);
        }

        private static LineInfo IncludeXmlDeclarationInRoot(Container root, Container dummyRoot)
        {
            var rootEnd = root.LocationSpan.End;

            // there might be no declaration, such as when trying to parse XAML files
            var xmlDeclaration = dummyRoot.Children.OfType<TerminalNode>().FirstOrDefault();
            if (xmlDeclaration != null)
            {
                // let root include the XML declaration
                var rootStart = xmlDeclaration.LocationSpan.Start;

                // adjust positions
                root.LocationSpan = new LocationSpan(rootStart, rootEnd);
                root.HeaderSpan = new CharacterSpan(xmlDeclaration.Span.Start, root.HeaderSpan.End);
            }

            return rootEnd;
        }
    }
}