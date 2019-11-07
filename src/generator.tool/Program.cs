using System.Threading.Tasks;
using CommandLine;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using Tanka.GraphQL;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.SDL;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Generator.Tool
{
    public class Program
    {
        private static async Task<int> Main(string[] args)
        {
            MSBuildLocator.RegisterDefaults();

            var result = CommandLine.Parser.Default.ParseArguments<GenerateCommandOptions>(args);
            var retCode = await result.MapResult(
                RunGenerateCommand,
                _ => Task.FromResult(1));

            return retCode;
        }

        private static async Task<int> RunGenerateCommand(GenerateCommandOptions opts)
        {
            //using var workspace = MSBuildWorkspace.Create();
            //var project = await workspace.OpenProjectAsync(opts.Project);
            

            foreach (var inputFile in opts.InputFiles)
            {
                var generator = new CodeGenerator(inputFile, opts.OutputFolder);
                await generator.Generate();
            }
           

            return 1;
        }
    }
}