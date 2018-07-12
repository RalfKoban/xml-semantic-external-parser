using System;
using System.Diagnostics;
using System.Xml;
using YamlDotNet.Serialization;

namespace MiKoSolutions.SemanticParsers.Xml.Yaml
{
    public sealed class LineInfo : IEquatable<LineInfo>
    {
        public LineInfo(int lineNumber, int linePosition)
        {
            LineNumber = lineNumber;
            LinePosition = linePosition;
        }

        [YamlMember(Alias = "line")]
        public int LineNumber { get; }

        [YamlMember(Alias = "column")]
        public int LinePosition { get; }

        public bool Equals(LineInfo other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return LineNumber == other.LineNumber && LinePosition == other.LinePosition;
        }

        public override bool Equals(object obj) => Equals(obj as LineInfo);

        public override int GetHashCode()
        {
            unchecked
            {
                return (LineNumber * 397) ^ LinePosition;
            }
        }

        public override string ToString() => $"Line: {LineNumber}, Position: {LinePosition}";
    }
}