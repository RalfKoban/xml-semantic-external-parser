using System.Collections.Generic;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml
{
    public static class Traverser
    {
        public static IReadOnlyList<ContainerOrTerminalNode> Traverse(File file)
        {
            var results = new List<ContainerOrTerminalNode>();

            Fill(file.Children, results);

            return results;
        }

        private static void Fill(IEnumerable<ContainerOrTerminalNode> nodes, List<ContainerOrTerminalNode> results)
        {
            foreach (var node in nodes)
            {
                Fill(node, results);
            }
        }

        private static void Fill(ContainerOrTerminalNode node, List<ContainerOrTerminalNode> results)
        {
            results.Add(node);

            if (node is Container c)
            {
                Fill(c.Children, results);
            }
        }
    }
}