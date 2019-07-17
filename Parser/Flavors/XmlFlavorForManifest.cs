using System;
using System.Collections.Generic;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public sealed class XmlFlavorForManifest : XmlFlavor
    {
        private static readonly HashSet<string> NonTerminalNodeNames = new HashSet<string>
                                                                           {
                                                                               ElementNames.Assembly,
                                                                               ElementNames.AssemblyIdentity,
                                                                               ElementNames.Dependency,
                                                                               ElementNames.DependentAssembly,
                                                                               ElementNames.File,
                                                                           };

        public override bool ParseAttributesEnabled => false;

        public override bool Supports(string filePath) => filePath.EndsWith(".manifest", StringComparison.OrdinalIgnoreCase);

        public override bool Supports(DocumentInfo info)
        {
            if (info.RootElement == ElementNames.Assembly)
            {
                if (info.Namespace is null)
                {
                    return true;
                }

                return info.Namespace.EndsWith("urn:schemas-microsoft-com:asm.v1", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        public override string GetName(XmlTextReader reader)
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

        public override string GetType(XmlTextReader reader) => reader.NodeType == XmlNodeType.Element ? reader.Name : base.GetType(reader);

        protected override bool ShallBeTerminalNode(ContainerOrTerminalNode node) => !NonTerminalNodeNames.Contains(node?.Type);

        private static string GetAttributeName(string name)
        {
            switch (name)
            {
                case ElementNames.AssemblyIdentity:
                case ElementNames.File:
                    return AttributeNames.Name;

                case ElementNames.ComClass:
                    return AttributeNames.Clsid;

                default:
                    return AttributeNames.Name;
            }
        }

        private static class ElementNames
        {
            internal const string Assembly = "assembly";
            internal const string AssemblyIdentity = "assemblyIdentity";
            internal const string ComClass = "comClass";
            internal const string Dependency = "dependency";
            internal const string DependentAssembly = "dependentAssembly";

            internal const string File = "file";
        }

        private static class AttributeNames
        {
            internal const string Clsid = "clsid";
            internal const string Name = "name";
        }
    }
}