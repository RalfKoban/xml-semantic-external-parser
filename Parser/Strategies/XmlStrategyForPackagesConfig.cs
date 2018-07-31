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

                    return $"{reader.Name} '{value}'";
                }

                case XmlNodeType.Attribute:
                {
                    var readerName = reader.Name;
                    var value = reader.Value;

                    return $"{readerName} '{value}'";
                }

                default:
                {
                    return base.GetName(reader);
                }
            }
        }

        public override string GetType(XmlTextReader reader) => reader.NodeType == XmlNodeType.Element ? reader.Name : base.GetType(reader);
    }
}