using System.Xml;

namespace MiKoSolutions.SemanticParsers.Xml.Strategies
{
    public interface ISpecialXmlStrategy
    {
        string GetName(XmlTextReader reader);

        string GetType(XmlTextReader reader);
    }
}