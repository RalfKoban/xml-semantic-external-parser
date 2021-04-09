using System;
using System.Collections.Generic;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public sealed class XmlFlavorForRanorexTestSuite : XmlFlavor
    {
        private const string SpecialElement = "reference";

        private static readonly HashSet<string> TerminalNodeNames = new HashSet<string>
                                                                        {
                                                                            SpecialElement,
                                                                            "testcontainer",
                                                                            "testmodule",
                                                                        };

        public override bool ParseAttributesEnabled => false;

        public override bool Supports(string filePath) => filePath.EndsWith(".rxtst", StringComparison.OrdinalIgnoreCase) // test suite
                                                       || filePath.EndsWith(".rxtmg", StringComparison.OrdinalIgnoreCase); // module group

        public override bool Supports(DocumentInfo info) => string.Equals(info.RootElement, "testsuitedoc", StringComparison.OrdinalIgnoreCase)
                                                         || string.Equals(info.RootElement, "modulegroupdoc", StringComparison.OrdinalIgnoreCase);

        public override string GetName(XmlReader reader)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                var name = reader.GetAttribute("name") ?? reader.LocalName;

                switch (reader.Name)
                {
                    case "setup":
                    case "teardown":
                    case "testcontainer":
                    case "testmodule":
                    {
                        var id = reader.GetAttribute("id");
                        return $"{name} ({id})";
                    }

                    default:
                        return name;
                }
            }

            return base.GetName(reader);
        }

        public override string GetType(XmlReader reader) => reader.NodeType == XmlNodeType.Element ? reader.LocalName : base.GetType(reader);

        public override ContainerOrTerminalNode FinalAdjustAfterParsingComplete(ContainerOrTerminalNode node)
        {
            if (node is Container c)
            {
                switch (c.Type)
                {
                    case SpecialElement:
                    {
                        if (c.Children.Count == 1)
                        {
                            node.Name = c.Children[0]?.Content?.Trim();
                        }

                        break;
                    }

                    case "flatlistofchildren":
                    {
                        for (var index = 0; index < c.Children.Count; index++)
                        {
                            var child = c.Children[index];
                            c.Children[index] = child.ToTerminalNode();
                        }

                        break;
                    }
                }
            }

            return base.FinalAdjustAfterParsingComplete(node);
        }

        protected override bool ShallBeTerminalNode(ContainerOrTerminalNode node) => TerminalNodeNames.Contains(node?.Type);
    }
}