using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public sealed class XmlFlavorForVisualBuild : XmlFlavor
    {
        private const string RootElement = "project";
        private const string StepElement = "step";
        private const string NameElement = "name";
        private const string MacroElement = "macro";

        private static readonly HashSet<string> NonTerminalNodeNames = new HashSet<string>
                                                                           {
                                                                               RootElement,
                                                                               "macros",
                                                                               "steps",
                                                                           };

        public override bool ParseAttributesEnabled => true;

        public override bool Supports(string filePath) => filePath.EndsWith(".bld", StringComparison.OrdinalIgnoreCase);

        public override bool Supports(DocumentInfo info) => string.Equals(info.RootElement, RootElement, StringComparison.Ordinal)
                                                         && info.Namespace is null;

        public override string GetType(XmlReader reader) => reader.NodeType == XmlNodeType.Element ? reader.LocalName : base.GetType(reader);

        public override ContainerOrTerminalNode FinalAdjustAfterParsingComplete(ContainerOrTerminalNode node)
        {
            if (node is Container c)
            {
                switch (node.Type)
                {
                    case MacroElement:
                    {
                        FinalAdjustMacroElement(c);
                        break;
                    }

                    case NameElement:
                    {
                        FinalAdjustNameElement(c);
                        break;
                    }

                    case StepElement:
                    {
                        FinalAdjustStepElement(c);
                        break;
                    }
                }

                // do not report the attributes for the moment as that causes structural issues in SemanticMerge/GMaster (due to gaps between the attributes)
                c.Children.RemoveAll(_ => _.Type == NodeType.Attribute);
            }

            return base.FinalAdjustAfterParsingComplete(node);
        }

        protected override bool ShallBeTerminalNode(ContainerOrTerminalNode node) => !NonTerminalNodeNames.Contains(node?.Type);

        private static void FinalAdjustMacroElement(Container c)
        {
            var name = c.Children.FirstOrDefault(_ => _.Type == NodeType.Attribute && _.Name == "name");
            if (name != null)
            {
                c.Name = name.Content;
            }
        }

        private static void FinalAdjustNameElement(Container c)
        {
            var text = c.Children.FirstOrDefault(_ => _.Type == NodeType.Text);
            if (text != null)
            {
                c.Name = text.Content;
            }
        }

        private static void FinalAdjustStepElement(Container c)
        {
            var action = c.Children.FirstOrDefault(_ => _.Type == NodeType.Attribute && _.Name == "action");
            if (action != null)
            {
                c.Type = $"{c.Type} '{action.Content}'";
            }

            var name = c.Children.FirstOrDefault(_ => _.Type == NameElement);
            if (name != null)
            {
                c.Name = name.Name;
            }
        }
    }
}