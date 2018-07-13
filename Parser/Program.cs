using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using MiKoSolutions.SemanticParsers.Xml.Yaml.Converters;

using YamlDotNet.Serialization;

using File = System.IO.File;

namespace MiKoSolutions.SemanticParsers.Xml
{
    public static class Program
    {
        private const string Category = "RKN Semantic";

        public static async Task<int> Main(string[] args)
        {
            if (args.Length != 2)
            {
                return -1;
            }

            var shell = args[0]; // reserved for future usage
            var flagFile = args[1];

            File.WriteAllBytes(flagFile, new byte[] { 0x42 });

            while (true)
            {
                var fileToParse = await Console.In.ReadLineAsync();

                if ("end".Equals(fileToParse, StringComparison.OrdinalIgnoreCase) || fileToParse == null)
                {
                    // session is done
                    return 0;
                }

                var encodingToUse = Console.In.ReadLine();
                var outputFileToWrite = Console.In.ReadLine();

                try
                {
                    var file = Parser.Parse(fileToParse);
                    Resorter.Resort(file);

                    using (var writer = File.CreateText(outputFileToWrite))
                    {
                        Yaml(writer, file);
                    }

                    var result = file.ParsingErrorsDetected ? "KO" : "OK";

                    Debug.WriteLine($"Parsed result: {result}", Category);

                    Console.WriteLine(result);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception: {ex}", Category);
                    throw;
                }
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
