using LINQ_ExpressionCompiler;

var scanner = new Scanner(
    """    
(    
    price > 100 and 
    price >= 101 and 
    price < 1000 and 
    price <= 999 and 
    category == "Books" 
    and category != "Games"
) 
or 
inStock == true

"""
);

var tokens = scanner.Scan().ToList();

Console.WriteLine("Tokens:");
foreach (var token in tokens)
    Console.WriteLine($"{token}");
Console.WriteLine('\n');

var parser = new Parser(tokens);
var ast = parser.Parse();

Console.WriteLine("AST:");
var printer = new AstPrinter(ast);
printer.Print();
