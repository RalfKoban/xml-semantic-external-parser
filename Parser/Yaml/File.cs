using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace MiKoSolutions.SemanticParsers.Xml.Yaml
{
    public sealed class File
    {
        [YamlMember(Alias = "type")]
        public string Type { get; } = "file";

        [YamlMember(Alias = "name")]
        public string Name { get; set; }

        [YamlMember(Alias = "locationSpan")]
        public LocationSpan LocationSpan { get; set; }

        [YamlMember(Alias = "footerSpan")]
        public CharacterSpan FooterSpan { get; set; }

        [YamlMember(Alias = "children", Order = 42)]
        public List<Container> Children { get; } = new List<Container>();

        [YamlMember(Alias = "parsingErrorsDetected")]
        public bool ParsingErrorsDetected => ParsingErrors.Any();

        [YamlMember(Alias = "parsingError")]
        public List<ParsingError> ParsingErrors { get; } = new List<ParsingError>();
    }
}