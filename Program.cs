var products = new[]
{
    new Product("C# in Depth", "Books", 120m, true),
    new Product("Chess Board", "Games", 80m, true),
    new Product("Rare Book", "Books", 250m, false),
    new Product("Clean Code", "Books", 45m, true),
    new Product("Mechanical Keyboard", "Electronics", 150m, true),
    new Product("The Pragmatic Programmer", "Books", 135m, true),
    new Product("Wireless Mouse", "Electronics", 65m, false),
    new Product("Strategy Card Game", "Games", 35m, true),
    new Product("Collector's Encyclopedia", "Books", 180m, false),
    new Product("Standing Desk", "Furniture", 420m, true),
    new Product("Notebook Set", "Stationery", 18m, true),
    new Product("Algorithms Handbook", "Books", 145m, true),
    new Product("Noise-Cancelling Headphones", "Electronics", 275m, false),
};

Console.WriteLine("Collection:");
foreach (var product in products)
{
    Console.WriteLine(product);
}
Console.WriteLine();

var customExpression = "(category is \"Books\" and price > 130) and instock";
Console.WriteLine($"Expression: {customExpression}\n");

var expressionCompiler = new Linde.PredicateCompiler<Product>(
    printTokens: true,
    printAst: true,
    printExpression: true
);

var predicate = expressionCompiler.CompileExpression(customExpression);

var matchingProducts = products.Where(predicate);

Console.WriteLine("Result(s):");
foreach (var product in matchingProducts)
{
    Console.WriteLine(product);
}

public record Product(string Name, string Category, decimal Price, bool InStock);
