using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public interface IXmlFlavor
    {
        bool ParseAttributesEnabled { get; }

        string GetName(XmlReader reader);

        string GetType(XmlReader reader);

        string GetContent(XmlReader reader);

        ContainerOrTerminalNode FinalAdjustAfterParsingComplete(ContainerOrTerminalNode node);
    }
}