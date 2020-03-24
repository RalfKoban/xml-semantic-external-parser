using System;
using System.Xml;
using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public sealed class XmlFlavorForRanorexRepository : XmlFlavor
    {
        public override bool ParseAttributesEnabled => false;

        public override bool Supports(string filePath) => filePath.EndsWith(".rxrep", StringComparison.OrdinalIgnoreCase);

        public override bool Supports(DocumentInfo info) => string.Equals(info.RootElement, "repository", StringComparison.OrdinalIgnoreCase);

        public override string GetName(XmlReader reader)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                return reader.GetAttribute("name") ?? reader.LocalName;
            }

            return base.GetName(reader);
        }

        public override string GetType(XmlReader reader) => reader.NodeType == XmlNodeType.Element ? reader.LocalName : base.GetType(reader);

        protected override bool ShallBeTerminalNode(ContainerOrTerminalNode node)
        {
            switch (node?.Type)
            {
                case "item":
                case "icon":
                case "basepath":
                case "var":
                    return true;

                default:
                    return false;
            }
        }
    }
}