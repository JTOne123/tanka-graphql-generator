﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Tanka.GraphQL.Generator.Core.Generators
{
    public class InputObjectGenerator
    {
        private InputObjectType _inputObjectType;
        private SchemaBuilder _schema;

        public InputObjectGenerator(InputObjectType inputObjectType, SchemaBuilder schema)
        {
            _inputObjectType = inputObjectType;
            _schema = schema;
        }

        public MemberDeclarationSyntax Generate()
        {
            var name = _inputObjectType.Name.ToModelName();
            return ClassDeclaration(name)
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword),
                        Token(SyntaxKind.PartialKeyword)
                    )
                )
                .WithBaseList(
                    BaseList(
                        SingletonSeparatedList<BaseTypeSyntax>(
                            SimpleBaseType(IdentifierName(nameof(IReadFromObjectDictionary))
                        )
                    )
                ))
                .WithMembers(List(GenerateFields()));
        }

        private MemberDeclarationSyntax GenerateRead()
        {
            return MethodDeclaration(
                    PredefinedType(
                        Token(SyntaxKind.VoidKeyword)),
                    Identifier("Read"))
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(
                    ParameterList(
                        SingletonSeparatedList<ParameterSyntax>(
                            Parameter(
                                Identifier("source"))
                            .WithType(
                                GenericName(
                                    Identifier("IReadOnlyDictionary"))
                                .WithTypeArgumentList(
                                    TypeArgumentList(
                                        SeparatedList<TypeSyntax>(
                                            new SyntaxNodeOrToken[]{
                                                PredefinedType(
                                                    Token(SyntaxKind.StringKeyword)),
                                                Token(SyntaxKind.CommaToken),
                                                PredefinedType(
                                                    Token(SyntaxKind.ObjectKeyword))})))))))
                .WithBody(Block(GenerateReadMethodBody()));
        }

        private IEnumerable<StatementSyntax> GenerateReadMethodBody()
        {
            var fields = _schema.GetInputFields(_inputObjectType);

            foreach(var fieldDefinition in fields)
            {
                var fieldName = fieldDefinition.Key.ToFieldResolverName();
                var rawFieldName = fieldDefinition.Key;
                var fieldTypeName = CodeModel.SelectFieldTypeName(_schema, _inputObjectType, fieldDefinition);
                yield return ExpressionStatement(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                IdentifierName(fieldName),
                                InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("source"),
                                        GenericName(
                                            Identifier("GetValue"))
                                        .WithTypeArgumentList(
                                            TypeArgumentList(
                                                SingletonSeparatedList<TypeSyntax>(
                                                    ParseTypeName(fieldTypeName))))))
                                .WithArgumentList(
                                    ArgumentList(
                                        SingletonSeparatedList<ArgumentSyntax>(
                                            Argument(
                                                LiteralExpression(
                                                    SyntaxKind.StringLiteralExpression,
                                                    Literal(rawFieldName))))))));
            }
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateFields()
        {
            var fields = _schema.GetInputFields(_inputObjectType);

            foreach(var fieldDefinition in fields)
            {
                var fieldName = fieldDefinition.Key.ToFieldResolverName();
                var fieldTypeName = CodeModel.SelectFieldTypeName(_schema, _inputObjectType, fieldDefinition);
                yield return PropertyDeclaration(
                    IdentifierName(fieldTypeName),
                    Identifier(fieldName))
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword)))
                .WithLeadingTrivia(CodeModel.ToXmlComment(fieldDefinition.Value.Description))
                .WithAccessorList(
                    AccessorList(
                        List(
                            new[]
                            {
                                AccessorDeclaration(
                                        SyntaxKind.GetAccessorDeclaration)
                                    .WithSemicolonToken(
                                        Token(SyntaxKind.SemicolonToken)),
                                AccessorDeclaration(
                                        SyntaxKind.SetAccessorDeclaration)
                                    .WithSemicolonToken(
                                        Token(SyntaxKind.SemicolonToken))
                            })));
            }

            yield return GenerateRead();
        }
    }
}