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
            using (var reader = new XmlTextReader(filePath))
            {
                var file = new File
                               {
                                   Name = filePath,
                                   FooterSpan = new CharacterSpan(0, -1), // there is no footer
                               };

                var map = CreateCharacterCountUntilLineMap(filePath);

                var fileBegin = new LineInfo(reader.LineNumber + 1, reader.LinePosition);

                try
                {
                    var dummyRoot = new Container();

                    // Parse the XML and display the text content of each of the elements.
                    while (reader.Read())
                    {
                        Parse(reader, dummyRoot, map);
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
                        file.FooterSpan = GetCharacterSpan(new LocationSpan(positionAfterLastElement, fileEnd), map); // TODO: RKN user FooterSpan (0, -1) if there is no footer
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

        private static IReadOnlyDictionary<int, int> CreateCharacterCountUntilLineMap(string filePath)
        {
            var i = 1;
            var count = -1;

            var characterCountUntilLine = new Dictionary<int, int> { { 0, count } };
            foreach (var line in System.IO.File.ReadLines(filePath))
            {
                count += line.Length + Environment.NewLine.Length;
                characterCountUntilLine[i++] = count;
            }

            characterCountUntilLine[i] = count;

            return characterCountUntilLine;
        }

        private static void Parse(XmlTextReader reader, Container parent, IReadOnlyDictionary<int, int> lineLengthMap)
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                {
                    ParseElement(reader, parent, lineLengthMap);
                    break;
                }

                case XmlNodeType.ProcessingInstruction:
                {
                    ParseProcessingInstruction(reader, parent, lineLengthMap);
                    break;
                }

                case XmlNodeType.Comment:
                {
                    ParseComment(reader, parent, lineLengthMap);
                    break;
                }

                case XmlNodeType.XmlDeclaration:
                {
                    ParseXmlDeclaration(reader, parent, lineLengthMap);
                    break;
                }
            }
        }

        private static void ParseXmlDeclaration(XmlTextReader reader, Container parent, IReadOnlyDictionary<int, int> lineLengthMap)
        {
            var name = reader.Value;
            var locationSpan = GetLocationSpan(reader);
            var span = GetCharacterSpan(locationSpan, lineLengthMap);

            parent.Children.Add(new TerminalNode
                                    {
                                        Type = nameof(XmlNodeType.XmlDeclaration),
                                        Name = name,
                                        LocationSpan = locationSpan,
                                        Span = span,
                                    });
        }

        private static void ParseComment(XmlTextReader reader, Container parent, IReadOnlyDictionary<int, int> lineLengthMap)
        {
            var name = reader.Value;
            var locationSpan = GetLocationSpan(reader);
            var span = GetCharacterSpan(locationSpan, lineLengthMap);

            parent.Children.Add(new TerminalNode
                                    {
                                        Type = nameof(XmlNodeType.Comment),
                                        Name = name,
                                        LocationSpan = locationSpan,
                                        Span = span,
                                    });
        }

        private static void ParseProcessingInstruction(XmlTextReader reader, Container parent, IReadOnlyDictionary<int, int> lineLengthMap)
        {
            var name = reader.Name;
            var locationSpan = GetLocationSpan(reader);
            var span = GetCharacterSpan(locationSpan, lineLengthMap);

            parent.Children.Add(new TerminalNode
                                    {
                                        Type = nameof(XmlNodeType.ProcessingInstruction),
                                        Name = name,
                                        LocationSpan = locationSpan,
                                        Span = span,
                                    });
        }

        private static void ParseElement(XmlTextReader reader, Container parent, IReadOnlyDictionary<int, int> lineLengthMap)
        {
            var container = new Container
                                {
                                    Type = nameof(XmlNodeType.Element),
                                    Name = reader.Name,
                                };

            parent.Children.Add(container);

            var isEmpty = reader.IsEmptyElement;

            if (isEmpty)
            {
                var locationSpan = GetLocationSpan(reader);
                var headerSpan = GetCharacterSpan(locationSpan, lineLengthMap);
                var footerSpan = new CharacterSpan(0, -1); // no footer

                container.LocationSpan = locationSpan;
                container.HeaderSpan = headerSpan;
                container.FooterSpan = footerSpan;
            }
            else
            {
                var startingSpan = GetLocationSpan(reader);

                while (reader.NodeType != XmlNodeType.EndElement)
                {
                    Parse(reader, container, lineLengthMap);

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
                container.HeaderSpan = GetCharacterSpan(startingSpan, lineLengthMap);
                container.FooterSpan = GetCharacterSpan(endingSpan, lineLengthMap);
            }
        }

        private static int GetPositionCorrectionByReader(XmlReader reader)
        {
            switch (reader.NodeType)
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

        private static CharacterSpan GetCharacterSpan(LocationSpan locationSpan, IReadOnlyDictionary<int, int> lineLengthMap)
        {
            var startPos = GetCharacterPosition(locationSpan.Start, lineLengthMap);
            var endPos = GetCharacterPosition(locationSpan.End, lineLengthMap);
            return new CharacterSpan(startPos, endPos);
        }

        private static int GetCharacterPosition(LineInfo lineInfo, IReadOnlyDictionary<int, int> lineLengthMap)
        {
            var position = lineLengthMap[lineInfo.LineNumber - 1] + lineInfo.LinePosition;
            return position;
        }
    }
}