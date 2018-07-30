using System;

using YamlDotNet.Serialization;

namespace MiKoSolutions.SemanticParsers.Xml.Yaml
{
    public sealed class LineInfo : IEquatable<LineInfo>, IComparable<LineInfo>
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

        public static bool operator <(LineInfo left, LineInfo right)
        {
            return left is null ? !(right is null) : left.CompareTo(right) < 0;
        }

        public static bool operator >(LineInfo left, LineInfo right)
        {
            return left is null ? !(right is null) : left.CompareTo(right) > 0;
        }

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

        public int CompareTo(LineInfo other)
        {
            if (other is null)
            {
                return -1;
            }

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