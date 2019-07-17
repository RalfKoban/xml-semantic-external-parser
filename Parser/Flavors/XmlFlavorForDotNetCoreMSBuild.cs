using System;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public class XmlFlavorForDotNetCoreMSBuild : XmlFlavorForMSBuild
    {
        public override bool Supports(DocumentInfo info) => string.Equals(info.RootElement, ElementNames.Project, StringComparison.Ordinal)
                                                         && string.Equals(info.Namespace, "Microsoft.NET.Sdk", StringComparison.OrdinalIgnoreCase);
    }
}