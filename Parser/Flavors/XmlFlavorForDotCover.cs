using System.Collections.Generic;
using System.Linq;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public sealed class XmlFlavorForDotCover : XmlFlavor
    {
        private const string Assembly = "Assembly";
        private const string Namespace = "Namespace";
        private const string Type = "Type";
        private const string Method = "Method";

        private static readonly HashSet<string> TerminalNodeNames = new HashSet<string>
                                                                        {
                                                                            Method,
                                                                        };

        public override bool ParseAttributesEnabled => false;

        public override bool Supports(DocumentInfo info) => info.Attributes.Any(_ => _.Key == "DotCoverVersion") &&
                                                            info.Attributes.Any(_ => _.Key == "ReportType" && _.Value == "NDependXML");

        public override string GetName(XmlTextReader reader)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                var name = reader.Name;
                var attributeName = GetAttribute(name);
                var identifier = string.IsNullOrWhiteSpace(attributeName) ? null : reader.GetAttribute(attributeName);
                return identifier ?? name;
            }

            return base.GetName(reader);
        }

        public override string GetType(XmlTextReader reader) => reader.NodeType == XmlNodeType.Element ? reader.Name : base.GetType(reader);

        protected override bool ShallBeTerminalNode(ContainerOrTerminalNode node) => TerminalNodeNames.Contains(node?.Type);

        private static string GetAttribute(string name)
        {
            switch (name)
            {
                case Assembly:
                case Namespace:
                case Type:
                case Method:
                    return "Name";

                default:
                    return null;
            }
        }
    }
}
