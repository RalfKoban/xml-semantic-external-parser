using System;
using System.Xml;

namespace MiKoSolutions.SemanticParsers.Xml.Strategies
{
    public sealed class XmlStrategyForPackagesConfig : XmlStrategy
    {
        public override string GetName(XmlTextReader reader)
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    return reader.GetAttribute("id");

                case XmlNodeType.Attribute:
                    var readerName = reader.Name;
                    var readerValue = reader.Value;

                    return $"{readerName}=\"{readerValue.Substring(0, Math.Min(readerValue.Length, 20))}\"";

                default:
                    return base.GetName(reader);
            }
        }

        public override string GetType(XmlTextReader reader) => reader.NodeType == XmlNodeType.Element ? reader.Name : base.GetType(reader);
    }
}