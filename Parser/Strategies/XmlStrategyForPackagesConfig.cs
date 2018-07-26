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
                {
                    var value = reader.GetAttribute("id");
                    if (value is null)
                    {
                        return reader.Name;
                    }

                    return $"{reader.Name} \"{TrimLength(value, 20)}\"";
                }

                case XmlNodeType.Attribute:
                {
                    var readerName = reader.Name;
                    var value = reader.Value;

                    return $"{readerName}=\"{TrimLength(value, 20)}\"";
                }

                default:
                {
                    return base.GetName(reader);
                }
            }
        }

        public override string GetType(XmlTextReader reader) => reader.NodeType == XmlNodeType.Element ? reader.Name : base.GetType(reader);

        private static string TrimLength(string value, int maxLength) => value.Substring(0, Math.Min(value.Length, maxLength));
    }
}