﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Tanka.GraphQL.Introspection;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.TypeSystem;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Tanka.GraphQL.Generator.Core.Generators
{
    public class ObjectTypeModelGenerator
    {
        private readonly ObjectType _objectType;
        private readonly SchemaBuilder _schema;

        public ObjectTypeModelGenerator(ObjectType objectType, SchemaBuilder schema)
        {
            _objectType = objectType;
            _schema = schema;
        }

        public MemberDeclarationSyntax Generate()
        {
            var modelName = _objectType.Name.ToModelName();
            var classDeclaration = ClassDeclaration(modelName)
                
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword),
                        Token(SyntaxKind.PartialKeyword)))
                .WithLeadingTrivia(CodeModel.ToXmlComment(_objectType.Description))
                .WithMembers(
                    List(GenerateProperties()));

            if (_objectType.Interfaces != null && _objectType.Interfaces.Any())
            {
                classDeclaration = classDeclaration.WithBaseList(WithInterfaces());
            }

            return classDeclaration;
        }

        private BaseListSyntax WithInterfaces()
        {
            var interfaceList = new List<SyntaxNodeOrToken>();
            var implementedInterfaces = _objectType.Interfaces.ToList();
            var interfaceCount = implementedInterfaces.Count;

            if (interfaceCount == 0)
                return BaseList();

            for (int i = 0; i < interfaceCount; i++)
            {
                var implementedInterface = implementedInterfaces[i];
                var modelName = implementedInterface.Name.ToModelInterfaceName();
                interfaceList.Add(SimpleBaseType(IdentifierName(modelName)));

                if (interfaceCount > 1 && i < interfaceCount - 1)
                    interfaceList.Add(Token(SyntaxKind.CommaToken));
            }

            return BaseList(
                SeparatedList<BaseTypeSyntax>(interfaceList));
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateProperties()
        {
            var props = new List<MemberDeclarationSyntax>();
            props.Add(CodeModel.TypenameProperty(_objectType.Name));

            var fields = _schema.GetFields(_objectType);

            foreach (var field in fields)
            {
                if (ObjectTypeAbstractControllerBaseGenerator.IsAbstract(
                    _schema, 
                    _objectType, 
                    field))
                    continue;

                props.Add(GenerateProperty(field));
            }

            return props;
        }

        private MemberDeclarationSyntax GenerateProperty(KeyValuePair<string, IField> field)
        {
            var propertyName = field.Key.ToFieldResolverName();
            var typeName = SelectFieldType(field);
            return PropertyDeclaration(
                    IdentifierName(typeName),
                    Identifier(propertyName))
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword)))
                .WithLeadingTrivia(CodeModel.ToXmlComment(field.Value.Description))
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

        public string SelectFieldType(KeyValuePair<string, IField> field)
        {
            return CodeModel.SelectFieldTypeName(_schema, _objectType, field);
        }
    }
}