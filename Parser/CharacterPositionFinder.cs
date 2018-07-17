using System;
using System.Collections.Generic;
using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml
{
    public class CharacterPositionFinder
    {
        private IReadOnlyDictionary<int, KeyValuePair<int, int>> _map;

        private CharacterPositionFinder(IReadOnlyDictionary<int, KeyValuePair<int, int>> map)
        {
            _map = map;
        }

        public static CharacterPositionFinder CreateFrom(string filePath)
        {
            var i = 1;
            var count = -1;

            var map = new Dictionary<int, KeyValuePair<int, int>>
                          {
                              { 0, new KeyValuePair<int, int>(0, count) },
                          };

            foreach (var line in System.IO.File.ReadLines(filePath))
            {
                var lineLength = line.Length + Environment.NewLine.Length;
                count += lineLength;
                map[i++] = new KeyValuePair<int, int>(lineLength, count);
            }

            map[i] = new KeyValuePair<int, int>(0, count);

            return new CharacterPositionFinder(map);
        }

        public int GetCharacterPosition(LineInfo lineInfo)
        {
            var pair = _map[lineInfo.LineNumber - 1]; // get previous line and then add the line position

            return pair.Value + lineInfo.LinePosition;
        }

        public int GetLineLength(LineInfo lineInfo) => GetLineLength(lineInfo.LineNumber);

        public int GetLineLength(int lineNumber)
        {
            var pair = _map[lineNumber];
            return pair.Key;
        }
    }
}