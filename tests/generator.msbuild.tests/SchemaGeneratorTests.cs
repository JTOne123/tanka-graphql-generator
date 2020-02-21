using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Utilities.ProjectCreation;
using Xunit;
using Xunit.Abstractions;

namespace Tanka.GraphQL.Generator.MSBuild.Tests
{
    public class SchemaGeneratorTests : MSBuildTestBase
    {
        public SchemaGeneratorTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private readonly ITestOutputHelper _output;

        private void WriteLog(BuildOutput log)
        {
            
            _output.WriteLine("Messages");
            foreach (var message in log.Messages) _output.WriteLine(message);

            if (log.Succeeded == false)
            {
                _output.WriteLine("");
                _output.WriteLine("Errors");
                foreach (var error in log.Errors)
                    _output.WriteLine(error);
            }
        }

        /*
         * <PropertyGroup>
            <TankaSchemaTaskAssembly>$(MSBuildProjectDirectory)../../../src/generator/bin/$(Configuration)/netstandard2.0/tanka.graphql.generator.dll</TankaSchemaTaskAssembly>
            <TankaGeneratorToolCommand>$(MSBuildProjectDirectory)../../../src/generator.tool/bin/$(Configuration)/netcoreapp3.0/tanka.graphql.generator.tool.exe</TankaGeneratorToolCommand>
            <TankaGeneratorToolCommandArgs>gen-model</TankaGeneratorToolCommandArgs>
        </PropertyGroup>
  <Import Project="$(MSBuildProjectDirectory)../../../src/generator/build/Tanka.GraphQL.Generator.props" />
  <Import Project="$(MSBuildProjectDirectory)../../../src/generator/build/Tanka.GraphQL.Generator.targets" />
         */

        [Fact(Skip = "ProjectCreator does not run the BeforeGenerateGraphQL target")]
        public void BuildTest()
        {
#if DEBUG
            string configuration = "Debug";
#endif
#if RELEASE
            string configuration = "Release";
#endif

            var path = "./projects1/project1.csproj";
            var tempTaskFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var project = ProjectCreator
                .Create(path, defaultTargets:"GenerateGraphQL")
                .PropertyGroup()
                .Property("Configuration", configuration)
                .Property("CodeGenerationRoot", "./Generated/")
                .Property("TempTaskFolder", tempTaskFolder)
                .Property("TankaSchemaTaskAssembly", "$(TempTaskFolder)/tanka.graphql.generator.dll")
                .Property("RootNamespace", "Tanka.GraphQL.Generator.Tests")
                .Property("TankaGeneratorToolCommand", "dotnet")
                .Property("TankaGeneratorToolCommandArgs", "run --no-build -p $(MSBuildProjectDirectory)/../../../../../../src/generator.tool/ -- gen-model")
                .Property("TankaGeneratorForce", "true")
                .Target("BeforeGenerateGraphQL", inputs:"$(TempTaskFolder)")
                .TaskMessage("TempTaskFolder: $(TempTaskFolder)")
                .TargetItemGroup()
                .TargetItemInclude("_TaskFiles", "$(MSBuildProjectDirectory)/../../../../../../src/generator/bin/$(Configuration)/netstandard2.0/**/*.*")
                .Task("Copy", parameters: new Dictionary<string, string>()
                {
                    ["SourceFiles"] = "@(_TaskFiles)",
                    ["DestinationFolder"] ="$(TempTaskFolder)"
                })
                .Import("$(MSBuildProjectDirectory)/../../../../../../src/generator/build/tanka.graphql.generator.targets")
                .ItemInclude("GraphQL", "Data\\CRM.graphql");

            project.TryBuild(out var success, out var log);
            WriteLog(log);
            log.Dispose();
            Assert.True(success);
        }
    }
}