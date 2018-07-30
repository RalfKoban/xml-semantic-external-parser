using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Strategies
{
    public interface IXmlStrategy
    {
        bool ParseAttributesEnabled { get; }

        string GetName(XmlTextReader reader);

        string GetType(XmlTextReader reader);

        bool ShallBeTerminalNode(Container container);
    }
}