﻿using System.Collections.Generic;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Strategies
{
    public sealed class XmlStrategyForWix : XmlStrategy
    {
        private static readonly HashSet<string> TerminalNodeNames = new HashSet<string>
                                                                        {
                                                                            "ApprovedExeForElevation",
                                                                            "AllocateRegistrySpace",
                                                                            "AppSearch",
                                                                            "AssemblyName",
                                                                            "BinaryRef",
                                                                            "BindImage",
                                                                            "Catalog",
                                                                            "CCPSearch",
                                                                            "Column",
                                                                            "CommandLine",
                                                                            "ComponentGroupRef",
                                                                            "ComponentRef",
                                                                            "Condition",
                                                                            "Configuration",
                                                                            "ConfigurationData",
                                                                            "ContainerRef",
                                                                            "CopyFile",
                                                                            "CostFinalize",
                                                                            "CostInitialize",
                                                                            "CreateFolders",
                                                                            "CreateShortcuts",
                                                                            "Custom",
                                                                            "CustomAction",
                                                                            "CustomActionRef",
                                                                            "CustomProperty",
                                                                            "Data",
                                                                            "DeleteServices",
                                                                            "Dependency",
                                                                            "DialogRef",
                                                                            "DigitalCertificate",
                                                                            "DigitalCertificateRef",
                                                                            "DisableRollback",
                                                                            "DuplicateFiles",
                                                                            "EmbeddedChainer",
                                                                            "EmbeddedChainerRef",
                                                                            "EmbeddedUIResource",
                                                                            "EnsureTable",
                                                                            "Environment",
                                                                            "Error",
                                                                            "Exclusion",
                                                                            "ExecuteAction",
                                                                            "ExitCode",
                                                                            "Failure",
                                                                            "FeatureGroupRef",
                                                                            "FileCost",
                                                                            "FileSearch",
                                                                            "FileSearchRef",
                                                                            "FileTypeMask",
                                                                            "FindRelatedProducts",
                                                                            "ForceReboot",
                                                                            "Icon",
                                                                            "IconRef",
                                                                            "IgnoreModularization",
                                                                            "IgnoreRange",
                                                                            "IgnoreTable",
                                                                            "IniFile",
                                                                            "InstallAdminPackage",
                                                                            "InstallExecute",
                                                                            "InstallExecuteAgain",
                                                                            "InstallFiles",
                                                                            "InstallFinalize",
                                                                            "InstallInitialize",
                                                                            "InstallODBC",
                                                                            "InstallServices",
                                                                            "InstallValidate",
                                                                            "Instance",
                                                                            "Interface",
                                                                            "IsolateComponent",
                                                                            "IsolateComponents",
                                                                            "MajorUpgrade",
                                                                            "MergeRef",
                                                                            "UIRef",
                                                                            "Variable",
                                                                        };

        public override bool ParseAttributesEnabled => false;

        public override string GetName(XmlTextReader reader) => reader.NodeType == XmlNodeType.Element ? reader.GetAttribute("Id") ?? reader.Name : base.GetName(reader);

        public override string GetType(XmlTextReader reader) => reader.NodeType == XmlNodeType.Element ? reader.Name : base.GetType(reader);

        public override bool ShallBeTerminalNode(Container container) => TerminalNodeNames.Contains(container?.Type);
    }
}