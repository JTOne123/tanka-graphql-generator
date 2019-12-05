using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Tanka.GraphQL.Generator.Core.Generators
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
            return InterfaceDeclaration(controllerInterfaceName)
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword)))
                .WithMembers(List(GenerateFields(_objectType, _schema)));
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
            var methodName = field.Key.ToFieldResolverName();
            return MethodDeclaration(
                    GenericName(Identifier(nameof(ValueTask)))
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                SingletonSeparatedList<TypeSyntax>(
                                    IdentifierName(nameof(IResolverResult))))),
                    Identifier(methodName))
                .WithParameterList(
                    ParameterList(
                        SingletonSeparatedList(
                            Parameter(
                                    Identifier("context"))
                                .WithType(
                                    IdentifierName(nameof(IResolverContext))))))
                .WithSemicolonToken(
                    Token(SyntaxKind.SemicolonToken));
        }
    }
}