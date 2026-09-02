using System.Diagnostics.CodeAnalysis;
using System.Reflection;
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

var products = new[]
{
    new Product("C# in Depth", "Books", 120m, true),
    new Product("Chess Board", "Games", 80m, true),
    new Product("Rare Book", "Books", 250m, false),
};

Dictionary<string, PropertyInfo> Create<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T
>()
{
    return typeof(T)
        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
        .Where(property =>
            property.GetMethod is not null
            && !property.GetMethod.IsStatic
            && property.GetIndexParameters().Length == 0
        )
        .ToDictionary(
            property => property.Name,
            property => property,
            StringComparer.OrdinalIgnoreCase
        );
}

var properties = Create<Product>();

foreach (var product in products)
{
    Console.WriteLine($"Product: {product.Name}");

    foreach (var (name, property) in properties)
    {
        var value = property.GetValue(product);
        Console.WriteLine($"  {name}: {value}");
    }
}
