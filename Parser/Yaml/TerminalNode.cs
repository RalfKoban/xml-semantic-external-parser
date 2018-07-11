using YamlDotNet.Serialization;

namespace MiKoSolutions.SemanticParsers.Xml.Yaml
{
    public sealed class TerminalNode : ContainerOrTerminalNode
    {
        [YamlMember(Alias = "span")]
        public CharacterSpan Span { get; set; }
    }
}