using System;
using System.Collections.Generic;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public sealed class XmlFlavorForRuleSets : XmlFlavor
    {
        private const string IncludeAll = "IncludeAll";
        private const string RuleSet = "RuleSet";
        private const string Rules = "Rules";
        private const string Rule = "Rule";

        private static readonly HashSet<string> TerminalNodeNames = new HashSet<string>
                                                                        {
                                                                            IncludeAll,
                                                                            Rule,
                                                                        };

        public override bool ParseAttributesEnabled => false;

        public override bool Supports(string filePath) => filePath.EndsWith(".ruleset", StringComparison.OrdinalIgnoreCase);

        public override bool Supports(DocumentInfo info) => string.Equals(info.RootElement, RuleSet, StringComparison.OrdinalIgnoreCase);

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
                case RuleSet: return "Name";
                case Rules: return "AnalyzerId";
                case Rule: return "Id";
                default: return null;
            }
        }
    }
}