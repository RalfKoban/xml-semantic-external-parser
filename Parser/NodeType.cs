using System.Xml;

namespace MiKoSolutions.SemanticParsers.Xml
{
    public static class NodeType
    {
        public const string Attribute = nameof(XmlNodeType.Attribute);
        public const string Element = nameof(XmlNodeType.Element);
        public const string CDATA = nameof(XmlNodeType.CDATA);
        public const string XmlDeclaration = nameof(XmlNodeType.XmlDeclaration);
        public const string Comment = nameof(XmlNodeType.Comment);
        public const string ProcessingInstruction = nameof(XmlNodeType.ProcessingInstruction);
        public const string Text = nameof(XmlNodeType.Text);
    }
}