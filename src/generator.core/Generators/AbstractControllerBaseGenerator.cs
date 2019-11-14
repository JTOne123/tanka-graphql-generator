using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Tanka.GraphQL.Generator.Core.Generators
{
    public class AbstractControllerBaseGenerator
    {
        private readonly ObjectType _objectType;
        private readonly SchemaBuilder _schema;

        public AbstractControllerBaseGenerator(ObjectType objectType, SchemaBuilder schema)
        {
            _objectType = objectType;
            _schema = schema;
        }

        public MemberDeclarationSyntax Generate()
        {
            var interfaceName = _objectType.Name.ToControllerName().ToInterfaceName();
            var name = $"{_objectType.Name.ToControllerName()}Base";

            return ClassDeclaration(name)
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword),
                        Token(SyntaxKind.AbstractKeyword)
                    )
                )
                .WithTypeParameterList(
                    TypeParameterList(
                        SingletonSeparatedList(
                            TypeParameter(
                                Identifier("T")))))
                .WithBaseList(
                    BaseList(
                        SingletonSeparatedList<BaseTypeSyntax>(
                            SimpleBaseType(IdentifierName(interfaceName))
                        )
                    )
                )
                .WithMembers(List(GenerateFields(_objectType, _schema)));
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateFields(ObjectType objectType, SchemaBuilder schema)
        {
            var members = new List<MemberDeclarationSyntax>();
            schema.Connections(connections =>
            {
                members = connections.GetFields(objectType)
                    .SelectMany(field => GenerateField(objectType, field, schema))
                    .ToList();
            });
            return members;
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateField(
            ObjectType objectType,
            KeyValuePair<string, IField> field,
            SchemaBuilder schema)
        {
            var methodName = field.Key.Capitalize();

            yield return MethodDeclaration(
                    GenericName(Identifier(nameof(ValueTask)))
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                SingletonSeparatedList<TypeSyntax>(
                                    IdentifierName(nameof(IResolveResult))))),
                    Identifier(methodName))
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword),
                        Token(SyntaxKind.VirtualKeyword),
                        Token(SyntaxKind.AsyncKeyword)))
                .WithParameterList(
                    ParameterList(
                        SingletonSeparatedList(
                            Parameter(
                                    Identifier("context"))
                                .WithType(
                                    IdentifierName(nameof(ResolverContext))))))
                .WithBody(
                    Block(
                        LocalDeclarationStatement(
                            VariableDeclaration(
                                    IdentifierName("var"))
                                .WithVariables(
                                    SingletonSeparatedList(
                                        VariableDeclarator(
                                                Identifier("parent"))
                                            .WithInitializer(
                                                EqualsValueClause(
                                                    CastExpression(
                                                        IdentifierName("T"),
                                                        MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            IdentifierName("context"),
                                                            IdentifierName("ObjectValue")))))))),
                        LocalDeclarationStatement(
                            VariableDeclaration(
                                    IdentifierName("var"))
                                .WithVariables(
                                    SingletonSeparatedList(
                                        VariableDeclarator(
                                                Identifier("result"))
                                            .WithInitializer(
                                                EqualsValueClause(
                                                    AwaitExpression(
                                                        InvocationExpression(
                                                                IdentifierName(methodName))
                                                            .WithArgumentList(
                                                                ArgumentList(
                                                                    SeparatedList<ArgumentSyntax>(
                                                                        new SyntaxNodeOrToken[]
                                                                        {
                                                                            Argument(
                                                                                IdentifierName("parent")),
                                                                            Token(SyntaxKind.CommaToken),
                                                                            Argument(
                                                                                IdentifierName("context"))
                                                                        }))))))))),
                        ReturnStatement(
                            InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("Resolve"),
                                        IdentifierName("As")))
                                .WithArgumentList(
                                    ArgumentList(
                                        SingletonSeparatedList(
                                            Argument(
                                                IdentifierName("result")))))))
                )
                .WithTrailingTrivia(CarriageReturnLineFeed);

            if (IsAbstract(objectType, field))
                yield return WithAbstractFieldMethod(methodName, objectType, field);
            else
                yield return WithPropertyFieldMethod(methodName, objectType, field);
        }

        private bool IsAbstract(ObjectType objectType, KeyValuePair<string, IField> field)
        {
            var args = field.Value.Arguments;

            // if field has arguments then automatically require implementation for it
            if (args.Any())
                return true;

            var type = field.Value.Type;

            // if complex type (Object, Interface) then requires implementation
            if (type is ComplexType)
                return true;

            // unions require implementation as they require the actual graph type to be
            // given
            if (type is UnionType)
                return true;

            return false;
        }

        private MethodDeclarationSyntax WithAbstractFieldMethod(string methodName, ObjectType objectType,
            KeyValuePair<string, IField> field)
        {
            return MethodDeclaration(
                    GenericName(
                            Identifier("ValueTask"))
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                SingletonSeparatedList<TypeSyntax>(
                                    PredefinedType(
                                        Token(SyntaxKind.ObjectKeyword))))),
                    Identifier(methodName))
                .WithModifiers(
                    TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.AbstractKeyword)))
                .WithParameterList(
                    ParameterList(
                        SeparatedList<ParameterSyntax>(
                            new SyntaxNodeOrToken[]
                            {
                                Parameter(Identifier("parent"))
                                    .WithType(IdentifierName("T")),
                                Token(SyntaxKind.CommaToken),
                                Parameter(Identifier("context"))
                                    .WithType(IdentifierName("ResolverContext"))
                            })))
                .WithSemicolonToken(
                    Token(SyntaxKind.SemicolonToken));
        }

        private MethodDeclarationSyntax WithPropertyFieldMethod(string methodName, ObjectType objectType,
            KeyValuePair<string, IField> field)
        {
            return MethodDeclaration(
                    GenericName(
                            Identifier("ValueTask"))
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                SingletonSeparatedList<TypeSyntax>(
                                    PredefinedType(
                                        Token(SyntaxKind.ObjectKeyword))))),
                    Identifier(methodName))
                .WithModifiers(
                    TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.AbstractKeyword)))
                .WithParameterList(
                    ParameterList(
                        SeparatedList<ParameterSyntax>(
                            new SyntaxNodeOrToken[]
                            {
                                Parameter(Identifier("parent"))
                                    .WithType(IdentifierName("T")),
                                Token(SyntaxKind.CommaToken),
                                Parameter(Identifier("context"))
                                    .WithType(IdentifierName("ResolverContext"))
                            })))
                .WithSemicolonToken(
                    Token(SyntaxKind.SemicolonToken));
        }
    }
}