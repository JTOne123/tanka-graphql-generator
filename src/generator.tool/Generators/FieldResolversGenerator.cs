﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Tanka.GraphQL.TypeSystem;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Tanka.GraphQL.Generator.Tool.Generators
{
    internal class FieldResolversGenerator
    {
        private readonly ObjectType _objectType;
        private readonly ISchema _schema;

        public FieldResolversGenerator(ObjectType objectType, ISchema schema)
        {
            _objectType = objectType;
            _schema = schema;
        }

        public MemberDeclarationSyntax Generate()
        {
            var name = $"{_objectType.Name}Resolvers";

            return ClassDeclaration(name)
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword)))
                .WithBaseList(
                    BaseList(
                        SingletonSeparatedList<BaseTypeSyntax>(
                            SimpleBaseType(
                                IdentifierName("FieldResolversMap")))))
                .WithMembers(
                    SingletonList<MemberDeclarationSyntax>(
                        ConstructorDeclaration(
                                Identifier(name))
                            .WithModifiers(
                                TokenList(
                                    Token(SyntaxKind.PublicKeyword)))
                            .WithBody(Block(WithResolvers()))));
        }

        private IEnumerable<StatementSyntax> WithResolvers()
        {
            var fields = _schema.GetFields(_objectType.Name);
            return fields.Select(field => WithAddResolver(field));
        }

        private StatementSyntax WithAddResolver(in KeyValuePair<string, IField> field)
        {
            var interfaceName = _objectType.Name.ToControllerName().ToInterfaceName();
            var fieldName = field.Key;
            var methodName = fieldName.Capitalize();
            return ExpressionStatement(
                InvocationExpression(
                        IdentifierName("Add"))
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