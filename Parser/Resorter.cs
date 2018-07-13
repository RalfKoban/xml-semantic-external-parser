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
            var xLineNumber = x.LocationSpan.Start.LineNumber;
            var yLineNumber = y.LocationSpan.Start.LineNumber;
            if (xLineNumber < yLineNumber)
            {
                return -1;
            }

            if (xLineNumber > yLineNumber)
            {
                return 1;
            }

            return x.LocationSpan.Start.LinePosition - y.LocationSpan.Start.LinePosition;
        }
    }
}