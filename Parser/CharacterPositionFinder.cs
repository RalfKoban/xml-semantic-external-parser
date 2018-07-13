using System;
using System.Collections.Generic;
using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml
{
    public class CharacterPositionFinder
    {
        private IReadOnlyDictionary<int, int> _map;

        private CharacterPositionFinder(IReadOnlyDictionary<int, int> map)
        {
            _map = map;
        }

        public static CharacterPositionFinder CreateFrom(string filePath)
        {
            var i = 1;
            var count = -1;

            var map = new Dictionary<int, int> { { 0, count } };
            foreach (var line in System.IO.File.ReadLines(filePath))
            {
                count += line.Length + Environment.NewLine.Length;
                map[i++] = count;
            }

            map[i] = count;

            return new CharacterPositionFinder(map);
        }

        public int GetCharacterPosition(LineInfo lineInfo)
        {
            var position = _map[lineInfo.LineNumber - 1] + lineInfo.LinePosition;
            return position;
        }
    }
}