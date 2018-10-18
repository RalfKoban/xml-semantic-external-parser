using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    public sealed class XmlFlavorForEnterpriseArchitectExport : XmlFlavor
    {
        private static readonly HashSet<string> TerminalNodeNames = new HashSet<string>
                                                                        {
                                                                            Attribute,
                                                                            Class,
                                                                            Interface,

                                                                            Association,
                                                                            Classifier,
                                                                            ClassifierRole,
                                                                            Comment,
                                                                            Dependency,
                                                                            Diagram,
                                                                            DiagramElement,
                                                                            Expression,
                                                                            Parameter,
                                                                            Stereotype,
                                                                            TaggedValue,

                                                                            EAStub,
                                                                        };

        private static readonly Dictionary<string, string> SubstitionNodeNames = new Dictionary<string, string>
                                                                                     {
                                                                                         { Association_Connection,  "Connection" },
                                                                                         { Collaboration_Interaction, "Interactions" },
                                                                                         { ModelElement_TaggedValue, "Tagged Values" },
                                                                                         { ModelElement_Stereotype, "Stereotypes" },
                                                                                         { Namespace_OwnedElement, "Types" },
                                                                                         { Comment, Comment },

                                                                                     };

        public override bool ParseAttributesEnabled => false;

        public override bool Supports(DocumentInfo info) => string.Equals(info.RootElement, XMI, StringComparison.OrdinalIgnoreCase);

        public override string GetName(XmlTextReader reader)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                var name = reader.LocalName;

                if (SubstitionNodeNames.TryGetValue(name, out var replacement))
                {
                    return replacement;
                }

                switch (name)
                {
                    case TaggedValue:
                    {
                        var identifier = reader.GetAttribute("tag");
                        return identifier ?? name;
                    }

                    case XMI_extensions:
                    {
                        var identifier = reader.GetAttribute("xmi.extender");
                        return identifier ?? name;
                    }

                    default:
                    {
                        var identifier = reader.GetAttribute("name");
                        return identifier ?? name;
                    }
                }
            }

            return base.GetName(reader);
        }

        public override string GetType(XmlTextReader reader) => reader.NodeType == XmlNodeType.Element ? reader.LocalName : base.GetType(reader);

        public override string GetContent(XmlTextReader reader) => reader.NodeType == XmlNodeType.Element && reader.LocalName == TaggedValue ? reader.GetAttribute("value") : base.GetContent(reader);

        public override ContainerOrTerminalNode FinalAdjustAfterParsingComplete(ContainerOrTerminalNode node)
        {
            if (node is Container c)
            {
                switch (c.Type)
                {
                    case ModelElement_TaggedValue:
                        return c.ToTerminalNode();

                    case AssociationEnd:
                    {
                        var associationType = c.Children.OfType<Container>().FirstOrDefault(_ => _.Type == ModelElement_TaggedValue)?.Children.FirstOrDefault(_ => _.Type == TaggedValue && _.Name == "ea_end");
                        if (associationType != null)
                        {
                            c.Name = associationType.Content;
                        }

                        break;
                    }
                }
            }

            return base.FinalAdjustAfterParsingComplete(node);
        }

        protected override bool ShallBeTerminalNode(ContainerOrTerminalNode node) => TerminalNodeNames.Contains(node?.Type);

        private const string Association = "Association";
        private const string AssociationEnd = "AssociationEnd";
        private const string Attribute = "Attribute";
        private const string Class = "Class";
        private const string Classifier = "Classifier";
        private const string ClassifierRole = "ClassifierRole";
        private const string Comment = "Comment";
        private const string Dependency = "Dependency";
        private const string Diagram = "Diagram";
        private const string DiagramElement = "DiagramElement";
        private const string Expression = "Expression";
        private const string Interface = "Interface";
        private const string Parameter = "Parameter";
        private const string Stereotype = "Stereotype";
        private const string TaggedValue = "TaggedValue";

        private const string Association_Connection = "Association.connection";
        private const string Collaboration_Interaction = "Collaboration.interaction";
        private const string ModelElement_Stereotype = "ModelElement.stereotype";
        private const string ModelElement_TaggedValue = "ModelElement.taggedValue";
        private const string Namespace_OwnedElement = "Namespace.ownedElement";

        private const string EAStub = "EAStub";

        private const string XMI = "XMI";
        private const string XMI_extensions = "extensions";
    }
}