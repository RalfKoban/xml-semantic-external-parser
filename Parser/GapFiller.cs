using System.Linq;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml
{
    public static class GapFiller
    {
        public static void Fill(File file, CharacterPositionFinder finder)
        {
            foreach (var rootChild in file.Children)
            {
                // adjust based on gaps
                foreach (var child in rootChild.Children)
                {
                    AdjustNode(child, rootChild, finder);
                }
            }
        }

        private static void AdjustNode(ContainerOrTerminalNode node, Container parent, CharacterPositionFinder finder)
        {
            var newStartPos = -1;
            var newEndPos = -1;

            var index = parent.Children.IndexOf(node);

            var first = node == parent.Children.First();
            if (first)
            {
                // first child, so adjust parent's header span and terminal node's line start and span begin
                newStartPos = parent.HeaderSpan.End + 1;

                if (parent.LocationSpan.Start.LineNumber < node.LocationSpan.Start.LineNumber)
                {
                    // different lines, so adjust line end of parent
                    newStartPos = AdjustHeaderToLineEnd(parent, finder);
                }
            }
            else
            {
                var siblingBefore = parent.Children.ElementAt(index - 1);
                newStartPos = siblingBefore.GetTotalSpan().End + 1;

                var siblingBeforeEndLineNumber = siblingBefore.LocationSpan.End.LineNumber;
                if (siblingBeforeEndLineNumber != node.LocationSpan.Start.LineNumber)
                {
                    newStartPos = finder.GetCharacterPosition(new LineInfo(siblingBeforeEndLineNumber + 1, 1));

                    // TODO: get new char pos at begin of line
                    //                    var lineLength = finder.GetLineLength(node.LocationSpan.Start);
                    //                    var lineInfo = new LineInfo(node.LocationSpan.Start.LineNumber, lineLength);
                    //                    newStartPosition = finder.GetCharacterPosition(lineInfo);
                }
            }

            var last = node == parent.Children.Last();
            if (last)
            {
                // last child, so adjust parent's footer span and terminal node's end start and span end
                newEndPos = parent.FooterSpan.Start - 1;

                var parentFooterLocation = finder.GetLineInfo(parent.FooterSpan.Start);
                if (parentFooterLocation.LineNumber == node.LocationSpan.End.LineNumber)
                {
                    newEndPos = AdjustParentFooterToLineBegin(parent, finder);
                }
            }
            else
            {
                var siblingAfter = parent.Children.ElementAt(index + 1);
                newEndPos = siblingAfter.GetTotalSpan().Start - 1;

                if (siblingAfter.LocationSpan.Start.LineNumber != node.LocationSpan.End.LineNumber)
                {
                    var lineLength = finder.GetLineLength(node.LocationSpan.End);
                    var lineInfo = new LineInfo(node.LocationSpan.End.LineNumber, lineLength);
                    newEndPos = finder.GetCharacterPosition(lineInfo);
                }
            }

            // somewhere in the middle, so adjust only node span and location, as well as that from the siblings
            node.LocationSpan = new LocationSpan(finder.GetLineInfo(newStartPos), finder.GetLineInfo(newEndPos));

            // now adjust terminal node's start position
            if (node is Container c)
            {
                c.HeaderSpan = new CharacterSpan(newStartPos, c.HeaderSpan.End);
                c.FooterSpan = new CharacterSpan(c.FooterSpan.Start, newEndPos);

                if (c.Children.Any())
                {
                    // TODO: Adjust header and footer span
                    foreach (var child in c.Children)
                    {
                        AdjustNode(child, c, finder);
                    }
                }
                else
                {
                    // TODO: Adjust header and footer span
                    var headerEndLine = finder.GetLineInfo(c.HeaderSpan.End).LineNumber;
                    var footerStartLine = finder.GetLineInfo(c.FooterSpan.Start).LineNumber;
                    if (headerEndLine != footerStartLine)
                    {
                        var endPos = finder.GetLineLength(headerEndLine);
                        var headerEndPos = finder.GetCharacterPosition(headerEndLine, endPos);
                        c.HeaderSpan = new CharacterSpan(newStartPos, headerEndPos);
                        c.FooterSpan = new CharacterSpan(headerEndPos + 1, newEndPos);
                    }
                    else
                    {
                        c.HeaderSpan = new CharacterSpan(newStartPos, c.FooterSpan.Start - 1);
                        c.FooterSpan = new CharacterSpan(c.FooterSpan.Start, newEndPos);
                    }
                }
            }
            else if (node is TerminalNode t)
            {
                t.Span = new CharacterSpan(newStartPos, newEndPos);
            }
        }

        private static int AdjustHeaderToLineEnd(Container node, CharacterPositionFinder finder)
        {
            var info = finder.GetLineInfo(node.HeaderSpan.End);
            var lineLength = finder.GetLineLength(info.LineNumber);

            var endPosition = new LineInfo(info.LineNumber, lineLength);

            return AdjustHeaderEnd(node, finder, endPosition);
        }

        private static int AdjustHeaderEnd(Container node, CharacterPositionFinder finder, LineInfo position)
        {
            var characterPosition = finder.GetCharacterPosition(position);

            node.HeaderSpan = new CharacterSpan(node.HeaderSpan.Start, characterPosition);

            return characterPosition + 1;
        }

        private static int AdjustParentFooterToLineBegin(Container parent, CharacterPositionFinder finder)
        {
            var startPosition = finder.GetLineInfo(parent.FooterSpan.Start);
            var startPositionBefore = finder.GetLineInfo(parent.FooterSpan.Start - 1);

            if (startPositionBefore.LineNumber < startPosition.LineNumber)
            {
                // before
            }

            return AdjustParentFooter(parent, finder, startPosition);
        }

        private static int AdjustParentFooter(Container parent, CharacterPositionFinder finder, LineInfo position)
        {
            var characterPosition = finder.GetCharacterPosition(position);

            parent.FooterSpan = new CharacterSpan(characterPosition, parent.FooterSpan.End);

            return characterPosition - 1;
        }
    }
}