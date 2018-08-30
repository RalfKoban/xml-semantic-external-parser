using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public sealed class XmlFlavorForWix : XmlFlavor
    {
        private const string Fragment = "Fragment";
        private const string ComponentGroup = "ComponentGroup";
        private const string Component = "Component";
        private const string File = "File";
        private const string Shortcut = "Shortcut";
        private const string Custom = "Custom";
        private const string CustomAction = "CustomAction";
        private const string SetProperty = "SetProperty";
        private const string Property = "Property";
        private const string CreateFolder = "CreateFolder";
        private const string RegistryKey = "RegistryKey";
        private const string RegistryValue = "RegistryValue";

        private const string Util_RemoveFolderEx = "RemoveFolderEx";

        private static readonly char[] DirectorySeparators = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };

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
                                                                            CreateFolder, // This is not a real terminal node but we consider it to be one as it's about a single folder
                                                                            "CreateFolders",
                                                                            "CreateShortcuts",
                                                                            Custom,
                                                                            CustomAction,
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
                                                                            File, // This is not a real terminal node but we consider it to be one as it's about a single file
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
                                                                            RegistryKey, // This is not a real terminal node but we consider it to be one as it's about a single entry in the registry
                                                                            "RegistrySearch", // This is not a real terminal node but we consider it to be one as it's about a single search in the registry
                                                                            "RegistrySearchRef",
                                                                            RegistryValue, // This is not a real terminal node but we consider it to be one as it's about a single entry in the registry
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
                                                                            SetProperty,
                                                                            "SFPFile",
                                                                            Shortcut, // This is not a real terminal node but we consider it to be one as it's about a single shortcut
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

        private static readonly HashSet<string> UtilTerminalNodeNames = new HashSet<string>
                                                                        {
                                                                            "CloseApplication",
                                                                            "ComponentSearch",
                                                                            "ComponentSearchRef",
                                                                            "DirectorySearch",
                                                                            "DirectorySearchRef",
                                                                            "EventManifest",
                                                                            "EventSource",
                                                                            "FileSearch",
                                                                            "FileSearchRef",
                                                                            "FileSharePermission",
                                                                            "Group",
                                                                            "GroupRef",
                                                                            "InternetShortcut",
                                                                            "PerfCounter",
                                                                            "PerfCounterManifest",
                                                                            "PerformanceCounter",
                                                                            "PermissionEx",
                                                                            "ProductSearch",
                                                                            "ProductSearchRef",
                                                                            "RegistrySearch",
                                                                            "RegistrySearchRef",
                                                                            Util_RemoveFolderEx,
                                                                            "RestartResource",
                                                                            "ServiceConfig",
                                                                            "XmlFile",
                                                                        };

        public override bool ParseAttributesEnabled => false;

        public override bool Supports(string filePath) => filePath.EndsWith(".wxs", StringComparison.OrdinalIgnoreCase);

        public override bool Supports(DocumentInfo info) => string.Equals(info.RootElement, "Wix", StringComparison.OrdinalIgnoreCase);

        public override string GetName(XmlTextReader reader)
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                {
                    var name = reader.LocalName;
                    switch (name)
                    {
                        case File:
                        {
                            // try to get source, then get ID
                            var source = reader.GetAttribute("Source");
                            return source != null ? GetFileName(source) : reader.GetAttribute("Id");
                        }

                        case Custom:
                            return reader.GetAttribute("Action");

                        case CustomAction:
                        case Property:
                        case SetProperty:
                        case Shortcut:
                            return reader.GetAttribute("Id");

                        case Util_RemoveFolderEx:
                            return reader.GetAttribute("Id") ?? reader.GetAttribute("Property");

                        case CreateFolder:
                            return reader.GetAttribute("Directory") ?? name;

                        case RegistryKey:
                        case RegistryValue:
                        {
                            var root = reader.GetAttribute("Root");
                            var key = reader.GetAttribute("Key");
                            var value = reader.GetAttribute("Name");

                            var regKey = string.Concat("[", string.Join("\\", root, key), "]");
                            return value is null
                                    ? regKey
                                    : string.Join(Environment.NewLine, regKey, string.Concat("\"", value, "\""));
                        }
                    }

                    var identifier = reader.GetAttribute("Name") ?? reader.GetAttribute("Key") ?? reader.GetAttribute("Id");
                    return identifier ?? name;
                }

                case XmlNodeType.ProcessingInstruction:
                {
                    // TODO: RKN fix duplicated code (WIX configuration)
                    var name = reader.LocalName;
                    var value = reader.Value;
                    var index = value.IndexOf('=');
                    var identifier = index > 0 ? value.Substring(0, index) : value;

                    return $"{name} '{identifier.Trim()}'";
                }

                default:
                    return base.GetName(reader);
            }
        }

        public override string GetType(XmlTextReader reader) => reader.NodeType == XmlNodeType.Element ? reader.LocalName : base.GetType(reader);

        public override ContainerOrTerminalNode FinalAdjustAfterParsingComplete(ContainerOrTerminalNode node)
        {
            if (node is Container c)
            {
                switch (node.Type)
                {
                    case Component:
                    {
                        var child = c.Children.FirstOrDefault(_ => _.Type == File) ?? c.Children.FirstOrDefault(_ => _.Type == Shortcut);
                        if (child != null)
                        {
                            c.Name = child.Name;
                        }

                        break;
                    }

                    case Fragment:
                    {
                        var child = c.Children.FirstOrDefault(_ => _.Type == ComponentGroup) ?? c.Children.FirstOrDefault(_ => _.Type == Component);
                        if (child != null)
                        {
                            c.Name = Fragment + $" '{child.Name}'";
                        }

                        break;
                    }
                }
            }

            return base.FinalAdjustAfterParsingComplete(node);
        }

        protected override bool ShallBeTerminalNode(ContainerOrTerminalNode node)
        {
            var nodeType = node?.Type;
            if (nodeType is null)
            {
                return false;
            }

            return TerminalNodeNames.Contains(nodeType) || UtilTerminalNodeNames.Contains(nodeType);
        }

        private static string GetFileName(string result)
        {
            // TODO: RKN fix duplicated code
            // get rid of backslash or slash as we only are interested in the name, not the path
            // (and just add 1 and we get rid of situation that index might not be available ;))
            return result.Substring(result.LastIndexOfAny(DirectorySeparators) + 1);
        }
    }
}