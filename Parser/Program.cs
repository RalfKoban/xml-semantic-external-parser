using System;
using System.Diagnostics;
using System.IO;
using System.Runtime;
using System.Threading.Tasks;

using MiKoSolutions.SemanticParsers.Xml.Flavors;
using MiKoSolutions.SemanticParsers.Xml.Yaml;

using SystemFile = System.IO.File;

namespace MiKoSolutions.SemanticParsers.Xml
{
    public static class Program
    {
        private static readonly Guid InstanceId = Guid.NewGuid();

        public static async Task<int> Main(string[] args)
        {
            // check for GMaster or PlasticSCM or SemanticMerge arguments (to allow debugging without the tools)
            if (args.Length == 2)
            {
                var shell = args[0]; // reserved for future usage
                var flagFile = args[1];

                SystemFile.WriteAllBytes(flagFile, new byte[] { 0x42 });
            }

            var watch = Stopwatch.StartNew();
            var gcWatch = Stopwatch.StartNew();
            while (true)
            {
                var inputFile = await Console.In.ReadLineAsync();
                if (inputFile == null || "end".Equals(inputFile, StringComparison.OrdinalIgnoreCase))
                {
                    // session is done
                    Tracer.Trace($"Terminating as session was ended (instance {InstanceId:B})");
                    return 0;
                }

                var encodingToUse = await Console.In.ReadLineAsync();
                var outputFile = await Console.In.ReadLineAsync();

                try
                {
                    var parseErrors = false;
                    try
                    {
                        watch.Restart();

                        // we find a flavor here, as we want to support different main methods, based on file ending
                        var flavor = XmlFlavorFinder.Find(inputFile);

                        var file = Parser.Parse(inputFile, encodingToUse, flavor);

                        using (var writer = SystemFile.CreateText(outputFile))
                        {
                            YamlWriter.Write(writer, file);
                        }

                        parseErrors = file.ParsingErrorsDetected == true;
                        if (parseErrors)
                        {
                            var parsingError = file.ParsingErrors[0];
                            Tracer.Trace(parsingError.ErrorMessage);
                            Tracer.Trace(parsingError.Location);
                        }

                        // clean-up after big files
                        if (IsBigFile(inputFile))
                        {
                            gcWatch.Restart();

                            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, false, true);

                            Tracer.Trace($"Garbage collection took {gcWatch.Elapsed:s\\.fff} secs  (instance {InstanceId:B})");
                        }

                        Console.WriteLine(parseErrors ? "KO" : "OK");
                    }
                    finally
                    {
                        Tracer.Trace($"Parsing took {watch.Elapsed:s\\.fff} secs  (instance {InstanceId:B}), errors found: {parseErrors}");
                    }
                }
                catch (Exception ex)
                {
                    Tracer.Trace($"Exception: {ex}", ex);

                    Console.WriteLine("KO");

                    return 0;
                }
            }
        }

        private static bool IsBigFile(string inputFile)
        {
            var info = new FileInfo(inputFile);

            return info.Exists && info.Length > 10_000_000;
        }
    }
}
