using System;
using System.Diagnostics;
using System.Threading.Tasks;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

using SystemFile = System.IO.File;

namespace MiKoSolutions.SemanticParsers.Xml
{
    public static class Program
    {
        private const string Category = "RKN Semantic";

        private static readonly Guid InstanceId = Guid.NewGuid();

        public static async Task<int> Main(string[] args)
        {
            if (args.Length != 2)
            {
                return -1;
            }

            var shell = args[0]; // reserved for future usage
            var flagFile = args[1];

            SystemFile.WriteAllBytes(flagFile, new byte[] { 0x42 });

            while (true)
            {
                var inputFile = await Console.In.ReadLineAsync();
                if (inputFile == null || "end".Equals(inputFile, StringComparison.OrdinalIgnoreCase))
                {
                    // session is done
                    return 0;
                }

                var encodingToUse = await Console.In.ReadLineAsync();
                var outputFileToWrite = await Console.In.ReadLineAsync();

                try
                {
                    var watch = Stopwatch.StartNew();
                    try
                    {
                        var file = Parser.Parse(inputFile);

                        using (var writer = SystemFile.CreateText(outputFileToWrite))
                        {
                            YamlWriter.Write(writer, file);
                        }

                        var parseErrors = file.ParsingErrorsDetected == true;

                        if (parseErrors)
                        {
                            var parsingError = file.ParsingErrors[0];
                            Trace.WriteLine(parsingError.ErrorMessage, Category);
                            Trace.WriteLine(parsingError.Location, Category);
                        }

                        Console.WriteLine(parseErrors ? "KO" : "OK");
                    }
                    finally
                    {
                        Trace.WriteLine($"Parsing took {watch.Elapsed:s\\.fff} ms  (on instance {InstanceId:B})", Category);
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Exception: {ex}", Category);

                    foreach (var stackTraceLine in ex.StackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        Trace.WriteLine(stackTraceLine, Category);
                    }

                    Console.WriteLine("KO");

                    throw;
                }
            }
        }
    }
}
