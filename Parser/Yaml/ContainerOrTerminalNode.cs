using System.Diagnostics;

using YamlDotNet.Serialization;

namespace MiKoSolutions.SemanticParsers.Xml.Yaml
{
    [DebuggerDisplay("Type={Type}, Name={Name}")]
    public abstract class ContainerOrTerminalNode
    {
        [YamlMember(Alias = "type", Order = 1)]
        public string Type { get; set; }

        [YamlMember(Alias = "name", Order = 2)]
        public string Name { get; set; }

        [YamlMember(Alias = "locationSpan", Order = 3)]
        public LocationSpan LocationSpan { get; set; }

        [YamlIgnore]
        public string Content { get; set; }

        public abstract CharacterSpan GetTotalSpan();

        public abstract TerminalNode ToTerminalNode();
    }
}