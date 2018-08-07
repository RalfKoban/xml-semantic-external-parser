using System;
using System.Xml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public static class XmlFlavorFinder
    {
        public static IXmlFlavor Find(string filePath, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if (filePath.EndsWith("packages.config"))
            {
                return new XmlFlavorForPackagesConfig();
            }

            if (filePath.EndsWith(".csproj", comparison))
            {
                return new XmlFlavorForProject();
            }

            if (filePath.EndsWith(".ndproj", comparison) || filePath.EndsWith(".ndrules", comparison))
            {
                return new XmlFlavorForNDepend();
            }

            if (filePath.EndsWith(".wxi", comparison) || filePath.EndsWith(".wxs", comparison))
            {
                return new XmlFlavorForWix();
            }

            if (filePath.EndsWith(".xaml", comparison))
            {
                return new XmlFlavorForXaml();
            }

            var tuple = GetDocumentInfo(filePath);

            var name = tuple?.Item1;
            var ns = tuple?.Item2;
            if (string.Equals(name, "packages", comparison))
            {
                return new XmlFlavorForPackagesConfig();
            }

            if (string.Equals(name, "NDepend", comparison) || string.Equals(name, "Queries", comparison))
            {
                return new XmlFlavorForNDepend();
            }

            if (string.Equals(name, "Project", comparison) && string.Equals(ns, "http://schemas.microsoft.com/developer/msbuild/2003", comparison))
            {
                return new XmlFlavorForProject();
            }

            if (string.Equals(name, "Wix", comparison) || string.Equals(name, "Include", comparison))
            {
                return new XmlFlavorForWix();
            }

            if (string.Equals(ns, "http://schemas.microsoft.com/winfx/2006/xaml/presentation", comparison))
            {
                return new XmlFlavorForXaml();
            }

            return new XmlFlavor();
        }

        private static Tuple<string, string> GetDocumentInfo(string filePath)
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
                            return new Tuple<string, string>(reader.Name, reader.GetAttribute("xmlns"));
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