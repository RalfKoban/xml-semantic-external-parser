﻿using YamlDotNet.Serialization;

namespace MiKoSolutions.SemanticParsers.Xml.Yaml
{
    public sealed class TerminalNode : ContainerOrTerminalNode
    {
        [YamlMember(Alias = "span", Order = 4)]
        public CharacterSpan Span { get; set; }
    }
}