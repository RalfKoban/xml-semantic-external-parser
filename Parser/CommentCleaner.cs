using System;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

using Container = MiKoSolutions.SemanticParsers.Xml.Yaml.Container;

namespace MiKoSolutions.SemanticParsers.Xml
{
    public static class CommentCleaner
    {
        public static void Clean(File file)
        {
            foreach (var child in file.Children)
            {
                Clean(child, true);
            }
        }

        private static void Clean(Container parent, bool initial = false)
        {
            var children = parent.Children;

            const int first = 0;
            var last = children.Count - 1;

            var i = first;
            while (i <= last)
            {
                var child = children[i];
                if (child is Container c)
                {
                    Clean(c);
                }
                else
                {
                    var comment = child as TerminalNode;
                    if (comment?.Type == NodeType.Comment)
                    {
                        if (initial && i == last)
                        {
                            // special situation, a comment is the last element before the root footer
                            // so we simply adjust the footer instead
                            AdjustLocationSpan(comment, parent);
                            parent.FooterSpan = new CharacterSpan(comment.Span.Start, parent.FooterSpan.End);
                        }
                        else
                        {
                            var node = GetNodeToAdjust(parent, comment, i, first, last);

                            Adjust(comment, node);
                        }

                        children.Remove(comment);

                        last--;
                        i--;
                    }
                }

                i++;
            }
        }

        private static ContainerOrTerminalNode GetNodeToAdjust(Container parent, TerminalNode comment, int index, int first, int last)
        {
            var containers = parent.Children;

            if (index == first && index == last)
            {
                return parent;
            }

            if (index == first)
            {
                return containers[index + 1];
            }

            if (index == last)
            {
                return containers[index - 1];
            }

            var next = containers[index + 1];
            var before = containers[index - 1];

            var trailing = comment.LocationSpan.Start.LineNumber == before.LocationSpan.End.LineNumber;
            return trailing ? before : next;
        }

        private static void Adjust(TerminalNode comment, ContainerOrTerminalNode nodeToAdjust)
        {
            AdjustLocationSpan(comment, nodeToAdjust);

            if (nodeToAdjust is TerminalNode t)
            {
                AdjustSpan(comment, t);
            }
            else if (nodeToAdjust is Container c)
            {
                AdjustSpan(comment, c);
            }
        }

        private static void AdjustLocationSpan(TerminalNode comment, ContainerOrTerminalNode nodeToAdjust)
        {
            var commentStart = comment.LocationSpan.Start;
            var commentEnd = comment.LocationSpan.End;

            var nodeStart = nodeToAdjust.LocationSpan.Start;
            var nodeEnd = nodeToAdjust.LocationSpan.End;

            var start = nodeStart < commentStart ? nodeStart : commentStart;
            var end = nodeEnd < commentEnd ? commentEnd : nodeEnd;

            nodeToAdjust.LocationSpan = new LocationSpan(start, end);
        }

        private static void AdjustSpan(TerminalNode comment, TerminalNode nodeToAdjust)
        {
            var min = Math.Min(nodeToAdjust.Span.Start, comment.Span.Start);
            var max = Math.Max(nodeToAdjust.Span.End, comment.Span.End);
            nodeToAdjust.Span = new CharacterSpan(min, max);
        }

        private static void AdjustSpan(TerminalNode comment, Container nodeToAdjust)
        {
            var commentStart = comment.Span.Start;
            var commentEnd = comment.Span.End;

            var headerStart = nodeToAdjust.HeaderSpan.Start;
            var headerEnd = nodeToAdjust.HeaderSpan.End;

            var footerStart = nodeToAdjust.FooterSpan.Start;
            var footerEnd = nodeToAdjust.FooterSpan.End;

            if (commentStart > headerEnd && commentStart < footerStart)
            {
                // comment if after header
                nodeToAdjust.HeaderSpan = new CharacterSpan(headerStart, commentEnd);
            }
            else
            {
                var min = Math.Min(headerStart, commentStart);
                var max = Math.Max(footerEnd, commentEnd);
                nodeToAdjust.HeaderSpan = new CharacterSpan(min, headerEnd);
                nodeToAdjust.FooterSpan = new CharacterSpan(footerStart, max);
            }
        }
    }
}