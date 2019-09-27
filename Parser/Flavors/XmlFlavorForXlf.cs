using System;
using System.Collections.Generic;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public sealed class XmlFlavorForXlf : XmlFlavor
    {
        private static readonly HashSet<string> TerminalNodeNames = new HashSet<string>
                                                                        {
                                                                            ElementNames.Source,
                                                                            ElementNames.Target,
                                                                            ElementNames.TransUnit,
                                                                        };

        public override bool ParseAttributesEnabled => false;

        public override bool Supports(string filePath) => filePath.EndsWith(".xlf", StringComparison.OrdinalIgnoreCase);

        public override bool Supports(DocumentInfo info) => string.Equals(info.RootElement, ElementNames.XLiff, StringComparison.OrdinalIgnoreCase);

        public override string GetName(XmlReader reader)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                var name = reader.LocalName;
                var attr = GetAttributeName(name);

                if (attr is null)
                {
                    return name;
                }

                return reader.GetAttribute(attr);
            }

            return base.GetName(reader);
        }

        public override string GetType(XmlReader reader) => reader.NodeType == XmlNodeType.Element ? reader.LocalName : base.GetType(reader);

        protected override bool ShallBeTerminalNode(ContainerOrTerminalNode node) => TerminalNodeNames.Contains(node?.Type);

        private string GetAttributeName(string elementName)
        {
            switch (elementName)
            {
                case ElementNames.File: return AttributeNames.Original;
                case ElementNames.TransUnit: return AttributeNames.Id;
                default: return elementName;
            }
        }

        private static class ElementNames
        {
            internal const string File = "file";
            internal const string TransUnit = "trans-unit";
            internal const string Source = "source";
            internal const string Target = "target";

            internal const string XLiff = "xliff";
        }

        private static class AttributeNames
        {
            internal const string Original = "original";
            internal const string Id = "id";
        }
    }
}