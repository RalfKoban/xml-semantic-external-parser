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
                                                        .Concat(new[] { new XmlFlavor() }) // add XML flavor here as that is the fall-back type
                                                        .ToArray();

        public static IXmlFlavor Find(string filePath)
        {
            var flavors = Flavors.Where(_ => _.Supports(filePath)).ToList();
            return flavors.Count == 1 ? flavors[0] : GetXmlFlavorForDocument(filePath) ?? new XmlFlavor(); // just in case use XML flavor as fall-back (should never happen)
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
                            var name = reader.LocalName;
                            var ns = reader.LookupNamespace(reader.Prefix);

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