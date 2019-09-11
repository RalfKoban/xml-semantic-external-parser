using System;
using System.Collections.Generic;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public sealed class XmlFlavorForDependencies : XmlFlavor
    {
        private static readonly HashSet<string> TerminalNodeNames = new HashSet<string>
                                                                        {
                                                                            ElementNames.Dependency,
                                                                        };

        public override bool ParseAttributesEnabled => false;

        public override bool Supports(DocumentInfo info) => string.Equals(info.RootElement, ElementNames.Dependencies, StringComparison.OrdinalIgnoreCase);

        public override string GetName(XmlReader reader)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                var name = reader.Name;
                var attributeName = GetAttributeName(name);
                var identifier = GetAttribute(reader, attributeName);

                return identifier ?? name;
            }

            return base.GetName(reader);
        }

        public override string GetType(XmlReader reader) => reader.NodeType == XmlNodeType.Element ? reader.LocalName : base.GetType(reader);

        protected override bool ShallBeTerminalNode(ContainerOrTerminalNode node) => TerminalNodeNames.Contains(node?.Type);

        private static string GetAttributeName(string name)
        {
            switch (name)
            {
                case ElementNames.Dependency:
                    return AttributeNames.Name;

                default:
                    return null;
            }
        }

        private static string GetAttribute(XmlReader reader, string attributeName) => attributeName is null ? null : reader.GetAttribute(attributeName);

        private static class ElementNames
        {
            internal const string Dependencies = "Dependencies";
            internal const string Dependency = "Dependency";
        }

        private static class AttributeNames
        {
            internal const string Name = "Name";
        }
    }
}