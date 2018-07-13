using System;
using System.Diagnostics;

using YamlDotNet.Serialization;

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

        [YamlMember(Alias = "start")]
        public LineInfo Start { get; }

        [YamlMember(Alias = "end")]
        public LineInfo End { get; }

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

            return Equals(Start, other.Start) && Equals(End, other.End);
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