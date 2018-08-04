using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public sealed class XmlFlavorForPackagesConfig : XmlFlavor
    {
        public override bool ParseAttributesEnabled => false;

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

        public override bool ShallBeTerminalNode(Container container) => container?.Type == "package";
    }
}