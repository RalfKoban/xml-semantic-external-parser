using System;
using System.Linq;
using System.Xml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public sealed class XmlFlavorForWixConfiguration : XmlFlavor
    {
        public override bool ParseAttributesEnabled => true;

        public override bool Supports(string filePath) => filePath.EndsWith(".wxi", StringComparison.OrdinalIgnoreCase);

        public override bool Supports(DocumentInfo info) => string.Equals(info.RootElement, "Include", StringComparison.OrdinalIgnoreCase);

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