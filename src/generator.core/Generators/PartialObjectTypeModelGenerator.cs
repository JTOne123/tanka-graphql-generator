using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
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
                    if (AbstractControllerBaseGenerator.IsAbstract(
                        _schema, 
                        _objectType, 
                        field))
                        continue;

                    props.Add(GenerateProperty(field));
                }
            });

            return props;
        }

        private MemberDeclarationSyntax GenerateProperty(KeyValuePair<string, IField> field)
        {
            var propertyName = field.Key.Capitalize();
            var typeName = SelectFieldType(field.Value);
            return PropertyDeclaration(
                    IdentifierName(typeName),
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

        public static string SelectFieldType(IField field)
        {
            var type = field.Type;
            return SelectTypeName(type);
        }

        public static string SelectTypeName(IType type)
        {
            if (type is NonNull nonNull)
            {
                return SelectTypeName(nonNull.OfType);
            }

            if (type is List list)
            {
                var ofType = SelectTypeName(list.OfType);
                return $"IEnumerable<{ofType}>";
            }

            return type switch
            {
                ScalarType scalar => SelectTypeName(scalar),
                ObjectType objectType => SelectTypeName(objectType),
                EnumType enumType => SelectTypeName(enumType),
                //todo: union special wrapping
                _ => "object"
            };
        }

        private static string SelectTypeName(EnumType objectType)
        {
            return objectType.Name.ToModelName();
        }

        private static string SelectTypeName(ObjectType objectType)
        {
            return objectType.Name.ToModelName();
        }

        private static string SelectTypeName(ScalarType scalar)
        {
            if (StandardScalarToClrType.TryGetValue(scalar.Name, out var value))
            {
                return value;
            }

            return "object";
        }

        private static readonly Dictionary<string, string> StandardScalarToClrType = new Dictionary<string, string>()
        {
            [ScalarType.Float.Name] = "float",
            [ScalarType.Boolean.Name] = "bool",
            [ScalarType.ID.Name] = "string",
            [ScalarType.Int.Name] = "int",
            [ScalarType.String.Name] ="string"
        };
    }
}