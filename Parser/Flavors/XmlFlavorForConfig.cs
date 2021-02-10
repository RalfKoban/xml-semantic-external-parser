using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public sealed class XmlFlavorForConfig : XmlFlavor
    {
        private const string SupportedRuntime = "supportedRuntime";
        private const string Section = "section";
        private const string Setting = "setting";

        private const string Add = "add";
        private const string Remove = "remove";
        private const string Clear = "clear";

        private const string Parameter = "parameter";
        private const string Probing = "probing";
        private const string Provider = "provider";

        private const string DependentAssembly = "dependentAssembly";
        private const string AssemblyIdentity = "assemblyIdentity";

        private const string Binding = "binding";
        private const string Endpoint = "endpoint";
        private const string Behavior = "behavior";

        // Log4net
        private const string Appender = "appender";
        private const string AppenderRef = "appender-ref";
        private const string Level = "level";
        private const string Logger = "logger";

        // folder
        private const string Folder = "folder";
        private const string Assembly = "assembly";

        private static readonly HashSet<string> TerminalNodeNames = new HashSet<string>
                                                                        {
                                                                            Add,
                                                                            Remove,
                                                                            Clear,
                                                                            Binding,
                                                                            Endpoint,
                                                                            Behavior,
                                                                            Parameter,
                                                                            Provider,
                                                                            Section,
                                                                            Setting,
                                                                            SupportedRuntime,
                                                                            DependentAssembly,
                                                                            Probing,
                                                                            Appender,
                                                                            AppenderRef,
                                                                            Level,
                                                                            Logger,
                                                                            Assembly,
                                                                        };

        public override bool ParseAttributesEnabled => false;

        public override bool Supports(string filePath) => filePath.EndsWith(".config", StringComparison.OrdinalIgnoreCase);

        public override bool Supports(DocumentInfo info) => string.Equals(info.RootElement, "configuration", StringComparison.OrdinalIgnoreCase);

        public override string GetName(XmlReader reader)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                var name = reader.Name;
                var identifier = GetIdentifier(reader, name);
                return identifier ?? name;
            }

            return base.GetName(reader);
        }

        public override string GetType(XmlReader reader) => reader.NodeType == XmlNodeType.Element ? reader.Name : base.GetType(reader);

        public override ContainerOrTerminalNode FinalAdjustAfterParsingComplete(ContainerOrTerminalNode node)
        {
            if (node is Container c && c.Type == DependentAssembly)
            {
                var identity = c.Children.FirstOrDefault(_ => _.Type == AssemblyIdentity);
                if (identity != null)
                {
                    c.Name = identity.Name;
                }
            }

            return base.FinalAdjustAfterParsingComplete(node);
        }

        protected override bool ShallBeTerminalNode(ContainerOrTerminalNode node) => TerminalNodeNames.Contains(node?.Type);

        private static string GetIdentifier(XmlReader reader, string name)
        {
            switch (name)
            {
                case Parameter:
                    return reader.GetAttribute("value");

                case Probing:
                    return reader.GetAttribute("privatePath");

                case Provider:
                    return reader.GetAttribute("name") ?? reader.GetAttribute("invariantName") ?? reader.GetAttribute("type");

                case Add:
                    return reader.GetAttribute("name") ?? reader.GetAttribute("key");

                case Remove:
                    return reader.GetAttribute("name") ?? reader.GetAttribute("invariant");

                case Clear:
                    return string.Empty;

                case SupportedRuntime:
                    var identifier = reader.GetAttribute("sku") ?? string.Empty;
                    var index = identifier.IndexOf(",", StringComparison.OrdinalIgnoreCase);

                    return index > 0
                        ? identifier.Substring(0, index)
                        : identifier;

                case Section:
                case Setting:
                    return reader.GetAttribute("name");

                case Behavior:
                case Binding:
                case Endpoint:
                    return reader.GetAttribute("name");

                case Appender:
                case Logger:
                    return reader.GetAttribute("name");

                case AppenderRef:
                    return reader.GetAttribute("ref");

                case Folder:
                    return reader.GetAttribute("relativepath") ?? reader.GetAttribute("name");

                default:
                    return reader.GetAttribute("name") ?? reader.GetAttribute("key");
            }
        }
    }
}