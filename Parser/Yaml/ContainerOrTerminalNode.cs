using System.Diagnostics;
using YamlDotNet.Serialization;

namespace MiKoSolutions.SemanticParsers.Xml.Yaml
{
    [DebuggerDisplay("Type={Type}, Name={Name}")]
    public abstract class ContainerOrTerminalNode
    {
        [YamlMember(Alias = "type")]
        public string Type { get; set; }

        [YamlMember(Alias = "name")]
        public string Name { get; set; }

        [YamlMember(Alias = "locationSpan")]
        public LocationSpan LocationSpan { get; set; }
    }
}