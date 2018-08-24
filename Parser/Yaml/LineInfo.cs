using System;

using YamlDotNet.Serialization;

namespace MiKoSolutions.SemanticParsers.Xml.Yaml
{
    public struct LineInfo : IEquatable<LineInfo>, IComparable<LineInfo>
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

        public static bool operator ==(LineInfo left, LineInfo right) => Equals(left, right);

        public static bool operator !=(LineInfo left, LineInfo right) => !Equals(left, right);

        public static bool operator <(LineInfo left, LineInfo right) => left.CompareTo(right) < 0;

        public static bool operator >(LineInfo left, LineInfo right) => left.CompareTo(right) > 0;

        public bool Equals(LineInfo other) => LineNumber == other.LineNumber && LinePosition == other.LinePosition;

        public override bool Equals(object obj) => obj is LineInfo other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (LineNumber * 397) ^ LinePosition;
            }
        }

        public int CompareTo(LineInfo other)
        {
            if (LineNumber < other.LineNumber)
            {
                return -1;
            }

            if (LineNumber > other.LineNumber)
            {
                return 1;
            }

            return LinePosition - other.LinePosition;
        }

        public override string ToString() => $"Line: {LineNumber}, Position: {LinePosition}";
    }
}