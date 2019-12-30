using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.TypeSystem;

namespace Tanka.GraphQL.Generator.Core.Generators
{
    public class NamedTypeGenerator
    {
        private readonly SchemaBuilder _schema;
        private readonly INamedType _type;

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
            yield return new ObjectTypeControllerInterfaceGenerator(objectType, _schema).Generate();
            yield return new ObjectTypeFieldResolversGenerator(objectType, _schema).Generate();
            yield return new ObjectTypeAbstractControllerBaseGenerator(objectType, _schema).Generate();
            yield return new ObjectTypeModelGenerator(objectType, _schema).Generate();
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateInputObjectType(InputObjectType inputObjectType)
        {
            yield return new InputObjectModelGenerator(inputObjectType, _schema).Generate();
        }
    }
}