using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public static class XmlFlavorFinder
    {
        private static readonly Type XmlFlavorType = typeof(XmlFlavor);

        private static readonly XmlFlavor[] Flavors = typeof(XmlFlavorFinder).Assembly
                                                        .GetTypes()
                                                        .Where(_ => !_.IsAbstract)
                                                        .Where(_ => _.IsClass)
                                                        .Where(_ => _ != XmlFlavorType) // ignore XML flavor here as that is the fall-back type
                                                        .Where(_ => XmlFlavorType.IsAssignableFrom(_))
                                                        .Select(_ => _.GetConstructor(Type.EmptyTypes))
                                                        .Select(_ => _?.Invoke(null))
                                                        .OfType<XmlFlavor>()
                                                        .ToArray();

        public static IXmlFlavor Find(string filePath)
        {
            return Flavors.FirstOrDefault(_ => _.Supports(filePath)) ?? GetXmlFlavorForDocument(filePath) ?? new XmlFlavor();
        }

        private static IXmlFlavor GetXmlFlavorForDocument(string filePath)
        {
            var info = GetDocumentInfo(filePath, Flavors.Select(_ => _.PreferredNamespacePrefix).ToHashSet());
            return info != null ? Flavors.FirstOrDefault(_ => _.Supports(info)) : null;
        }

        private static DocumentInfo GetDocumentInfo(string filePath, IEnumerable<string> namespaces)
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
                            var name = reader.Name;
                            var ns = namespaces.Select(reader.GetAttribute).FirstOrDefault(_ => _ != null);

                            return new DocumentInfo
                                       {
                                           RootElement = name,
                                           Namespace = ns,
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