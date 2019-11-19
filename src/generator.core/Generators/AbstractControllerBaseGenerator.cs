using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Tanka.GraphQL.SchemaBuilding;
using Tanka.GraphQL.TypeSystem;
using Tanka.GraphQL.ValueResolution;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Tanka.GraphQL.Generator.Core.Generators
{
    public class AbstractControllerBaseGenerator
    {
        private readonly ObjectType _objectType;
        private readonly SchemaBuilder _schema;

        public AbstractControllerBaseGenerator(ObjectType objectType, SchemaBuilder schema)
        {
            _objectType = objectType;
            _schema = schema;
        }

        public MemberDeclarationSyntax Generate()
        {
            var interfaceName = _objectType.Name.ToControllerName().ToInterfaceName();
            var modelName = _objectType.Name.ToModelName();
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
                .WithConstraintClauses(
                    SingletonList(
                        TypeParameterConstraintClause(
                                IdentifierName("T"))
                            .WithConstraints(
                                SingletonSeparatedList<TypeParameterConstraintSyntax>(
                                    TypeConstraint(
                                        IdentifierName(modelName))))))
                .WithMembers(List(GenerateFields(_objectType, _schema)));
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateFields(ObjectType objectType, SchemaBuilder schema)
        {
            var members = new List<MemberDeclarationSyntax>();
            schema.Connections(connections =>
            {
                members = connections.GetFields(objectType)
                    .SelectMany(field => GenerateField(objectType, field, schema))
                    .ToList();
            });
            return members;
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateField(
            ObjectType objectType,
            KeyValuePair<string, IField> field,
            SchemaBuilder schema)
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
                                    IdentifierName(nameof(IResolverContext))))))
                .WithBody(
                    Block(
                        LocalDeclarationStatement(
                            VariableDeclaration(
                                    IdentifierName("var"))
                                .WithVariables(
                                    SingletonSeparatedList(
                                        VariableDeclarator(
                                                Identifier("objectValue"))
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
                                    SingletonSeparatedList(
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
                                                                        new SyntaxNodeOrToken[]
                                                                        {
                                                                            Argument(
                                                                                IdentifierName("objectValue")),
                                                                            Token(SyntaxKind.CommaToken),
                                                                            Argument(
                                                                                IdentifierName("context"))
                                                                        }))))))))),
                        ReturnStatement(
                            InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("Resolve"),
                                        IdentifierName("As")))
                                .WithArgumentList(
                                    ArgumentList(
                                        SingletonSeparatedList(
                                            Argument(
                                                IdentifierName("result")))))))
                )
                .WithTrailingTrivia(CarriageReturnLineFeed);

            if (IsAbstract(schema, objectType, field))
                yield return WithAbstractFieldMethod(methodName, objectType, field);
            else
                yield return WithPropertyFieldMethod(methodName, objectType, field);
        }

        public static bool IsAbstract(SchemaBuilder schema, ObjectType objectType, KeyValuePair<string, IField> field)
        {
            return CodeModel.IsAbstract(schema, objectType, field);
        }

        private MethodDeclarationSyntax WithAbstractFieldMethod(string methodName, ObjectType objectType,
            KeyValuePair<string, IField> field)
        {
            var resultTypeName = CodeModel.SelectFieldTypeName(_schema, _objectType, field);
            return MethodDeclaration(
                    GenericName(
                            Identifier("ValueTask"))
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                SingletonSeparatedList<TypeSyntax>(
                                    IdentifierName(resultTypeName)))),
                    Identifier(methodName))
                .WithModifiers(
                    TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.AbstractKeyword)))
                .WithParameterList(
                    ParameterList(
                        SeparatedList<ParameterSyntax>(
                            new SyntaxNodeOrToken[]
                            {
                                Parameter(Identifier("objectValue"))
                                    .WithType(IdentifierName("T")),
                                Token(SyntaxKind.CommaToken),
                                Parameter(Identifier("context"))
                                    .WithType(IdentifierName(nameof(IResolverContext)))
                            })))
                .WithSemicolonToken(
                    Token(SyntaxKind.SemicolonToken));
        }

        private MethodDeclarationSyntax WithPropertyFieldMethod(
            string methodName, 
            ObjectType objectType,
            KeyValuePair<string, IField> field)
        {
            var resultTypeName = CodeModel.SelectFieldTypeName(_schema, _objectType, field);
            return MethodDeclaration(
                    GenericName(
                            Identifier("ValueTask"))
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                SingletonSeparatedList<TypeSyntax>(
                                    IdentifierName(resultTypeName)))),
                    Identifier(methodName))
                .WithModifiers(
                    TokenList(
                        new[]
                        {
                            Token(SyntaxKind.PublicKeyword),
                            Token(SyntaxKind.VirtualKeyword)
                        }))
                .WithParameterList(
                    ParameterList(
                        SeparatedList<ParameterSyntax>(
                            new SyntaxNodeOrToken[]
                            {
                                Parameter(
                                        Identifier("objectValue"))
                                    .WithType(
                                        IdentifierName("T")),
                                Token(SyntaxKind.CommaToken),
                                Parameter(
                                        Identifier("context"))
                                    .WithType(
                                        IdentifierName(nameof(IResolverContext)))
                            })))
                .WithBody(
                    Block(
                        SingletonList<StatementSyntax>(
                            ReturnStatement(
                                ObjectCreationExpression(
                                        GenericName(
                                                Identifier("ValueTask"))
                                            .WithTypeArgumentList(
                                                TypeArgumentList(
                                                    SingletonSeparatedList<TypeSyntax>(
                                                        IdentifierName(resultTypeName)))))
                                    .WithArgumentList(
                                        ArgumentList(
                                            SingletonSeparatedList<ArgumentSyntax>(
                                                Argument(
                                                    MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        IdentifierName("objectValue"),
                                                        IdentifierName(methodName))))))))));
        }
    }
}