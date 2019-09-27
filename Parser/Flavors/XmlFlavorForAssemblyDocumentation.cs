using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public sealed class XmlFlavorForAssemblyDocumentation : XmlFlavor
    {
        private const string Assembly = "assembly";
        private const string Overloads = "overloads";
        private const string Summary = "summary";
        private const string Remarks = "remarks";
        private const string Param = "param";
        private const string Returns = "returns";
        private const string Exception = "exception";
        private const string SeeAlso = "seealso";
        private const string Example = "example";
        private const string Exclude = "exclude";

        private static readonly HashSet<string> TerminalNodeNames = new HashSet<string>
                                                                        {
                                                                            Overloads,
                                                                            Summary,
                                                                            Remarks,
                                                                            Param,
                                                                            Returns,
                                                                            Exception,
                                                                            SeeAlso,
                                                                            Example,
                                                                            Exclude,
                                                                        };

        private static readonly Dictionary<string, string> NameMap = TerminalNodeNames.ToDictionary(_ => _, __ => string.Concat("<", __, ">"));

        public override bool ParseAttributesEnabled => false;

        public override bool Supports(DocumentInfo info) => string.Equals(info.RootElement, "doc", StringComparison.OrdinalIgnoreCase);

        public override string GetName(XmlReader reader) => reader.NodeType == XmlNodeType.Element ? GetElementName(reader, reader.LocalName) : base.GetName(reader);

        public override string GetType(XmlReader reader) => reader.NodeType == XmlNodeType.Element ? reader.LocalName : base.GetType(reader);

        public override ContainerOrTerminalNode FinalAdjustAfterParsingComplete(ContainerOrTerminalNode node)
        {
            if (node is Container c && c.Type == Assembly)
            {
                var name = c.Children.FirstOrDefault(_ => _.Name == "name");
                if (name != null)
                {
                    c.Children.RemoveAll(_ => _.Name == "name");
                    c.Name = name.Content;
                }
            }

            return base.FinalAdjustAfterParsingComplete(node);
        }

        protected override bool ShallBeTerminalNode(ContainerOrTerminalNode node) => TerminalNodeNames.Contains(node?.Type);

        private static string GetElementName(XmlReader reader, string name) => (reader.GetAttribute("name") ?? reader.GetAttribute("cref")) ?? GetElementNameMapped(name);

        private static string GetElementNameMapped(string name) => NameMap.TryGetValue(name, out var mappedName) ? mappedName : name;
    }
}