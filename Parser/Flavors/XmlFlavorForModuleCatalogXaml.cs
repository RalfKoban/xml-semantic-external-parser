using System;
using System.Collections.Generic;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public sealed class XmlFlavorForModuleCatalogXaml : XmlFlavor
    {
        private const string PrismNamespace = "clr-namespace:Microsoft.Practices.Prism.Modularity;assembly=Microsoft.Practices.Prism";

        private static readonly HashSet<string> TerminalNodeNames = new HashSet<string>
                                                                        {
                                                                            "ModuleInfo",
                                                                        };

        public override bool ParseAttributesEnabled => true;

        public override string PreferredNamespacePrefix => "xmlns:Modularity";

        public override bool Supports(string filePath) => filePath.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase);

        public override bool Supports(DocumentInfo info) =>
            string.Equals(info.RootElement, "ModuleCatalog", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(info.Namespace, PrismNamespace, StringComparison.OrdinalIgnoreCase);

        public override string GetName(XmlTextReader reader)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                var name = reader.LocalName;
                return reader.GetAttribute("ModuleName", PrismNamespace) ?? name;
            }

            return base.GetName(reader);
        }

        public override string GetType(XmlTextReader reader) => reader.NodeType == XmlNodeType.Element ? reader.LocalName : base.GetType(reader);

        protected override bool ShallBeTerminalNode(ContainerOrTerminalNode node) => TerminalNodeNames.Contains(node?.Type);
    }
}