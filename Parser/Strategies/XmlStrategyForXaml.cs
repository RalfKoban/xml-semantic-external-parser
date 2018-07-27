using System.Xml;

namespace MiKoSolutions.SemanticParsers.Xml.Strategies
{
    public sealed class XmlStrategyForXaml : XmlStrategy
    {
        public override string GetName(XmlTextReader reader) => reader.NodeType == XmlNodeType.Element ? reader.GetAttribute("x:Name") ?? reader.Name : base.GetName(reader);

        public override string GetType(XmlTextReader reader) => reader.NodeType == XmlNodeType.Element ? reader.Name : base.GetType(reader);
    }
}