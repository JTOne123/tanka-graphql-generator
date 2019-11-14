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
    public class SchemaResolversGenerator
    {
        private readonly SchemaBuilder _schema;
        private readonly string _name;

        public SchemaResolversGenerator(SchemaBuilder schema, string name)
        {
            _schema = schema;
            _name = name;
        }

        public MemberDeclarationSyntax Generate()
        {
            var resolversName = $"{_name}Resolvers";
            return ClassDeclaration(resolversName)
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
                                Identifier(resolversName))
                            .WithModifiers(
                                TokenList(
                                    Token(SyntaxKind.PublicKeyword)))
                            .WithBody(Block(WithAddObjectResolvers()))));
        }

        private IEnumerable<StatementSyntax> WithAddObjectResolvers()
        {
            var objectTypes = _schema.GetTypes<ObjectType>();
            return objectTypes.Select(WithAddObjectFieldResolvers);
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