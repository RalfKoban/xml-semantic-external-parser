using System;
using System.Linq;

namespace MiKoSolutions.SemanticParsers.Xml
{
    public static class Tracer
    {
        private const string Category = "RKN Semantic";

        public static void Trace(string text) => System.Diagnostics.Trace.WriteLine(text, Category);

        public static void Trace(string text, Exception ex)
        {
            Trace(text);

            var stackTraceLines = ex.StackTrace?.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries) ?? Enumerable.Empty<string>();
            foreach (var stackTraceLine in stackTraceLines)
            {
                Trace(stackTraceLine);
            }
        }

        public static void Trace(object obj) => System.Diagnostics.Trace.WriteLine(obj, Category);
    }
}