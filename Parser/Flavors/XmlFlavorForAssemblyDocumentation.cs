using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public sealed class XmlFlavorForAssemblyDocumentation : XmlFlavor
    {
        private const string Assembly = "assembly";
        private const string Summary = "summary";
        private const string Remarks = "remarks";
        private const string Param = "param";
        private const string Returns = "returns";

        private static readonly HashSet<string> TerminalNodeNames = new HashSet<string>
                                                                        {
                                                                            Summary,
                                                                            Remarks,
                                                                            Param,
                                                                            Returns,
                                                                        };

        public override bool ParseAttributesEnabled => false;

        public override bool Supports(string filePath) => filePath.EndsWith(".ruleset", StringComparison.OrdinalIgnoreCase);

        public override bool Supports(DocumentInfo info) => string.Equals(info.RootElement, "doc", StringComparison.OrdinalIgnoreCase);

        public override string GetName(XmlTextReader reader)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                var name = reader.Name;
                var identifier = reader.GetAttribute("name");
                return identifier ?? name;
            }

            return base.GetName(reader);
        }

        public override string GetType(XmlTextReader reader) => reader.NodeType == XmlNodeType.Element ? reader.Name : base.GetType(reader);

        public override ContainerOrTerminalNode FinalAdjustAfterParsingComplete(ContainerOrTerminalNode node)
        {
            if (node is Container c && c.Type == Assembly)
            {
                var name = c.Children.FirstOrDefault(_ => _.Name == "name");
                if (name != null)
                {
                    c.Children.RemoveAll(_ => _.Name == "name");
                    c.Name = name.Content;
                }
            }

            return base.FinalAdjustAfterParsingComplete(node);
        }

        protected override bool ShallBeTerminalNode(ContainerOrTerminalNode node) => TerminalNodeNames.Contains(node?.Type);
    }
}