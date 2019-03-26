using System;
using System.Collections.Generic;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public sealed class XmlFlavorForNLS : XmlFlavor
    {
        // localization
        private static readonly HashSet<string> TerminalNodeNames = new HashSet<string>
                                                                        {
                                                                            ElementNames.Comment,
                                                                            ElementNames.Date,
                                                                            ElementNames.Language,
                                                                            ElementNames.Locale,
                                                                        };

        public override bool ParseAttributesEnabled => false;

        public override bool Supports(string filePath) => filePath.EndsWith(".xml", StringComparison.OrdinalIgnoreCase);

        public override bool Supports(DocumentInfo info) => string.Equals(info.RootElement, ElementNames.Localization, StringComparison.OrdinalIgnoreCase);

        public override string GetName(XmlTextReader reader)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                var name = reader.LocalName;
                var attr = GetAttributeName(name);

                if (attr is null) return name;
                return reader.GetAttribute(attr);
            }

            return base.GetName(reader);
        }

        public override string GetType(XmlTextReader reader) => reader.NodeType == XmlNodeType.Element ? reader.Name : base.GetType(reader);

        protected override bool ShallBeTerminalNode(ContainerOrTerminalNode node) => TerminalNodeNames.Contains(node?.Type);

        private string GetAttributeName(string elementName)
        {
            switch (elementName)
            {
                case ElementNames.Language: return AttributeNames.LCID;
                case ElementNames.Locale: return AttributeNames.Code;
                case ElementNames.String: return AttributeNames.Key;
                default: return elementName;
            }
        }

        private static class ElementNames
        {
            internal const string Comment = "comment";
            internal const string Date = "date";
            internal const string Language = "language";
            internal const string Locale = "locale";
            internal const string Localization = "localization";
            internal const string String = "string";
        }

        private static class AttributeNames
        {
            internal const string LCID = "lcid";
            internal const string Code = "code";
            internal const string Key = "key";
        }
    }
}