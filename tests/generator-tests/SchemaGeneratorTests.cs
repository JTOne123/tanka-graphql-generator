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
            _output.WriteLine("Errors");
            if (log.Succeeded == false)
                foreach (var error in log.Errors)
                    _output.WriteLine(error);

            _output.WriteLine("");
            _output.WriteLine("Messages");
            foreach (var message in log.Messages) _output.WriteLine(message);
        }

        [Fact]
        public void BuildTest()
        {
            var project = ProjectCreator
                .Create()
                //.SdkCsproj("project1.csproj")
                .PropertyGroup()
                //.Property("TankaSchemaTaskAssembly", "tanka.graphql.generator.dll")
                .Property("TankaGeneratorToolCommand", "run -p ../../../../../src/generator.tool/generator.tool.csproj -- ")
                .Import("build\\Tanka.GraphQL.Generator.props")
                .Import("build\\Tanka.GraphQL.Generator.targets");
                //.ItemInclude("TankaSchema", "Data/Schema.graphql");

            project.TryBuild(out var success, out var log);
            WriteLog(log);
            Assert.True(success);
        }
    }
}