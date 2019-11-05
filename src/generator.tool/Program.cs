using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;

namespace generator.tool
{
    public class Program
    {
        private static async Task<int> Main(string[] args)
        {
            MSBuildLocator.RegisterDefaults();

            var result = Parser.Default.ParseArguments<GenerateCommandOptions>(args);
            var retCode = await result.MapResult(
                RunGenerateCommand,
                _ => Task.FromResult(1));

            return retCode;
        }

        private static async Task<int> RunGenerateCommand(GenerateCommandOptions opts)
        {
            using var workspace = MSBuildWorkspace.Create();
            await workspace.OpenSolutionAsync(opts.Solution);
            


            return 1;
        }
    }

    [Verb("gen")]
    public class GenerateCommandOptions
    {
        [Option('p', "project", Required = true, HelpText = "Project file")]
        public string Solution { get; set; }

        [Option('f', "files", Required = true, HelpText = "Input files to be processed")]
        public IEnumerable<string> InputFiles { get; set; }

        [Option('o', "output", HelpText = "Output folder")] 
        public string OutputFolder { get; set; }
    }
}