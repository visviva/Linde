namespace Linde.Syntax;

[Serializable]
internal sealed class ParserException : Exception
{
    public ParserException(SyntaxToken token)
        : base($"Unexpected token: {token} at position: {token.Position}") { }

    public ParserException(SyntaxToken token, SyntaxKind expected)
        : base($"Unexpected token: {token} at position: {token.Position}, expected {expected}") { }

    public ParserException(SyntaxToken token, SyntaxKind expected1, SyntaxKind expected2)
        : base(
            $"Unexpected token: {token} at position: {token.Position}, expected {expected1} or {expected2}"
        ) { }
}
