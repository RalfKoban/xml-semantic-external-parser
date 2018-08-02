using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Strategies
{
    public class XmlStrategy : IXmlStrategy
    {
        public virtual bool ParseAttributesEnabled => true;

        public virtual string GetName(XmlTextReader reader)
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                case XmlNodeType.ProcessingInstruction:
                case XmlNodeType.Attribute:
                {
                    return reader.Name;
                }

                case XmlNodeType.Comment:
                case XmlNodeType.XmlDeclaration:
                case XmlNodeType.CDATA:
                {
                    return reader.Value;
                }

                default:
                {
                    return string.Empty;
                }
            }
        }

        public virtual string GetType(XmlTextReader reader)
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Attribute: return NodeType.Attribute;
                case XmlNodeType.CDATA: return NodeType.CDATA;
                case XmlNodeType.Comment: return NodeType.Comment;
                case XmlNodeType.Element: return NodeType.Element;
                case XmlNodeType.ProcessingInstruction: return NodeType.ProcessingInstruction;
                case XmlNodeType.Text: return NodeType.Text;
                case XmlNodeType.XmlDeclaration: return NodeType.XmlDeclaration;

                default: return string.Empty;
            }
        }

        public virtual bool ShallBeTerminalNode(Container container) => false;
    }
}