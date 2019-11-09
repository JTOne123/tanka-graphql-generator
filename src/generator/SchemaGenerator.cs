using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Tanka.GraphQL.Generator
{
    public class SchemaGenerator : Task
    {
        [Required]
        public string OutputPath { get; set; }

        [Required]
        public string Command { get; set; }

        [Required]
        public string RootNamespace { get; set; }

        [Required]
        public ITaskItem[] InputFiles { get; set; }

        [Output] 
        public ITaskItem[] OutputFiles { get; set; }

        public override bool Execute()
        {
            if (InputFiles == null)
                return true;

            var outputFiles = new List<ITaskItem>();
            for (int i = 0; i < InputFiles.Length; i++)
            {
                var inputFile = InputFiles[i];
                var filePath = Path.GetFullPath(inputFile.ItemSpec);
                
                // build command line
                var argsBuilder = new StringBuilder();
                argsBuilder.Append(Command);

                if (!Command.EndsWith(" "))
                    argsBuilder.Append(" ");

                var ns = Path.GetDirectoryName(inputFile.ItemSpec)
                    .Replace(Path.DirectorySeparatorChar, '.');
                    
                var schemaNamespace = $"{RootNamespace}.{ns}";
                argsBuilder.Append($"-n {schemaNamespace} ");

                argsBuilder.Append($"-o {OutputPath} ");

                argsBuilder.Append("-f ");
                argsBuilder.Append(filePath);

                var startInfo = new ProcessStartInfo("dotnet")
                {
                    Arguments = argsBuilder.ToString(),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                Log.LogCommandLine($"dotnet {startInfo.Arguments}");

                var result = Process.Start(startInfo);
                result.WaitForExit();

                var output = result.StandardOutput.ReadToEnd();
                if (result.ExitCode > 0)
                {
                    var error = result.StandardError.ReadToEnd();
                    Log.LogError(
                        $"Failed to execute generator command 'dotnet {startInfo.Arguments}'.\nError: {error}\n Output: {output}");
                    return false;
                }

                var files = JsonSerializer.Deserialize<List<string>>(output);

                foreach (var outputFile in files)
                {
                    outputFiles.Add(new TaskItem(outputFile));
                    Log.LogMessage($"Out: {outputFile}");
                }
            }

            OutputFiles = outputFiles.ToArray();

            return true;
        }
    }
}