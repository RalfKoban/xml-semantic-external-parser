using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public sealed class XmlFlavorForMSBuild : XmlFlavor
    {
        private static readonly HashSet<string> NonTerminalNodeNames = new HashSet<string>
                                                                        {
                                                                            "ItemGroup",
                                                                            "ItemDefinitionGroup",
                                                                            "ImportGroup",
                                                                            "Project",
                                                                            "ProjectConfiguration",
                                                                            "PropertyGroup",
                                                                            "Target",
                                                                        };

        public override bool ParseAttributesEnabled => false;

        public override bool Supports(string filePath) => filePath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase)
                                                       || filePath.EndsWith(".vcxproj", StringComparison.OrdinalIgnoreCase)
                                                       || filePath.EndsWith(".vbproj", StringComparison.OrdinalIgnoreCase)
                                                       || filePath.EndsWith(".proj", StringComparison.OrdinalIgnoreCase)
                                                       || filePath.EndsWith(".modelproj", StringComparison.OrdinalIgnoreCase)
                                                       || filePath.EndsWith(".shproj", StringComparison.OrdinalIgnoreCase)
                                                       || filePath.EndsWith(".sqlproj", StringComparison.OrdinalIgnoreCase)
                                                       || filePath.EndsWith(".targets", StringComparison.OrdinalIgnoreCase);

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
                    case "Resource":
                    case "Validate":
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

        public override ContainerOrTerminalNode FinalAdjustAfterParsingComplete(ContainerOrTerminalNode node)
        {
            if (node is Container c)
            {
                var content = c.Children.FirstOrDefault(_ => _.Type == NodeType.Text)?.Content;
                if (content != null)
                {
                    c.Name = content;
                }
            }

            return base.FinalAdjustAfterParsingComplete(node);
        }

        protected override bool ShallBeTerminalNode(ContainerOrTerminalNode node) => !NonTerminalNodeNames.Contains(node?.Type);

        private string GetName(XmlTextReader reader, string name, string attributeName)
        {
            var attribute = reader.GetAttribute(attributeName);
            if (attribute != null)
            {
                // just add 1 and we get rid of situation that index might not be available ;)
                var commaIndex = attribute.IndexOf(',');
                if (commaIndex > 0)
                {
                    attribute = attribute.Substring(0, commaIndex);
                }

                // just add 1 and we get rid of situation that index might not be available ;)
                return attribute.Substring(attribute.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase) + 1);
            }

            return name;
        }
    }
}