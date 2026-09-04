using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using Linde.Syntax;

namespace Linde.Binder;

internal sealed class Binder<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(
    ExpressionSyntax Ast
)
{
    private static readonly Dictionary<string, PropertyInfo> properties = typeof(T)
        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
        .Where(property =>
            property.GetMethod is not null
            && !property.GetMethod.IsStatic
            && property.GetIndexParameters().Length == 0
        )
        .ToDictionary(property => property.Name, property => property, StringComparer.OrdinalIgnoreCase);

    private readonly ParameterExpression parameter = Expression.Parameter(typeof(T), "p");

    private static ConstantExpression CompileNumberExpression(NumericLiteralExpressionSyntax number) =>
        Expression.Constant(number.Value);

    private static ConstantExpression CompileBooleanExpression(BooleanLiteralExpressionSyntax boolean) =>
        Expression.Constant(boolean.Value);

    private static ConstantExpression CompileStringExpression(StringLiteralExpressionSyntax literal) =>
        Expression.Constant(literal.Value);

    private MemberExpression CompileIdentifierExpression(NameExpressionSyntax identifier) =>
        MemberExpression.Property(
            parameter,
            properties.TryGetValue(identifier.Identifier, out var property)
                ? property
                : throw new BinderException(
                    $"IdentifierToken {identifier.Identifier} is unknown and not a member of {typeof(T).Name}."
                )
        );

    private Expression CompileUnaryExpression(PrefixUnaryExpressionSyntax unary) =>
        unary.OperatorToken.Kind switch
        {
            SyntaxKind.MinusToken => Expression.Multiply(
                Expression.Constant(-1.0m),
                CompileExpression(unary.Operand)
            ),
            SyntaxKind.BangToken => Expression.Not(CompileExpression(unary.Operand)),

            _ => throw new BinderException(
                $"BadToken unary operator: {unary.OperatorToken.Text} at position {unary.OperatorToken.Position}"
            ),
        };

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(decimal))]
    private BinaryExpression CompileBinaryExpression(BinaryExpressionSyntax binary)
    {
        var left = CompileExpression(binary.Left);
        var right = CompileExpression(binary.Right);

        return binary.OperatorToken.Kind switch
        {
            SyntaxKind.PlusToken => BinaryExpression.Add(left, right),
            SyntaxKind.MinusToken => BinaryExpression.Subtract(left, right),
            SyntaxKind.StarToken => BinaryExpression.Multiply(left, right),
            SyntaxKind.SlashToken => BinaryExpression.Divide(left, right),
            SyntaxKind.EqualEqualToken => BinaryExpression.Equal(left, right),
            SyntaxKind.BangEqualToken => BinaryExpression.NotEqual(left, right),
            SyntaxKind.GreaterThanToken => BinaryExpression.GreaterThan(left, right),
            SyntaxKind.LessThanToken => BinaryExpression.LessThan(left, right),
            SyntaxKind.GreaterThanOrEqualsToken => BinaryExpression.GreaterThanOrEqual(left, right),
            SyntaxKind.LessThanOrEqualsToken => BinaryExpression.LessThanOrEqual(left, right),
            SyntaxKind.AmpersandAmpersandToken => BinaryExpression.AndAlso(left, right),
            SyntaxKind.PipePipeToken => BinaryExpression.OrElse(left, right),
            _ => throw new BinderException(
                $"BadToken unary operator: {binary.OperatorToken.Text} at position {binary.OperatorToken.Position}"
            ),
        };
    }

    private Expression CompileExpression(ExpressionSyntax node) =>
        node switch
        {
            NumericLiteralExpressionSyntax numberExpression => CompileNumberExpression(numberExpression),
            BooleanLiteralExpressionSyntax boolExpression => CompileBooleanExpression(boolExpression),
            StringLiteralExpressionSyntax stringExpression => CompileStringExpression(stringExpression),
            NameExpressionSyntax identifierExpression => CompileIdentifierExpression(identifierExpression),
            PrefixUnaryExpressionSyntax unaryExpression => CompileUnaryExpression(unaryExpression),
            BinaryExpressionSyntax binaryExpression => CompileBinaryExpression(binaryExpression),
            _ => throw new BinderException($"BadToken AST node: {node}"),
        };

    public Expression<Func<T, bool>> Compile()
    {
        var body = CompileExpression(Ast);

        if (body.Type != typeof(bool))
        {
            throw new BinderException($"Predicate must have type of bool, but has type of {body.Type.Name}");
        }

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}
