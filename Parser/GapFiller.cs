using MiKoSolutions.SemanticParsers.Xml.Yaml;

namespace MiKoSolutions.SemanticParsers.Xml
{
    public static class GapFiller
    {
        public static void Fill(File file)
        {
            var allNodes = Traverser.Traverse(file);
            foreach (var node in allNodes)
            {
                // adjust based on gaps
            }
        }
    }
}