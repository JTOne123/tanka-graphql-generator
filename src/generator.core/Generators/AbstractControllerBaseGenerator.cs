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

        public static bool IsAbstract(SchemaBuilder schema, ObjectType objectType, KeyValuePair<string, IField> field)
        {
            return CodeModel.IsAbstract(schema, objectType, field);
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
            var methodName = field.Key.ToFieldResolverName();

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
                .WithBody(Block(WithFieldMethodBody(objectType, field, methodName)))
                .WithTrailingTrivia(CarriageReturnLineFeed);

            if (IsAbstract(schema, objectType, field))
                yield return WithAbstractFieldMethod(methodName, objectType, field);
            else
                yield return WithPropertyFieldMethod(methodName, objectType, field);
        }

        private IEnumerable<StatementSyntax> WithFieldMethodBody(
            ObjectType objectType,
            KeyValuePair<string, IField> field,
            string methodName)
        {
            yield return LocalDeclarationStatement(
                VariableDeclaration(
                        IdentifierName("var"))
                    .WithVariables(
                        SingletonSeparatedList(
                            VariableDeclarator(
                                    Identifier("objectValue"))
                                .WithInitializer(
                                    EqualsValueClause(
                                        BinaryExpression(
                                            SyntaxKind.AsExpression,
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                IdentifierName("context"),
                                                IdentifierName("ObjectValue")),
                                            IdentifierName("T")))))));
            yield return IfStatement(
                    BinaryExpression(
                        SyntaxKind.EqualsExpression,
                        IdentifierName("objectValue"),
                        LiteralExpression(
                            SyntaxKind.NullLiteralExpression)),
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
                                            LiteralExpression(
                                                SyntaxKind.NullLiteralExpression)))))))
                .WithIfKeyword(
                    Token(
                        TriviaList(
                            Comment("// if parent field was null this should never run")),
                        SyntaxKind.IfKeyword,
                        TriviaList()));

            yield return LocalDeclarationStatement(
                VariableDeclaration(
                        IdentifierName("var"))
                    .WithVariables(
                        SingletonSeparatedList(
                            VariableDeclarator(
                                    Identifier("resultTask"))
                                .WithInitializer(
                                    EqualsValueClause(
                                        InvocationExpression(
                                                IdentifierName(methodName))
                                            .WithArgumentList(
                                                ArgumentList(
                                                    SeparatedList<ArgumentSyntax>(
                                                        WithArguments(objectType, field)))))))));

            yield return IfStatement(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("resultTask"),
                    IdentifierName("IsCompletedSuccessfully")),
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
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName("resultTask"),
                                            IdentifierName("Result"))))))));

            yield return LocalDeclarationStatement(
                VariableDeclaration(
                        IdentifierName("var"))
                    .WithVariables(
                        SingletonSeparatedList(
                            VariableDeclarator(
                                    Identifier("result"))
                                .WithInitializer(
                                    EqualsValueClause(
                                        AwaitExpression(
                                            IdentifierName("resultTask")))))));

            yield return ReturnStatement(
                InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("Resolve"),
                            IdentifierName("As")))
                    .WithArgumentList(
                        ArgumentList(
                            SingletonSeparatedList(
                                Argument(
                                    IdentifierName("result"))))));
        }

        private IEnumerable<SyntaxNodeOrToken> WithArguments(
            ObjectType objectType,
            KeyValuePair<string, IField> fieldDefinition)
        {
            yield return Argument(IdentifierName("objectValue"));

            var arguments = fieldDefinition.Value.Arguments;

            foreach (var argumentDefinition in arguments)
            {
                yield return Token(SyntaxKind.CommaToken);
                yield return WithArgument(argumentDefinition);
            }

            yield return Token(SyntaxKind.CommaToken);

            yield return Argument(IdentifierName("context"));
        }

        private SyntaxNodeOrToken WithArgument(KeyValuePair<string, Argument> argumentDefinition)
        {
            var rawArgumentName = argumentDefinition.Key;
            var argument = argumentDefinition.Value;
            var typeName = CodeModel.SelectTypeName(argument.Type);

            var getArgumentMethodName = argument.Type is InputObjectType ? "GetObjectArgument" : "GetArgument";

            var getArgumentValue = InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("context"),
                        GenericName(
                                Identifier(getArgumentMethodName))
                            .WithTypeArgumentList(
                                TypeArgumentList(
                                    SingletonSeparatedList<TypeSyntax>(
                                        IdentifierName(typeName))))))
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(
                            Argument(LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                Literal(rawArgumentName))))));

            return Argument(getArgumentValue);
        }

        private MethodDeclarationSyntax WithAbstractFieldMethod(
            string methodName,
            ObjectType objectType,
            KeyValuePair<string, IField> field)
        {
            var resultTypeName = CodeModel.SelectFieldTypeName(_schema, _objectType, field);
            return MethodDeclaration(
                    GenericName(
                            Identifier(nameof(ValueTask)))
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
                            WithParameters(objectType, field))))
                .WithSemicolonToken(
                    Token(SyntaxKind.SemicolonToken));
        }

        private IEnumerable<SyntaxNodeOrToken> WithParameters(
            ObjectType objectType,
            KeyValuePair<string, IField> field)
        {
            yield return Parameter(Identifier("objectValue"))
                .WithType(IdentifierName("T"));

            var arguments = field.Value.Arguments;

            foreach (var argumentDefinition in arguments)
            {
                yield return Token(SyntaxKind.CommaToken);
                yield return WithParameter(argumentDefinition);
            }

            yield return Token(SyntaxKind.CommaToken);

            yield return Parameter(Identifier("context"))
                .WithType(IdentifierName(nameof(IResolverContext)));
        }

        private SyntaxNodeOrToken WithParameter(
            KeyValuePair<string, Argument> argumentDefinition)
        {
            var argumentName = argumentDefinition.Key.ToFieldArgumentName();
            var argument = argumentDefinition.Value;
            var typeName = CodeModel.SelectTypeName(argument.Type);

            return Parameter(Identifier(argumentName))
                .WithType(ParseTypeName(typeName));
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
                    TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.VirtualKeyword)))
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
                                            SingletonSeparatedList(
                                                Argument(
                                                    MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        IdentifierName("objectValue"),
                                                        IdentifierName(methodName))))))))));
        }
    }
}