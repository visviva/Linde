namespace LINQ_ExpressionCompiler;

internal abstract record ExpressionSyntax;

internal sealed record NumberExpressionSyntax(decimal Value, Token Token) : ExpressionSyntax;

internal sealed record BoolExpressionSyntax(bool Value, Token Token) : ExpressionSyntax;

internal sealed record StringExpressionSyntax(string Value, Token Token) : ExpressionSyntax;

internal sealed record IdentifierExpressionSyntax(string Identifier, Token Token)
    : ExpressionSyntax;

internal sealed record UnaryExpressionSyntax(Token Operator, ExpressionSyntax Value)
    : ExpressionSyntax;

internal sealed record BinaryExpressionSyntax(
    ExpressionSyntax Left,
    Token Operator,
    ExpressionSyntax Right
) : ExpressionSyntax;
