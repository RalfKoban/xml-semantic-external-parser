using System.Collections.Generic;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Strategies
{
    public sealed class XmlStrategyForProject : XmlStrategy
    {
        private static readonly HashSet<string> TerminalNodeNames = new HashSet<string>
                                                                        {
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

                // var identifier = reader.GetAttribute("Include");
                // return identifier is null ? name : $"{name} '{identifier}'";
                return name;
            }

            return base.GetName(reader);
        }

        public override string GetType(XmlTextReader reader) => reader.NodeType == XmlNodeType.Element ? reader.Name : base.GetType(reader);

        public override bool ShallBeTerminalNode(Container container) => TerminalNodeNames.Contains(container?.Type);
    }
}