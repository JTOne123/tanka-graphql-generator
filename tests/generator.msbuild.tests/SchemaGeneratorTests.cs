﻿using Microsoft.Build.Utilities.ProjectCreation;
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

        [Fact]
        public void BuildTest()
        {
#if DEBUG
            string configuration = "Debug";
#endif
#if RELEASE
            string configuration = "Release";
#endif

            var path = "./projects1/project1.csproj";
            var project = ProjectCreator
                .Create(path, defaultTargets:"GenerateTankaSchema")
                .PropertyGroup()
                .Property("Configuration", configuration)
                .Property("CodeGenerationRoot", "./Generated/")
                .Property("TankaSchemaTaskAssembly", "$(MSBuildProjectDirectory)/../../../../../../src/generator/bin/$(Configuration)/netstandard2.0/tanka.graphql.generator.dll")
                .Property("RootNamespace", "Tanka.GraphQL.Generator.Tests")
                .Property("TankaGeneratorToolCommand", "dotnet")
                .Property("TankaGeneratorToolCommandArgs", "run --no-build -p $(MSBuildProjectDirectory)/../../../../../../src/generator.tool/ -- gen-model")
                .Property("TankaGeneratorForce", "true")
                .Import("$(MSBuildProjectDirectory)/../../../../../../src/generator/build/tanka.graphql.generator.props")
                .Import("$(MSBuildProjectDirectory)/../../../../../../src/generator/build/tanka.graphql.generator.targets");

            project.TryBuild(out var success, out var log);
            WriteLog(log);
            log.Dispose();
            Assert.True(success);
        }
    }
}