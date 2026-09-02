Start with this example model:

public sealed record Product( string Name, string Category, decimal Price, bool InStock);

For experimentation, use some in-memory data:

```csharp
var products = new[]
{
    new Product("C# in Depth", "Books", 120m, true),
    new Product("Chess Board", "Games", 80m, true),
    new Product("Rare Book", "Books", 250m, false),
};
```

## The identifier mapping

Keep the query-language names separate from the C# property names:

```
 Query identifier    C# property
━━━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━━━
 name                Product.Name
──────────────────  ──────────────────
 category            Product.Category
──────────────────  ──────────────────
 price               Product.Price
──────────────────  ──────────────────
 inStock             Product.InStock
```

The compiler can represent this mapping using PropertyInfo:

```csharp
var properties = new Dictionary<string, PropertyInfo>(
    StringComparer.OrdinalIgnoreCase)
{
    ["name"] = typeof(Product).GetProperty(nameof(Product.Name))!,
    ["category"] = typeof(Product).GetProperty(nameof(Product.Category))!,
    ["price"] = typeof(Product).GetProperty(nameof(Product.Price))!,
    ["inStock"] = typeof(Product).GetProperty(nameof(Product.InStock))!,
};
```

An explicit mapping is useful because your query-language names don’t have to be identical to your
C# names. It also controls which properties users are allowed to query.

## Turning an AST identifier into an expression

At the beginning of compilation, create one parameter:

```csharp
var parameter = Expression.Parameter(typeof(Product), "p");
```

This represents the p in:

```csharp
p => ...
```

When your compiler encounters:

IdentifierExpressionSyntax("price", token)

it should:

1. Look up "price" in the property map.
2. Obtain the PropertyInfo for Product.Price.
3. Create property access using the shared parameter:

Expression.Property(parameter, propertyInfo)

That expression represents:

```csharp
p.Price
```

Its Type will be decimal, because Product.Price is a decimal.

The important point is that the AST identifier does not become another parameter. It becomes
property access on the existing parameter.

```
Identifier "price"
        │
        │ look up
        ▼
PropertyInfo for Product.Price
        │
        │ combine with parameter p
        ▼
Expression representing p.Price
```

## The first expression to compile

For this input:

```csharp
price > 100
```

your parser produces approximately:

```
BinaryExpressionSyntax
├── IdentifierExpressionSyntax("price")
└── NumberExpressionSyntax(100)
```

Compile it bottom-up:

```
Identifier "price" → p.Price
Number 100         → constant 100m
Operator >         → GreaterThan(p.Price, 100m)
```

Finally, combine the body with the original parameter:

```
Lambda
├── Parameter: p
└── Body: p.Price > 100m
```

The result is equivalent to:

```csharp
Expression<Func<Product, bool>> predicate =
    p => p.Price > 100m;
```

## What your compiler needs to retain

Your compiler will need two important pieces of context:

ParameterExpression: The single p used throughout the expression.

Identifier/property map: Maps "price" to Product.Price, etc.

Do not create a new parameter while compiling each identifier. These are distinct expression-tree
objects even when they both have the name "p". Every property access and the final lambda must use
the same ParameterExpression instance.

## Your next implementation exercise

Implement only these three AST cases initially:

- IdentifierExpressionSyntax
- NumberExpressionSyntax
- BinaryExpressionSyntax with >

Then try:

```csharp
price > 100
```

Ignore strings, Booleans, logical operators, and generics until that produces the correct
expression. Afterward, category == "Books" is the natural second case.

One trap to avoid: don’t map identifiers to Func<Product, object> delegates. A delegate is
executable code, while you need an expression-tree node that an IQueryable provider can inspect and
translate. PropertyInfo is a clean starting point.

```

```
