using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Tanka.GraphQL.Generator
{
    public class SchemaGenerator : Task
    {
        [Required]
        // The folder where we will write all of our generated code.
        public string OutputPath { get; set; }

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

            //todo: parse cmd output
            //todo: add output files to OutputFiles

            return true;
        }
    }
}