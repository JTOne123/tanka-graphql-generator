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
        // The folder where we will write all of our generated code.
        public string OutputPath { get; set; }

        [Required]
        public string Command { get; set; }

        public ITaskItem[] InputFiles { get; set; }

        // Will contain all of the generated coded we create 
        [Output] 
        public ITaskItem[] OutputFiles { get; set; }


        // The method that is called to invoke our task.
        public override bool Execute()
        {
            if (InputFiles == null)
                return true;

            //todo: build dotnet tanka-graphql-generator cmd line
            var argsBuilder = new StringBuilder();
            argsBuilder.Append(Command);

            if (!Command.EndsWith(" "))
                argsBuilder.Append(" ");
            
            argsBuilder.Append("-o ");
            argsBuilder.Append(OutputPath);
            argsBuilder.Append(" ");
           
            for (int i = 0; i < InputFiles.Length; i++)
            {
                if (i > 0 && i < InputFiles.Length -1)
                    argsBuilder.Append(" ");

                var inputFile = InputFiles[i];
                var filePath = Path.GetFullPath(inputFile.ItemSpec);
                argsBuilder.Append("-f ");
                argsBuilder.Append(filePath);
            }

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

            var outputFiles = new List<ITaskItem>();
            foreach (var filePath in files)
            {
               outputFiles.Add(new TaskItem(filePath));
               Log.LogMessage($"Out: {filePath}");
            }

            OutputFiles = outputFiles.ToArray();

            return true;
        }
    }
}