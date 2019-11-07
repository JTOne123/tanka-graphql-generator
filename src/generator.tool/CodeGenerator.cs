using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.SDL;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Tanka.GraphQL.Generator.Tool
{
    internal class CodeGenerator
    {
        private readonly string _inputFile;
        private readonly string _outputFolder;

        public CodeGenerator(string inputFile, string outputFolder)
        {
            _inputFile = inputFile;
            _outputFolder = outputFolder;
        }

        public async Task Generate()
        {
            var schema = await LoadSchema();
            var nsName = Path.GetFileNameWithoutExtension(_inputFile);

            var unit = CompilationUnit()
                .WithUsings(List(GenerateUsings()))
                .WithMembers(SingletonList<MemberDeclarationSyntax>(
                        NamespaceDeclaration(IdentifierName(nsName))
                            .WithMembers(List(GenerateTypes(schema)))))
                .NormalizeWhitespace();

            var sourceText = unit.ToFullString();
        }

        private IEnumerable<UsingDirectiveSyntax> GenerateUsings()
        {
            return
                List(new[]
                {
                    UsingDirective(ParseName("Tanka.GraphQL")),
                    UsingDirective(ParseName("Tanka.GraphQL.ValueResolution"))
                });
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateTypes(ISchema schema)
        {
            return schema.QueryTypes<ObjectType>()
                .Select(objectType => GenerateType(objectType, schema));
        }

        private MemberDeclarationSyntax GenerateType(ObjectType objectType, ISchema schema)
        {
            var typeName = objectType.Name.ToControllerName().ToInterfaceName();
            return InterfaceDeclaration(typeName)
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword)))
                .WithMembers(List(GenerateFields(objectType, schema)));
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateFields(ObjectType objectType, ISchema schema)
        {
            return schema.GetFields(objectType.Name)
                .Select(field => GenerateField(objectType, field, schema));
        }

        private MemberDeclarationSyntax GenerateField(ObjectType objectType, in KeyValuePair<string, IField> field,
            ISchema schema)
        {
            var methodName = field.Key.Capitalize();
            return MethodDeclaration(
                    GenericName(Identifier(nameof(ValueTask)))
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                SingletonSeparatedList<TypeSyntax>(
                                    IdentifierName(nameof(IResolveResult))))),
                    Identifier(methodName))
                .WithParameterList(
                    ParameterList(
                        SingletonSeparatedList(
                            Parameter(
                                    Identifier("context"))
                                .WithType(
                                    IdentifierName(nameof(ResolverContext))))))
                .WithSemicolonToken(
                    Token(SyntaxKind.SemicolonToken));
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