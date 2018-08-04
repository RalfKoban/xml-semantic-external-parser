using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public interface IXmlFlavor
    {
        bool ParseAttributesEnabled { get; }

        string GetName(XmlTextReader reader);

        string GetType(XmlTextReader reader);

        bool ShallBeTerminalNode(Container container);

        void FinalAdjustAfterParsingComplete(ContainerOrTerminalNode node);
    }
}