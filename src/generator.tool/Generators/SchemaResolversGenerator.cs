﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Tanka.GraphQL.TypeSystem;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Tanka.GraphQL.Generator.Tool.Generators
{
    internal class SchemaResolversGenerator
    {
        private readonly ISchema _schema;

        public SchemaResolversGenerator(ISchema schema)
        {
            _schema = schema;
        }

        public MemberDeclarationSyntax Generate()
        {
            return ClassDeclaration("SchemaResolvers")
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword)))
                .WithBaseList(
                    BaseList(
                        SingletonSeparatedList<BaseTypeSyntax>(
                            SimpleBaseType(
                                IdentifierName("ObjectTypeMap")))))
                .WithMembers(
                    SingletonList<MemberDeclarationSyntax>(
                        ConstructorDeclaration(
                                Identifier("SchemaResolvers"))
                            .WithModifiers(
                                TokenList(
                                    Token(SyntaxKind.PublicKeyword)))
                            .WithBody(Block(WithAddObjectResolvers()))));
        }

        private IEnumerable<StatementSyntax> WithAddObjectResolvers()
        {
            var objectTypes = _schema.QueryTypes<ObjectType>();
            return objectTypes.Select(objectType => WithAddObjectFieldResolvers(objectType));
        }

        private StatementSyntax WithAddObjectFieldResolvers(ObjectType objectType)
        {
            var objectName = objectType.Name;
            var resolversName = $"{objectName}Resolvers";
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
                                            Literal(objectName))),
                                    Token(SyntaxKind.CommaToken),
                                    Argument(
                                        ObjectCreationExpression(
                                                IdentifierName(resolversName))
                                            .WithArgumentList(
                                                ArgumentList()))
                                }))));
        }
    }
}