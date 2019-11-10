﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Tanka.GraphQL.Generator.Tool.Generators
{
    public class AbstractControllerBaseGenerator
    {
        private readonly ObjectType _objectType;
        private readonly ISchema _schema;

        public AbstractControllerBaseGenerator(ObjectType objectType, ISchema schema)
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

        private IEnumerable<MemberDeclarationSyntax> GenerateFields(ObjectType objectType, ISchema schema)
        {
            return schema.GetFields(objectType.Name)
                .SelectMany(field => GenerateField(objectType, field, schema));
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateField(
            ObjectType objectType,
            KeyValuePair<string, IField> field,
            ISchema schema)
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
                                    SingletonSeparatedList<VariableDeclaratorSyntax>(
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
                                    SingletonSeparatedList<VariableDeclaratorSyntax>(
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
                                                                new SyntaxNodeOrToken[]{
                                                                    Argument(
                                                                        IdentifierName("parent")),
                                                                    Token(SyntaxKind.CommaToken),
                                                                    Argument(
                                                                        IdentifierName("context"))}))))))))),
                            ReturnStatement(
                                InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("Resolve"),
                                        IdentifierName("As")))
                                .WithArgumentList(
                                    ArgumentList(
                                        SingletonSeparatedList<ArgumentSyntax>(
                                            Argument(
                                                IdentifierName("result")))))))
                    )
                .WithTrailingTrivia(CarriageReturnLineFeed);

            yield return MethodDeclaration(
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