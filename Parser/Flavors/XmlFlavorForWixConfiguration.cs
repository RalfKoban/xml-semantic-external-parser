using System.Linq;
using System.Xml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public sealed class XmlFlavorForWixConfiguration : XmlFlavor
    {
        public override bool ParseAttributesEnabled => true;

        public override string GetName(XmlTextReader reader)
        {
            if (reader.NodeType == XmlNodeType.ProcessingInstruction)
            {
                var name = reader.Name;
                var parts = reader.Value.Split('=');
                var identifier = parts.Any() ? parts[0].Trim() : null;
                return identifier is null ? name : $"{name} '{identifier}'";
            }

            return base.GetName(reader);
        }

        public override string GetType(XmlTextReader reader) => reader.NodeType == XmlNodeType.Element ? reader.Name : base.GetType(reader);
    }
}