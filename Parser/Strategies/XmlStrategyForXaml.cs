﻿using System.Xml;

namespace MiKoSolutions.SemanticParsers.Xml.Strategies
{
    public sealed class XmlStrategyForXaml : XmlStrategy
    {
        public override bool ParseAttributesEnabled => false;

        public override string GetName(XmlTextReader reader)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                var name = reader.Name;

                return reader.GetAttribute("Name", "http://schemas.microsoft.com/winfx/2006/xaml") ??
                       reader.GetAttribute("Key", "http://schemas.microsoft.com/winfx/2006/xaml") ??
                       name;
            }

            return base.GetName(reader);
        }

        public override string GetType(XmlTextReader reader) => reader.NodeType == XmlNodeType.Element ? reader.Name : base.GetType(reader);
    }
}