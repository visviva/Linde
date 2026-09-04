using Linde.Scanner;

namespace Linde.Parser;

internal sealed class Parser(IReadOnlyList<Token> tokens)
{
    private int position;

    private Token Current => tokens[position];
    private Token Previous => tokens[position - 1];

    private void Consume()
    {
        if (position < tokens.Count)
        {
            position++;
        }
    }

    private bool Match(params ReadOnlySpan<TokenType> tokenTypes)
    {
        if (!tokenTypes.Contains(Current.Type))
        {
            return false;
        }

        Consume();
        return true;
    }

    private Token Expect(TokenType tokenType)
    {
        if (Current.Type != tokenType)
        {
            throw new ParseException(Current, tokenType);
        }

        var token = Current;

        Consume();

        return token;
    }

    private NumberExpressionSyntax ParseNumber()
    {
        var token = Expect(TokenType.Number);
        return new NumberExpressionSyntax(decimal.Parse(token.Value), token);
    }

    private IdentifierExpressionSyntax ParseIdentifier()
    {
        var token = Expect(TokenType.Identifier);
        return new IdentifierExpressionSyntax(token.Value, token);
    }

    private BoolExpressionSyntax ParseBooleanFalse()
    {
        var token = Expect(TokenType.False);
        return new BoolExpressionSyntax(false, token);
    }

    private BoolExpressionSyntax ParseBooleanTrue()
    {
        var token = Expect(TokenType.True);
        return new BoolExpressionSyntax(true, token);
    }

    private StringExpressionSyntax ParseStringLiteral()
    {
        var token = Expect(TokenType.StringLiteral);
        return new StringExpressionSyntax(token.Value, token);
    }

    private ExpressionSyntax ParseParenthesizedExpression()
    {
        Expect(TokenType.ParenthesisOpen);
        var expression = ParseExpression();
        Expect(TokenType.ParenthesisClose);
        return expression;
    }

    private ExpressionSyntax ParsePrimary() =>
        Current.Type switch
        {
            TokenType.Number => ParseNumber(),
            TokenType.Identifier => ParseIdentifier(),
            TokenType.StringLiteral => ParseStringLiteral(),
            TokenType.True => ParseBooleanTrue(),
            TokenType.False => ParseBooleanFalse(),
            TokenType.ParenthesisOpen => ParseParenthesizedExpression(),
            _ => throw new ParseException(Current),
        };

    private ExpressionSyntax ParseUnary()
    {
        if (Match(TokenType.OperatorMinus, TokenType.Not))
        {
            var op = Previous;
            return new UnaryExpressionSyntax(op, ParseUnary());
        }

        return ParsePrimary();
    }

    private ExpressionSyntax ParseProduct()
    {
        var left = ParseUnary();

        while (Match(TokenType.OperatorMultiply, TokenType.OperatorDivide))
        {
            var op = Previous;
            var right = ParseUnary();
            left = new BinaryExpressionSyntax(left, op, right);
        }

        return left;
    }

    private ExpressionSyntax ParseSum()
    {
        var left = ParseProduct();

        while (Match(TokenType.OperatorPlus, TokenType.OperatorMinus))
        {
            var op = Previous;
            var right = ParseProduct();
            left = new BinaryExpressionSyntax(left, op, right);
        }

        return left;
    }

    private ExpressionSyntax ParseComparison()
    {
        var left = ParseSum();

        if (
            Match(
                TokenType.LessThan,
                TokenType.GreaterThan,
                TokenType.LessThanOrEqual,
                TokenType.GreaterThanOrEqual
            )
        )
        {
            var op = Previous;
            var right = ParseSum();
            left = new BinaryExpressionSyntax(left, op, right);
        }

        return left;
    }

    private ExpressionSyntax ParseEquality()
    {
        var left = ParseComparison();

        if (Match(TokenType.Equal, TokenType.NotEqual))
        {
            var op = Previous;
            var right = ParseComparison();
            left = new BinaryExpressionSyntax(left, op, right);
        }

        return left;
    }

    private ExpressionSyntax ParseLogicalAnd()
    {
        var left = ParseEquality();

        while (Match(TokenType.And))
        {
            var op = Previous;
            var right = ParseEquality();
            left = new BinaryExpressionSyntax(left, op, right);
        }

        return left;
    }

    private ExpressionSyntax ParseLogicalOr()
    {
        var left = ParseLogicalAnd();

        while (Match(TokenType.Or))
        {
            var op = Previous;
            var right = ParseLogicalAnd();
            left = new BinaryExpressionSyntax(left, op, right);
        }

        return left;
    }

    private ExpressionSyntax ParseExpression() => ParseLogicalOr();

    public ExpressionSyntax Parse()
    {
        var expr = ParseExpression();
        Expect(TokenType.EndOfInput);
        return expr;
    }
}
