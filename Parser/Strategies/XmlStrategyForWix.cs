using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Strategies
{
    public sealed class XmlStrategyForWix : XmlStrategy
    {
        public override bool ParseAttributesEnabled => false;

        public override string GetName(XmlTextReader reader) => reader.NodeType == XmlNodeType.Element ? reader.GetAttribute("Id") ?? reader.Name : base.GetName(reader);

        public override string GetType(XmlTextReader reader) => reader.NodeType == XmlNodeType.Element ? reader.Name : base.GetType(reader);

        public override bool ShallBeTerminalNode(Container container)
        {
            switch (container?.Type)
            {
                case "ApprovedExeForElevation":
                case "AllocateRegistrySpace":
                case "AppSearch":
                case "AssemblyName":
                case "BinaryRef":
                case "BindImage":
                case "Catalog":
                case "CCPSearch":
                case "Column":
                case "CommandLine":
                case "ComponentGroupRef":
                case "ComponentRef":
                case "Condition":
                case "Configuration":
                case "ConfigurationData":
                case "ContainerRef":
                case "CopyFile":
                case "CostFinalize":
                case "CostInitialize":
                case "CreateFolders":
                case "CreateShortcuts":
                case "Custom":
                case "DuplicateFiles":
                case "Icon":
                case "IconRef":
                case "MajorUpgrade":
                case "MergeRef":
                case "UIRef":
                case "Variable":
                    return true;

                default:
                    return false;
            }
        }
    }
}