using System.Collections.Generic;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Strategies
{
    public sealed class XmlStrategyForXaml : XmlStrategy
    {
        private static readonly HashSet<string> TerminalNodeNames = new HashSet<string>
                                                                        {
                                                                            "Button",
                                                                            "Binding",
                                                                            "ColumnDefinition",
                                                                            "CheckBox",
                                                                            "DataGridTemplateColumn",
                                                                            "Image",
                                                                            "KeyBinding",
                                                                            "Label",
                                                                            "RowDefinition",
                                                                            "Setter",
                                                                            "TextBlock",
                                                                            "TextBox",
                                                                        };

        public override bool ParseAttributesEnabled => false;

        public override string GetName(XmlTextReader reader)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                var name = reader.Name;

                return reader.GetAttribute("Name", "http://schemas.microsoft.com/winfx/2006/xaml") ??
                       reader.GetAttribute("Key", "http://schemas.microsoft.com/winfx/2006/xaml") ??
                       name;
            }

            return base.GetName(reader);
        }

        public override string GetType(XmlTextReader reader) => reader.NodeType == XmlNodeType.Element ? reader.Name : base.GetType(reader);

        public override bool ShallBeTerminalNode(Container container) => TerminalNodeNames.Contains(container?.Type);
    }
}