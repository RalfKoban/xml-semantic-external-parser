﻿using System;
using System.Collections.Generic;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public sealed class XmlFlavorForWixLocation : XmlFlavor
    {
        private static readonly HashSet<string> TerminalNodeNames = new HashSet<string>
                                                                        {
                                                                            "String",
                                                                            "UI",
                                                                        };

        public override bool ParseAttributesEnabled => false;

        public override bool Supports(string filePath) => filePath.EndsWith(".wxl", StringComparison.OrdinalIgnoreCase);

        public override bool Supports(DocumentInfo info) => string.Equals(info.RootElement, "WixLocalization", StringComparison.OrdinalIgnoreCase)
                                                         && string.Equals(info.Namespace, "http://schemas.microsoft.com/wix/2006/localization", StringComparison.OrdinalIgnoreCase);

        public override string GetName(XmlReader reader)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                var name = reader.LocalName;
                var identifier = reader.GetAttribute("Id") ?? reader.GetAttribute("Control") ?? reader.GetAttribute("Dialog");
                return identifier is null ? name : $"{name} '{identifier}'";
            }

            return base.GetName(reader);
        }

        public override string GetType(XmlReader reader) => reader.NodeType == XmlNodeType.Element ? reader.LocalName : base.GetType(reader);

        protected override bool ShallBeTerminalNode(ContainerOrTerminalNode node) => TerminalNodeNames.Contains(node?.Type);
    }
}