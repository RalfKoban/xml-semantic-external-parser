using System;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public sealed class XmlFlavorForNCrunchSolution : XmlFlavor
    {
        public override bool Supports(string filePath) => filePath.EndsWith(".ncrunchsolution", StringComparison.OrdinalIgnoreCase);

        public override bool Supports(DocumentInfo info) => string.Equals(info.RootElement, "SolutionConfiguration", StringComparison.OrdinalIgnoreCase);

        public override string GetName(XmlReader reader) => reader.NodeType == XmlNodeType.Element ? reader.LocalName : base.GetName(reader);

        public override string GetType(XmlReader reader) => reader.NodeType == XmlNodeType.Element ? reader.LocalName : base.GetType(reader);

        protected override bool ShallBeTerminalNode(ContainerOrTerminalNode node)
        {
            switch (node.Type)
            {
                case "SolutionConfiguration":
                case "Settings":
                case "HotSpotsExclusionList":
                case "MetricsExclusionList":
                    return false;

                default:
                    return true;
            }
        }
    }
}