﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Tanka.GraphQL.Generator.Core.Generators;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.SDL;
using Tanka.GraphQL.TypeSystem;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Tanka.GraphQL.Generator.Core
{
    public class CodeGenerator
    {
        private readonly string _inputFile;
        private readonly string _targetNamespace;
        private string _schemaName;

        public CodeGenerator(string inputFile, string targetNamespace)
        {
            _inputFile = inputFile;
            _targetNamespace = targetNamespace;
            _schemaName = Path.GetFileNameWithoutExtension(inputFile);
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

        private IEnumerable<MemberDeclarationSyntax> GenerateTypes(SchemaBuilder schema)
        {
            return schema.GetTypes<ObjectType>()
                .SelectMany(objectType => GenerateType(objectType, schema))
                .Concat(GenerateSchema(schema));
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateSchema(SchemaBuilder schema)
        {
            yield return new SchemaResolversGenerator(schema, _schemaName).Generate();
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateType(ObjectType objectType, SchemaBuilder schema)
        {
            yield return new ControllerInterfaceGenerator(objectType, schema).Generate();
            yield return new AbstractControllerBaseGenerator(objectType, schema).Generate();
            yield return new FieldResolversGenerator(objectType, schema).Generate();
        }
        
        private Task<SchemaBuilder> LoadSchema()
        {
            var content = File.ReadAllText(_inputFile);
            var builder = new SchemaBuilder()
                .Sdl(content);

            return Task.FromResult(builder);
        }
    }
}