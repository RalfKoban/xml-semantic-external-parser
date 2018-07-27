﻿using System.Linq;

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
            var index = parent.Children.IndexOf(node);

            var newStartPos = AdjustBegin(node, parent, finder, index);
            var newEndPos = AdjustEnd(node, parent, finder, index);

            // somewhere in the middle, so adjust only node span and location, as well as that from the siblings
            var newStartLine = finder.GetLineInfo(newStartPos);
            var newEndLine = finder.GetLineInfo(newEndPos);

            node.LocationSpan = new LocationSpan(newStartLine, newEndLine);

            // now adjust terminal node's start position
            if (node is Container c)
            {
                // only adjust child nodes that are no attributes
                var children = c.Children.Where(_ => _.Type != NodeType.Attribute).ToList();
                if (children.Any())
                {
                    c.HeaderSpan = new CharacterSpan(newStartPos, c.HeaderSpan.End);

                    foreach (var child in children)
                    {
                        AdjustNode(child, c, finder);
                    }

                    c.FooterSpan = new CharacterSpan(c.FooterSpan.Start, newEndPos);
                }
                else
                {
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

        private static int AdjustBegin(ContainerOrTerminalNode node, Container parent, CharacterPositionFinder finder, int index)
        {
            int newStartPos;

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
                }
            }

            return newStartPos;
        }

        private static int AdjustEnd(ContainerOrTerminalNode node, Container parent, CharacterPositionFinder finder, int index)
        {
            int newEndPos;

            var last = node == parent.Children.Last();
            if (last)
            {
                // last child, so adjust parent's footer span and terminal node's end start and span end
                newEndPos = parent.FooterSpan.Start - 1;

                var parentFooterLocation = finder.GetLineInfo(parent.FooterSpan.Start);
                if (parentFooterLocation.LineNumber != node.LocationSpan.End.LineNumber)
                {
                    var startPosition = new LineInfo(parentFooterLocation.LineNumber, 1);
                    newEndPos = AdjustParentFooter(parent, finder, startPosition);
                }
            }
            else
            {
                var siblingAfter = parent.Children.ElementAt(index + 1);
                newEndPos = siblingAfter.GetTotalSpan().Start - 1;

                var startLine = siblingAfter.LocationSpan.Start.LineNumber;
                if (startLine != node.LocationSpan.End.LineNumber)
                {
                    var lineLength = finder.GetLineLength(node.LocationSpan.End);
                    var lineInfo = new LineInfo(node.LocationSpan.End.LineNumber, lineLength);
                    newEndPos = finder.GetCharacterPosition(lineInfo);
                }
            }

            return newEndPos;
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

        private static int AdjustParentFooter(Container parent, CharacterPositionFinder finder, LineInfo position)
        {
            var characterPosition = finder.GetCharacterPosition(position);

            parent.FooterSpan = new CharacterSpan(characterPosition, parent.FooterSpan.End);

            return characterPosition - 1;
        }
    }
}