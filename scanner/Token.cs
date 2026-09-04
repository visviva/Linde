namespace Linde.Scanner;

internal sealed record class Token(TokenType Type, string Value, int Position)
{
    public override string ToString() =>
        $"Token(Type: {Type}, Value: '{Value}', Position: {Position})";
}
