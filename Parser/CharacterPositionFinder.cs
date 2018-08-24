using System;
using System.Collections.Generic;
using System.IO;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

using SystemFile = System.IO.File;

namespace MiKoSolutions.SemanticParsers.Xml
{
    public sealed class CharacterPositionFinder : IDisposable
    {
        private const int NewLine = 10; // '\n'
        private const int CarriageReturn = 13; // '\r'

        private readonly List<MapInfo> _lineNumberToLengthAndCountMap;
        private readonly List<LineInfo> _characterPositionToLineInfoMap;

        private CharacterPositionFinder(List<MapInfo> lineNumberToLengthAndCountMap, List<LineInfo> characterPositionToLineInfoMap)
        {
            _lineNumberToLengthAndCountMap = lineNumberToLengthAndCountMap;
            _characterPositionToLineInfoMap = characterPositionToLineInfoMap;
        }

        public static CharacterPositionFinder CreateFrom(string filePath)
        {
            var lineNumber = 1;
            var count = -1;

            var capacity = (int)new FileInfo(filePath).Length + 1;

            var map = new List<MapInfo>(capacity / 4)
                          {
                              new MapInfo(0, count),
                          };

            var charPosToLineMap = new List<LineInfo>(capacity)
                                       {
                                           new LineInfo(1, 1),
                                       };

            var lineLength = 0;
            using (var reader = SystemFile.OpenText(filePath))
            {
                while (!reader.EndOfStream)
                {
                    lineLength++;
                    count++;

                    charPosToLineMap.Insert(count, new LineInfo(lineNumber, lineLength));

                    var index = reader.Read();
                    switch (index)
                    {
                        case NewLine:
                        {
                            map.Insert(lineNumber++, new MapInfo(lineLength, count));
                            lineLength = 0;
                            break;
                        }

                        case CarriageReturn:
                        {
                            // additional line break character ?
                            var next = reader.Peek();
                            if (next == NewLine)
                            {
                                // read over the character
                                reader.Read();

                                lineLength++;
                                count++;

                                charPosToLineMap.Insert(count, new LineInfo(lineNumber, lineLength));
                            }

                            map.Insert(lineNumber++, new MapInfo(lineLength, count));
                            lineLength = 0;
                            break;
                        }
                    }
                }
            }

            map.Insert(lineNumber, new MapInfo(lineLength, count));

            return new CharacterPositionFinder(map, charPosToLineMap);
        }

        public void Dispose()
        {
            _lineNumberToLengthAndCountMap.Clear();
            _characterPositionToLineInfoMap.Clear();
        }

        public int GetCharacterPosition(LineInfo lineInfo) => GetCharacterPosition(lineInfo.LineNumber, lineInfo.LinePosition);

        public int GetCharacterPosition(int lineNumber, int linePosition)
        {
            var info = _lineNumberToLengthAndCountMap[lineNumber - 1]; // get previous line and then add the line position

            return info.CharacterCount + linePosition;
        }

        public int GetLineLength(LineInfo lineInfo) => GetLineLength(lineInfo.LineNumber);

        public int GetLineLength(int lineNumber)
        {
            var info = _lineNumberToLengthAndCountMap[lineNumber];

            return info.LineLength;
        }

        public LineInfo GetLineInfo(int characterPosition) => _characterPositionToLineInfoMap[characterPosition];

        private struct MapInfo : IEquatable<MapInfo>
        {
            internal readonly int LineLength;
            internal readonly int CharacterCount;

            internal MapInfo(int lineLength, int characterCount)
            {
                LineLength = lineLength;
                CharacterCount = characterCount;
            }

            public static bool operator ==(MapInfo left, MapInfo right) => left.Equals(right);

            public static bool operator !=(MapInfo left, MapInfo right) => !left.Equals(right);

            public bool Equals(MapInfo other) => LineLength == other.LineLength && CharacterCount == other.CharacterCount;

            public override bool Equals(object obj) => !ReferenceEquals(null, obj) && obj is MapInfo other && Equals(other);

            public override int GetHashCode()
            {
                unchecked
                {
                    return (LineLength * 397) ^ CharacterCount;
                }
            }

            public override string ToString() => string.Concat(nameof(LineLength) + "=", LineLength, " " + nameof(CharacterCount) + "=", CharacterCount);
        }
    }
}