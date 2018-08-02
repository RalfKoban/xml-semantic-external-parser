using System.Collections.Generic;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Strategies
{
    public sealed class XmlStrategyForProject : XmlStrategy
    {
        private static readonly HashSet<string> TerminalNodeNames = new HashSet<string>
                                                                        {
                                                                            "Analyzer",
                                                                            "AppDesignerFolder",
                                                                            "ApplicationRevision",
                                                                            "ApplicationVersion",
                                                                            "AssemblyName",
                                                                            "AssemblyOriginatorKeyFile",
                                                                            "BootstrapperEnabled",
                                                                            "BootstrapperPackage",
                                                                            "CodeAnalysisDictionary",
                                                                            "CodeAnalysisRuleSet",
                                                                            "CodeAnalysisTreatWarningsAsErrors",
                                                                            "Compile",
                                                                            "Configuration",
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
                                                                            "FileAlignment",
                                                                            "FileUpgradeFlags",
                                                                            "Import",
                                                                            "Install",
                                                                            "InstallFrom",
                                                                            "IsWebBootstrapper",
                                                                            "LangVersion",
                                                                            "MapFileExtensions",
                                                                            "None",
                                                                            "NuGetPackageImportStamp",
                                                                            "OldToolsVersion",
                                                                            "Optimize",
                                                                            "OutputPath",
                                                                            "OutputType",
                                                                            "Platform",
                                                                            "PostBuildEvent",
                                                                            "PostSharpObfuscationAwarenessEnabled",
                                                                            "PostSharpOptimizationMode",
                                                                            "PostSharpTargetProcessor",
                                                                            "PreBuildEvent",
                                                                            "Prefer32Bit",
                                                                            "ProductVersion",
                                                                            "ProjectGuid",
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
                                                                            "UpdateEnabled",
                                                                            "UpdateInterval",
                                                                            "UpdateIntervalUnits",
                                                                            "UpdateMode",
                                                                            "UpdatePeriodically",
                                                                            "UpdateRequired",
                                                                            "UpgradeBackupLocation",
                                                                            "UseApplicationTrust",
                                                                            "Warning",
                                                                            "WarningLevel",
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