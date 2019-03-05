using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public sealed class XmlFlavorForXaml : XmlFlavor
    {
        private const string XamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";
        private const string XamlPresentationNamespace = XamlNamespace + "/presentation";

        private const string ActiproNamespace = "http://schemas.actiprosoftware.com/winfx/xaml";
        private const string DevExpressNamespace = "http://schemas.devexpress.com/winfx/2008/xaml";

        private const StringComparison OrdinalIgnoreCase = StringComparison.OrdinalIgnoreCase;

        private static readonly HashSet<string> TerminalNodeNames = new HashSet<string>
                                                                        {
                                                                            ElementNames.Button,
                                                                            ElementNames.Binding,
                                                                            ElementNames.ColumnDefinition,
                                                                            ElementNames.CheckBox,
                                                                            ElementNames.DataGridTemplateColumn,
                                                                            ElementNames.EventSetter,
                                                                            ElementNames.Image,
                                                                            ElementNames.KeyBinding,
                                                                            ElementNames.Label,
                                                                            ElementNames.RowDefinition,
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
                                                                                              { ElementNames.EventSetter, AttributeNames.Event },
                                                                                              { ElementNames.GlobalResourceDictionary, AttributeNames.Source },
                                                                                              { ElementNames.Setter, AttributeNames.Property },
                                                                                              { ElementNames.Style, AttributeNames.TargetType },
                                                                                              { ElementNames.Trigger, AttributeNames.Property },
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

            if (info.Namespace.StartsWith(ActiproNamespace, OrdinalIgnoreCase))
            {
                return true;
            }

            if (info.Namespace.StartsWith(DevExpressNamespace, OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        public override string GetName(XmlTextReader reader)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                var defaultName = reader.GetAttribute(AttributeNames.Name, XamlNamespace) ?? reader.GetAttribute(AttributeNames.Key, XamlNamespace);
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

        public override string GetType(XmlTextReader reader) => reader.NodeType == XmlNodeType.Element ? reader.LocalName : base.GetType(reader);

        protected override bool ShallBeTerminalNode(ContainerOrTerminalNode node) => TerminalNodeNames.Contains(node?.Type);

        private static readonly char[] DirectorySeparators = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };

        private static string GetNameFromAttribute(XmlTextReader reader, string name)
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

        private static string GetFileName(string result)
        {
            // get rid of backslash or slash as we only are interested in the name, not the path
            // (and just add 1 and we get rid of situation that index might not be available ;))
            var fileName = result.Substring(result.LastIndexOfAny(DirectorySeparators) + 1);
            return fileName;
        }

        private static class ElementNames
        {
            internal const string Button = "Button";
            internal const string Binding = "Binding";
            internal const string ColumnDefinition = "ColumnDefinition";
            internal const string CheckBox = "CheckBox";
            internal const string ContentPresenter = "ContentPresenter";
            internal const string DataGridTemplateColumn = "DataGridTemplateColumn";
            internal const string EventSetter = "EventSetter";
            internal const string GlobalResourceDictionary = "GlobalResourceDictionary";
            internal const string Image = "Image";
            internal const string KeyBinding = "KeyBinding";
            internal const string Label = "Label";
            internal const string RowDefinition = "RowDefinition";
            internal const string Setter = "Setter";
            internal const string Style = "Style";
            internal const string TextBlock = "TextBlock";
            internal const string TextBox = "TextBox";
            internal const string Trigger = "Trigger";
        }

        private static class AttributeNames
        {
            internal const string Name = "Name";
            internal const string Key = "Key";
            internal const string Command = "Command";
            internal const string Content = "Content";
            internal const string Event = "Event";
            internal const string Property = "Property";
            internal const string Source = "Source";
            internal const string TargetType = "TargetType";
        }
    }
}