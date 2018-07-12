using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

using File = System.IO.File;

namespace MiKoSolutions.SemanticParsers.Xml
{
    public sealed class Parser
    {
        private readonly List<string> _lines = new List<string>();

        public Yaml.File Parse(string path)
        {
            _lines.Clear();

            var newLine = Environment.NewLine;

            var allText = File.ReadAllText(path);

            var allLines = allText.Split(new[] { newLine }, StringSplitOptions.None);
            foreach (var line in allLines)
            {
                _lines.Add(line + newLine);
            }

            var document = XDocument.Parse(allText, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
            var documentRoot = document.Root;

            var root = new Container
                           {
                               Type = documentRoot.NodeType.ToString(),
                               Name = documentRoot.Name.LocalName,
                               LocationSpan = GetLocationSpan(documentRoot),
                           };

            root.Children.AddRange(ParseElement(documentRoot));

            var file = new Yaml.File
                           {
                               Name = path,
                               LocationSpan = new LocationSpan(new LineInfo(1, 0), new LineInfo(allLines.Length, 0)),
                               FooterSpan = new CharacterSpan(GetCharacterLengthToPosition(documentRoot.NextNode), allText.Length - 1),
                           };
            file.Children.Add(root);

            Resort(file.Children);

            return file;
        }

        private static XNode FindNextNode(XNode node) => node.NextNode ?? FindNextNode(node.Parent);

        private ContainerOrTerminalNode Parse(XNode node)
        {
            var nodeType = node.NodeType;
            switch (nodeType)
            {
                case XmlNodeType.Element:
                {
                    var element = (XElement)node;

                    var container = new Container
                                        {
                                            Type = nodeType.ToString(),
                                            Name = element.Name.LocalName,
                                            LocationSpan = GetLocationSpan(node),
                                            // HeaderSpan = TODO: RKN
                                            // FooterSpan = // TODO: RKN : get next node, get at that position and take a look until the first '</' or '/>' is found --> that's the footer
                                        };

                    container.Children.AddRange(ParseElement(element));

                    return container;
                }

                case XmlNodeType.ProcessingInstruction:
                case XmlNodeType.Comment:
                {
                    return new TerminalNode
                               {
                                   Type = nodeType.ToString(),
                                   Name = node.ToString(),
                                   LocationSpan = GetLocationSpan(node),
                                   Span = GetCharacterSpan(node),
                               };
                }

                default:
                    return null;
            }
        }

        private IEnumerable<ContainerOrTerminalNode> ParseElement(XElement element) => element.Nodes().Select(Parse).Where(_ => _ != null);

        private CharacterSpan GetCharacterSpan(XNode node)
        {
            IXmlLineInfo info = node;
            IXmlLineInfo next = FindNextNode(node);

            // we have to correct position because we want the leading '<' or trailing '>'
            var startCorrection = GetStartCorrection(info);
            var endCorrection = GetEndCorrection(next);

            var start = GetCharacterLengthToLine(info) + startCorrection;
            var end = GetCharacterLengthToLine(next) + endCorrection;

            return new CharacterSpan(start, end);
        }

        private int GetCharacterLengthToPosition(IXmlLineInfo info) => GetCharacterLengthToLine(info) + info.LinePosition; // TODO: RKN Footer is strange

        private int GetCharacterLengthToLine(IXmlLineInfo info) => GetCharacterLengthToLine(info.LineNumber - 1);

        private int GetCharacterLengthToLine(int line) => _lines.Take(line).Sum(_ => _.Length);

        private int GetStartCorrection(IXmlLineInfo info) => GetCorrection(info, '<');

        private int GetEndCorrection(IXmlLineInfo info) => GetCorrection(info, '>');

        private int GetCorrection(IXmlLineInfo info, char value)
        {
            if (info is null)
            {
                return -1;
            }

            var line = _lines[info.LineNumber - 1];
            return line.Substring(0, info.LinePosition).LastIndexOf(value) + 1;
        }

        private LocationSpan GetLocationSpan(XNode node)
        {
            IXmlLineInfo info = node;
            IXmlLineInfo next = FindNextNode(node); // we won't have a nested child

            // we have to correct position because we want to be before the leading '<' or after the trailing '>'
            var startPosition = GetStartCorrection(info);
            var endPosition = GetEndCorrection(next);

            var start = new LineInfo(info.LineNumber, startPosition);
            var end = new LineInfo(next.LineNumber, endPosition);

            return new LocationSpan(start, end);
        }

        private void Resort(List<ContainerOrTerminalNode> nodes)
        {
            nodes.Sort(CompareStartPosition);

            foreach (var node in nodes.OfType<Container>())
            {
                Resort(node.Children);
            }
        }

        private void Resort(List<Container> nodes)
        {
            nodes.Sort(CompareStartPosition);

            foreach (var node in nodes)
            {
                Resort(node.Children);
            }
        }

        private static int CompareStartPosition(ContainerOrTerminalNode x, ContainerOrTerminalNode y)
        {
            var xLineNumber = x.LocationSpan.Start.LineNumber;
            var yLineNumber = y.LocationSpan.Start.LineNumber;
            if (xLineNumber < yLineNumber)
            {
                return -1;
            }

            if (xLineNumber > yLineNumber)
            {
                return 1;
            }

            return x.LocationSpan.Start.LinePosition - y.LocationSpan.Start.LinePosition;
        }
    }
}