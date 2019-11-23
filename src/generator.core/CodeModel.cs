using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Generator.Core
{
    public class CodeModel
    {
        public static bool IsNullable(IType type)
        {
            if (type is NonNull)
                return false;

            if (type is List list)
                return IsNullable(list.OfType);

            return true;
        }

        public static string SelectFieldTypeName(SchemaBuilder schema, ObjectType objectType, KeyValuePair<string, IField> field)
        {
            if (schema.TryGetDirective("gen", out _))
            {
                if (field.Value.HasDirective("gen"))
                {
                    var gen = field.Value.GetDirective("gen");

                    var clrType = gen.GetArgument<string>("clrType");
                    if (!string.IsNullOrEmpty(clrType))
                        return clrType;
                }
            }

            return SelectTypeName(field.Value.Type);
        }

        public static string SelectTypeName(IType type, bool nullable = true)
        {
            if (type is NonNull nonNull)
            {
                return SelectTypeName(nonNull.OfType, false);
            }

            if (type is List list)
            {
                var ofType = SelectTypeName(list.OfType);
                return $"IEnumerable<{ofType}>";
            }

            var typeName = SelectTypeName((INamedType)type);

            if (nullable)
                return $"{typeName}?";

            return typeName;
        }

        public static string SelectTypeName(INamedType namedType)
        {
            return namedType switch
            {
                ScalarType scalar => SelectScalarTypeName(scalar),
                ObjectType objectType => SelectObjectTypeName(objectType),
                EnumType enumType => SelectEnumTypeName(enumType),
                InputObjectType inputObjectType=> SelectInputObjectTypeName(inputObjectType),
                //todo: union special wrapping
                _ => "object"
            };
        }

        public static string SelectFieldTypeName(SchemaBuilder schema, InputObjectType inputObjectType, KeyValuePair<string, InputObjectField> fieldDefinition)
        {
            if (schema.TryGetDirective("gen", out _))
            {
                if (fieldDefinition.Value.HasDirective("gen"))
                {
                    var gen = fieldDefinition.Value.GetDirective("gen");

                    var clrType = gen.GetArgument<string>("clrType");
                    if (!string.IsNullOrEmpty(clrType))
                        return clrType;
                }
            }

            return SelectTypeName(fieldDefinition.Value.Type);
        }

        private static string SelectEnumTypeName(EnumType objectType)
        {
            return objectType.Name.ToModelName();
        }

        private static string SelectObjectTypeName(ObjectType objectType)
        {
            return objectType.Name.ToModelName();
        }

        private static string SelectScalarTypeName(ScalarType scalar)
        {
            if (StandardScalarToClrType.TryGetValue(scalar.Name, out var value))
            {
                return value;
            }

            return "object";
        }

        private static string SelectInputObjectTypeName(InputObjectType inputObjectType)
        {
            return inputObjectType.Name.ToModelName();
        }

        private static readonly Dictionary<string, string> StandardScalarToClrType = new Dictionary<string, string>()
        {
            [ScalarType.Float.Name] = "double",
            [ScalarType.Boolean.Name] = "bool",
            [ScalarType.ID.Name] = "string",
            [ScalarType.Int.Name] = "int",
            [ScalarType.String.Name] ="string"
        };

        public static SyntaxTriviaList ToXmlComment(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return SyntaxTriviaList.Empty;

            var comment = $"/// <summary>{Environment.NewLine}"
                          + string.Join(Environment.NewLine, text.Select(line => $"/// {line}"))
                + $"/// </summary>{Environment.NewLine}";
            return SyntaxFactory.ParseLeadingTrivia(comment);
        }

        public static bool IsAbstract(SchemaBuilder schema, ObjectType objectType, KeyValuePair<string, IField> field)
        {
            // Check for gen override directive
            if (schema.TryGetDirective("gen", out _))
            {
                if (field.Value.HasDirective("gen"))
                {
                    var gen = field.Value.GetDirective("gen");

                    var asAbstract = gen.GetArgument<bool>("asAbstract");

                    if (asAbstract)
                        return true;

                    var asProperty = gen.GetArgument<bool>("asProperty");

                    if (asProperty)
                        return false;
                }
            }

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
    }
}