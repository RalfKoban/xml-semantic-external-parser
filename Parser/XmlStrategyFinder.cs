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

            var tuple = GetDocumentInfo(filePath);

            var name = tuple?.Item1;
            var ns = tuple?.Item2;
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

            if (string.Equals(ns, "http://schemas.microsoft.com/winfx/2006/xaml/presentation", comparison))
            {
                return new XmlStrategyForXaml();
            }

            return new XmlStrategy();
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