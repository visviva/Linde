using System.Diagnostics.CodeAnalysis;
using Linde.Syntax;

namespace Linde;

internal sealed class PredicateCompiler<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T
>(bool printTokens = false, bool printAst = false, bool printExpression = false)
{
    public Func<T, bool> CompileExpression(string predicateExpression)
    {
        var scanner = new Syntax.Lexer(predicateExpression);
        var tokens = scanner.Scan().ToList();

        if (printTokens)
        {
            Console.WriteLine("Tokens:\n");
            foreach (var token in tokens)
            {
                Console.WriteLine(token);
            }
            Console.WriteLine();
        }

        var parser = new Parser(tokens);
        var ast = parser.Parse();

        if (printAst)
        {
            var astPrinter = new SyntaxTreePrinter(ast);
            Console.WriteLine("Abstract Syntax Tree:\n");
            astPrinter.Print();
            Console.WriteLine();
        }

        var compiler = new Binder.Binder<T>(ast);
        var expression = compiler.Compile();

        if (printExpression)
        {
            Console.WriteLine($"Compiled Expression: {expression}\n");
        }

        return expression.Compile();
    }
}
