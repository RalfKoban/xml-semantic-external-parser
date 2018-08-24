using System;
using System.Diagnostics;

namespace MiKoSolutions.SemanticParsers.Xml.Yaml
{
    [DebuggerDisplay("Start: {Start}, End: {End}")]
    public struct LocationSpan : IEquatable<LocationSpan>
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

        public bool Equals(LocationSpan other) => Start == other.Start && End == other.End;

        public override bool Equals(object obj) => obj is LocationSpan other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (Start.GetHashCode() * 397) ^ End.GetHashCode();
            }
        }

        public override string ToString() => $"Start: {Start}, End: {End}";
    }
}