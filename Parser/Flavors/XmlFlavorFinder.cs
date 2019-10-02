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
            // var flavors = Flavors.Where(_ => _.Supports(filePath)).ToList();
            // return flavors.Count == 1 ? flavors[0] : GetXmlFlavorForDocument(filePath) ?? new XmlFlavor(); // just in case use XML flavor as fall-back (happens e.g. if XML encoding is wrong and an XmlException gets thrown)

            return GetXmlFlavorForDocument(filePath) ?? new XmlFlavor(); // just in case use XML flavor as fall-back (happens e.g. if XML encoding is wrong and an XmlException gets thrown)
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
                            var name = reader.LocalName;
                            var ns = reader.LookupNamespace(reader.Prefix);

                            var attributes = new List<KeyValuePair<string, string>>();
                            var attributeCount = reader.AttributeCount;
                            for (var i = 0; i < attributeCount; i++)
                            {
                                reader.MoveToAttribute(i);
                                var attributeName = reader.LocalName;
                                var attributeValue = reader.GetAttribute(i);
                                attributes.Add(new KeyValuePair<string, string>(attributeName, attributeValue));
                            }

                            return new DocumentInfo
                                       {
                                           RootElement = name,
                                           Namespace = ns,
                                           Attributes = attributes,
                                       };
                        }
                    }
                }
            }
            catch (XmlException ex)
            {
                // root element not contained, so ignore
                Tracer.Trace($"While parsing '{filePath}', following {ex.GetType().Name} was thrown: {ex}", ex);
            }

            return null;
        }
    }
}