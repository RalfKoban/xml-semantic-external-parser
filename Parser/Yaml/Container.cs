using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace MiKoSolutions.SemanticParsers.Xml.Yaml
{
    public sealed class Container : ContainerOrTerminalNode
    {
        [YamlMember(Alias = "headerSpan")]
        public CharacterSpan HeaderSpan { get; set; }

        [YamlMember(Alias = "footerSpan")]
        public CharacterSpan FooterSpan { get; set; }

        [YamlMember(Alias = "children", Order = 42)]
        public List<ContainerOrTerminalNode> Children { get; } = new List<ContainerOrTerminalNode>();
    }
}