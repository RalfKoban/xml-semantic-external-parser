using System.IO;
using MiKoSolutions.SemanticParsers.Xml.Yaml.Converters;
using YamlDotNet.Serialization;

namespace MiKoSolutions.SemanticParsers.Xml
{
    public static class YamlWriter
    {
        public static void Write(TextWriter writer, object graph)
        {
            var serializer = new SerializerBuilder()
                .WithTypeConverter(new CharacterSpanConverter())
                .WithTypeConverter(new LocationSpanConverter())
                .Build();
            serializer.Serialize(writer, graph);
        }
    }
}