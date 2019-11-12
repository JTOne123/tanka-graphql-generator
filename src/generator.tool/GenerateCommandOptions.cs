using System.Collections.Generic;
using CommandLine;

namespace Tanka.GraphQL.Generator.Tool
{
    [Verb("gen")]
    public class GenerateCommandOptions
    {
        [Option('f', "file", Required = true, HelpText = "Input file")]
        public string InputFile { get; set; }

        [Option('o', "output", Required = true, HelpText = "Output file")] 
        public string OutputFile { get; set; }

        [Option('n', "namespace", HelpText = "Namespace")]
        public string Namespace { get; set; }
    }
}