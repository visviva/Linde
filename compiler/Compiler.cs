using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using Linde.Parser;
using Linde.Scanner;

namespace Linde.Compiler;

internal sealed class Compiler<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T
>(ExpressionSyntax Ast)
{
    private static readonly Dictionary<string, PropertyInfo> properties = typeof(T)
        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
        .Where(property =>
            property.GetMethod is not null
            && !property.GetMethod.IsStatic
            && property.GetIndexParameters().Length == 0
        )
        .ToDictionary(
            property => property.Name,
            property => property,
            StringComparer.OrdinalIgnoreCase
        );

    private readonly ParameterExpression parameter = Expression.Parameter(typeof(T), "p");

    private static ConstantExpression CompileNumberExpression(NumberExpressionSyntax number) =>
        Expression.Constant(number.Value);

    private static ConstantExpression CompileBooleanExpression(BoolExpressionSyntax boolean) =>
        Expression.Constant(boolean.Value);

    private static ConstantExpression CompileStringExpression(StringExpressionSyntax literal) =>
        Expression.Constant(literal.Value);

    private MemberExpression CompileIdentifierExpression(
        IdentifierExpressionSyntax identifier
    ) =>
        MemberExpression.Property(
            parameter,
            properties.TryGetValue(identifier.Identifier, out var property)
                ? property
                : throw new CompileException(
                    $"Identifier {identifier.Identifier} is unknown and not a member of {typeof(T).Name}."
                )
        );

    private Expression CompileUnaryExpression(UnaryExpressionSyntax unary) =>
        unary.Operator.Type switch
        {
            TokenType.OperatorMinus => Expression.Multiply(
                Expression.Constant(-1.0m),
                CompileExpression(unary.Value)
            ),
            TokenType.Not => Expression.Not(CompileExpression(unary.Value)),

            _ => throw new CompileException(
                $"Unknown unary operator: {unary.Operator.Value} at position {unary.Operator.Position}"
            ),
        };

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(decimal))]
    private BinaryExpression CompileBinaryExpression(BinaryExpressionSyntax binary)
    {
        var left = CompileExpression(binary.Left);
        var right = CompileExpression(binary.Right);

        return binary.Operator.Type switch
        {
            TokenType.OperatorPlus => BinaryExpression.Add(left, right),
            TokenType.OperatorMinus => BinaryExpression.Subtract(left, right),
            TokenType.OperatorMultiply => BinaryExpression.Multiply(left, right),
            TokenType.OperatorDivide => BinaryExpression.Divide(left, right),
            TokenType.Equal => BinaryExpression.Equal(left, right),
            TokenType.NotEqual => BinaryExpression.NotEqual(left, right),
            TokenType.GreaterThan => BinaryExpression.GreaterThan(left, right),
            TokenType.LessThan => BinaryExpression.LessThan(left, right),
            TokenType.GreaterThanOrEqual => BinaryExpression.GreaterThanOrEqual(left, right),
            TokenType.LessThanOrEqual => BinaryExpression.LessThanOrEqual(left, right),
            TokenType.And => BinaryExpression.AndAlso(left, right),
            TokenType.Or => BinaryExpression.OrElse(left, right),
            _ => throw new CompileException(
                $"Unknown unary operator: {binary.Operator.Value} at position {binary.Operator.Position}"
            ),
        };
    }

    private Expression CompileExpression(ExpressionSyntax node) =>
        node switch
        {
            NumberExpressionSyntax numberExpression => CompileNumberExpression(
                numberExpression
            ),
            BoolExpressionSyntax boolExpression => CompileBooleanExpression(boolExpression),
            StringExpressionSyntax stringExpression => CompileStringExpression(
                stringExpression
            ),
            IdentifierExpressionSyntax identifierExpression => CompileIdentifierExpression(
                identifierExpression
            ),
            UnaryExpressionSyntax unaryExpression => CompileUnaryExpression(unaryExpression),
            BinaryExpressionSyntax binaryExpression => CompileBinaryExpression(
                binaryExpression
            ),
            _ => throw new CompileException($"Unknown AST node: {node}"),
        };

    public Expression<Func<T, bool>> Compile()
    {
        var body = CompileExpression(Ast);

        if (body.Type != typeof(bool))
        {
            throw new CompileException(
                $"Predicate must have type of bool, but has type of {body.Type.Name}"
            );
        }

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}
