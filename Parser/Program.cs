using System;
using System.Diagnostics;
using System.IO;
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

                var watch = Stopwatch.StartNew();
                try
                {
                    var fileSize = (int)new FileInfo(inputFile).Length;

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
                            Tracer.Trace(parsingError.ErrorMessage);
                            Tracer.Trace(parsingError.Location);
                        }

                        Console.WriteLine(parseErrors ? "KO" : "OK");
                    }
                    finally
                    {
                        Tracer.Trace($"Parsing took {watch.Elapsed:s\\.fff} secs  (on instance {InstanceId:B})");
                        watch.Restart();
                    }

                    // clean-up after big files
                    if (fileSize > 10_000_000)
                    {
                        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, false, true);

                        Tracer.Trace($"Garbage collection took {watch.Elapsed:s\\.fff} secs  (on instance {InstanceId:B})");
                    }
                }
                catch (Exception ex)
                {
                    Tracer.Trace($"Exception: {ex}");

                    foreach (var stackTraceLine in ex.StackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
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
