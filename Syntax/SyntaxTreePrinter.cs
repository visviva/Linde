namespace Linde.Syntax;

internal sealed class SyntaxTreePrinter(ExpressionSyntax expression)
{
    public void Print()
    {
        Console.WriteLine(GetLabel(expression));
        PrintChildren(expression, string.Empty);
    }

    private static void PrintChildren(ExpressionSyntax node, string prefix)
    {
        switch (node)
        {
            case BinaryExpressionSyntax binary:
                PrintBranch(binary.Left, prefix, isLast: false);
                PrintBranch(binary.Right, prefix, isLast: true);
                break;

            case PrefixUnaryExpressionSyntax unary:
                PrintBranch(unary.Operand, prefix, isLast: true);
                break;
        }
    }

    private static void PrintBranch(ExpressionSyntax node, string prefix, bool isLast)
    {
        var connector = isLast ? "└── " : "├── ";
        Console.WriteLine($"{prefix}{connector}{GetLabel(node)}");

        var childPrefix = prefix + (isLast ? "    " : "│   ");
        PrintChildren(node, childPrefix);
    }

    private static string GetLabel(ExpressionSyntax node) =>
        node switch
        {
            NumericLiteralExpressionSyntax number => number.Value.ToString(),
            BooleanLiteralExpressionSyntax boolean => boolean.Value ? "true" : "false",
            StringLiteralExpressionSyntax text => $"\"{text.Value}\"",
            NameExpressionSyntax identifier => identifier.Identifier,
            BinaryExpressionSyntax binary => binary.OperatorToken.Text,
            PrefixUnaryExpressionSyntax unary => unary.OperatorToken.Text,

            _ => throw new InvalidOperationException($"Unsupported expression syntax: {node.GetType().Name}"),
        };
}
