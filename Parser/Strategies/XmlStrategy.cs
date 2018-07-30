using System.Runtime.Remoting.Messaging;
using System.Xml;

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
                case XmlNodeType.Element: return NodeType.Element;
                case XmlNodeType.ProcessingInstruction: return NodeType.ProcessingInstruction;
                case XmlNodeType.Comment: return NodeType.Comment;
                case XmlNodeType.XmlDeclaration: return NodeType.XmlDeclaration;
                case XmlNodeType.CDATA: return NodeType.CDATA;
                default: return string.Empty;
            }
        }
    }
}