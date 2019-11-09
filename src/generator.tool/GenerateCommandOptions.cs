using System.Collections.Generic;
using CommandLine;

namespace Tanka.GraphQL.Generator.Tool
{
    [Verb("gen")]
    public class GenerateCommandOptions
    {
        [Option('f', "files", Required = true, HelpText = "Input files to be processed")]
        public IEnumerable<string> InputFiles { get; set; }

        [Option('o', "output", Required = true, HelpText = "Output folder")] 
        public string OutputFolder { get; set; }

        [Option('n', "namespace", HelpText = "Namespace")]
        public string Namespace { get; set; }
    }
}