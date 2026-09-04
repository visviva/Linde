namespace Linde.Syntax;

internal sealed class Parser(IReadOnlyList<SyntaxToken> tokens)
{
    private int position;

    private SyntaxToken Current => tokens[position];
    private SyntaxToken Previous => tokens[position - 1];

    private void Consume()
    {
        if (position < tokens.Count)
        {
            position++;
        }
    }

    private bool Match(params ReadOnlySpan<SyntaxKind> tokenTypes)
    {
        if (!tokenTypes.Contains(Current.Kind))
        {
            return false;
        }

        Consume();
        return true;
    }

    private SyntaxToken Expect(SyntaxKind tokenType)
    {
        if (Current.Kind != tokenType)
        {
            throw new ParserException(Current, tokenType);
        }

        var token = Current;

        Consume();

        return token;
    }

    private NumericLiteralExpressionSyntax ParseNumericLiteralExpression()
    {
        var token = Expect(SyntaxKind.NumberToken);
        return new NumericLiteralExpressionSyntax(decimal.Parse(token.Text), token);
    }

    private NameExpressionSyntax ParseNameExpression()
    {
        var token = Expect(SyntaxKind.IdentifierToken);
        return new NameExpressionSyntax(token.Text, token);
    }

    private BooleanLiteralExpressionSyntax ParseBooleanLiteralExpression()
    {
        if (Match(SyntaxKind.TrueKeyword, SyntaxKind.FalseKeyword))
        {
            return new BooleanLiteralExpressionSyntax(
                Previous.Kind == SyntaxKind.TrueKeyword ? true : false,
                Previous
            );
        }

        throw new ParserException(Previous, SyntaxKind.TrueKeyword, SyntaxKind.FalseKeyword);
    }

    private StringLiteralExpressionSyntax ParseStringLiteralExpression()
    {
        var token = Expect(SyntaxKind.StringToken);
        return new StringLiteralExpressionSyntax(token.Text, token);
    }

    private ExpressionSyntax ParseParenthesizedExpression()
    {
        Expect(SyntaxKind.OpenParenthesisToken);
        var expression = ParseExpression();
        Expect(SyntaxKind.CloseParenthesisToken);
        return expression;
    }

    private ExpressionSyntax ParsePrimaryExpression() =>
        Current.Kind switch
        {
            SyntaxKind.NumberToken => ParseNumericLiteralExpression(),
            SyntaxKind.IdentifierToken => ParseNameExpression(),
            SyntaxKind.StringToken => ParseStringLiteralExpression(),
            SyntaxKind.TrueKeyword => ParseBooleanLiteralExpression(),
            SyntaxKind.FalseKeyword => ParseBooleanLiteralExpression(),
            SyntaxKind.OpenParenthesisToken => ParseParenthesizedExpression(),
            _ => throw new ParserException(Current),
        };

    private ExpressionSyntax ParseUnaryExpression()
    {
        if (Match(SyntaxKind.MinusToken, SyntaxKind.BangToken))
        {
            var op = Previous;
            return new PrefixUnaryExpressionSyntax(op, ParseUnaryExpression());
        }

        return ParsePrimaryExpression();
    }

    private ExpressionSyntax ParseMultiplicativeExpression()
    {
        var left = ParseUnaryExpression();

        while (Match(SyntaxKind.StarToken, SyntaxKind.SlashToken))
        {
            var op = Previous;
            var right = ParseUnaryExpression();
            left = new BinaryExpressionSyntax(left, op, right);
        }

        return left;
    }

    private ExpressionSyntax ParseAdditiveExpression()
    {
        var left = ParseMultiplicativeExpression();

        while (Match(SyntaxKind.PlusToken, SyntaxKind.MinusToken))
        {
            var op = Previous;
            var right = ParseMultiplicativeExpression();
            left = new BinaryExpressionSyntax(left, op, right);
        }

        return left;
    }

    private ExpressionSyntax ParseRelationalExpression()
    {
        var left = ParseAdditiveExpression();

        if (
            Match(
                SyntaxKind.LessThanToken,
                SyntaxKind.GreaterThanToken,
                SyntaxKind.LessThanOrEqualsToken,
                SyntaxKind.GreaterThanOrEqualsToken
            )
        )
        {
            var op = Previous;
            var right = ParseAdditiveExpression();
            left = new BinaryExpressionSyntax(left, op, right);
        }

        return left;
    }

    private ExpressionSyntax ParseEqualityExpression()
    {
        var left = ParseRelationalExpression();

        if (Match(SyntaxKind.EqualEqualToken, SyntaxKind.BangEqualToken))
        {
            var op = Previous;
            var right = ParseRelationalExpression();
            left = new BinaryExpressionSyntax(left, op, right);
        }

        return left;
    }

    private ExpressionSyntax ParseLogicalAndExpression()
    {
        var left = ParseEqualityExpression();

        while (Match(SyntaxKind.AmpersandAmpersandToken))
        {
            var op = Previous;
            var right = ParseEqualityExpression();
            left = new BinaryExpressionSyntax(left, op, right);
        }

        return left;
    }

    private ExpressionSyntax ParseLogicalOrExpression()
    {
        var left = ParseLogicalAndExpression();

        while (Match(SyntaxKind.PipePipeToken))
        {
            var op = Previous;
            var right = ParseLogicalAndExpression();
            left = new BinaryExpressionSyntax(left, op, right);
        }

        return left;
    }

    private ExpressionSyntax ParseExpression() => ParseLogicalOrExpression();

    public ExpressionSyntax Parse()
    {
        var expr = ParseExpression();
        Expect(SyntaxKind.EndOfInputToken);
        return expr;
    }
}
