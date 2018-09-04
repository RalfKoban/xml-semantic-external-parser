using System;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public class XmlFlavorForDotNetCoreMSBuild : XmlFlavorForMSBuild
    {
        public override string PreferredNamespacePrefix => "Sdk";

        public override bool Supports(DocumentInfo info) => string.Equals(info.RootElement, ElementNames.Project, StringComparison.OrdinalIgnoreCase)
                                                            && string.Equals(info.Namespace, "Microsoft.NET.Sdk", StringComparison.OrdinalIgnoreCase);
    }
}