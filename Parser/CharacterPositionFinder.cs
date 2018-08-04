using System.Collections.Generic;
using System.IO;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

using SystemFile = System.IO.File;

namespace MiKoSolutions.SemanticParsers.Xml
{
    public class CharacterPositionFinder
    {
        private readonly IReadOnlyDictionary<int, KeyValuePair<int, int>> _map;

        private CharacterPositionFinder(IReadOnlyDictionary<int, KeyValuePair<int, int>> map) => _map = map;

        public static CharacterPositionFinder CreateFrom(string filePath)
        {
            var i = 1;
            var count = -1;

            var map = new Dictionary<int, KeyValuePair<int, int>>
                          {
                              { 0, new KeyValuePair<int, int>(0, count) },
                          };

            var lineLength = 0;
            using (var reader = SystemFile.OpenText(filePath))
            {
                while (!reader.EndOfStream)
                {
                    var index = reader.Read();
                    switch (index)
                    {
                        case 10: // '\n'
                        case 13: // '\r'
                        {
                            var lineBreaks = ReadLineBreaks(reader);
                            lineLength += lineBreaks;

                            count += lineBreaks;
                            map[i++] = new KeyValuePair<int, int>(lineLength, count);

                            lineLength = 0;
                            break;
                        }

                        default:
                        {
                            count++;
                            lineLength++;
                            break;
                        }
                    }
                }
            }

            map[i] = new KeyValuePair<int, int>(lineLength, count);

            return new CharacterPositionFinder(map);
        }

        public int GetCharacterPosition(LineInfo lineInfo) => GetCharacterPosition(lineInfo.LineNumber, lineInfo.LinePosition);

        public int GetCharacterPosition(int lineNumber, int linePosition)
        {
            var pair = _map[lineNumber - 1]; // get previous line and then add the line position

            return pair.Value + linePosition;
        }

        public int GetLineLength(LineInfo lineInfo) => GetLineLength(lineInfo.LineNumber);

        public int GetLineLength(int lineNumber)
        {
            var pair = _map[lineNumber];
            return pair.Key;
        }

        public LineInfo GetLineInfo(int characterPosition)
        {
            foreach (var pair in _map)
            {
                var difference = pair.Value.Value - characterPosition;
                if (difference < 0)
                {
                    continue;
                }

                return new LineInfo(pair.Key, pair.Value.Key - difference);
            }

            return null;
        }

        private static int ReadLineBreaks(StreamReader reader)
        {
            var next = reader.Peek();
            switch (next)
            {
                case 10: // '\n'
                case 13: // '\r'
                    reader.Read(); // read over the character
                    return 2;

                default:
                    return 1;
            }
        }
    }
}