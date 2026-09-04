namespace Linde.Parser;

internal sealed class AstPrinter(ExpressionSyntax expression)
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

            case UnaryExpressionSyntax unary:
                PrintBranch(unary.Value, prefix, isLast: true);
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
            NumberExpressionSyntax number => number.Value.ToString(),
            BoolExpressionSyntax boolean => boolean.Value ? "true" : "false",
            StringExpressionSyntax text => $"\"{text.Value}\"",
            IdentifierExpressionSyntax identifier => identifier.Identifier,
            BinaryExpressionSyntax binary => binary.Operator.Value,
            UnaryExpressionSyntax unary => unary.Operator.Value,

            _ => throw new InvalidOperationException(
                $"Unsupported expression syntax: {node.GetType().Name}"
            ),
        };
}
