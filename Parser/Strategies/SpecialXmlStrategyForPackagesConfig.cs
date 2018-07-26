using System.Xml;

namespace MiKoSolutions.SemanticParsers.Xml.Strategies
{
    public sealed class SpecialXmlStrategyForPackagesConfig : SpecialXmlStrategy
    {
        public override string GetName(XmlTextReader reader) => reader.NodeType == XmlNodeType.Element ? reader.GetAttribute("id") : base.GetName(reader);

        public override string GetType(XmlTextReader reader) => reader.NodeType == XmlNodeType.Element ? reader.Name : base.GetType(reader);
    }
}