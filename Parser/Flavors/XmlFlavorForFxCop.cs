using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public sealed class XmlFlavorForFxCop : XmlFlavor
    {
        private static readonly HashSet<string> TerminalNodeNames = new HashSet<string>
                                                                           {
                                                                               "Acronym",
                                                                               "Term",
                                                                               "Word",
                                                                           };

        public override bool ParseAttributesEnabled => false;

        public override bool Supports(string filePath) => filePath.StartsWith("CustomDictionary", StringComparison.OrdinalIgnoreCase) && filePath.EndsWith(".xml", StringComparison.OrdinalIgnoreCase);

        public override bool Supports(DocumentInfo info) => string.Equals(info.RootElement, "Dictionary", StringComparison.OrdinalIgnoreCase);

        public override string GetType(XmlReader reader) => reader.NodeType == XmlNodeType.Element ? reader.Name : base.GetType(reader);

        public override ContainerOrTerminalNode FinalAdjustAfterParsingComplete(ContainerOrTerminalNode node)
        {
            if (node is Container c)
            {
                var textNode = c.Children.FirstOrDefault(_ => _.Type == NodeType.Text);
                if (textNode != null)
                {
                    c.Name = textNode.Content;
                }
            }

            return base.FinalAdjustAfterParsingComplete(node);
        }

        protected override bool ShallBeTerminalNode(ContainerOrTerminalNode node) => TerminalNodeNames.Contains(node?.Type);
    }
}