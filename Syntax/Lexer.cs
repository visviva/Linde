namespace Linde.Syntax;

internal sealed class Lexer(string text)
{
    public string Text { get; } = text;

    private int position = 0;

    private bool IsAtEnd => position >= Text.Length;

    private char Peek(int lookAhead) =>
        position + lookAhead < Text.Length ? Text[position + lookAhead] : '\0';

    private char Current => Peek(0);
    private char Next => Peek(1);

    private void Advance() => position++;

    private SyntaxToken ReadToken(SyntaxKind tokenType, int length = 1)
    {
        var start = position;
        position += length;
        return new SyntaxToken(tokenType, Text[start..position], start);
    }

    private SyntaxToken ReadCompoundToken(
        char secondCharacter,
        SyntaxKind singleType,
        SyntaxKind compoundType
    )
    {
        if (Next == secondCharacter)
        {
            return ReadToken(compoundType, 2);
        }
        return ReadToken(singleType, 1);
    }

    private SyntaxToken ReadString()
    {
        var start = position;
        Advance(); // Skip the opening quote

        var startOfString = position;

        while (Current != '"')
        {
            if (IsAtEnd)
            {
                throw new LexerException($"Unterminated string literal at position {IsAtEnd}");
            }
            Advance();
        }

        var endOfString = position;

        Advance(); // Skip the closing quote

        return new SyntaxToken(SyntaxKind.StringToken, Text[startOfString..endOfString], start);
    }

    private SyntaxToken ReadIdentifier()
    {
        var start = position;

        while ((char.IsLetter(Current) || char.IsDigit(Current) || Current == '_'))
        {
            Advance();
        }

        var identifier = Text[start..position];

        return new SyntaxToken(SyntaxKind.IdentifierToken, identifier, start);
    }

    private static SyntaxToken TransformToReservedKeywordOrKeep(SyntaxToken token) =>
        token.Text.ToUpperInvariant() switch
        {
            "AND" => token with { Kind = SyntaxKind.AmpersandAmpersandToken },
            "OR" => token with { Kind = SyntaxKind.PipePipeToken },
            "NOT" => token with { Kind = SyntaxKind.BangToken },
            "TRUE" => token with { Kind = SyntaxKind.TrueKeyword },
            "FALSE" => token with { Kind = SyntaxKind.FalseKeyword },
            "IS" => token with { Kind = SyntaxKind.EqualEqualToken },
            _ => token,
        };

    private SyntaxToken ReadNumber()
    {
        var start = position;

        while (char.IsDigit(Current))
        {
            Advance();
        }

        return new SyntaxToken(SyntaxKind.NumberToken, Text[start..position], start);
    }

    private void SkipWhitespace()
    {
        while (char.IsWhiteSpace(Current))
        {
            Advance();
        }
    }

    private SyntaxToken NextToken()
    {
        SkipWhitespace();

        if (IsAtEnd)
        {
            return new SyntaxToken(SyntaxKind.EndOfInputToken, string.Empty, position);
        }

        return Current switch
        {
            '+' => ReadToken(SyntaxKind.PlusToken),
            '-' => ReadToken(SyntaxKind.MinusToken),
            '*' => ReadToken(SyntaxKind.StarToken),
            '/' => ReadToken(SyntaxKind.SlashToken),

            '(' => ReadToken(SyntaxKind.OpenParenthesisToken),
            ')' => ReadToken(SyntaxKind.CloseParenthesisToken),

            '=' => ReadCompoundToken('=', SyntaxKind.BadToken, SyntaxKind.EqualEqualToken),
            '!' => ReadCompoundToken('=', SyntaxKind.BangToken, SyntaxKind.BangEqualToken),
            '<' => ReadCompoundToken('=', SyntaxKind.LessThanToken, SyntaxKind.LessThanOrEqualsToken),
            '>' => ReadCompoundToken('=', SyntaxKind.GreaterThanToken, SyntaxKind.GreaterThanOrEqualsToken),

            '&' when Next is '&' => ReadToken(SyntaxKind.AmpersandAmpersandToken, 2),
            '|' when Next is '|' => ReadToken(SyntaxKind.PipePipeToken, 2),

            '"' => ReadString(),

            var c when char.IsDigit(c) => ReadNumber(),
            var c when char.IsLetter(c) || c is '_' => TransformToReservedKeywordOrKeep(ReadIdentifier()),

            _ => ReadToken(SyntaxKind.BadToken),
        };
    }

    public IEnumerable<SyntaxToken> Scan()
    {
        SyntaxToken token;

        do
        {
            token = NextToken();
            yield return token;
        } while (token.Kind != SyntaxKind.EndOfInputToken);
    }
}
