using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.TypeSystem;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Tanka.GraphQL.Generator.Core.Generators
{
    public class PartialObjectTypeModelGenerator
    {
        private readonly ObjectType _objectType;
        private readonly SchemaBuilder _schema;

        public PartialObjectTypeModelGenerator(ObjectType objectType, SchemaBuilder schema)
        {
            _objectType = objectType;
            _schema = schema;
        }

        public MemberDeclarationSyntax Generate()
        {
            var modelName = _objectType.Name.ToModelName();
            return ClassDeclaration(modelName)
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword),
                        Token(SyntaxKind.PartialKeyword)))
                .WithMembers(
                    List(GenerateProperties()));
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateProperties()
        {
            var props = new List<MemberDeclarationSyntax>();

            _schema.Connections(connections =>
            {
                var fields = connections.GetFields(_objectType);

                foreach (var field in fields)
                {
                    props.Add(GenerateProperty(field));
                }
            });

            return props;
        }

        private MemberDeclarationSyntax GenerateProperty(KeyValuePair<string, IField> field)
        {
            var propertyName = field.Key.Capitalize();
            return PropertyDeclaration(
                    PredefinedType(
                        Token(SyntaxKind.ObjectKeyword)),
                    Identifier(propertyName))
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword)))
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
    }
}