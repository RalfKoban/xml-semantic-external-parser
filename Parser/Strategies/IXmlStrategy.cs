using System.Xml;

namespace MiKoSolutions.SemanticParsers.Xml.Strategies
{
    public interface IXmlStrategy
    {
        string GetName(XmlTextReader reader);

        string GetType(XmlTextReader reader);
    }
}