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
                                                                            "AssemblyName",
                                                                            "AssemblyOriginatorKeyFile",
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
                                                                            "EmbeddedResource",
                                                                            "Error",
                                                                            "ErrorReport",
                                                                            "ErrorText",
                                                                            "FileAlignment",
                                                                            "FileUpgradeFlags",
                                                                            "Import",
                                                                            "LangVersion",
                                                                            "None",
                                                                            "NuGetPackageImportStamp",
                                                                            "Optimize",
                                                                            "OutputPath",
                                                                            "OutputType",
                                                                            "Platform",
                                                                            "PostSharpTargetProcessor",
                                                                            "Prefer32Bit",
                                                                            "PostBuildEvent",
                                                                            "PreBuildEvent",
                                                                            "ProjectGuid",
                                                                            "ProjectReference",
                                                                            "Reference",
                                                                            "RestorePackages",
                                                                            "RootNamespace",
                                                                            "RunCodeAnalysis",
                                                                            "SignAssembly",
                                                                            "SolutionDir",
                                                                            "StyleCopEnabled",
                                                                            "StyleCopTreatErrorsAsWarnings",
                                                                            "TargetFrameworkProfile",
                                                                            "TargetFrameworkVersion",
                                                                            "TreatWarningsAsErrors",
                                                                            "WarningLevel",
                                                                            "OldToolsVersion",
                                                                            "UpgradeBackupLocation",
                                                                            "PublishUrl",
                                                                            "Install",
                                                                            "InstallFrom",
                                                                            "UpdateEnabled",
                                                                            "UpdateMode",
                                                                            "UpdateInterval",
                                                                            "UpdateIntervalUnits",
                                                                            "UpdatePeriodically",
                                                                            "UpdateRequired",
                                                                            "MapFileExtensions",
                                                                            "ApplicationRevision",
                                                                            "ApplicationVersion",
                                                                            "IsWebBootstrapper",
                                                                            "UseApplicationTrust",
                                                                            "BootstrapperEnabled",
                                                                            "ProductVersion",
                                                                            "SchemaVersion",
                                                                            "Warning",
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