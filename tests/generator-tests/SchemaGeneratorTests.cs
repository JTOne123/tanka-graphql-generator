using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Utilities.ProjectCreation;
using Xunit;
using Xunit.Abstractions;

namespace Tanka.GraphQL.Generator.Tests
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

        [Fact]
        public void BuildTest()
        {
            var path = "./projects1/project1.csproj";
            const string targetFramework = "netcoreapp3.0";

            /*ProjectCreator.Templates.SdkCsproj(
                    targetFramework: targetFramework,
                    projectCollection: projectCollection)
                .Save(path)
                .TryBuild("Restore", out bool restoreResult, out BuildOutput restoreBuildOutput);

            Assert.True(restoreResult);*/

            var project = ProjectCreator
                .Create(path, defaultTargets:"GenerateTankaSchema")
                .PropertyGroup()
                .Property("CodeGenerationRoot", "./Generated/")
                .Property("TankaSchemaTaskAssembly", "../bin/Debug/netstandard2.0/tanka.graphql.generator.dll")
                .Property("RootNamespace", "Tanka.GraphQL.Generator.Tests")
                .Property("TankaGeneratorToolCommand",
                    "../../../../../../src/generator.tool/bin/Debug/netcoreapp3.0/tanka.graphql.generator.tool.exe")
                .Property("TankaGeneratorToolCommandArgs", "")
                .Import("../../../../../../src/generator/build/Tanka.GraphQL.Generator.props")
                .Import("../../../../../../src/generator/build/Tanka.GraphQL.Generator.targets");
                //.ItemInclude("TankaSchema", "Data/CRM.graphql");

            project.TryBuild(out var success, out var log);
            WriteLog(log);
            Assert.True(success);
        }
    }
}