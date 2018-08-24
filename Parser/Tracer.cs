namespace MiKoSolutions.SemanticParsers.Xml
{
    public static class Tracer
    {
        private const string Category = "RKN Semantic";

        public static void Trace(string text) => System.Diagnostics.Trace.WriteLine(text, Category);

        public static void Trace(object obj) => System.Diagnostics.Trace.WriteLine(obj, Category);
    }
}