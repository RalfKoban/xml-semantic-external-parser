﻿using System.Collections.Generic;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public sealed class XmlFlavorForWix : XmlFlavor
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
                                                                            "LaunchConditions",
                                                                            "ListItem",
                                                                            "Log",
                                                                            "MajorUpgrade",
                                                                            "MediaTemplate",
                                                                            "MergeRef",
                                                                            "MigrateFeatureStates",
                                                                            "MIME",
                                                                            "MoveFiles",
                                                                            "MsiProperty",
                                                                            "MsiPublishAssemblies",
                                                                            "MsiUnpublishAssemblies",
                                                                            "MultiStringValue",
                                                                            "ODBCTranslator",
                                                                            "OptimizeCustomActions",
                                                                            "OptionalUpdateRegistration",
                                                                            "Package",
                                                                            "PackageGroupRef",
                                                                            "PatchFamilyGroupRef",
                                                                            "PatchFamilyRef",
                                                                            "PatchFiles",
                                                                            "PatchInformation",
                                                                            "PatchProperty",
                                                                            "PatchSequence",
                                                                            "Payload",
                                                                            "PayloadGroupRef",
                                                                            "Permission",
                                                                            "ProcessComponents",
                                                                            "ProductSearch",
                                                                            "ProgressText",
                                                                            "PropertyRef",
                                                                            "ProtectRange",
                                                                            "Publish",
                                                                            "PublishComponents",
                                                                            "PublishFeatures",
                                                                            "PublishProduct",
                                                                            "RadioButton",
                                                                            "RegisterClassInfo",
                                                                            "RegisterComPlus",
                                                                            "RegisterExtensionInfo",
                                                                            "RegisterFonts",
                                                                            "RegisterMIMEInfo",
                                                                            "RegisterProduct",
                                                                            "RegisterProgIdInfo",
                                                                            "RegisterTypeLibraries",
                                                                            "RegisterUser",
                                                                            "RegistrySearchRef",
                                                                            "RelatedBundle",
                                                                            "RemotePayload",
                                                                            "RemoveDuplicateFiles",
                                                                            "RemoveEnvironmentStrings",
                                                                            "RemoveExistingProducts",
                                                                            "RemoveFile",
                                                                            "RemoveFiles",
                                                                            "RemoveFolder",
                                                                            "RemoveFolders",
                                                                            "RemoveIniValues",
                                                                            "RemoveODBC",
                                                                            "RemoveRegistryKey",
                                                                            "RemoveRegistryValue",
                                                                            "RemoveRegistryValues",
                                                                            "RemoveShortcuts",
                                                                            "ReplacePatch",
                                                                            "RequiredPrivilege",
                                                                            "ReserveCost",
                                                                            "ResolveSource",
                                                                            "RMCCPSearch",
                                                                            "ScheduleReboot",
                                                                            "SelfRegModules",
                                                                            "SelfUnregModules",
                                                                            "ServiceArgument",
                                                                            "ServiceDependency",
                                                                            "SetDirectory",
                                                                            "SetODBCFolders",
                                                                            "SetProperty",
                                                                            "SFPFile",
                                                                            "ShortcutProperty",
                                                                            "Show",
                                                                            "SlipstreamMsp",
                                                                            "StartServices",
                                                                            "StopServices",
                                                                            "Subscribe",
                                                                            "Substitution",
                                                                            "SymbolPath",
                                                                            "TargetProductCode",
                                                                            "Text",
                                                                            "TextStyle",
                                                                            "UIRef",
                                                                            "UIText",
                                                                            "UnpublishComponents",
                                                                            "UnpublishFeatures",
                                                                            "UnregisterClassInfo",
                                                                            "UnregisterComPlus",
                                                                            "UnregisterExtensionInfo",
                                                                            "UnregisterFonts",
                                                                            "UnregisterMIMEInfo",
                                                                            "UnregisterProgIdInfo",
                                                                            "UnregisterTypeLibraries",
                                                                            "Update",
                                                                            "UpgradeVersion",
                                                                            "Validate",
                                                                            "ValidateProductID",
                                                                            "Variable",
                                                                            "Verb",
                                                                            "WixVariable",
                                                                            "WriteEnvironmentStrings",
                                                                            "WriteIniValues",
                                                                            "WriteRegistryValues",
                                                                        };

        public override bool ParseAttributesEnabled => false;

        public override string GetName(XmlTextReader reader)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                var name = reader.Name;
                var identifier = reader.GetAttribute("Name") ?? reader.GetAttribute("Key") ?? reader.GetAttribute("Id") ?? reader.GetAttribute("Action");
                return identifier is null ? name : $"{name} '{identifier}'";
            }

            return base.GetName(reader);
        }

        public override string GetType(XmlTextReader reader) => reader.NodeType == XmlNodeType.Element ? reader.Name : base.GetType(reader);

        public override ContainerOrTerminalNode FinalAdjustAfterParsingComplete(ContainerOrTerminalNode node)
        {
            if (node.Name == "define" && node.Type == NodeType.ProcessingInstruction)
            {
                if (node.Content?.Contains("=") == true)
                {
                    var identifier = node.Content?.Split('=')[0].Trim();
                    node.Name += $" '{identifier}'";
                }
            }

            return base.FinalAdjustAfterParsingComplete(node);
        }

        protected override bool ShallBeTerminalNode(ContainerOrTerminalNode node) => TerminalNodeNames.Contains(node?.Type);
    }
}