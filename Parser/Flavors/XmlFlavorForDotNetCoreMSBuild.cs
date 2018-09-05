using System;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public class XmlFlavorForDotNetCoreMSBuild : XmlFlavorForMSBuild
    {
        public override string PreferredNamespacePrefix => "Sdk"; // actually, its only a normal attribute but we pretend it to be a namespace indicator

        public override bool Supports(DocumentInfo info) => string.Equals(info.RootElement, ElementNames.Project, StringComparison.Ordinal)
                                                         && string.Equals(info.Namespace, "Microsoft.NET.Sdk", StringComparison.OrdinalIgnoreCase);
    }
}