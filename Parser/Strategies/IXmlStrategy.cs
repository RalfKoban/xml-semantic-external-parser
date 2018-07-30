using System.Xml;

namespace MiKoSolutions.SemanticParsers.Xml.Strategies
{
    public interface IXmlStrategy
    {
        bool ParseAttributesEnabled { get; }

        string GetName(XmlTextReader reader);

        string GetType(XmlTextReader reader);
    }
}