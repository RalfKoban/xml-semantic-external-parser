using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public sealed class XmlFlavorForMSBuild : XmlFlavor
    {
        private static readonly char[] Separator = { '\'' };

        private static readonly HashSet<string> NonTerminalNodeNames = new HashSet<string>
                                                                        {
                                                                            "ItemGroup",
                                                                            "ItemDefinitionGroup",
                                                                            "ImportGroup",
                                                                            "Project",
                                                                            "ProjectConfiguration",
                                                                            "PropertyGroup",
                                                                            "Target",
                                                                        };

        public override bool ParseAttributesEnabled => true;

        public override bool Supports(string filePath) => filePath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase)
                                                       || filePath.EndsWith(".vcxproj", StringComparison.OrdinalIgnoreCase)
                                                       || filePath.EndsWith(".vbproj", StringComparison.OrdinalIgnoreCase)
                                                       || filePath.EndsWith(".proj", StringComparison.OrdinalIgnoreCase)
                                                       || filePath.EndsWith(".modelproj", StringComparison.OrdinalIgnoreCase)
                                                       || filePath.EndsWith(".shproj", StringComparison.OrdinalIgnoreCase)
                                                       || filePath.EndsWith(".sqlproj", StringComparison.OrdinalIgnoreCase)
                                                       || filePath.EndsWith(".targets", StringComparison.OrdinalIgnoreCase)
                                                       || filePath.EndsWith(".props", StringComparison.OrdinalIgnoreCase)
                                                       || filePath.EndsWith(".projitems", StringComparison.OrdinalIgnoreCase);

        public override bool Supports(DocumentInfo info) => string.Equals(info.RootElement, "Project", StringComparison.OrdinalIgnoreCase)
                                                         && string.Equals(info.Namespace, "http://schemas.microsoft.com/developer/msbuild/2003", StringComparison.OrdinalIgnoreCase);

        public override string GetName(XmlTextReader reader)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                var name = reader.Name;
                switch (name)
                {
                    case "Analyzer":
                    case "BootstrapperPackage":
                    case "Compile":
                    case "Content":
                    case "EmbeddedResource":
                    case "None":
                    case "Page":
                    case "ProjectReference":
                    case "Reference":
                    case "Resource":
                    case "Validate":
                    {
                        return GetName(reader, name, "Include");
                    }

                    case "Import":
                    {
                        return GetName(reader, name, "Project");
                    }

                    case "Target":
                    {
                        return GetName(reader, name, "Name");
                    }

                    default:
                        return name;
                }
            }

            return base.GetName(reader);
        }

        public override string GetType(XmlTextReader reader) => reader.NodeType == XmlNodeType.Element ? reader.Name : base.GetType(reader);

        public override ContainerOrTerminalNode FinalAdjustAfterParsingComplete(ContainerOrTerminalNode node)
        {
            if (node is Container c)
            {
                var content = c.Children.FirstOrDefault(_ => _.Type == NodeType.Text)?.Content;
                if (content != null)
                {
                    c.Name = content;
                }
                else
                {
                    var attributes = c.Children.Where(_ => _.Type == NodeType.Attribute).OfType<TerminalNode>().ToList();
                    foreach (var attribute in attributes)
                    {
                        c.Children.Remove(attribute);
                    }

                    var suffix = GetNameSuffixForItemGroup(c) ?? GetNameSuffixForPropertyGroup(c, attributes);
                    if (!string.IsNullOrEmpty(suffix))
                    {
                        c.Name = string.Concat(c.Name, " '", suffix, "'");
                    }
                }
            }

            return base.FinalAdjustAfterParsingComplete(node);
        }

        protected override bool ShallBeTerminalNode(ContainerOrTerminalNode node) => !NonTerminalNodeNames.Contains(node?.Type);

        private static string GetName(XmlTextReader reader, string name, string attributeName)
        {
            var attribute = reader.GetAttribute(attributeName);
            if (attribute is null)
            {
                return name;
            }

            // if there is a comma, then we want to get the name before the comma
            var resultingName = attribute;
            var commaIndex = resultingName.IndexOf(',');
            if (commaIndex > 0)
            {
                resultingName = resultingName.Substring(0, commaIndex);
            }

            // get rid of backslash or slash as we only are interested in the name, not the path
            // just add 1 and we get rid of situation that index might not be available ;)
            resultingName = resultingName.Substring(resultingName.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase) + 1);
            resultingName = resultingName.Substring(resultingName.LastIndexOf('/') + 1);

            return resultingName;
        }

        private static string GetNameSuffixForItemGroup(Container container) => container.Name == "ItemGroup" && container.Children.Count > 0 ? container.Children[0].Type : null; // try to find special name for item groups, based on their children

        private static string GetNameSuffixForPropertyGroup(Container container, IEnumerable<TerminalNode> attributes)
        {
            if (container.Name == "PropertyGroup" && container.Children.Count > 0)
            {
                // try to find 'ProjectGuid' to see if we have default property group
                if (container.Children.Any(_ => _.Type == "ProjectGuid"))
                {
                    return "(default)";
                }

                var condition = attributes.FirstOrDefault(_ => _.Name == "Condition");
                if (condition != null)
                {
                    return condition.Content.Trim().Split(Separator, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
                }
            }

            return null;
        }
    }
}