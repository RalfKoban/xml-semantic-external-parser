using System;
using System.IO;
using MiKoSolutions.SemanticParsers.Xml.Yaml.Converters;

using YamlDotNet.Serialization;

using File = System.IO.File;

namespace MiKoSolutions.SemanticParsers.Xml
{
    class Program
    {
        static void Main(string[] args)
        {
            var input = args[0];
            var output = args[1];

            var file = Parser.Parse(input);
            Resorter.Resort(file);

            using (var writer = File.CreateText(output))
            {
                Yaml(writer, file);
            }
        }

        private static void Yaml(TextWriter writer, object graph)
        {
            var serializer = new SerializerBuilder()
                .WithTypeConverter(new CharacterSpanConverter())
                .WithTypeConverter(new LocationSpanConverter())
                .Build();
            serializer.Serialize(writer, graph);
        }
    }
}
