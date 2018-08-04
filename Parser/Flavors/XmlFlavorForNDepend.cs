using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public sealed class XmlFlavorForNDepend : XmlFlavor
    {
        private const StringComparison Comparison = StringComparison.OrdinalIgnoreCase;

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

        public override void FinalAdjustAfterParsingComplete(ContainerOrTerminalNode node)
        {
            if (node.Type != "Query")
            {
                return;
            }

            if (node is Container c)
            {
                // try to find out CDATA section to get the name
                var cdata = c.Children.FirstOrDefault(_ => _.Type == NodeType.CDATA)?.Content;
                if (string.IsNullOrWhiteSpace(cdata))
                {
                    return;
                }

                var start = cdata.IndexOf("<Name>", Comparison);
                if (start < 0)
                {
                    return;
                }

                start += 6;

                var end = cdata.IndexOf("</Name>", start, Comparison);
                if (end < 0)
                {
                    return;
                }

                node.Name = cdata.Substring(start, end - start);
            }
        }
    }
}