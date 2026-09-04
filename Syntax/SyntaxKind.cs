namespace Linde.Syntax;

internal enum SyntaxKind
{
    BadToken,
    NumberToken,
    PlusToken,
    MinusToken,
    StarToken,
    SlashToken,
    EqualEqualToken,
    BangEqualToken,
    LessThanToken,
    GreaterThanToken,
    GreaterThanOrEqualsToken,
    LessThanOrEqualsToken,
    OpenParenthesisToken,
    CloseParenthesisToken,
    IdentifierToken,
    StringToken,
    BangToken,
    AmpersandAmpersandToken,
    PipePipeToken,
    TrueKeyword,
    FalseKeyword,
    EndOfInputToken,
}
