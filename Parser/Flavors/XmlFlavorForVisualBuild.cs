using System;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public class XmlFlavorForVisualBuild : XmlFlavor
    {
        public override bool ParseAttributesEnabled => false;

        public override bool Supports(string filePath) => filePath.EndsWith(".bld", StringComparison.OrdinalIgnoreCase);

        public override bool Supports(DocumentInfo info) => string.Equals(info.RootElement, "project", StringComparison.Ordinal)
                                                         && info.Namespace is null;
    }
}