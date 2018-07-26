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

            return new XmlStrategy();
        }

        private static string GetDocumentName(string filePath)
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

            return null;
        }
    }
}