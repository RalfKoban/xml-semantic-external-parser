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
            var lineEndingLength = -1;

            var i = 1;
            var count = -1;

            var map = new Dictionary<int, KeyValuePair<int, int>>
                          {
                              { 0, new KeyValuePair<int, int>(0, count) },
                          };

            foreach (var line in SystemFile.ReadLines(filePath))
            {
                if (lineEndingLength == -1)
                {
                    lineEndingLength = GetLineEndingLength(filePath, line.Length);
                }

                var lineLength = line.Length + lineEndingLength;
                count += lineLength;
                map[i++] = new KeyValuePair<int, int>(lineLength, count);
            }

            map[i] = new KeyValuePair<int, int>(0, count);

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

        private static int GetLineEndingLength(string filePath, int startPosition)
        {
            // try to figure out the line endings length (as they might differ because they could be '\n', '\r' or '\r\n'
            using (var fileStream = SystemFile.OpenRead(filePath))
            {
                var characters = new byte[10];
                fileStream.Seek(startPosition, SeekOrigin.Begin);
                var charactersRead = fileStream.Read(characters, 0, characters.Length);

                for (var i = 0; i < charactersRead; i++)
                {
                    var c = (char)characters[i];
                    switch (c)
                    {
                        case '\n':
                        {
                            return 1;
                        }

                        case '\r':
                        {
                            var next = i + 1;
                            return next < charactersRead && characters[next] == '\n' ? 2 : 1;
                        }
                    }
                }

                return 0;
            }
        }
    }
}