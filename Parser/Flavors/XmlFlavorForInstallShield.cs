using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public sealed class XmlFlavorForInstallShield : XmlFlavor
    {
        private const string COL = "col";
        private const string ROW = "row";
        private const string TABLE = "table";
        private const string TD = "td";

        private static readonly HashSet<string> TerminalNodeNames = new HashSet<string>
                                                                        {
                                                                            COL,
                                                                            TD,
                                                                            "codepage",
                                                                            "title",
                                                                            "subject",
                                                                            "author",
                                                                            "keywords",
                                                                            "comments",
                                                                            "template",
                                                                            "lastauthor",
                                                                            "revnumber",
                                                                            "lastprinted",
                                                                            "createdtm",
                                                                            "lastsavedtm",
                                                                            "pagecount",
                                                                            "wordcount",
                                                                            "charcount",
                                                                            "appname",
                                                                            "security",
                                                                        };

        public override bool ParseAttributesEnabled => false;

        public override bool Supports(string filePath) => filePath.EndsWith(".ism", StringComparison.OrdinalIgnoreCase);

        public override bool Supports(DocumentInfo info) => string.Equals(info.RootElement, "msi", StringComparison.OrdinalIgnoreCase);

        public override string GetName(XmlTextReader reader)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                var name = reader.Name;
                return name == TABLE ? reader.GetAttribute("name") : name;
            }

            return base.GetName(reader);
        }

        public override string GetType(XmlTextReader reader) => reader.NodeType == XmlNodeType.Element ? reader.Name : base.GetType(reader);

        public override ContainerOrTerminalNode FinalAdjustAfterParsingComplete(ContainerOrTerminalNode node)
        {
            if (node is Container c)
            {
                switch (c.Type)
                {
                    case COL:
                    case TD:
                    {
                        node.Name = c.Children.FirstOrDefault(_ => _.Type == NodeType.Text)?.Content ?? string.Empty;
                        break;
                    }

                    case TABLE:
                    {
                        // rename all TDs in the rows
                        var i = 0;
                        var columnNames = c.Children.Where(_ => _.Type == COL).ToDictionary(_ => i++, _ => _.Name);

                        var currentRow = 0;
                        foreach (var row in c.Children.Where(_ => _.Type == ROW).OfType<Container>())
                        {
                            row.Name = $"{ROW}_{currentRow++}";

                            var columns = row.Children.Where(_ => _.Type == TD).ToList();
                            for (var j = 0; j < columns.Count; j++)
                            {
                                columns[j].Type = $"{COL}_{columnNames[j]}_Value";
                            }
                        }

                        break;
                    }
                }
            }

            return base.FinalAdjustAfterParsingComplete(node);
        }

        protected override bool ShallBeTerminalNode(ContainerOrTerminalNode node) => TerminalNodeNames.Contains(node?.Type);
    }
}