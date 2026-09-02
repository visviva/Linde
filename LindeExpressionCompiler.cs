using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Linde.Compiler;
using Linde.Parser;
using Linde.Scanner;

namespace Linde
{
    internal class LindeExpressionCompiler<
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

            // Check for top level node being a predicate
            TokenType[] predicateOperators =
            [
                TokenType.And,
                TokenType.Or,
                TokenType.LessThan,
                TokenType.GreaterThan,
                TokenType.GreaterThanOrEqual,
                TokenType.LessThanOrEqual,
            ];

            var isPredicate = ast switch
            {
                BoolExpressionSyntax => true,
                UnaryExpressionSyntax unaryExpression => unaryExpression.Operator.Type
                    == TokenType.Not,
                BinaryExpressionSyntax binaryExpression => predicateOperators.Contains(
                    binaryExpression.Operator.Type
                ),
                _ => false,
            };

            if (!isPredicate)
            {
                throw new CompileException($"Expression must be a boolean predicate");
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
}
