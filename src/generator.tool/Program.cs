using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommandLine;

namespace generator.tool
{
    public class Program
    {
        private static async Task<int> Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<GenerateCommandOptions>(args);
            var retCode = await result.MapResult(
                options => RunGenerateCommand(options),
                _ => Task.FromResult(1));

            return retCode;
        }

        private static async Task<int> RunGenerateCommand(GenerateCommandOptions opts)
        {
            await Task.Delay(0);

            return 1;
        }
    }

    [Verb("gen")]
    public class GenerateCommandOptions
    {
        [Option('f', "files", Required = true, HelpText = "Input files to be processed")]
        public IEnumerable<string> InputFiles { get; set; }

        [Option('o', "output", HelpText = "Output folder")] 
        public string OutputFolder { get; set; }
    }
}