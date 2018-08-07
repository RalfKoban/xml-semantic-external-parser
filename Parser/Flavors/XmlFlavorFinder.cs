using System.Linq;
using System.Xml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public static class XmlFlavorFinder
    {
        private static readonly XmlFlavor[] Flavors =
                                                        {
                                                            new XmlFlavorForPackagesConfig(),
                                                            new XmlFlavorForProject(),
                                                            new XmlFlavorForNDepend(),
                                                            new XmlFlavorForWix(),
                                                            new XmlFlavorForWixConfiguration(),
                                                            new XmlFlavorForWixLocation(),
                                                            new XmlFlavorForXaml(),
                                                        };

        public static IXmlFlavor Find(string filePath)
        {
            return Flavors.FirstOrDefault(_ => _.Supports(filePath)) ?? GetXmlFlavorForDocument(filePath) ?? new XmlFlavor();
        }

        private static IXmlFlavor GetXmlFlavorForDocument(string filePath)
        {
            var info = GetDocumentInfo(filePath);
            return info != null ? Flavors.FirstOrDefault(_ => _.Supports(info)) : null;
        }

        private static DocumentInfo GetDocumentInfo(string filePath)
        {
            try
            {
                using (var reader = new XmlTextReader(filePath))
                {
                    while (reader.Read())
                    {
                        // get first element
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            return new DocumentInfo
                                       {
                                           RootElement = reader.Name,
                                           Namespace = reader.GetAttribute("xmlns"),
                                       };
                        }
                    }
                }
            }
            catch (XmlException)
            {
                // root element not contained, so ignore
            }

            return null;
        }
    }
}