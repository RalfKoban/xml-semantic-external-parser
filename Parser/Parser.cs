using System.Linq;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml
{
    public static class Parser
    {
        public static File Parse(string filePath)
        {
            using (var reader = new XmlTextReader(filePath))
            {
                var fileBegin = new LineInfo(reader.LineNumber + 1, reader.LinePosition);

                var root = new Container
                               {
                                   Type = "root",
                                   Name = "root",
                               };

                // Parse the XML and display the text content of each of the elements.
                while (reader.Read())
                {
                    Parse(reader, root);
                }

                root.LocationSpan = new LocationSpan(root.Children.First().LocationSpan.Start, root.Children.Last().LocationSpan.End);

                var fileEnd = new LineInfo(reader.LineNumber, reader.LinePosition - 1);

                var file = new File
                               {
                                   Name = filePath,
                                   LocationSpan = new LocationSpan(fileBegin, fileEnd),
                                   // FooterSpan = new CharacterSpan(GetCharacterLengthToPosition(documentRoot.NextNode), allText.Length - 1),
                               };

                file.Children.Add(root);

                return file;
            }
        }

        private static void Parse(XmlTextReader reader, Container parent)
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                {
                    ParseElement(reader, parent);
                    break;
                }

                case XmlNodeType.ProcessingInstruction:
                {
                    ParseProcessingInstruction(reader, parent);
                    break;
                }

                case XmlNodeType.Comment:
                {
                    ParseComment(reader, parent);
                    break;
                }

                case XmlNodeType.XmlDeclaration:
                {
                    ParseXmlDeclaration(reader, parent);
                    break;
                }
            }
        }

        private static void ParseXmlDeclaration(XmlTextReader reader, Container parent)
        {
            var name = reader.Value;
            var locationSpan = GetLocationSpan(reader);

            var node = new TerminalNode
                           {
                               Type = nameof(XmlNodeType.XmlDeclaration),
                               Name = name,
                               LocationSpan = locationSpan,
                               // Span = GetCharacterSpan(node),
                           };

            parent.Children.Add(node);
        }

        private static void ParseComment(XmlTextReader reader, Container parent)
        {
            var name = reader.Value;
            var locationSpan = GetLocationSpan(reader);

            var node = new TerminalNode
                           {
                               Type = nameof(XmlNodeType.Comment),
                               Name = name,
                               LocationSpan = locationSpan,
                               // Span = GetCharacterSpan(node),
                           };

            parent.Children.Add(node);
        }

        private static void ParseProcessingInstruction(XmlTextReader reader, Container parent)
        {
            var name = reader.Name;
            var locationSpan = GetLocationSpan(reader);

            var node = new TerminalNode
                           {
                               Type = nameof(XmlNodeType.ProcessingInstruction),
                               Name = name,
                               LocationSpan = locationSpan,
                               // Span = GetCharacterSpan(node),
                           };

            parent.Children.Add(node);
        }

        private static void ParseElement(XmlTextReader reader, Container parent)
        {
            var container = new Container
                                {
                                    Type = nameof(XmlNodeType.Element),
                                    Name = reader.Name,
                                    // HeaderSpan = TODO: RKN
                                    // FooterSpan = // TODO: RKN : get next node, get at that position and take a look until the first '</' or '/>' is found --> that's the footer
                                };

            parent.Children.Add(container);

            var isEmpty = reader.IsEmptyElement;

            if (isEmpty)
            {
                container.LocationSpan = GetLocationSpan(reader);
            }
            else
            {
                var startingSpan = GetLocationSpan(reader);

                while (reader.NodeType != XmlNodeType.EndElement)
                {
                    Parse(reader, container);

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
    }
}