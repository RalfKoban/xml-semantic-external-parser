using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public sealed class XmlFlavorForLayerDiagram : XmlFlavor
    {
        private static readonly HashSet<string> TerminalNodeNames = new HashSet<string>
                                                                        {
                                                                            "comment",
                                                                            "reference",
                                                                            "dependencyFromLayerToLayer",
                                                                        };

        public override bool ParseAttributesEnabled => false;

        public override string PreferredNamespacePrefix => "xmlns";

        public override bool Supports(string filePath) => filePath.EndsWith(".layerdiagram", StringComparison.OrdinalIgnoreCase);

        public override bool Supports(DocumentInfo info) =>
                                            string.Equals(info.RootElement, "layerModel", StringComparison.OrdinalIgnoreCase) &&
                                            string.Equals(info.Namespace, "http://schemas.microsoft.com/VisualStudio/TeamArchitect/LayerDesigner", StringComparison.OrdinalIgnoreCase);

        public override string GetName(XmlTextReader reader)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                var elementName = reader.LocalName;
                var name = reader.GetAttribute("name");
                return name ?? elementName;
            }

            return base.GetName(reader);
        }

        public override string GetType(XmlTextReader reader)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                var elementName = reader.LocalName;

                var id = reader.GetAttribute("Id");

                return id is null ? elementName : $"{elementName} ID=({id})";
            }

            return base.GetType(reader);
        }

        public override ContainerOrTerminalNode FinalAdjustAfterParsingComplete(ContainerOrTerminalNode node)
        {
            if (node.Type.StartsWith("layer ") && node is Container c)
            {
                AdjustChildType(c, "childLayers");
                AdjustChildType(c, "references");
                AdjustChildType(c, "dependencyToLayers");
            }

            return base.FinalAdjustAfterParsingComplete(node);
        }

        protected override bool ShallBeTerminalNode(ContainerOrTerminalNode node)
        {
            var nodeType = node?.Type ?? string.Empty;

            if (TerminalNodeNames.Contains(nodeType))
            {
                return true;
            }

            foreach (var name in TerminalNodeNames)
            {
                if (nodeType.StartsWith(name + " "))
                {
                    return true;
                }
            }

            return false;
        }

        private static void AdjustChildType(Container node, string type)
        {
            var child = node.Children.FirstOrDefault(_ => _.Type == type);
            if (child != null)
            {
                child.Type = string.Concat(child.Type, " (", node.Type, ")");
            }
        }
    }
}