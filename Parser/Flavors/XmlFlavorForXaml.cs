using System;
using System.Collections.Generic;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public sealed class XmlFlavorForXaml : XmlFlavor
    {
        private const string XamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";
        private const string XamlPresentationNamespace = XamlNamespace + "/presentation";

        private const StringComparison OrdinalIgnoreCase = StringComparison.OrdinalIgnoreCase;

        private static readonly string[] AlternativeSupportedNamespaces =
                                                                        {
                                                                            // Actipro
                                                                            "http://schemas.actiprosoftware.com/winfx/xaml",
                                                                            "clr-namespace:ActiproSoftware.",

                                                                            // DevExpress
                                                                            "http://schemas.devexpress.com/winfx/2008/xaml",
                                                                            "clr-namespace:DevExpress.",
                                                                        };

        private static readonly HashSet<string> TerminalNodeNames = new HashSet<string>
                                                                        {
                                                                            ElementNames.Button,
                                                                            ElementNames.Binding,
                                                                            ElementNames.BitmapImage,
                                                                            ElementNames.ColumnDefinition,
                                                                            ElementNames.CheckBox,
                                                                            ElementNames.DataGridTemplateColumn,
                                                                            ElementNames.EventSetter,
                                                                            ElementNames.GlobalResourceDictionary,
                                                                            ElementNames.Image,
                                                                            ElementNames.KeyBinding,
                                                                            ElementNames.Label,
                                                                            ElementNames.RowDefinition,
                                                                            ElementNames.Separator,
                                                                            ElementNames.Setter,
                                                                            ElementNames.TextBlock,
                                                                            ElementNames.TextBox,

                                                                            // common .NET types
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

        private static readonly IDictionary<string, string> ElementAttributeMapping = new Dictionary<string, string>
                                                                                          {
                                                                                              { ElementNames.Button, AttributeNames.Command },
                                                                                              { ElementNames.ContentPresenter, AttributeNames.Content },
                                                                                              { ElementNames.DataTemplate, AttributeNames.DataType },
                                                                                              { ElementNames.EventSetter, AttributeNames.Event },
                                                                                              { ElementNames.GlobalResourceDictionary, AttributeNames.Source },
                                                                                              { ElementNames.Image, AttributeNames.Source },
                                                                                              { ElementNames.ListView, AttributeNames.ItemsSource },
                                                                                              { ElementNames.Setter, AttributeNames.Property },
                                                                                              { ElementNames.StaticResource, AttributeNames.ResourceKey },
                                                                                              { ElementNames.Style, AttributeNames.TargetType },
                                                                                              { ElementNames.TextBlock, AttributeNames.Text },
                                                                                              { ElementNames.Trigger, AttributeNames.Property },
                                                                                              { ElementNames.UserControl, AttributeNames.Class },
                                                                                              { ElementNames.Window, AttributeNames.Class },
                                                                                          };

        public override bool ParseAttributesEnabled => false;

        public override bool Supports(string filePath) => filePath.EndsWith(".xaml", OrdinalIgnoreCase);

        public override bool Supports(DocumentInfo info)
        {
            if (info.Namespace is null)
            {
                return false;
            }

            if (string.Equals(info.Namespace, XamlPresentationNamespace, OrdinalIgnoreCase))
            {
                return true;
            }

            foreach (var ns in AlternativeSupportedNamespaces)
            {
                if (info.Namespace.StartsWith(ns, OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public override string GetName(XmlReader reader)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                // some have a name, some have a key, and some might have an AutomationId or a Uid
                var defaultName = reader.GetAttribute(AttributeNames.Name, XamlNamespace)
                               ?? reader.GetAttribute(AttributeNames.Key, XamlNamespace)
                               ?? reader.GetAttribute(AttributeNames.Uid, XamlNamespace)
                               ?? reader.GetAttribute(AttributeNames.AutomationId);

                if (defaultName is null)
                {
                    var name = reader.LocalName;
                    var proposedName = GetNameFromAttribute(reader, name);
                    return proposedName ?? name;
                }

                return defaultName;
            }

            return base.GetName(reader);
        }

        public override string GetType(XmlReader reader) => reader.NodeType == XmlNodeType.Element ? reader.LocalName : base.GetType(reader);

        protected override bool ShallBeTerminalNode(ContainerOrTerminalNode node)
        {
            if (node is null)
            {
                return false;
            }

            var nodeType = node.Type;
            if (TerminalNodeNames.Contains(nodeType))
            {
                return true;
            }

            if (nodeType.EndsWith(ElementNames.Converter))
            {
                // even unknown converters shall be terminal nodes
                return true;
            }

            return false;
        }

        private static string GetNameFromAttribute(XmlReader reader, string name)
        {
            if (ElementAttributeMapping.TryGetValue(name, out string value))
            {
                var proposedName = reader.GetAttribute(value);
                if (proposedName != null)
                {
                    return GetFileName(proposedName);
                }
            }

            return null;
        }

        private static class ElementNames
        {
            internal const string Binding = "Binding";
            internal const string BitmapImage = "BitmapImage";
            internal const string Button = "Button";
            internal const string CheckBox = "CheckBox";
            internal const string ColumnDefinition = "ColumnDefinition";
            internal const string ContentPresenter = "ContentPresenter";
            internal const string Converter = "Converter";
            internal const string DataGridTemplateColumn = "DataGridTemplateColumn";
            internal const string DataTemplate = "DataTemplate";
            internal const string EventSetter = "EventSetter";
            internal const string GlobalResourceDictionary = "GlobalResourceDictionary";
            internal const string Image = "Image";
            internal const string KeyBinding = "KeyBinding";
            internal const string Label = "Label";
            internal const string ListView = "ListView";
            internal const string RowDefinition = "RowDefinition";
            internal const string Separator = "Separator";
            internal const string Setter = "Setter";
            internal const string StaticResource = "StaticResource";
            internal const string Style = "Style";
            internal const string TextBlock = "TextBlock";
            internal const string TextBox = "TextBox";
            internal const string Trigger = "Trigger";
            internal const string UserControl = "UserControl";
            internal const string Window = "Window";
        }

        private static class AttributeNames
        {
            internal const string AutomationId = "AutomationProperties.AutomationId";
            internal const string Class = "x:Class";
            internal const string Command = "Command";
            internal const string Content = "Content";
            internal const string DataType = "DataType";
            internal const string Event = "Event";
            internal const string ItemsSource = "ItemsSource";
            internal const string Key = "Key";
            internal const string Name = "Name";
            internal const string Property = "Property";
            internal const string ResourceKey = "ResourceKey";
            internal const string Source = "Source";
            internal const string TargetType = "TargetType";
            internal const string Text = "Text";
            internal const string Uid = "Uid";
        }
    }
}