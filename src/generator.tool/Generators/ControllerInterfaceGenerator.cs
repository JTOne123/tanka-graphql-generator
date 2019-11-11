using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;

namespace Tanka.GraphQL.Generator.Tool.Generators
{
    public class ControllerInterfaceGenerator
    {
        private readonly ObjectType _objectType;
        private readonly SchemaBuilder _schema;

        public ControllerInterfaceGenerator(ObjectType objectType, SchemaBuilder schema)
        {
            _objectType = objectType;
            _schema = schema;
        }

        public MemberDeclarationSyntax Generate()
        {
            var controllerInterfaceName = _objectType.Name.ToControllerName().ToInterfaceName();
            return SyntaxFactory.InterfaceDeclaration(controllerInterfaceName)
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithMembers(SyntaxFactory.List(GenerateFields(_objectType, _schema)));
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateFields(ObjectType objectType, SchemaBuilder schema)
        {
            var members = new List<MemberDeclarationSyntax>();
            schema.Connections(connections =>
            {
                members = connections.GetFields(objectType)
                    .Select(field => GenerateField(objectType, field, schema))
                    .ToList();
            });
            return members;
        }

        private MemberDeclarationSyntax GenerateField(ObjectType objectType, in KeyValuePair<string, IField> field,
            SchemaBuilder schema)
        {
            var methodName = field.Key.Capitalize();
            return SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.GenericName(SyntaxFactory.Identifier(nameof(ValueTask)))
                        .WithTypeArgumentList(
                            SyntaxFactory.TypeArgumentList(
                                SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                    SyntaxFactory.IdentifierName(nameof(IResolveResult))))),
                    SyntaxFactory.Identifier(methodName))
                .WithParameterList(
                    SyntaxFactory.ParameterList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Parameter(
                                    SyntaxFactory.Identifier("context"))
                                .WithType(
                                    SyntaxFactory.IdentifierName(nameof(ResolverContext))))))
                .WithSemicolonToken(
                    SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }
    }
}