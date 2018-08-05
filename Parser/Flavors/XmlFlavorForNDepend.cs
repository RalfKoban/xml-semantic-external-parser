using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public sealed class XmlFlavorForNDepend : XmlFlavor
    {
        private const StringComparison Comparison = StringComparison.OrdinalIgnoreCase;

        private const string QualityGate = "QualityGate";
        private const string Query = "Query";
        private const string TrendMetric = "TrendMetric";

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
                return reader.GetAttribute("Name") ?? reader.GetAttribute("MetricName") ?? reader.GetAttribute("Path") ?? name;
            }

            return base.GetName(reader);
        }

        public override string GetType(XmlTextReader reader) => reader.NodeType == XmlNodeType.Element ? reader.Name : base.GetType(reader);

        public override ContainerOrTerminalNode FinalAdjustAfterParsingComplete(ContainerOrTerminalNode node)
        {
            if (node is Container c)
            {
                switch (node.Type)
                {
                    case Query:
                        AdjustQuery(node, c);
                        break;

                    default:
                        {
                            var textNode = c.Children.FirstOrDefault(_ => _.Type == NodeType.Text);
                            if (textNode != null)
                            {
                                c.Name = $"{c.Name}=\"{textNode.Content}\"";
                            }

                            break;
                        }
                }
            }

            return base.FinalAdjustAfterParsingComplete(node);
        }

        protected override bool ShallBeTerminalNode(ContainerOrTerminalNode node) => !NonTerminalNodeNames.Contains(node?.Type);

        private static void AdjustQuery(ContainerOrTerminalNode node, Container container)
        {
            // try to find out CDATA section to get the name
            var cdata = container.Children.FirstOrDefault(_ => _.Type == NodeType.CDATA)?.Content;
            if (!string.IsNullOrWhiteSpace(cdata))
            {
                // we might have a normal query
                if (TryGetQueryNameFromCData(cdata, out var queryName))
                {
                    node.Name = queryName;
                }

                // we might have a trend metric
                if (TryGetNameFromCData(cdata, TrendMetric, out var trendMetricName))
                {
                    node.Name = trendMetricName;
                    node.Type = TrendMetric;
                }

                // we might have a quality gate
                if (TryGetNameFromCData(cdata, QualityGate, out var qualityGateName))
                {
                    node.Name = qualityGateName;
                    node.Type = QualityGate;
                }
            }
        }

        private static bool TryGetQueryNameFromCData(string cdata, out string name)
        {
            name = null;

            var start = cdata.IndexOf("<Name>", Comparison);
            if (start < 0)
            {
                return false;
            }

            var end = cdata.IndexOf("</Name>", start, Comparison);
            if (end < 0)
            {
                return false;
            }

            start += 6;
            name = cdata.Substring(start, end - start);
            return true;
        }

        private static bool TryGetNameFromCData(string cdata, string queryType, out string name)
        {
            name = null;

            var start = cdata.IndexOf("<" + queryType + " ", Comparison);
            if (start < 0)
            {
                return false;
            }

            var end = cdata.IndexOf("/>", Comparison);
            if (end < 0)
            {
                return false;
            }

            end += 2;

            var xml = cdata.Substring(start, end - start);

            // parse
            name = XDocument.Parse(xml).Root?.Attributes("Name").Select(_ => _.Value).FirstOrDefault();
            return name != null;
        }
    }
}