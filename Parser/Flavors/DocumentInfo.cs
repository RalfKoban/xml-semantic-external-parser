using System.Diagnostics;

namespace MiKoSolutions.SemanticParsers.Xml.Flavors
{
    [DebuggerDisplay("Root={RootElement} Namespace={Namespace}")]
    public sealed class DocumentInfo
    {
        public string RootElement { get; set; }

        public string Namespace { get; set; }
    }
}