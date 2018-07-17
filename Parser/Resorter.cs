using System.Collections.Generic;
using System.Linq;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml
{
    public static class Resorter
    {
        public static void Resort(File file)
        {
            file.Children.Sort(CompareStartPosition);

            foreach (var node in file.Children)
            {
                Resort(node.Children);
            }
        }

        private static void Resort(List<ContainerOrTerminalNode> nodes)
        {
            nodes.Sort(CompareStartPosition);

            foreach (var node in nodes.OfType<Container>())
            {
                Resort(node.Children);
            }
        }

        private static int CompareStartPosition(ContainerOrTerminalNode x, ContainerOrTerminalNode y)
        {
            var startX = x.LocationSpan.Start;
            var startY = y.LocationSpan.Start;

            var result = startX.LineNumber - startY.LineNumber;
            if (result == 0)
            {
                result = startX.LinePosition - startY.LinePosition;
            }

            return result;
        }
    }
}