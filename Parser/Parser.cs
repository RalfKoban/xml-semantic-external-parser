using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

using Container = MiKoSolutions.SemanticParsers.Xml.Yaml.Container;

namespace MiKoSolutions.SemanticParsers.Xml
{
    public static class Parser
    {
        public static File Parse(string filePath)
        {
            var finder = CharacterPositionFinder.CreateFrom(filePath);

            var file = ParseCore(filePath, finder);

            Resorter.Resort(file);
            GapFiller.Fill(file, finder);

            return file;
        }

        public static File ParseCore(string filePath, CharacterPositionFinder finder)
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
                        Parse(reader, dummyRoot, finder);
                    }

                    var xmlDeclaration = dummyRoot.Children.OfType<TerminalNode>().First();
                    var root = dummyRoot.Children.OfType<Container>().Last();

                    // let root include the XML declaration
                    var rootStart = xmlDeclaration.LocationSpan.Start;
                    var rootEnd = root.LocationSpan.End;

                    // adjust positions
                    root.LocationSpan = new LocationSpan(rootStart, rootEnd);
                    root.HeaderSpan = new CharacterSpan(xmlDeclaration.Span.Start, root.HeaderSpan.End);

                    var fileEnd = new LineInfo(reader.LineNumber, reader.LinePosition - 1);

                    var positionAfterLastElement = new LineInfo(rootEnd.LineNumber, rootEnd.LinePosition + 1); // we calculate the next one (either a new line character or a regular one)

                    file.LocationSpan = new LocationSpan(fileBegin, fileEnd);

                    if (positionAfterLastElement != fileEnd)
                    {
                        file.FooterSpan = GetCharacterSpan(new LocationSpan(positionAfterLastElement, fileEnd), finder); // TODO: RKN user FooterSpan (0, -1) if there is no footer
                    }

                    file.Children.Add(root);
                }
                catch (Exception ex)
                {
                    file.ParsingErrors.Add(new ParsingError { ErrorMessage = ex.Message });
                }

                return file;
            }
        }

        private static void Parse(XmlTextReader reader, Container parent, CharacterPositionFinder finder)
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                {
                    ParseElement(reader, parent, finder);
                    break;
                }

                case XmlNodeType.ProcessingInstruction:
                {
                    ParseProcessingInstruction(reader, parent, finder);
                    break;
                }

                case XmlNodeType.Comment:
                {
                    ParseComment(reader, parent, finder);
                    break;
                }

                case XmlNodeType.XmlDeclaration:
                {
                    ParseXmlDeclaration(reader, parent, finder);
                    break;
                }

                case XmlNodeType.CDATA:
                {
                    ParseCData(reader, parent, finder);
                    break;
                }
            }
        }

        private static void ParseCData(XmlTextReader reader, Container parent, CharacterPositionFinder finder)
        {
            var name = reader.Value;
            var locationSpan = GetLocationSpan(reader);
            var span = GetCharacterSpan(locationSpan, finder);

            parent.Children.Add(new TerminalNode
                                    {
                                        Type = NodeType.CDATA,
                                        Name = name,
                                        LocationSpan = locationSpan,
                                        Span = span,
                                    });
        }

        private static void ParseXmlDeclaration(XmlTextReader reader, Container parent, CharacterPositionFinder finder)
        {
            var name = reader.Value;
            var locationSpan = GetLocationSpan(reader);
            var span = GetCharacterSpan(locationSpan, finder);

            parent.Children.Add(new TerminalNode
                                    {
                                        Type = NodeType.XmlDeclaration,
                                        Name = name,
                                        LocationSpan = locationSpan,
                                        Span = span,
                                    });
        }

        private static void ParseComment(XmlTextReader reader, Container parent, CharacterPositionFinder finder)
        {
            var name = reader.Value;
            var locationSpan = GetLocationSpan(reader);
            var span = GetCharacterSpan(locationSpan, finder);

            parent.Children.Add(new TerminalNode
                                    {
                                        Type = NodeType.Comment,
                                        Name = name,
                                        LocationSpan = locationSpan,
                                        Span = span,
                                    });
        }

        private static void ParseProcessingInstruction(XmlTextReader reader, Container parent, CharacterPositionFinder finder)
        {
            var name = reader.Name;
            var locationSpan = GetLocationSpan(reader);
            var span = GetCharacterSpan(locationSpan, finder);

            parent.Children.Add(new TerminalNode
                                    {
                                        Type = NodeType.ProcessingInstruction,
                                        Name = name,
                                        LocationSpan = locationSpan,
                                        Span = span,
                                    });
        }

        private static void ParseElement(XmlTextReader reader, Container parent, CharacterPositionFinder finder)
        {
            var container = new Container
                                {
                                    Type = NodeType.Element,
                                    Name = reader.Name,
                                };

            parent.Children.Add(container);

            var isEmpty = reader.IsEmptyElement;

            ParseAttributes(reader, container, finder);

            if (isEmpty)
            {
                var locationSpan = GetLocationSpan(reader);
                var headerSpan = GetCharacterSpan(locationSpan, finder);

                // there is no content, so we have to get away of the footer by just using the '/>' characters as footer
                var headerSpanCorrected = new CharacterSpan(headerSpan.Start, headerSpan.End - 2);
                var footerSpan = new CharacterSpan(headerSpan.End - 1, headerSpan.End);

                container.LocationSpan = locationSpan;
                container.HeaderSpan = headerSpanCorrected;
                container.FooterSpan = footerSpan;
            }
            else
            {
                var startingSpan = GetLocationSpan(reader);

                while (reader.NodeType != XmlNodeType.EndElement)
                {
                    Parse(reader, container, finder);

                    // we had a side effect (reading further on stream to get the location span), so we have to check whether we found already an end element
                    if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        break;
                    }

                    if (!reader.Read())
                    {
                        break;
                    }
                }

                var endingSpan = GetLocationSpan(reader);

                container.LocationSpan = new LocationSpan(startingSpan.Start, endingSpan.End);
                container.HeaderSpan = GetCharacterSpan(startingSpan, finder);
                container.FooterSpan = GetCharacterSpan(endingSpan, finder);
            }
        }

        private static void ParseAttributes(XmlTextReader reader, Container parent, CharacterPositionFinder finder)
        {
            if (!reader.HasAttributes)
            {
                return;
            }

            var attributes = new List<TerminalNode>();
            reader.MoveToFirstAttribute();

            // read all attributes
            do
            {
                var attributeStartPos = new LineInfo(reader.LineNumber, reader.LinePosition);

                var name = reader.Name;

                reader.ReadAttributeValue();
                var value = reader.Value;

                var attributeEndPos = new LineInfo(reader.LineNumber, reader.LinePosition + value.Length);

                var startPos = finder.GetCharacterPosition(attributeStartPos);
                var endPos = finder.GetCharacterPosition(attributeEndPos);

                attributes.Add(new TerminalNode
                                   {
                                       Type = NodeType.Attribute,
                                       Name = name,
                                       LocationSpan = new LocationSpan(attributeStartPos, attributeEndPos),
                                       Span = new CharacterSpan(startPos, endPos),
                                   });
            }
            while (reader.MoveToNextAttribute());

            parent.Children.AddRange(attributes);
        }

        private static int GetPositionCorrectionByReader(XmlReader reader) => GetPositionCorrectionByXmlNodeType(reader.NodeType);

        private static int GetPositionCorrectionByXmlNodeType(XmlNodeType type)
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

        //// ATTENTION !!!! SIDE EFFECT AS WE READ FURTHER !!!!
        private static LocationSpan GetLocationSpan(XmlTextReader reader)
        {
            var startCorrection = GetPositionCorrectionByReader(reader);
            var start = new LineInfo(reader.LineNumber, reader.LinePosition - startCorrection);

            if (reader.Read())
            {
                var endCorrection = GetPositionCorrectionByReader(reader);
                var end = new LineInfo(reader.LineNumber, reader.LinePosition - endCorrection - 1); // previous character needed

                return new LocationSpan(start, end);
            }
            else
            {
                return new LocationSpan(start, start);
            }
        }

        private static CharacterSpan GetCharacterSpan(LocationSpan locationSpan, CharacterPositionFinder finder)
        {
            var startPos = finder.GetCharacterPosition(locationSpan.Start);
            var endPos = finder.GetCharacterPosition(locationSpan.End);
            return new CharacterSpan(startPos, endPos);
        }
    }
}