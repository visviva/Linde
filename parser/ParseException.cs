using Linde.Scanner;

namespace Linde.Parser;

[Serializable]
internal sealed class ParseException : Exception
{
    public Token Token { get; }

    public ParseException(Token token)
        : base($"Unexpected token: {token} at position: {token.Position}")
    {
        Token = token;
    }

    public ParseException(Token token, TokenType expected)
        : base($"Unexpected token: {token} at position: {token.Position}, expected {expected}")
    {
        Token = token;
    }
}
