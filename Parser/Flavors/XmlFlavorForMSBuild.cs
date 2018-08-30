using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public sealed class XmlFlavorForMSBuild : XmlFlavor
    {
        private static readonly char[] Separator = { '\'' };

        private static readonly char[] DirectorySeparators = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };

        private static readonly HashSet<string> NonTerminalNodeNames = new HashSet<string>
                                                                        {
                                                                            ElementNames.ItemGroup,
                                                                            ElementNames.ItemDefinitionGroup,
                                                                            ElementNames.ImportGroup,
                                                                            ElementNames.Project,
                                                                            ElementNames.ProjectConfiguration,
                                                                            ElementNames.PropertyGroup,
                                                                            ElementNames.Target,
                                                                        };

        private static readonly Regex VersionNumberRegex = new Regex("(.{1}([0-9]+.{1})+)+"); // format of version numbers is ".1.2.3.4"

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

        public override bool Supports(DocumentInfo info) => string.Equals(info.RootElement, ElementNames.Project, StringComparison.OrdinalIgnoreCase)
                                                         && string.Equals(info.Namespace, "http://schemas.microsoft.com/developer/msbuild/2003", StringComparison.OrdinalIgnoreCase);

        public override string GetName(XmlTextReader reader)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                var name = reader.Name;
                var attributeName = GetAttributeName(name);
                return attributeName == null ? name : GetName(reader, name, attributeName);
            }

            return base.GetName(reader);
        }

        public override string GetType(XmlTextReader reader) => reader.NodeType == XmlNodeType.Element ? reader.Name : base.GetType(reader);

        public override string GetContent(XmlTextReader reader)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                var name = reader.Name;
                var attributeName = GetAttributeName(name);
                if (attributeName != null)
                {
                    // we want to have the full path here to be able to adjust the item group later on
                    var attribute = reader.GetAttribute(attributeName);
                    return attribute ?? reader.Value;
                }

                // other elements shall not have a content
                return null;
            }

            return base.GetContent(reader);
        }

        public override ContainerOrTerminalNode FinalAdjustAfterParsingComplete(ContainerOrTerminalNode node)
        {
            if (node is Container c)
            {
                var attributes = c.Children.Where(_ => _.Type == NodeType.Attribute).OfType<TerminalNode>().ToList();
                c.Children.RemoveAll(TypeCanBeIgnored);

                var content = c.Children.FirstOrDefault(_ => _.Type == NodeType.Text)?.Content;
                if (content != null)
                {
                    c.Name = content;
                }
                else
                {
                    var suffix = GetNameSuffixForItemGroup(c) ?? GetNameSuffixForPropertyGroup(c, attributes);
                    var typeSuffix = GetTypeSuffixForItemGroup(c);

                    if (!string.IsNullOrEmpty(suffix))
                    {
                        c.Name = typeSuffix ?? suffix;
                    }

                    if (!string.IsNullOrEmpty(typeSuffix))
                    {
                        c.Type = string.Concat(c.Type, " '", typeSuffix, "'");
                    }
                }
            }

            return base.FinalAdjustAfterParsingComplete(node);
        }

        protected override bool ShallBeTerminalNode(ContainerOrTerminalNode node)
        {
            var type = node?.Type ?? string.Empty;

            foreach (var name in NonTerminalNodeNames)
            {
                if (string.Equals(type, name, StringComparison.Ordinal) || (type.Length > name.Length && type.StartsWith(name + " ", StringComparison.Ordinal)))
                {
                    return false;
                }
            }

            return true;
        }

        private static string GetName(XmlTextReader reader, string name, string attributeName)
        {
            var result = reader.GetAttribute(attributeName);
            if (result is null)
            {
                return name;
            }

            // if there is a comma, then we want to get the name before the comma (except that we have a directory at the end)
            var commaIndex = result.IndexOf(',');
            var directorySeparatorIndex = result.LastIndexOfAny(DirectorySeparators);
            if (commaIndex > 0 && commaIndex > directorySeparatorIndex)
            {
                result = result.Substring(0, commaIndex);
            }

            return GetFileName(result);
        }

        private static string GetFileName(string result)
        {
            // get rid of backslash or slash as we only are interested in the name, not the path
            // (and just add 1 and we get rid of situation that index might not be available ;))
            var fileName = result.Substring(result.LastIndexOfAny(DirectorySeparators) + 1);
            return fileName;
        }

        private static string GetNameSuffixForItemGroup(Container container) => container.Name == ElementNames.ItemGroup
                                                                                ? container.Children.FirstOrDefault(_ => !TypeCanBeIgnored(_))?.Type
                                                                                : null;

        private static string GetNameSuffixForPropertyGroup(Container container, IEnumerable<TerminalNode> attributes)
        {
            if (container.Name == ElementNames.PropertyGroup && container.Children.Count > 0)
            {
                // try to find 'ProjectGuid' to see if we have default property group
                if (container.Children.Any(_ => _.Type == ElementNames.ProjectGuid))
                {
                    return "(default)";
                }

                var condition = attributes.FirstOrDefault(_ => _.Name == AttributeNames.Condition);
                if (condition != null)
                {
                    return condition.Content.Trim().Split(Separator, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
                }
            }

            return null;
        }

        private static string GetTypeSuffixForItemGroup(Container container)
        {
            if (container.Name != ElementNames.ItemGroup)
            {
                return null;
            }

            // try to find special name for item groups, based on their children
            var children = container.Children.Where(_ => !TypeCanBeIgnored(_)).ToList();
            if (children.Count == 0)
            {
                return null;
            }

            var suffix = children[0].Type;
            var appendix = GetTypeSuffixForItemGroup(suffix, children);
            return appendix is null ? suffix : suffix + string.Concat(" '", appendix, "'");
        }

        private static string GetTypeSuffixForItemGroup(string suffix, List<ContainerOrTerminalNode> children)
        {
            switch (suffix)
            {
                case ElementNames.Analyzer:
                {
                    // try to find children with same starting path before name
                    var distinctContents = children
                                                .Select(_ => _.Content?.Replace(_.Name, string.Empty))
                                                .Where(_ => !string.IsNullOrWhiteSpace(_))
                                                .Select(_ => _.Substring(0, VersionNumberRegex.Match(_).Index))
                                                .Select(GetFileName)
                                                .ToHashSet();

                    if (distinctContents.Count == 1)
                    {
                        return distinctContents.First();
                    }

                    break;
                }

                case ElementNames.Compile:
                case ElementNames.None:
                case ElementNames.Page:
                {
                    // try to find children with same path before name
                    var distinctContents = children
                                                .Select(_ => _.Content?.Replace(_.Name, string.Empty))
                                                .ToHashSet();
                    if (distinctContents.Count == 1)
                    {
                        return distinctContents.First();
                    }

                    break;
                }
            }

            return null;
        }

        private static string GetAttributeName(string name)
        {
            switch (name)
            {
                case ElementNames.Analyzer:
                case ElementNames.BootstrapperPackage:
                case ElementNames.Compile:
                case ElementNames.Content:
                case ElementNames.EmbeddedResource:
                case ElementNames.None:
                case ElementNames.Page:
                case ElementNames.ProjectReference:
                case ElementNames.Reference:
                case ElementNames.Resource:
                case ElementNames.Validate:
                    return AttributeNames.Include;

                case ElementNames.Import:
                    return AttributeNames.Project;

                case ElementNames.Target:
                    return AttributeNames.Name;

                default:
                    return null;
            }
        }

        private static bool TypeCanBeIgnored(ContainerOrTerminalNode node)
        {
            var type = node?.Type;
            return type == NodeType.Comment || type == NodeType.Attribute;
        }

        private static class ElementNames
        {
            internal const string Analyzer = "Analyzer";
            internal const string BootstrapperPackage = "BootstrapperPackage";
            internal const string Compile = "Compile";
            internal const string Content = "Content";
            internal const string EmbeddedResource = "EmbeddedResource";
            internal const string Import = "Import";
            internal const string ImportGroup = "ImportGroup";
            internal const string ItemDefinitionGroup = "ItemDefinitionGroup";
            internal const string ItemGroup = "ItemGroup";
            internal const string None = "None";
            internal const string Page = "Page";
            internal const string Project = "Project";
            internal const string ProjectConfiguration = "ProjectConfiguration";
            internal const string ProjectGuid = "ProjectGuid";
            internal const string ProjectReference = "ProjectReference";
            internal const string PropertyGroup = "PropertyGroup";
            internal const string Reference = "Reference";
            internal const string Resource = "Resource";
            internal const string Target = "Target";
            internal const string Validate = "Validate";
        }

        private static class AttributeNames
        {
            internal const string Condition = "Condition";
            internal const string Include = "Include";
            internal const string Name = "Name";
            internal const string Project = "Project";
        }
    }
}