using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace MiKoSolutions.SemanticParsers.Xml.Yaml
{
    public sealed class TerminalNode : ContainerOrTerminalNode
    {
        [YamlMember(Alias = "span", Order = 4)]
        public CharacterSpan Span { get; set; }

        public override CharacterSpan GetTotalSpan() => Span;

        public override TerminalNode ToTerminalNode() => this;

        /// <summary>
        /// Gets the children the node had before it was converted from a <see cref="Container"/> to a <see cref="TerminalNode"/>.
        /// </summary>
        [YamlIgnore]
        public List<ContainerOrTerminalNode> Children { get; } = new List<ContainerOrTerminalNode>();
    }
}