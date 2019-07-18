using System.Collections.Generic;

using YamlDotNet.Serialization;

namespace MiKoSolutions.SemanticParsers.Xml.Yaml
{
    public sealed class Container : ContainerOrTerminalNode
    {
        [YamlMember(Alias = "headerSpan", Order = 4)]
        public CharacterSpan HeaderSpan { get; set; }

        [YamlMember(Alias = "footerSpan", Order = 5)]
        public CharacterSpan FooterSpan { get; set; }

        [YamlMember(Alias = "children", Order = 6)]
        public List<ContainerOrTerminalNode> Children { get; } = new List<ContainerOrTerminalNode>();

        public override CharacterSpan GetTotalSpan() => new CharacterSpan(HeaderSpan.Start, FooterSpan.End);

        public override TerminalNode ToTerminalNode()
        {
            var terminalNode = new TerminalNode
                                   {
                                       Type = Type,
                                       Name = Name,
                                       Content = Content,
                                       LocationSpan = LocationSpan,
                                       Span = GetTotalSpan(),
                                   };
            terminalNode.Children.AddRange(Children);

            return terminalNode;
        }
    }
}