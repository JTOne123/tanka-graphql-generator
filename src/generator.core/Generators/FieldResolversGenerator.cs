using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.TypeSystem;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Tanka.GraphQL.Generator.Core.Generators
{
    public class FieldResolversGenerator
    {
        private readonly ObjectType _objectType;
        private readonly SchemaBuilder _schema;

        public FieldResolversGenerator(ObjectType objectType, SchemaBuilder schema)
        {
            _objectType = objectType;
            _schema = schema;
        }

        public MemberDeclarationSyntax Generate()
        {
            var name = _objectType.Name.ToFieldResolversName();

            return ClassDeclaration(name)
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword)))
                .WithBaseList(
                    BaseList(
                        SingletonSeparatedList<BaseTypeSyntax>(
                            SimpleBaseType(
                                IdentifierName(nameof(FieldResolversMap))))))
                .WithMembers(
                    SingletonList<MemberDeclarationSyntax>(
                        ConstructorDeclaration(
                                Identifier(name))
                            .WithModifiers(
                                TokenList(
                                    Token(SyntaxKind.PublicKeyword)))
                            .WithBody(Block(WithResolvers()))));
        }

        private List<StatementSyntax> WithResolvers()
        {
            var statements = new List<StatementSyntax>();
            _schema.Connections(connections =>
            {
                var fields = connections.GetFields(_objectType);
                statements = fields.Select(field => WithAddResolver(field))
                    .ToList();
            });
            return statements;
        }

        private StatementSyntax WithAddResolver(in KeyValuePair<string, IField> field)
        {
            var interfaceName = _objectType.Name.ToControllerName().ToInterfaceName();
            var fieldName = field.Key;
            var methodName = fieldName.ToFieldResolverName();
            return ExpressionStatement(
                InvocationExpression(
                        IdentifierName(nameof(FieldResolversMap.Add)))
                    .WithArgumentList(
                        ArgumentList(
                            SeparatedList<ArgumentSyntax>(
                                new SyntaxNodeOrToken[]
                                {
                                    Argument(
                                        LiteralExpression(
                                            SyntaxKind.StringLiteralExpression,
                                            Literal(fieldName))),
                                    Token(SyntaxKind.CommaToken),
                                    Argument(
                                        SimpleLambdaExpression(
                                            Parameter(
                                                Identifier("context")),
                                            InvocationExpression(
                                                    MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        InvocationExpression(
                                                            MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                IdentifierName("context"),
                                                                GenericName(
                                                                        Identifier("Use"))
                                                                    .WithTypeArgumentList(
                                                                        TypeArgumentList(
                                                                            SingletonSeparatedList<TypeSyntax>(
                                                                                IdentifierName(interfaceName)))))),
                                                        IdentifierName(methodName)))
                                                .WithArgumentList(
                                                    ArgumentList(
                                                        SingletonSeparatedList(
                                                            Argument(
                                                                IdentifierName("context")))))))
                                }))));
        }
    }
}