using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Tanka.GraphQL.Generator.Tool.Generators;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.SDL;
using Tanka.GraphQL.TypeSystem;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Tanka.GraphQL.Generator.Tool
{
    internal class CodeGenerator
    {
        private readonly string _inputFile;
        private readonly string _outputFolder;
        private readonly string _targetNamespace;

        public CodeGenerator(string inputFile, string outputFolder, string targetNamespace)
        {
            _inputFile = inputFile;
            _outputFolder = outputFolder;
            _targetNamespace = targetNamespace;
        }

        public async Task<CompilationUnitSyntax> Generate()
        {
            var schema = await LoadSchema();
            var nsName = _targetNamespace;

            var unit = CompilationUnit()
                .WithUsings(List(GenerateUsings()))
                .WithMembers(SingletonList<MemberDeclarationSyntax>(
                        NamespaceDeclaration(IdentifierName(nsName))
                            .WithMembers(List(GenerateTypes(schema)))))
                .NormalizeWhitespace();

            return unit;
        }

        private IEnumerable<UsingDirectiveSyntax> GenerateUsings()
        {
            return new[]
                {
                    UsingDirective(ParseName("System.Threading.Tasks")),
                    UsingDirective(ParseName("Tanka.GraphQL")),
                    UsingDirective(ParseName("Tanka.GraphQL.ValueResolution")),
                    UsingDirective(ParseName("Tanka.GraphQL.Server"))
                };
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateTypes(ISchema schema)
        {
            return schema.QueryTypes<ObjectType>()
                .SelectMany(objectType => GenerateType(objectType, schema))
                .Concat(GenerateSchema(schema));
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateSchema(ISchema schema)
        {
            yield return new SchemaResolversGenerator(schema).Generate();
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateType(ObjectType objectType, ISchema schema)
        {
            yield return new ControllerInterfaceGenerator(objectType, schema).Generate();
            yield return new AbstractControllerBaseGenerator(objectType, schema).Generate();
            yield return new FieldResolversGenerator(objectType, schema).Generate();
        }
        
        private async Task<ISchema> LoadSchema()
        {
            var content = await File.ReadAllTextAsync(_inputFile);
            return new SchemaBuilder()
                .Sdl(content)
                .Build();
        }
    }
}