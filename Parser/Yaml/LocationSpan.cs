using System;
using System.Diagnostics;

namespace MiKoSolutions.SemanticParsers.Xml.Yaml
{
    [DebuggerDisplay("Start: {Start}, End: {End}")]
    public sealed class LocationSpan : IEquatable<LocationSpan>
    {
        public LocationSpan(LineInfo start, LineInfo end)
        {
            Start = start;
            End = end;
        }

        public LineInfo Start { get; }

        public LineInfo End { get; }

        public static bool operator ==(LocationSpan left, LocationSpan right) => Equals(left, right);

        public static bool operator !=(LocationSpan left, LocationSpan right) => !Equals(left, right);

        public bool Equals(LocationSpan other)
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

        public override bool Equals(object obj) => Equals(obj as LocationSpan);

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Start != null ? Start.GetHashCode() : 0) * 397) ^ (End != null ? End.GetHashCode() : 0);
            }
        }

        public override string ToString() => $"Start: {Start}, End: {End}";
    }
}