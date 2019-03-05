using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public sealed class XmlFlavorForEdmxV3 : XmlFlavor
    {
        private const string Namespace = "http://schemas.microsoft.com/ado/2009/11/edmx";

        private static readonly HashSet<string> TerminalNodeNames = new HashSet<string>
                                                                        {
                                                                            "EntitySet",
                                                                            "AssociationSet",
                                                                            //// "Association", // Unsure whether that's necessary
                                                                            "End",
                                                                            "OnDelete",
                                                                            "Key",
                                                                            "Principal",
                                                                            "PropertyRef",
                                                                            "Property",
                                                                            "NavigationProperty",
                                                                            "ScalarProperty",
                                                                            "Dependent",

                                                                            "DesignerProperty",
                                                                            "EntityTypeShape",
                                                                            "InheritanceConnector",
                                                                            "AssociationConnector",
                                                                            "edmx:DesignerProperty",
                                                                            "edmx:EntityTypeShape",
                                                                            "edmx:InheritanceConnector",
                                                                            "edmx:AssociationConnector",
                                                                        };

        public override bool ParseAttributesEnabled => false;

        public override bool Supports(string filePath) => filePath.EndsWith(".edmx", StringComparison.OrdinalIgnoreCase);

        public override bool Supports(DocumentInfo info) => string.Equals(info.RootElement, "Edmx", StringComparison.OrdinalIgnoreCase)
                                                            && string.Equals(info.Namespace, Namespace, StringComparison.OrdinalIgnoreCase);

        public override string GetName(XmlTextReader reader)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                var name = reader.Name;
                switch (name)
                {
                    case "Schema":
                        return GetIdentifier(reader, "Namespace") ?? name;

                    case "FunctionImportMapping":
                        return GetIdentifier(reader, "FunctionName") ?? name;

                    default:
                        var identifier = GetIdentifier(reader, "Name", "Role", "TypeName", "StoreEntitySet", "EntityType", "Association");
                        return identifier ?? name;
                }
            }

            return base.GetName(reader);
        }

        public override string GetType(XmlTextReader reader) => reader.NodeType == XmlNodeType.Element ? reader.Name : base.GetType(reader);

        protected override bool ShallBeTerminalNode(ContainerOrTerminalNode node) => TerminalNodeNames.Contains(node?.Type);

        private static string GetIdentifier(XmlTextReader reader, params string[] attributeNames) => attributeNames.Select(reader.GetAttribute).FirstOrDefault(_ => _ != null);
    }
}