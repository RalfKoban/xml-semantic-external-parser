using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public sealed class XmlFlavorForNuSpec : XmlFlavor
    {
        private static readonly HashSet<string> NonTerminalNodeNames = new HashSet<string>
                                                                           {
                                                                               ElementNames.Package,
                                                                               ElementNames.Metadata,
                                                                               ElementNames.Id,
                                                                               ElementNames.Files,
                                                                               ElementNames.FrameworkAssemblies,
                                                                               ElementNames.Dependencies,
                                                                               ElementNames.Group,
                                                                               ElementNames.References,
                                                                           };

        public override bool ParseAttributesEnabled => false;

        public override bool Supports(string filePath) => filePath.EndsWith(".nuspec", StringComparison.OrdinalIgnoreCase);

        public override bool Supports(DocumentInfo info)
        {
            if (info.RootElement == ElementNames.Package)
            {
                if (info.Namespace is null)
                {
                    return true;
                }

                return info.Namespace.EndsWith("/nuspec.xsd", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        public override string GetName(XmlReader reader)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                var name = reader.Name;
                var attributeName = GetAttributeName(name);
                var identifier = reader.GetAttribute(attributeName);
                return identifier ?? name;
            }

            return base.GetName(reader);
        }

        public override string GetType(XmlReader reader) => reader.NodeType == XmlNodeType.Element ? reader.Name : base.GetType(reader);

        public override ContainerOrTerminalNode FinalAdjustAfterParsingComplete(ContainerOrTerminalNode node)
        {
            if (node is Container c)
            {
                switch (c.Type)
                {
                    case ElementNames.Id:
                        // side effect: set content here to be able to get it for MetaData as well
                        node.Content = c.Children.FirstOrDefault(_ => _.Type == NodeType.Text)?.Content.Trim();

                        // ID shall be a terminal node
                        return node.ToTerminalNode();

                    case ElementNames.Metadata:
                        node.Name = c.Children.FirstOrDefault(_ => _.Type == ElementNames.Id)?.Content.Trim();
                        break;
                }
            }

            return base.FinalAdjustAfterParsingComplete(node);
        }

        protected override bool ShallBeTerminalNode(ContainerOrTerminalNode node) => !NonTerminalNodeNames.Contains(node?.Type);

        private static string GetAttributeName(string name)
        {
            switch (name)
            {
                case ElementNames.Group:
                    return AttributeNames.TargetFramework;

                case ElementNames.File:
                    return AttributeNames.Src;

                case ElementNames.Repository:
                    return AttributeNames.Url;

                default:
                    return AttributeNames.Id;
            }
        }

        private static class ElementNames
        {
            internal const string Dependencies = "dependencies";
            internal const string Files = "files";
            internal const string File = "file";
            internal const string FrameworkAssemblies = "frameworkAssemblies";
            internal const string Id = "id";
            internal const string Metadata = "metadata";
            internal const string Package = "package";
            internal const string References = "references";
            internal const string Repository = "repository";
            internal const string Group = "group";
        }

        private static class AttributeNames
        {
            internal const string Id = "id";
            internal const string Src = "src";
            internal const string TargetFramework = "targetFramework";
            internal const string Url = "url";
        }
    }
}