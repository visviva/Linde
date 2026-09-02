using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace LINQ_ExpressionCompiler
{
    internal class Compiler<
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

        private static ConstantExpression CompileNumber(NumberExpressionSyntax number) =>
            Expression.Constant(number.Value);

        private static ConstantExpression CompileBool(BoolExpressionSyntax boolean) =>
            Expression.Constant(boolean.Value);

        private static ConstantExpression CompileString(StringExpressionSyntax literal) =>
            Expression.Constant(literal.Value);

        private MemberExpression CompileIdentifier(IdentifierExpressionSyntax identifier)
        {
            var property = properties[identifier.Identifier];
            return MemberExpression.Property(parameter, property);
        }

        private Expression CompileUnary(UnaryExpressionSyntax unary) =>
            unary.Operator.Type switch
            {
                TokenType.OperatorMinus => Expression.Multiply(
                    Expression.Constant(-1.0m),
                    CompileExpression(unary.Value)
                ),
                TokenType.Not => Expression.Negate(CompileExpression(unary.Value)),

                _ => throw new CompileException(
                    $"Unknown unary operator: {unary.Operator.Value} at position {unary.Operator.Position}"
                ),
            };

        private Expression CompileBinary(BinaryExpressionSyntax binary)
        {
            var left = CompileExpression(binary.Left);
            var right = CompileExpression(binary.Right);

            return binary.Operator.Type switch
            {
                TokenType.OperatorPlus => Expression.Add(left, right),
                TokenType.OperatorMinus => Expression.Subtract(left, right),
                TokenType.OperatorMultiply => Expression.Multiply(left, right),
                TokenType.OperatorDivide => Expression.Divide(left, right),
                TokenType.EqualEqual => Expression.Equal(left, right),
                TokenType.NotEqual => Expression.NotEqual(left, right),
                TokenType.GreaterThan => Expression.GreaterThan(left, right),
                TokenType.LessThan => Expression.LessThan(left, right),
                TokenType.GreaterThanOrEqual => Expression.GreaterThanOrEqual(left, right),
                TokenType.LessThanOrEqual => Expression.LessThanOrEqual(left, right),
                TokenType.And => Expression.And(left, right),
                TokenType.Or => Expression.Or(left, right),
                _ => throw new CompileException(
                    $"Unknown unary operator: {binary.Operator.Value} at position {binary.Operator.Position}"
                ),
            };
        }

        private Expression CompileExpression(ExpressionSyntax node) =>
            node switch
            {
                NumberExpressionSyntax numberExpression => CompileNumber(numberExpression),
                BoolExpressionSyntax boolExpression => CompileBool(boolExpression),
                StringExpressionSyntax stringExpression => CompileString(stringExpression),
                IdentifierExpressionSyntax identifierExpression => CompileIdentifier(
                    identifierExpression
                ),
                UnaryExpressionSyntax unaryExpression => CompileUnary(unaryExpression),
                BinaryExpressionSyntax binaryExpression => CompileBinary(binaryExpression),
                _ => throw new CompileException($"Unknown AST node: {node}"),
            };

        public Expression<Func<T, bool>> Compile() =>
            Expression.Lambda<Func<T, bool>>(CompileExpression(Ast), parameter);
    }
}
