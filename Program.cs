using LINQ_ExpressionCompiler;

var scanner = new Scanner(" hello world 123 \"sadfasdf\" || && = - * /");

var tokens = scanner.Scan().ToList();

foreach (var token in tokens)
    Console.WriteLine($"{token}");
