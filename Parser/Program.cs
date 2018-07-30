﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;

using MiKoSolutions.SemanticParsers.Xml.Yaml;

using SystemFile = System.IO.File;

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

            SystemFile.WriteAllBytes(flagFile, new byte[] { 0x42 });

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

                    using (var writer = SystemFile.CreateText(outputFileToWrite))
                    {
                        YamlWriter.Write(writer, file);
                    }

                    Console.WriteLine("OK");
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Exception: {ex}", Category);

                    Console.WriteLine("KO");

                    throw;
                }
            }
        }
    }
}
