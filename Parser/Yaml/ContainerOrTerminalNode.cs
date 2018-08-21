using System.Diagnostics;

using YamlDotNet.Serialization;

namespace MiKoSolutions.SemanticParsers.Xml.Yaml
{
    [DebuggerDisplay("Type={Type}, Name={Name}")]
    public abstract class ContainerOrTerminalNode
    {
        [YamlIgnore]
        private string _type;

        [YamlIgnore]
        private string _name;

        [YamlMember(Alias = "type", Order = 1)]
        public string Type
        {
            get => _type;
            set => _type = WorkaroundForRegexIssue(value);
        }

        [YamlMember(Alias = "name", Order = 2)]
        public string Name
        {
            get => _name;
            set => _name = WorkaroundForRegexIssue(value);
        }

        [YamlMember(Alias = "locationSpan", Order = 3)]
        public LocationSpan LocationSpan { get; set; }

        [YamlIgnore]
        public string Content { get; set; }

        public abstract CharacterSpan GetTotalSpan();

        public abstract TerminalNode ToTerminalNode();

        private static string WorkaroundForRegexIssue(string value) => value?.Replace("\\", " \\ "); // workaround for Semantic/GMaster RegEx parsing exception that is not aware of special backslash character sequences
    }
}