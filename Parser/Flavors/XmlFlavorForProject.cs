using System;
using System.Collections.Generic;
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

        public override bool ParseAttributesEnabled => false;

        public override bool Supports(string filePath) => filePath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase) || filePath.EndsWith(".vcxproj", StringComparison.OrdinalIgnoreCase);

        public override bool Supports(DocumentInfo info) => string.Equals(info.RootElement, "Project", StringComparison.OrdinalIgnoreCase)
                                                         && string.Equals(info.Namespace, "http://schemas.microsoft.com/developer/msbuild/2003", StringComparison.OrdinalIgnoreCase);

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