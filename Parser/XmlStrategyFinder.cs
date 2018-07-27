using System;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Strategies;

namespace MiKoSolutions.SemanticParsers.Xml
{
    public static class XmlStrategyFinder
    {
        public static IXmlStrategy Find(string filePath)
        {
            var name = GetDocumentName(filePath);

            if (string.Equals(name, "packages", StringComparison.OrdinalIgnoreCase))
            {
                return new XmlStrategyForPackagesConfig();
            }

            if (string.Equals(name, "Wix", StringComparison.OrdinalIgnoreCase))
            {
                return new XmlStrategyForWix();
            }

            if (string.Equals(name, "Project", StringComparison.OrdinalIgnoreCase))
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