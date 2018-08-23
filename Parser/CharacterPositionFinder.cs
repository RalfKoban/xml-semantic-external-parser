using System.Collections.Generic;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

using SystemFile = System.IO.File;

namespace MiKoSolutions.SemanticParsers.Xml
{
    public class CharacterPositionFinder
    {
        private const int NewLine = 10; // '\n'
        private const int CariageReturn = 13; // '\r'

        /// <summary>
        /// Contains the information about the lines in following format:
        /// <para />
        /// Line | LineLength | CharacterCount until end of line.
        /// </summary>
        private readonly IReadOnlyDictionary<int, KeyValuePair<int, int>> _lineNumberToLengthAndCountMap;

        /// <summary>
        /// Contains the information about the lines (as flat list) in following format:
        /// <para />
        /// CharacterCount | LineInfo (number and length till the position)
        /// </summary>
        private readonly IReadOnlyDictionary<int, LineInfo> _characterPositionToLineInfoMap;

        private CharacterPositionFinder(IReadOnlyDictionary<int, KeyValuePair<int, int>> lineNumberToLengthAndCountMap, IReadOnlyDictionary<int, LineInfo> characterPositionToLineInfoMap)
        {
            _lineNumberToLengthAndCountMap = lineNumberToLengthAndCountMap;
            _characterPositionToLineInfoMap = characterPositionToLineInfoMap;
        }

        public static CharacterPositionFinder CreateFrom(string filePath)
        {
            var lineNumber = 1;
            var count = -1;

            var map = new Dictionary<int, KeyValuePair<int, int>>
                          {
                              { 0, new KeyValuePair<int, int>(0, count) },
                          };

            var charPosToLineMap = new Dictionary<int, LineInfo>
                                       {
                                           { 0, new LineInfo(1, 1) },
                                       };

            var lineLength = 0;
            using (var reader = SystemFile.OpenText(filePath))
            {
                while (!reader.EndOfStream)
                {
                    lineLength++;
                    count++;

                    charPosToLineMap[count] = new LineInfo(lineNumber, lineLength);

                    var index = reader.Read();
                    switch (index)
                    {
                        case NewLine:
                        {
                            map[lineNumber++] = new KeyValuePair<int, int>(lineLength, count);
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

                                charPosToLineMap[count] = new LineInfo(lineNumber, lineLength);
                            }

                            map[lineNumber++] = new KeyValuePair<int, int>(lineLength, count);
                            lineLength = 0;
                            break;
                        }
                    }
                }
            }

            map[lineNumber] = new KeyValuePair<int, int>(lineLength, count);

            return new CharacterPositionFinder(map, charPosToLineMap);
        }

        public int GetCharacterPosition(LineInfo lineInfo) => GetCharacterPosition(lineInfo.LineNumber, lineInfo.LinePosition);

        public int GetCharacterPosition(int lineNumber, int linePosition)
        {
            var pair = _lineNumberToLengthAndCountMap[lineNumber - 1]; // get previous line and then add the line position

            var characterCount = pair.Value;

            return characterCount + linePosition;
        }

        public int GetLineLength(LineInfo lineInfo) => GetLineLength(lineInfo.LineNumber);

        public int GetLineLength(int lineNumber)
        {
            var pair = _lineNumberToLengthAndCountMap[lineNumber];
            var lineLength = pair.Key;

            return lineLength;
        }

        public LineInfo GetLineInfo(int characterPosition) => _characterPositionToLineInfoMap[characterPosition];
    }
}