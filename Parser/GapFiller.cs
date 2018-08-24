using System.Collections.Generic;
using System.Linq;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml
{
    public static class GapFiller
    {
        public static void Fill(File file, CharacterPositionFinder finder)
        {
            foreach (var root in file.Children)
            {
                var children = root.Children.Where(IsNoAttribute).ToList();
                if (children.Any())
                {
                    AdjustBegin(root, children, 0, finder);
                    AdjustEnd(root, children, children.Count - 1, finder);
                }

                // adjust based on gaps, but only adjust child nodes that are no attributes and no text
                for (var index = 0; index < children.Count; index++)
                {
                    var child = children[index];
                    if (child.Type != NodeType.Text)
                    {
                        AdjustNode(root, children, index, finder);
                    }
                }
            }
        }

        private static void AdjustNode(Container parent, IList<ContainerOrTerminalNode> parentChildren, int indexInParentChildren, CharacterPositionFinder finder)
        {
            var newStartPos = AdjustBegin(parent, parentChildren, indexInParentChildren, finder);
            var newEndPos = AdjustEnd(parent, parentChildren, indexInParentChildren, finder);

            // somewhere in the middle, so adjust only node span and location, as well as that from the siblings
            var newStartLine = finder.GetLineInfo(newStartPos);
            var newEndLine = finder.GetLineInfo(newEndPos);

            var node = parentChildren[indexInParentChildren];

            node.LocationSpan = new LocationSpan(newStartLine, newEndLine);

            // now adjust terminal node's start position
            if (node is Container c)
            {
                // only adjust child nodes that are no attributes
                var children = c.Children.Where(IsNoAttribute).ToList();
                if (children.Any())
                {
                    c.HeaderSpan = new CharacterSpan(newStartPos, c.HeaderSpan.End);

                    for (var index = 0; index < children.Count; index++)
                    {
                        AdjustNode(c, children, index, finder);
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

        private static int AdjustBegin(Container parent, IList<ContainerOrTerminalNode> parentChildren, int indexInParentChildren, CharacterPositionFinder finder)
        {
            var node = parentChildren[indexInParentChildren];

            int newStartPos;

            var first = node == parentChildren.First();
            if (first)
            {
                // first child, so adjust parent's header span and terminal node's line start and span begin
                newStartPos = parent.HeaderSpan.End + 1;

                var parentHeaderSpanEnd = finder.GetLineInfo(parent.HeaderSpan.End);
                if (parentHeaderSpanEnd.LineNumber < node.LocationSpan.Start.LineNumber)
                {
                    // different lines, so adjust line end of parent
                    newStartPos = AdjustHeaderToLineEnd(parent, finder);
                }
            }
            else
            {
                var siblingBefore = parentChildren.ElementAt(indexInParentChildren - 1);
                newStartPos = siblingBefore.GetTotalSpan().End + 1;

                var siblingBeforeEndLineNumber = siblingBefore.LocationSpan.End.LineNumber;
                if (siblingBeforeEndLineNumber != node.LocationSpan.Start.LineNumber)
                {
                    newStartPos = finder.GetCharacterPosition(new LineInfo(siblingBeforeEndLineNumber + 1, 1));
                }
            }

            return newStartPos;
        }

        private static int AdjustEnd(Container parent, IList<ContainerOrTerminalNode> parentChildren, int indexInParentChildren, CharacterPositionFinder finder)
        {
            var node = parentChildren[indexInParentChildren];

            int newEndPos;

            var last = node == parentChildren.Last();
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
                var siblingAfter = parentChildren.ElementAt(indexInParentChildren + 1);
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

        private static bool IsNoAttribute(ContainerOrTerminalNode node) => node.Type != NodeType.Attribute;
    }
}