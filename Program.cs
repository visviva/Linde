using LINQ_ExpressionCompiler;

var scanner = new Scanner(
    """    
category == "Books" && price > 130
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

var products = new[]
{
    new Product("C# in Depth", "Books", 120m, true),
    new Product("Chess Board", "Games", 80m, true),
    new Product("Rare Book", "Books", 250m, false),
};

var compiler = new Compiler<Product>(ast);
var predicate = compiler.Compile();

Console.WriteLine($"\nPredicate = {predicate.ToString()}\n");

var matchingProducts = products.Where(predicate.Compile());

foreach (var product in matchingProducts)
    Console.WriteLine(product);

//var properties = Create<Product>();

//foreach (var product in products)
//{
//    Console.WriteLine($"Product: {product.Name}");

//    foreach (var (name, property) in properties)
//    {
//        var value = property.GetValue(product);
//        Console.WriteLine($"  {name}: {value}");
//    }
//}
