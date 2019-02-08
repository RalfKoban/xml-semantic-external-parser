using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public sealed class XmlFlavorForMaml : XmlFlavor
    {
        private static readonly HashSet<string> TerminalNodeNames = new HashSet<string>
                                                                        {
                                                                            ElementNames.alert,
                                                                            ElementNames.autoOutline,
                                                                            ElementNames.code,
                                                                            ElementNames.codeEntityReference,
                                                                            ElementNames.definition,
                                                                            ElementNames.definedTerm,
                                                                            ElementNames.entry,
                                                                            ElementNames.externalLink,
                                                                            ElementNames.image,
                                                                            ElementNames.link,
                                                                            ElementNames.listItem,
                                                                            ElementNames.para,
                                                                            ElementNames.row,
                                                                        };

        public override bool ParseAttributesEnabled => false;

        public override bool Supports(string filePath) => filePath.EndsWith(".aml", StringComparison.OrdinalIgnoreCase);

        public override bool Supports(DocumentInfo info) => string.Equals(info.RootElement, ElementNames.topic, StringComparison.OrdinalIgnoreCase);

        public override string GetName(XmlTextReader reader) => reader.NodeType == XmlNodeType.Element ? GetIdentifier(reader) : base.GetName(reader);

        public override string GetType(XmlTextReader reader) => reader.NodeType == XmlNodeType.Element ? reader.Name : base.GetType(reader);

        public override ContainerOrTerminalNode FinalAdjustAfterParsingComplete(ContainerOrTerminalNode node)
        {
            if (node is Container c)
            {
                switch (c.Type)
                {
                    case ElementNames.section:
                    case ElementNames.procedure:
                    {
                        string name = null;

                        var titleNode = c.Children.FirstOrDefault(_ => _.Type == ElementNames.title);
                        if (titleNode != null)
                        {
                            name = titleNode.Name;
                            c.Name = name;

                            // reset title node
                            titleNode.Name = null;
                        }

                        var contentNode = c.Children.FirstOrDefault(_ => _.Type == ElementNames.content);
                        if (contentNode != null)
                        {
                            contentNode.Name = name + " Contents";
                        }

                        break;
                    }

                    case ElementNames.externalLink:
                    {
                        c.Name = c.Children.FirstOrDefault(_ => _.Type == ElementNames.linkText)?.Name;
                        break;
                    }

                    case ElementNames.linkText:
                    case ElementNames.title:
                    {
                        c.Name = c.Children.FirstOrDefault(_ => _.Type == NodeType.Text)?.Content.Trim();
                        return c.ToTerminalNode();
                    }
                }
            }

            return base.FinalAdjustAfterParsingComplete(node);
        }

        protected override bool ShallBeTerminalNode(ContainerOrTerminalNode node) => TerminalNodeNames.Contains(node?.Type);

        private string GetIdentifier(XmlTextReader reader)
        {
            switch (reader.Name)
            {
                case ElementNames.topic:
                    return reader.GetAttribute(AttributeNames.id);

                case ElementNames.code:
                    return reader.GetAttribute(AttributeNames.title)?.Trim();

                default:
                    return null;
            }
        }

        private static class ElementNames
        {
            internal const string alert = "alert";
            internal const string autoOutline = "autoOutline";
            internal const string code = "code";
            internal const string codeEntityReference = "codeEntityReference";
            internal const string content = "content";
            internal const string definition = "definition";
            internal const string definedTerm = "definedTerm";
            internal const string entry = "entry";
            internal const string externalLink = "externalLink";
            internal const string image = "image";
            internal const string link = "link";
            internal const string linkText = "linkText";
            internal const string listItem = "listItem";
            internal const string para = "para";
            internal const string procedure = "procedure";
            internal const string row = "row";
            internal const string section = "section";
            internal const string title = "title";
            internal const string topic = "topic";
        }

        private static class AttributeNames
        {
            internal const string id = "id";
            internal const string title = "title";
        }
    }
}