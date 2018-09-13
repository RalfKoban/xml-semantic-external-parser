using System;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public sealed class XmlFlavorForPackagesConfig : XmlFlavor
    {
        public override bool ParseAttributesEnabled => false;

        public override bool Supports(string filePath) => filePath.EndsWith(".config", StringComparison.OrdinalIgnoreCase); // do not check for "packages.config" as e.g. TFS might use a temp <some guid>.config file instead

        public override bool Supports(DocumentInfo info) => string.Equals(info.RootElement, "packages", StringComparison.OrdinalIgnoreCase);

        public override string GetName(XmlTextReader reader)
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                {
                    return reader.GetAttribute("id") ?? reader.Name;
                }

                case XmlNodeType.Attribute:
                {
                    return $"{reader.Name} '{reader.Value}'";
                }

                default:
                {
                    return base.GetName(reader);
                }
            }
        }

        public override string GetType(XmlTextReader reader) => reader.NodeType == XmlNodeType.Element ? reader.Name : base.GetType(reader);

        protected override bool ShallBeTerminalNode(ContainerOrTerminalNode node) => node?.Type == "package";
    }
}