using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Linde;

internal sealed class LindeExpressionCompiler<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T
>(bool printTokens = false, bool printAst = false, bool printExpression = false)
{
    public Expression<Func<T, bool>> CompileExpression(string predicateExpression)
    {
        var scanner = new Scanner.Scanner(predicateExpression);
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

        var parser = new Parser.Parser(tokens);
        var ast = parser.Parse();

        if (printAst)
        {
            var astPrinter = new Parser.AstPrinter(ast);
            Console.WriteLine("Abstract Syntax Tree:\n");
            astPrinter.Print();
            Console.WriteLine();
        }

        var compiler = new Compiler.Compiler<T>(ast);
        var expression = compiler.Compile();

        if (printExpression)
        {
            Console.WriteLine($"Compiled Expression: {expression}\n");
        }

        return expression;
    }
}
