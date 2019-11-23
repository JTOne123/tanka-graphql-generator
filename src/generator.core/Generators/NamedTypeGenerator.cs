using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Generator.Core.Generators
{
    public class NamedTypeGenerator
    {
        private readonly IType _type;
        private readonly SchemaBuilder _schema;

        public NamedTypeGenerator(INamedType type, SchemaBuilder schema)
        {
            _type = type;
            _schema = schema;
        }

        public IEnumerable<MemberDeclarationSyntax> Generate()
        {
            var types = new List<MemberDeclarationSyntax>();

            switch (_type)
            {
                case ObjectType objectType:
                    types.AddRange(GenerateObjectType(objectType));
                    break;
                case InputObjectType inputObjectType:
                    types.AddRange(GenerateInputObjectType(inputObjectType));
                    break;
            }

            return types;
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateObjectType(ObjectType objectType)
        {
            yield return new ControllerInterfaceGenerator(objectType, _schema).Generate();
            yield return new FieldResolversGenerator(objectType, _schema).Generate();
            yield return new AbstractControllerBaseGenerator(objectType, _schema).Generate();
            yield return new PartialObjectTypeModelGenerator(objectType, _schema).Generate();
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateInputObjectType(InputObjectType inputObjectType)
        {
            yield return new InputObjectGenerator(inputObjectType, _schema).Generate();
        }
    }
}