using System.Collections.Generic;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

using SystemFile = System.IO.File;

namespace MiKoSolutions.SemanticParsers.Xml
{
    public class CharacterPositionFinder
    {
        private const int NewLine = 10; // '\n'
        private const int CariageReturn = 13; // '\r'

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
                    lineLength++;
                    count++;

                    var index = reader.Read();
                    switch (index)
                    {
                        case NewLine:
                        {
                            map[i++] = new KeyValuePair<int, int>(lineLength, count);
                            lineLength = 0;
                            break;
                        }

                        case CariageReturn:
                        {
                            // additional line break character ?
                            var next = reader.Peek();
                            if (next == NewLine)
                            {
                                // read over the character
                                reader.Read();

                                lineLength++;
                                count++;
                            }

                            map[i++] = new KeyValuePair<int, int>(lineLength, count);
                            lineLength = 0;
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
    }
}