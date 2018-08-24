using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Threading.Tasks;

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

            while (true)
            {
                Tracer.Trace($"Ready to parse, waiting for input (instance {InstanceId:B})");

                var inputFile = await Console.In.ReadLineAsync();
                if (inputFile == null || "end".Equals(inputFile, StringComparison.OrdinalIgnoreCase))
                {
                    // session is done
                    Tracer.Trace($"Terminating as session was ended (instance {InstanceId:B})");
                    return 0;
                }

                var encodingToUse = await Console.In.ReadLineAsync();
                var outputFile = await Console.In.ReadLineAsync();

                Tracer.Trace($"Trying to parse '{inputFile}' with encoding '{encodingToUse}', output will be written to '{outputFile}' (instance {InstanceId:B})");

                var watch = Stopwatch.StartNew();
                try
                {
                    var fileSize = (int)new FileInfo(inputFile).Length;

                    try
                    {
                        var file = Parser.Parse(inputFile);

                        using (var writer = SystemFile.CreateText(outputFile))
                        {
                            YamlWriter.Write(writer, file);
                        }

                        var parseErrors = file.ParsingErrorsDetected == true;

                        if (parseErrors)
                        {
                            var parsingError = file.ParsingErrors[0];
                            Tracer.Trace(parsingError.ErrorMessage);
                            Tracer.Trace(parsingError.Location);
                        }

                        Console.WriteLine(parseErrors ? "KO" : "OK");
                    }
                    finally
                    {
                        Tracer.Trace($"Parsing took {watch.Elapsed:s\\.fff} secs  (instance {InstanceId:B})");
                        watch.Restart();
                    }

                    // clean-up after big files
                    if (fileSize > 10_000_000)
                    {
                        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, false, true);

                        Tracer.Trace($"Garbage collection took {watch.Elapsed:s\\.fff} secs  (instance {InstanceId:B})");
                    }
                }
                catch (Exception ex)
                {
                    Tracer.Trace($"Exception: {ex}");

                    var stackTraceLines = ex.StackTrace?.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries) ?? Enumerable.Empty<string>();
                    foreach (var stackTraceLine in stackTraceLines)
                    {
                        Tracer.Trace(stackTraceLine);
                    }

                    Console.WriteLine("KO");

                    throw;
                }
                finally
                {
                    watch.Stop();
                }
            }
        }
    }
}
