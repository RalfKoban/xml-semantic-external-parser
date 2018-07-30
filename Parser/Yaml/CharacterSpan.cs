using System;

namespace MiKoSolutions.SemanticParsers.Xml.Yaml
{
    public sealed class CharacterSpan : IEquatable<CharacterSpan>
    {
        public CharacterSpan(int start, int end)
        {
            if (start > end && (start != 0 || end != -1))
            {
                throw new ArgumentException($"{nameof(start)} should be less than {nameof(end)} but {start} is greater than {end}!");
            }

            Start = start;
            End = end;
        }

        public int Start { get; }

        public int End { get; }

        public static bool operator ==(CharacterSpan left, CharacterSpan right) => Equals(left, right);

        public static bool operator !=(CharacterSpan left, CharacterSpan right) => !Equals(left, right);

        public bool Equals(CharacterSpan other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Start == other.Start && End == other.End;
        }

        public override bool Equals(object obj) => Equals(obj as CharacterSpan);

        public override int GetHashCode()
        {
            unchecked
            {
                return (Start * 397) ^ End;
            }
        }

        public override string ToString() => $"Span: {Start}, {End}";
    }
}