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

        [YamlIgnore]
        private string _content;

        [YamlMember(Alias = "type", Order = 1)]
        public string Type
        {
            get => _type;
            set => _type = value is null ? null : string.Intern(WorkaroundForRegexIssue(value)); // performance optimization for large files
        }

        [YamlMember(Alias = "name", Order = 2)]
        public string Name
        {
            get => _name;
            set => _name = value is null ? null : string.Intern(WorkaroundForRegexIssue(value)); // performance optimization for large files
        }

        [YamlMember(Alias = "locationSpan", Order = 3)]
        public LocationSpan LocationSpan { get; set; }

        [YamlIgnore]
        public string Content
        {
            get => _content;
            set => _content = value is null ? null : string.IsInterned(value) ?? value; // performance optimization for large files (if we have the string interned, we can re-use it)
        }

        public abstract CharacterSpan GetTotalSpan();

        public abstract TerminalNode ToTerminalNode();

        private static string WorkaroundForRegexIssue(string value) => value.Replace("\\", " \\ ").Replace("++", "+ +"); // workaround for Semantic/GMaster RegEx parsing exception that is not aware of special backslash character sequences
    }
}