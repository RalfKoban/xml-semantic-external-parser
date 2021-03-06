﻿using System;
using System.Collections.Generic;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public sealed class XmlFlavorForSettings : XmlFlavor
    {
        private static readonly HashSet<string> TerminalNodeNames = new HashSet<string>
                                                                        {
                                                                            "Setting",
                                                                            "Value",
                                                                        };

        public override bool ParseAttributesEnabled => false;

        public override bool Supports(string filePath) => filePath.EndsWith(".settings", StringComparison.OrdinalIgnoreCase);

        public override bool Supports(DocumentInfo info) => string.Equals(info.RootElement, "SettingsFile", StringComparison.OrdinalIgnoreCase)
                                                            && string.Equals(info.Namespace, "http://schemas.microsoft.com/VisualStudio/2004/01/settings", StringComparison.OrdinalIgnoreCase);

        public override string GetName(XmlReader reader)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                var name = reader.LocalName;
                var identifier = reader.GetAttribute("Name");
                return identifier ?? name;
            }

            return base.GetName(reader);
        }

        public override string GetType(XmlReader reader) => reader.NodeType == XmlNodeType.Element ? reader.LocalName : base.GetType(reader);

        protected override bool ShallBeTerminalNode(ContainerOrTerminalNode node) => TerminalNodeNames.Contains(node?.Type);
    }
}