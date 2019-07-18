using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public sealed class XmlFlavorForXsdSchemaDefinitions : XmlFlavor
    {
        private const string Namespace = "http://www.w3.org/2001/XMLSchema";

        private static readonly HashSet<string> TerminalNodeNames = new HashSet<string>
                                                                        {
                                                                            "xs:annotation",
                                                                            "xs:documentation",

                                                                            "xs:attribute",
                                                                            "xs:enumeration",
                                                                        };

        public override bool ParseAttributesEnabled => false;

        public override bool Supports(string filePath) => filePath.EndsWith(".xsd", StringComparison.OrdinalIgnoreCase);

        public override bool Supports(DocumentInfo info) => string.Equals(info.RootElement, "schema", StringComparison.OrdinalIgnoreCase)
                                                            && string.Equals(info.Namespace, Namespace, StringComparison.OrdinalIgnoreCase);

        public override string GetName(XmlTextReader reader)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                var name = reader.Name;
                var identifier = GetIdentifier(reader, "name", "value", "id");
                return identifier ?? name;
            }

            return base.GetName(reader);
        }

        public override string GetType(XmlTextReader reader) => reader.NodeType == XmlNodeType.Element ? reader.Name : base.GetType(reader);

        protected override bool ShallBeTerminalNode(ContainerOrTerminalNode node) => TerminalNodeNames.Contains(node?.Type);

        private static string GetIdentifier(XmlTextReader reader, params string[] attributeNames) => attributeNames.Select(reader.GetAttribute).FirstOrDefault(_ => _ != null);
    }
}