using System;
using System.Linq;
using System.Text;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Flavors;
using MiKoSolutions.SemanticParsers.Xml.Yaml;

using Container = MiKoSolutions.SemanticParsers.Xml.Yaml.Container;
using File = MiKoSolutions.SemanticParsers.Xml.Yaml.File;

using StreamReader = System.IO.StreamReader;
using SystemFile = System.IO.File;

namespace MiKoSolutions.SemanticParsers.Xml
{
    public static class Parser
    {
        // we have issues with UTF-8 encodings in files that should have an encoding='iso-8859-1'
        public static File Parse(string filePath, string encoding = "iso-8859-1")
        {
            var flavor = XmlFlavorFinder.Find(filePath);

            Tracer.Trace($"Using {flavor.GetType().Name} flavor for '{filePath}'.");

            File file;
            using (var finder = CharacterPositionFinder.CreateFrom(filePath))
            {
                file = ParseCore(filePath, finder, flavor, Encoding.GetEncoding(encoding));

                Resorter.Resort(file);
                GapFiller.Fill(file, finder);
            }

            CommentCleaner.Clean(file);

            return file;
        }

        public static File ParseCore(string filePath, CharacterPositionFinder finder, IXmlFlavor flavor, Encoding encoding)
        {
            using (var reader = new XmlTextReader(new StreamReader(SystemFile.OpenRead(filePath), encoding)))
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
                        Parse(reader, dummyRoot, finder, flavor);
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
                    // try to adjust location span to include full file content
                    // but ignore empty files as parsing errors
                    var lines = SystemFile.ReadLines(filePath).Count();
                    if (lines == 0)
                    {
                        file.LocationSpan = new LocationSpan(new LineInfo(0, -1), new LineInfo(0, -1));
                    }
                    else
                    {
                        file.ParsingErrors.Add(new ParsingError
                                                   {
                                                       ErrorMessage = ex.Message,
                                                       Location = new LineInfo(ex.LineNumber, ex.LinePosition),
                                                   });

                        file.LocationSpan = new LocationSpan(new LineInfo(1, 0), new LineInfo(lines + 1, 0));
                    }
                }

                return file;
            }
        }

        private static void Parse(XmlTextReader reader, Container parent, CharacterPositionFinder finder, IXmlFlavor flavor)
        {
            var nodeType = reader.NodeType;
            switch (nodeType)
            {
                case XmlNodeType.Element:
                {
                    ParseElement(reader, parent, finder, flavor);
                    break;
                }

                case XmlNodeType.ProcessingInstruction:
                case XmlNodeType.Comment:
                case XmlNodeType.XmlDeclaration:
                case XmlNodeType.CDATA:
                case XmlNodeType.Text:
                {
                    ParseTerminalNode(reader, parent, finder, flavor);
                    break;
                }
            }
        }

        private static void ParseElement(XmlTextReader reader, Container parent, CharacterPositionFinder finder, IXmlFlavor flavor)
        {
            var name = flavor.GetName(reader);
            var type = flavor.GetType(reader);
            var content = flavor.GetContent(reader);

            var container = new Container
                                {
                                    Type = type,
                                    Name = name,
                                    Content = content,
                                };

            var isEmpty = reader.IsEmptyElement;

            var startingSpan = GetLocationSpanWithParseAttributes(reader, container, finder, flavor);
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
                    Parse(reader, container, finder, flavor);

                    // we had a side effect (reading further on stream to get the location span), so we have to check whether we found already an element or end element
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        continue;
                    }

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
                var footerSpan = GetCharacterSpan(endingSpan, finder);

                container.LocationSpan = new LocationSpan(startingSpan.Start, endingSpan.End);
                container.HeaderSpan = headerSpan;
                container.FooterSpan = footerSpan;
            }

            // check whether we can use a terminal node instead
            var child = flavor.FinalAdjustAfterParsingComplete(container);

            parent.Children.Add(child);
        }

        private static void ParseAttributes(XmlTextReader reader, Container parent, CharacterPositionFinder finder, IXmlFlavor flavor)
        {
            if (flavor.ParseAttributesEnabled)
            {
                reader.MoveToFirstAttribute();

                // read all attributes
                do
                {
                    ParseAttribute(reader, parent, finder, flavor);
                }
                while (reader.MoveToNextAttribute());
            }
        }

        private static void ParseAttribute(XmlTextReader reader, Container parent, CharacterPositionFinder finder, IXmlFlavor flavor)
        {
            var attributeStartPos = new LineInfo(reader.LineNumber, reader.LinePosition);

            var name = flavor.GetName(reader);
            var type = flavor.GetType(reader);

            // important call to be able to read the attribute value
            reader.ReadAttributeValue();

            var content = flavor.GetContent(reader);

            var attributeEndPos = new LineInfo(reader.LineNumber, reader.LinePosition + content.Length);

            var startPos = finder.GetCharacterPosition(attributeStartPos);
            var endPos = finder.GetCharacterPosition(attributeEndPos);

            var locationSpan = new LocationSpan(attributeStartPos, attributeEndPos);
            var span = new CharacterSpan(startPos, endPos);

            var child = AddTerminalNode(parent, type, name, content, locationSpan, span);

            flavor.FinalAdjustAfterParsingComplete(child);
        }

        private static void ParseTerminalNode(XmlTextReader reader, Container parent, CharacterPositionFinder finder, IXmlFlavor flavor)
        {
            var name = flavor.GetName(reader);
            var type = flavor.GetType(reader);
            var content = flavor.GetContent(reader);

            var locationSpan = GetLocationSpan(reader);
            var span = GetCharacterSpan(locationSpan, finder);

            var child = AddTerminalNode(parent, type, name, content, locationSpan, span);

            flavor.FinalAdjustAfterParsingComplete(child);
        }

        private static TerminalNode AddTerminalNode(Container parent, string type, string name, string content, LocationSpan locationSpan, CharacterSpan span)
        {
            var child = new TerminalNode
                           {
                               Type = type,
                               Name = name,
                               Content = content,
                               LocationSpan = locationSpan,
                               Span = span,
                           };
            parent.Children.Add(child);

            return child;
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
                case XmlNodeType.CDATA: return 9;                 // 9 is length of <![CDATA[
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
        private static LocationSpan GetLocationSpan(XmlTextReader reader, Action<XmlTextReader> callback = null)
        {
            var start = GetStartLine(reader);

            callback?.Invoke(reader);

            // read to end of character
            reader.Read();

            var end = GetEndLine(reader);

            return new LocationSpan(start, end);
        }

        //// ATTENTION !!!! SIDE EFFECT AS WE READ FURTHER !!!!
        private static LocationSpan GetLocationSpanWithParseAttributes(XmlTextReader reader, Container container, CharacterPositionFinder finder, IXmlFlavor flavor)
        {
            return GetLocationSpan(reader, _ =>
                                                {
                                                    if (_.HasAttributes)
                                                    {
                                                        // we have to read the attributes
                                                        ParseAttributes(_, container, finder, flavor);
                                                    }
                                                });
        }

        private static CharacterSpan GetCharacterSpan(LocationSpan locationSpan, CharacterPositionFinder finder)
        {
            var startPos = finder.GetCharacterPosition(locationSpan.Start);
            var endPos = finder.GetCharacterPosition(locationSpan.End);

            return new CharacterSpan(startPos, endPos);
        }

        private static LineInfo IncludeXmlDeclarationInRoot(Container root, Container dummyRoot)
        {
            // there might be no declaration, such as when trying to parse XAML files
            var xmlDeclaration = dummyRoot.Children.OfType<TerminalNode>().FirstOrDefault(_ => _.Type == NodeType.XmlDeclaration);
            if (xmlDeclaration != null)
            {
                // let root include the XML declaration
                AdjustRoot(root, xmlDeclaration);
            }

            return root.LocationSpan.End;
        }

        private static void AdjustRoot(Container root, TerminalNode node)
        {
            var rootStart = node.LocationSpan.Start;

            // adjust positions
            root.LocationSpan = new LocationSpan(rootStart, root.LocationSpan.End);
            root.HeaderSpan = new CharacterSpan(node.Span.Start, root.HeaderSpan.End);
        }
    }
}