namespace Linde.Scanner;

internal class Scanner(string text)
{
    public string Text { get; } = text;

    private int position = 0;

    private bool IsAtEnd => position >= Text.Length;

    private char Peek(int lookAhead) =>
        position + lookAhead < Text.Length ? Text[position + lookAhead] : '\0';

    private char Current => Peek(0);
    private char Next => Peek(1);

    private void Advance() => position++;

    private Token ReadToken(TokenType tokenType, int length = 1)
    {
        var start = position;
        position += length;
        return new Token(tokenType, Text[start..position], start);
    }

    private Token ReadCompoundToken(
        char secondCharacter,
        TokenType singleType,
        TokenType compoundType
    )
    {
        if (Next == secondCharacter)
        {
            return ReadToken(compoundType, 2);
        }
        return ReadToken(singleType, 1);
    }

    private Token ReadString()
    {
        var start = position;
        Advance(); // Skip the opening quote

        var startOfString = position;

        while (Current != '"')
        {
            Advance();
        }

        var endOfString = position;

        Advance(); // Skip the closing quote

        return new Token(TokenType.StringLiteral, Text[startOfString..endOfString], start);
    }

    private Token ReadIdentifier()
    {
        var start = position;

        while ((char.IsLetter(Current) || char.IsDigit(Current) || Current == '_'))
        {
            Advance();
        }

        var identifier = Text[start..position];

        return new Token(TokenType.Identifier, identifier, start);
    }

    private static Token TransformToReservedKeywordOrKeep(Token token) =>
        token.Value.ToUpperInvariant() switch
        {
            "AND" => token with { Type = TokenType.And },
            "OR" => token with { Type = TokenType.Or },
            "NOT" => token with { Type = TokenType.Not },
            "TRUE" => token with { Type = TokenType.True },
            "FALSE" => token with { Type = TokenType.False },
            _ => token,
        };

    private Token ReadNumber()
    {
        var start = position;

        while (char.IsDigit(Current))
            Advance();

        return new Token(TokenType.Number, Text[start..position], start);
    }

    private void SkipWhitespace()
    {
        while (char.IsWhiteSpace(Current))
        {
            Advance();
        }
    }

    private Token NextToken()
    {
        SkipWhitespace();

        if (IsAtEnd)
        {
            return new Token(TokenType.EndOfInput, String.Empty, position);
        }

        return Current switch
        {
            '+' => ReadToken(TokenType.OperatorPlus),
            '-' => ReadToken(TokenType.OperatorMinus),
            '*' => ReadToken(TokenType.OperatorMultiply),
            '/' => ReadToken(TokenType.OperatorDivide),

            '(' => ReadToken(TokenType.ParenthesisOpen),
            ')' => ReadToken(TokenType.ParenthesisClose),

            '=' => ReadCompoundToken('=', TokenType.Equal, TokenType.Equal),
            '!' => ReadCompoundToken('=', TokenType.Not, TokenType.NotEqual),
            '<' => ReadCompoundToken('=', TokenType.LessThan, TokenType.LessThanOrEqual),
            '>' => ReadCompoundToken('=', TokenType.GreaterThan, TokenType.GreaterThanOrEqual),

            '&' when Next is '&' => ReadToken(TokenType.And, 2),
            '|' when Next is '|' => ReadToken(TokenType.Or, 2),

            '"' => ReadString(),

            var c when char.IsDigit(c) => ReadNumber(),
            var c when char.IsLetter(c) || c is '_' => TransformToReservedKeywordOrKeep(
                ReadIdentifier()
            ),

            _ => ReadToken(TokenType.Unknown),
        };
    }

    public IEnumerable<Token> Scan()
    {
        Token token;

        do
        {
            token = NextToken();
            yield return token;
        } while (token.Type != TokenType.EndOfInput);
    }
}
