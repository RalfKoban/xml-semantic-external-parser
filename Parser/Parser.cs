using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

using File = System.IO.File;

namespace MiKoSolutions.SemanticParsers.Xml
{
    internal class Parser
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

            return file;
        }

        private ContainerOrTerminalNode Parse(XNode node)
        {
            var nodeType = node.NodeType;
            switch (nodeType)
            {
                case XmlNodeType.Element:
                {
                    var element = (XElement) node;

                    var container = new Container
                                        {
                                            Type = nodeType.ToString(),
                                            Name = element.Name.LocalName,
                                            LocationSpan = GetLocationSpan(node),
                                            // HeaderSpan = TODO: RKN
                                            // FooterSpan = TODO: RKN
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

        private IEnumerable<ContainerOrTerminalNode> ParseElement(XElement element) => element.Nodes().Select(Parse).Where(_ => _ != null).OrderBy(_ => _.LocationSpan.Start.LineNumber).ThenBy(_ => _.LocationSpan.Start.LinePosition);

        private CharacterSpan GetCharacterSpan(XNode node)
        {
            IXmlLineInfo info = node;
            IXmlLineInfo next = node.NextNode;

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
            var line = _lines[info.LineNumber - 1];
            return line.Substring(0, info.LinePosition).LastIndexOf(value);
        }

        private LocationSpan GetLocationSpan(XNode node)
        {
            IXmlLineInfo info = node;
            IXmlLineInfo next = node.NextNode;

            // we have to correct position because we want to be before the leading '<' or after the trailing '>'
            var startPosition = GetStartCorrection(info);
            var endPosition = GetEndCorrection(next) + 1;

            var start = new LineInfo(info.LineNumber, startPosition);
            var end = new LineInfo(next.LineNumber, endPosition);

            return new LocationSpan(start, end);
        }
    }
}