﻿using System.Collections.Generic;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public sealed class XmlFlavorForProject : XmlFlavor
    {
        private static readonly HashSet<string> NonTerminalNodeNames = new HashSet<string>
                                                                        {
                                                                            "ItemGroup",
                                                                            "ItemDefinitionGroup",
                                                                            "ImportGroup",
                                                                            "Project",
                                                                            "ProjectConfiguration",
                                                                            "PropertyGroup",
                                                                        };

        private static readonly HashSet<string> TerminalNodeNames = new HashSet<string>
                                                                        {
                                                                            "AdditionalFiles",
                                                                            "AdditionalIncludeDirectories",
                                                                            "Analyzer",
                                                                            "AppDesignerFolder",
                                                                            "ApplicationRevision",
                                                                            "ApplicationVersion",
                                                                            "AssemblyName",
                                                                            "AssemblyOriginatorKeyFile",
                                                                            "BootstrapperEnabled",
                                                                            "BootstrapperPackage",
                                                                            "CharacterSet",
                                                                            "ClCompile",
                                                                            "ClInclude",
                                                                            "CodeAnalysisDictionary",
                                                                            "CodeAnalysisRuleAssemblies",
                                                                            "CodeAnalysisRules",
                                                                            "CodeAnalysisRuleSet",
                                                                            "CodeAnalysisTreatWarningsAsErrors",
                                                                            "Command",
                                                                            "Compile",
                                                                            "Configuration",
                                                                            "ConfigurationType",
                                                                            "Content",
                                                                            "DebugSymbols",
                                                                            "DebugType",
                                                                            "DefineConstants",
                                                                            "DocumentationFile",
                                                                            "DontImportPostSharp",
                                                                            "EmbeddedResource",
                                                                            "Error",
                                                                            "ErrorReport",
                                                                            "ErrorText",
                                                                            "ExcludePath",
                                                                            "ExcludedFromBuild",
                                                                            "FileAlignment",
                                                                            "FileUpgradeFlags",
                                                                            "Filter",
                                                                            "Import",
                                                                            "IncludePath",
                                                                            "Install",
                                                                            "InstallFrom",
                                                                            "IntDir",
                                                                            "IsWebBootstrapper",
                                                                            "Keyword",
                                                                            "LangVersion",
                                                                            "LinkIncremental",
                                                                            "MapFileExtensions",
                                                                            "None",
                                                                            "NuGetPackageImportStamp",
                                                                            "OldToolsVersion",
                                                                            "Optimize",
                                                                            "OutDir",
                                                                            "OutputPath",
                                                                            "OutputType",
                                                                            "Platform",
                                                                            "PlatformTarget",
                                                                            "PlatformToolset",
                                                                            "PostBuildEvent",
                                                                            "PostSharpObfuscationAwarenessEnabled",
                                                                            "PostSharpOptimizationMode",
                                                                            "PostSharpTargetProcessor",
                                                                            "PreBuildEvent",
                                                                            "PrecompiledHeader",
                                                                            "Prefer32Bit",
                                                                            "ProductVersion",
                                                                            "ProgramDatabaseFile",
                                                                            "ProjectGuid",
                                                                            "ProjectName",
                                                                            "ProjectReference",
                                                                            "PublishUrl",
                                                                            "Reference",
                                                                            "RestorePackages",
                                                                            "RootNamespace",
                                                                            "RunCodeAnalysis",
                                                                            "SchemaVersion",
                                                                            "SignAssembly",
                                                                            "SolutionDir",
                                                                            "StyleCopEnabled",
                                                                            "StyleCopMSBuildTargetsFile",
                                                                            "StyleCopTreatErrorsAsWarnings",
                                                                            "TargetFrameworkProfile",
                                                                            "TargetFrameworkVersion",
                                                                            "TargetName",
                                                                            "TreatWarningsAsErrors",
                                                                            "UniqueIdentifier",
                                                                            "UpdateEnabled",
                                                                            "UpdateInterval",
                                                                            "UpdateIntervalUnits",
                                                                            "UpdateMode",
                                                                            "UpdatePeriodically",
                                                                            "UpdateRequired",
                                                                            "UpgradeBackupLocation",
                                                                            "UseApplicationTrust",
                                                                            "UseDebugLibraries",
                                                                            "Warning",
                                                                            "WarningLevel",
                                                                            "WindowsTargetPlatformVersion",
                                                                            "XMLDocumentationFileName",
                                                                        };

        public override bool ParseAttributesEnabled => false;

        public override string GetName(XmlTextReader reader)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                var name = reader.Name;
                switch (name)
                {
                    case "Analyzer":
                    case "Compile":
                    case "Content":
                    case "EmbeddedResource":
                    case "None":
                    case "ProjectReference":
                    case "Reference":
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

        protected override bool ShallBeTerminalNode(ContainerOrTerminalNode node) => !NonTerminalNodeNames.Contains(node?.Type);

        private static string GetName(XmlTextReader reader, string name, string attributeName)
        {
            var identifier = reader.GetAttribute(attributeName)?.Replace("\\", " \\ "); // workaround for Semantic/GMaster RegEx parsing exception that is not aware of special backslash character sequences
            return $"{name} '{identifier}'";
        }
    }
}