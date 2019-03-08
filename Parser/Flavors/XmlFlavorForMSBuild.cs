using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public class XmlFlavorForMSBuild : XmlFlavor
    {
        private static readonly char[] DirectorySeparators = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };

        private static readonly HashSet<string> NonTerminalNodeNames = new HashSet<string>
                                                                           {
                                                                               ElementNames.Choose,
                                                                               ElementNames.ImportGroup,
                                                                               ElementNames.ItemDefinitionGroup,
                                                                               ElementNames.ItemGroup,
                                                                               ElementNames.Otherwise,
                                                                               ElementNames.Project,
                                                                               ElementNames.ProjectConfiguration,
                                                                               ElementNames.PropertyGroup,
                                                                               ElementNames.Target,
                                                                               ElementNames.When,
                                                                               SHFB.ElementNames.ApiFilter,
                                                                               SHFB.ElementNames.ComponentConfigurations,
                                                                               SHFB.ElementNames.DocumentationSources,
                                                                               SHFB.ElementNames.NamespaceSummaries,
                                                                               SHFB.ElementNames.PlugInConfigurations,
                                                                           };

        private static readonly Regex VersionNumberRegex = new Regex("(.{1}([0-9]+.{1})+)+"); // format of version numbers is ".1.2.3.4"

        public override bool ParseAttributesEnabled => true;

        public override bool Supports(string filePath) => filePath.EndsWith(".builds", StringComparison.OrdinalIgnoreCase)
                                                       || filePath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase)
                                                       || filePath.EndsWith(".fsproj", StringComparison.OrdinalIgnoreCase)
                                                       || filePath.EndsWith(".ilproj", StringComparison.OrdinalIgnoreCase)
                                                       || filePath.EndsWith(".jsproj", StringComparison.OrdinalIgnoreCase)
                                                       || filePath.EndsWith(".modelproj", StringComparison.OrdinalIgnoreCase)
                                                       || filePath.EndsWith(".nativeproj", StringComparison.OrdinalIgnoreCase)
                                                       || filePath.EndsWith(".njsproj", StringComparison.OrdinalIgnoreCase)
                                                       || filePath.EndsWith(".proj", StringComparison.OrdinalIgnoreCase)
                                                       || filePath.EndsWith(".props", StringComparison.OrdinalIgnoreCase)
                                                       || filePath.EndsWith(".projitems", StringComparison.OrdinalIgnoreCase)
                                                       || filePath.EndsWith(".pyproj", StringComparison.OrdinalIgnoreCase)
                                                       || filePath.EndsWith(".shproj", StringComparison.OrdinalIgnoreCase)
                                                       || filePath.EndsWith(".sqlproj", StringComparison.OrdinalIgnoreCase)
                                                       || filePath.EndsWith(".targets", StringComparison.OrdinalIgnoreCase)
                                                       || filePath.EndsWith(".vbproj", StringComparison.OrdinalIgnoreCase)
                                                       || filePath.EndsWith(".vcxproj", StringComparison.OrdinalIgnoreCase);

        public override bool Supports(DocumentInfo info)
        {
            switch (info.Namespace)
            {
                case "http://schemas.microsoft.com/developer/msbuild/2003":
                    return string.Equals(info.RootElement, ElementNames.Project, StringComparison.OrdinalIgnoreCase);
                case null:
                    return string.Equals(info.RootElement, ElementNames.Project, StringComparison.Ordinal);
                default:
                    return false;
            }
        }

        public override string GetName(XmlTextReader reader)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                var name = reader.Name;
                var attributeName = GetAttributeName(name);
                var alternativeAttributeName = GetAlternativeAttributeName(name);
                return GetName(reader, name, attributeName, alternativeAttributeName).Trim();
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
                // get all attributes here as they get vanished in the next line
                var attributes = c.Children.Where(_ => _.Type == NodeType.Attribute).OfType<TerminalNode>().ToList();
                c.Children.RemoveAll(TypeCanBeIgnored);

                var content = c.Children.FirstOrDefault(_ => _.Type == NodeType.Text)?.Content;
                if (content != null)
                {
                    FinalAdjustNodeWithContent(c, attributes, content);
                }
                else
                {
                    FinalAdjustNodeWithoutContent(c, attributes);
                }
            }

            return base.FinalAdjustAfterParsingComplete(node);
        }

        protected override bool ShallBeTerminalNode(ContainerOrTerminalNode node)
        {
            var type = node?.Type ?? string.Empty;

            foreach (var name in NonTerminalNodeNames)
            {
                if (IsNonTerminalNodeName(type, name))
                {
                    return false;
                }
            }

            // SHFB filters should only be adjusted for namespace
            if (IsNonTerminalNodeName(type, SHFB.ElementNames.Filter))
            {
                return !type.Contains(SHFB.Filter_EntryType_Namespace);
            }

            return true;
        }

        private static bool IsNonTerminalNodeName(string type, string nonTerminalNodeName) => type == nonTerminalNodeName || (type.Length > nonTerminalNodeName.Length && type.StartsWith(nonTerminalNodeName + " ", StringComparison.Ordinal));

        private static string GetName(XmlTextReader reader, string name, string attributeName, string alternativeAttributeName)
        {
            var result = GetAttribute(reader, attributeName) ?? GetAttribute(reader, alternativeAttributeName);
            if (result is null)
            {
                return name;
            }

            if (name == ElementNames.Folder)
            {
                // if it is a folder, then we want to have the complete name
                return result;
            }

            if (attributeName == AttributeNames.Condition)
            {
                // if it is a condition, we want to have the complete condition name
                return result;
            }

            // if there is a comma, then we want to get the name before the comma (except that we have a directory at the end)
            var commaIndex = result.IndexOf(',');
            var directorySeparatorIndex = result.LastIndexOfAny(DirectorySeparators);
            if (commaIndex > 0 && commaIndex > directorySeparatorIndex)
            {
                result = result.Substring(0, commaIndex);
            }

            const string Marker = "GetPathOfFileAbove('";
            var mentionedFileIndex = result.IndexOf(Marker);
            if (mentionedFileIndex >= 0)
            {
                result = result.Substring(mentionedFileIndex + Marker.Length);
                result = result.Substring(0, result.IndexOf('\''));
            }

            return GetFileName(result);
        }

        private static string GetAttribute(XmlTextReader reader, string attributeName) => attributeName is null ? null : reader.GetAttribute(attributeName);

        private static void FinalAdjustNodeWithContent(Container c, IEnumerable<TerminalNode> attributes, string content)
        {
            if (c.Type == SHFB.ElementNames.NamespaceSummaryItem)
            {
                    c.Name = attributes.FirstOrDefault(_ => _.Name == SHFB.AttributeNames.Name)?.Content;
            }
        }

        private static void FinalAdjustNodeWithoutContent(Container c, IEnumerable<TerminalNode> attributes)
        {
            var suffix = GetNameSuffix(c, attributes);
            var typeSuffix = GetTypeSuffix(c, attributes);

            if (!string.IsNullOrEmpty(suffix))
            {
                c.Name = typeSuffix ?? suffix;
            }

            if (!string.IsNullOrEmpty(typeSuffix))
            {
                c.Type = string.Concat(c.Type, " '", typeSuffix, "'");
            }
        }

        private static string GetNameSuffix(Container c, IEnumerable<TerminalNode> attributes)
        {
            switch (c.Name)
            {
                case ElementNames.ItemGroup:
                    return GetNameSuffixForItemGroup(c);

                case ElementNames.PropertyGroup:
                    return GetNameSuffixForPropertyGroup(c, attributes);

                default:
                    return null;
            }
        }

        private static string GetTypeSuffix(Container c, IEnumerable<TerminalNode> attributes)
        {
            switch (c.Type)
            {
                case ElementNames.ItemGroup:
                    return GetTypeSuffixForItemGroup(c);

                case SHFB.ElementNames.Filter:
                    return attributes?.FirstOrDefault(_ => _.Name == SHFB.AttributeNames.EntryType)?.Content;

                default:
                    return null;
            }
        }

        private static string GetFileName(string result)
        {
            // get rid of backslash or slash as we only are interested in the name, not the path
            // (and just add 1 and we get rid of situation that index might not be available ;))
            var fileName = result.Substring(result.LastIndexOfAny(DirectorySeparators) + 1);

            if (fileName == "*" || fileName == "**")
            {
                // get the path
                var path = GetFilePath(result);
                var pathWithFileName = result.Substring(path.LastIndexOfAny(DirectorySeparators) + 1);
                return pathWithFileName;
            }

            // try to get rid of last bracket
            // (and just add 1 and we get rid of situation that the bracket might not be available ;))
            var potentialFileName = fileName.Substring(fileName.LastIndexOf(')') + 1);
            return potentialFileName.Length > 0
                 ? potentialFileName
                 : fileName;
        }

        private static string GetFilePath(string result)
        {
            // get rid of backslash or slash as we only are interested in the name, not the path
            // (and just add 1 and we get rid of situation that index might not be available ;))
            var index = result.LastIndexOfAny(DirectorySeparators);
            return index > 0 ? result.Substring(0, index) : result;
        }

        private static string GetNameSuffixForItemGroup(Container container) => container.Children.FirstOrDefault(_ => !TypeCanBeIgnored(_))?.Type;

        private static string GetNameSuffixForPropertyGroup(Container container, IEnumerable<TerminalNode> attributes)
        {
            if (container.Children.Count > 0)
            {
                // try to find 'ProjectGuid' to see if we have default property group
                if (container.Children.Any(_ => _.Type == ElementNames.ProjectGuid))
                {
                    return "(default)";
                }

                // try to find the special group that has the pre/post build events defined and use that one as a single special group
                if (container.Children.Any(_ => _.Type == ElementNames.PreBuildEvent || _.Type == ElementNames.PostBuildEvent))
                {
                    return "Pre/Post-build events";
                }
            }

            var condition = attributes.FirstOrDefault(_ => _.Name == AttributeNames.Condition);
            return condition?.Content.Trim();
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
                                                .Select(_ => _.Content)
                                                .Where(_ => !string.IsNullOrWhiteSpace(_))
                                                .Select(GetFilePath)
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
/*
                case ElementNames.AdditionalFiles:
                case ElementNames.Analyzer:
                case ElementNames.AssemblyMetadata:
                case ElementNames.BootstrapperPackage:
                case ElementNames.CodeAnalysisDependentAssemblyPaths:
                case ElementNames.CodeAnalysisDictionary:
                case ElementNames.Compile:
                case ElementNames.Content:
                case ElementNames.CoreRootProjectLockJsonFiles:
                case ElementNames.CppCompile:
                case ElementNames.CrossGenFiles:
                case ElementNames.DependencyBuildInfo:
                case ElementNames.EmbeddedResource:
                case ElementNames.EntityDeploy:
                case ElementNames.ExcludeList:
                case ElementNames.ExcludeTraitsItems:
                case ElementNames.Folder:
                case ElementNames.IncludeTraitsItems:
                case ElementNames.NativeProjectBinaries:
                case ElementNames.None:
                case ElementNames.OfficialBuildRID:
                case ElementNames.PackageReference:
                case ElementNames.Page:
                case ElementNames.ProductProjectLockJsonFiles:
                case ElementNames.Project:
                case ElementNames.ProjectReference:
                case ElementNames.RefProjectLockJsonFiles:
                case ElementNames.Reference:
                case ElementNames.RemoteDependencyBuildInfo:
                case ElementNames.Resource:
                case ElementNames.Service:
                case ElementNames.SgenTypes:
                case ElementNames.StaticDependency:
                case ElementNames.TestTargetFramework:
                case ElementNames.VCRuntimeFiles:
                case ElementNames.Validate:
                case ElementNames.XUnitDependency:
                case ElementNames.XUnitPerformanceApiDependency:
                case ElementNames.XmlUpdateStep:
                case ElementNames.XunitPerformanceDependency:
                    return AttributeNames.Include;
*/
                case ElementNames.Import:
                    return AttributeNames.Project;

                case ElementNames.MSBuild:
                    return AttributeNames.Projects;

                case ElementNames.Target:
                    return AttributeNames.Name;

                case ElementNames.CallTarget:
                    return AttributeNames.Targets;

                case SHFB.ElementNames.ComponentConfig:
                case SHFB.ElementNames.PlugInConfig:
                    return SHFB.AttributeNames.Id;

                case SHFB.ElementNames.Filter:
                    return SHFB.AttributeNames.FullName;

                case SHFB.ElementNames.DocumentationSource:
                    return SHFB.AttributeNames.SourceFile;

                case SHFB.ElementNames.NamespaceSummaryItem:
                    return SHFB.AttributeNames.Name;

                case ElementNames.DefineConstants:
                    return AttributeNames.Condition;

                default:
                    return AttributeNames.Include;
            }
        }

        private static string GetAlternativeAttributeName(string name)
        {
            switch (name)
            {
                case ElementNames.Compile:
                case ElementNames.Content:
                case ElementNames.EmbeddedResource:
                case ElementNames.None:
                case ElementNames.PackageReference:
                case ElementNames.Page:
                case ElementNames.Resource:
                    return AttributeNames.Update;

                default:
                    return null;
            }
        }

        private static bool TypeCanBeIgnored(ContainerOrTerminalNode node)
        {
            var type = node?.Type;
            return type == NodeType.Comment || type == NodeType.Attribute;
        }

        private protected static class ElementNames
        {
            internal const string Analyzer = "Analyzer";
            internal const string CallTarget = "CallTarget";
            internal const string Choose = "Choose";
            internal const string Compile = "Compile";
            internal const string Content = "Content";
            internal const string DefineConstants = "DefineConstants";
            internal const string EmbeddedResource = "EmbeddedResource";
            internal const string Folder = "Folder";
            internal const string Import = "Import";
            internal const string ImportGroup = "ImportGroup";
            internal const string ItemDefinitionGroup = "ItemDefinitionGroup";
            internal const string ItemGroup = "ItemGroup";
            internal const string MSBuild = "MSBuild";
            internal const string None = "None";
            internal const string Otherwise = "Otherwise";
            internal const string PackageReference = "PackageReference";
            internal const string Page = "Page";
            internal const string PostBuildEvent = "PostBuildEvent";
            internal const string PreBuildEvent = "PreBuildEvent";
            internal const string Project = "Project";
            internal const string ProjectConfiguration = "ProjectConfiguration";
            internal const string ProjectGuid = "ProjectGuid";
            internal const string PropertyGroup = "PropertyGroup";
            internal const string Resource = "Resource";
            internal const string Target = "Target";
            internal const string When = "When";
/*
            internal const string AdditionalFiles = "AdditionalFiles";
            internal const string AssemblyMetadata = "AssemblyMetadata";
            internal const string BootstrapperPackage = "BootstrapperPackage";
            internal const string CodeAnalysisDependentAssemblyPaths = "CodeAnalysisDependentAssemblyPaths";
            internal const string CodeAnalysisDictionary = "CodeAnalysisDictionary";
            internal const string CoreRootProjectLockJsonFiles = "CoreRootProjectLockJsonFiles";
            internal const string CppCompile = "CppCompile";
            internal const string CrossGenFiles = "CrossGenFiles";
            internal const string DependencyBuildInfo = "DependencyBuildInfo";
            internal const string EntityDeploy = "EntityDeploy";
            internal const string ExcludeList = "ExcludeList";
            internal const string ExcludeTraitsItems = "ExcludeTraitsItems";
            internal const string IncludeTraitsItems = "IncludeTraitsItems";
            internal const string NativeProjectBinaries = "NativeProjectBinaries";
            internal const string OfficialBuildRID = "OfficialBuildRID";
            internal const string ProductProjectLockJsonFiles = "ProductProjectLockJsonFiles";
            internal const string ProjectReference = "ProjectReference";
            internal const string RefProjectLockJsonFiles = "RefProjectLockJsonFiles";
            internal const string Reference = "Reference";
            internal const string RemoteDependencyBuildInfo = "RemoteDependencyBuildInfo";
            internal const string Service = "Service";
            internal const string SgenTypes = "SgenTypes";
            internal const string StaticDependency = "StaticDependency";
            internal const string TestTargetFramework = "TestTargetFramework";
            internal const string VCRuntimeFiles = "VCRuntimeFiles";
            internal const string Validate = "Validate";
            internal const string XUnitDependency = "XUnitDependency";
            internal const string XUnitPerformanceApiDependency = "XUnitPerformanceApiDependency";
            internal const string XmlUpdateStep = "XmlUpdateStep";
            internal const string XunitPerformanceDependency = "XunitPerformanceDependency";
*/
        }

        private static class AttributeNames
        {
            internal const string Condition = "Condition";
            internal const string Include = "Include";
            internal const string Name = "Name";
            internal const string Project = "Project";
            internal const string Projects = "Projects";
            internal const string Targets = "Targets";
            internal const string Update = "Update";
        }

        private static class SHFB
        {
            internal const string Filter_EntryType_Namespace = "Namespace";

            internal static class ElementNames
            {
                internal const string ApiFilter = "ApiFilter";
                internal const string ComponentConfig = "ComponentConfig";
                internal const string ComponentConfigurations = "ComponentConfigurations";
                internal const string DocumentationSource = "DocumentationSource";
                internal const string DocumentationSources = "DocumentationSources";
                internal const string Filter = "Filter";
                internal const string NamespaceSummaries = "NamespaceSummaries";
                internal const string NamespaceSummaryItem = "NamespaceSummaryItem";
                internal const string PlugInConfig = "PlugInConfig";
                internal const string PlugInConfigurations = "PlugInConfigurations";
            }

            internal static class AttributeNames
            {
                internal const string EntryType = "entryType";
                internal const string FullName = "fullName";
                internal const string Id = "id";
                internal const string Name = "name";
                internal const string SourceFile = "sourceFile";
            }
        }
    }
}