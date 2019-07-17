using System;
using System.IO;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public class XmlFlavor : IXmlFlavor
    {
        private static readonly char[] DirectorySeparators = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };

        public virtual bool ParseAttributesEnabled => false;

        public virtual bool Supports(string filePath) => filePath.EndsWith(".xml", StringComparison.OrdinalIgnoreCase);

        public virtual bool Supports(DocumentInfo info) => true;

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
                {
                    return reader.Value;
                }

                case XmlNodeType.CDATA:
                case XmlNodeType.Text:
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

        public virtual string GetContent(XmlTextReader reader) => reader.Value;

        public virtual ContainerOrTerminalNode FinalAdjustAfterParsingComplete(ContainerOrTerminalNode node)
        {
            return ShallBeTerminalNode(node)
                ? node.ToTerminalNode()
                : node;
        }

        protected static string GetFileName(string path)
        {
            // get rid of backslash or slash as we only are interested in the name, not the path
            // (and just add 1 and we get rid of situation that index might not be available ;))
            return path?.Substring(path.LastIndexOfAny(DirectorySeparators) + 1);
        }

        protected virtual bool ShallBeTerminalNode(ContainerOrTerminalNode node) => false;
    }
}