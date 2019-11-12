using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using CommandLine;
using Tanka.GraphQL.Generator.Core;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Generator.Tool
{
    public class Program
    {
        private static async Task<int> Main(string[] args)
        {
            var result = CommandLine.Parser.Default.ParseArguments<GenerateCommandOptions>(args);
            var retCode = await result.MapResult(
                RunGenerateCommand,
                _ => Task.FromResult(1));

            return retCode;
        }

        private static async Task<int> RunGenerateCommand(GenerateCommandOptions opts)
        {
            var output = Path.GetFullPath(opts.OutputFolder);

            var files = new List<string>();
            foreach (var inputFile in opts.InputFiles)
            {
                var generator = new CodeGenerator(inputFile, opts.OutputFolder, opts.Namespace);
                var unit = await generator.Generate();
                var sourceText = unit.ToFullString();

                var schemaName = Path.GetFileNameWithoutExtension(inputFile);
                var path = Path.Combine(output, $"{opts.Namespace}.{schemaName}.g.cs");

                Directory.CreateDirectory(output);
                await File.WriteAllTextAsync(path, sourceText);
                files.Add(path);
            }

            Console.WriteLine(JsonSerializer.Serialize(files));

            return 0;
        }
    }
}