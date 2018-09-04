using System;
using System.Collections.Generic;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public sealed class XmlFlavorForXaml : XmlFlavor
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
                                                                            nameof(Boolean),
                                                                            nameof(Byte),
                                                                            nameof(Char),
                                                                            nameof(DateTime),
                                                                            nameof(DateTimeOffset),
                                                                            nameof(Decimal),
                                                                            nameof(Double),
                                                                            nameof(Guid),
                                                                            nameof(Int16),
                                                                            nameof(Int32),
                                                                            nameof(Int64),
                                                                            nameof(SByte),
                                                                            nameof(Single),
                                                                            nameof(String),
                                                                            nameof(TimeSpan),
                                                                            nameof(UInt16),
                                                                            nameof(UInt32),
                                                                            nameof(UInt64),
                                                                        };

        public override bool ParseAttributesEnabled => true;

        public override string PreferredNamespacePrefix => "xmlns:wpf";

        public override bool Supports(string filePath) => filePath.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase);

        public override bool Supports(DocumentInfo info) => string.Equals(info.Namespace, "http://schemas.microsoft.com/winfx/2006/xaml/presentation", StringComparison.OrdinalIgnoreCase);

        public override string GetName(XmlTextReader reader)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                var name = reader.LocalName;

                return reader.GetAttribute("Name", "http://schemas.microsoft.com/winfx/2006/xaml") ??
                       reader.GetAttribute("Key", "http://schemas.microsoft.com/winfx/2006/xaml") ??
                       name;
            }

            return base.GetName(reader);
        }

        public override string GetType(XmlTextReader reader) => reader.NodeType == XmlNodeType.Element ? reader.LocalName : base.GetType(reader);

        protected override bool ShallBeTerminalNode(ContainerOrTerminalNode node) => TerminalNodeNames.Contains(node?.Type);
    }
}