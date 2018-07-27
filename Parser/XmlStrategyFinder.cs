using System;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Strategies;

namespace MiKoSolutions.SemanticParsers.Xml
{
    public static class XmlStrategyFinder
    {
        public static IXmlStrategy Find(string filePath, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if (filePath.EndsWith("packages.config"))
            {
                return new XmlStrategyForPackagesConfig();
            }

            if (filePath.EndsWith(".csproj", comparison))
            {
                return new XmlStrategyForProject();
            }

            if (filePath.EndsWith(".wxi", comparison) || filePath.EndsWith(".wxs", comparison))
            {
                return new XmlStrategyForWix();
            }

            if (filePath.EndsWith(".xaml", comparison))
            {
                return new XmlStrategyForXaml();
            }

            var name = GetDocumentName(filePath);

            if (string.Equals(name, "packages", comparison))
            {
                return new XmlStrategyForPackagesConfig();
            }

            if (string.Equals(name, "Wix", comparison))
            {
                return new XmlStrategyForWix();
            }

            if (string.Equals(name, "Project", comparison))
            {
                return new XmlStrategyForProject();
            }

            return new XmlStrategy();
        }

        private static string GetDocumentName(string filePath)
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
                            return reader.Name;
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