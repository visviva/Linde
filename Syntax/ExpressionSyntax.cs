namespace Linde.Syntax;

internal abstract record ExpressionSyntax;

internal sealed record NumericLiteralExpressionSyntax(decimal Value, SyntaxToken Token) : ExpressionSyntax;

internal sealed record BooleanLiteralExpressionSyntax(bool Value, SyntaxToken Token) : ExpressionSyntax;

internal sealed record StringLiteralExpressionSyntax(string Value, SyntaxToken Token) : ExpressionSyntax;

internal sealed record NameExpressionSyntax(string Identifier, SyntaxToken Token) : ExpressionSyntax;

internal sealed record PrefixUnaryExpressionSyntax(SyntaxToken OperatorToken, ExpressionSyntax Operand)
    : ExpressionSyntax;

internal sealed record BinaryExpressionSyntax(
    ExpressionSyntax Left,
    SyntaxToken OperatorToken,
    ExpressionSyntax Right
) : ExpressionSyntax;
