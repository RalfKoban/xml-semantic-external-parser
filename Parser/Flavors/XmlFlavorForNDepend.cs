using System.Collections.Generic;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public sealed class XmlFlavorForNDepend : XmlFlavor
    {
        private static readonly HashSet<string> NonTerminalNodeNames = new HashSet<string>
                                                                        {
                                                                            "Assemblies",
                                                                            "DebtSettings",
                                                                            "Dirs",
                                                                            "FrameworkAssemblies",
                                                                            "Group",
                                                                            "NDepend",
                                                                            "PathVariables",
                                                                            "ProjectDebtSettings",
                                                                            "Queries",
                                                                            "RuleFiles",
                                                                            "TrendMetrics",
                                                                        };

        public override bool ParseAttributesEnabled => false;

        public override string GetName(XmlTextReader reader)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                var name = reader.Name;
                return reader.GetAttribute("Name") ?? name;
            }

            return base.GetName(reader);
        }

        public override string GetType(XmlTextReader reader) => reader.NodeType == XmlNodeType.Element ? reader.Name : base.GetType(reader);

        public override bool ShallBeTerminalNode(Container container) => !NonTerminalNodeNames.Contains(container?.Type);
    }
}