## 1. The problem

Imagine we're building an application that exposes products:

```csharp
public sealed record Product(
    string Name,
    string Category,
    decimal Price,
    bool InStock);
```

Internally, filtering is easy:

```csharp
var products = db.Products
    .Where(p => p.Price > 100 && p.InStock);
```

But now we have a requirement:

> Users should be able to define their own filters at runtime.

For example, perhaps we're building an admin interface, reporting system, automation engine, or saved-search feature.

We want users to be able to express:

```text
price > 100 and inStock == true
```

The important constraint is that **we don't know this condition when we compile our application**.

We somehow need to turn:

```text
"price > 100 and inStock == true"
```

into:

```csharp
p => p.Price > 100 && p.InStock == true
```

And ideally we want the result to be:

```csharp
Expression<Func<Product, bool>>
```

so it can be passed to:

```csharp
db.Products.Where(predicate);
```

That's our actual problem.

---

## 2. The obvious solution

Before introducing a language, I'd explore the straightforward approach.

Maybe the API doesn't accept a string at all. We could define a filter model:

```csharp
public sealed record ProductFilter(
    decimal? MinimumPrice,
    decimal? MaximumPrice,
    string? Category,
    bool? InStock);
```

Then construct the query:

```csharp
IQueryable<Product> ApplyFilter(
    IQueryable<Product> query,
    ProductFilter filter)
{
    if (filter.MinimumPrice is not null)
        query = query.Where(p => p.Price >= filter.MinimumPrice);

    if (filter.MaximumPrice is not null)
        query = query.Where(p => p.Price <= filter.MaximumPrice);

    if (filter.Category is not null)
        query = query.Where(p => p.Category == filter.Category);

    if (filter.InStock is not null)
        query = query.Where(p => p.InStock == filter.InStock);

    return query;
}
```

For many applications, **this is actually the correct solution**.

That's worth saying explicitly in the article. Don't invent a programming language when four optional properties solve your problem.

But suppose the requirements grow.

Users want:

```text
price > 100 and category == "Books"
```

Then:

```text
price > 100 and
(category == "Books" or category == "Games")
```

Then:

```text
(price < 20 or price > 100)
and inStock == true
```

Our simple filter object starts struggling because users aren't merely specifying **values** anymore.

They're specifying **logic**.

That's the transition point.

---

## 3. A possible solution: build the expression dynamically

C# already lets us represent code as data through expression trees.

Instead of writing:

```csharp
p => p.Price > 100
```

we can construct it:

```csharp
var parameter = Expression.Parameter(
    typeof(Product),
    "p");

var property = Expression.Property(
    parameter,
    nameof(Product.Price));

var value = Expression.Constant(100m);

var comparison = Expression.GreaterThan(
    property,
    value);

var predicate =
    Expression.Lambda<Func<Product, bool>>(
        comparison,
        parameter);
```

And we get:

```csharp
Expression<Func<Product, bool>>
```

representing approximately:

```csharp
p => p.Price > 100
```

Great.

So perhaps we can take:

```text
price > 100
```

split it into three pieces:

```text
price
>
100
```

and generate the corresponding expression tree.

Something crude like:

```csharp
var parts = input.Split(' ');

var propertyName = parts[0];
var op = parts[1];
var value = parts[2];
```

For our example, it works.

Then someone enters:

```text
price > 100 and inStock == true
```

Okay, perhaps we split on `and`.

Then:

```text
price > 100 and
(category == "Books" or category == "Games")
```

Now things become considerably less pleasant.

And what about:

```text
name == "War and Peace"
```

Splitting strings is starting to look suspicious.

---

## 4. We've accidentally invented a language

This is the point where I'd reveal what the problem really is.

Our users are writing:

```text
price > 100 and
(category == "Books" or category == "Games")
```

That isn't merely a string.

It has **syntax**.

It has literals:

```text
100
"Books"
true
```

Identifiers:

```text
price
category
inStock
```

Operators:

```text
>
==
and
or
```

And grouping:

```text
(...)
```

More importantly, it has rules.

For example:

```text
price > 100 and inStock == true
```

means:

```text
(price > 100) and (inStock == true)
```

Our application needs to understand that structure before it can generate an expression tree.

In other words:

> We don't have a string-processing problem anymore. We have a parsing problem.

---

# 5. The actual solution

Now the architecture of the article emerges naturally.

We're going to build a tiny query language.

The complete pipeline will be:

```text
SOURCE

price > 100 and category == "Books"

             ↓

LEXER

Identifier("price")
GreaterThan
Number("100")
And
Identifier("category")
Equal
String("Books")

             ↓

PARSER

              And
             /   \
            >     ==
           / \    / \
      price  100 category "Books"

             ↓

COMPILER

Expression<Func<Product, bool>>

             ↓

C#

p => p.Price > 100 &&
     p.Category == "Books"

             ↓

LINQ

db.Products.Where(predicate)
```

And I think **this should be the central idea of the article**.

We're not building a parser just to learn parsing.

We're building a miniature compiler whose target language happens to be **LINQ expression trees**.

---

## 6. Define our goal before writing the parser

Then I'd establish a small end-to-end API.

By the end, we want this:

```csharp
var predicate = QueryCompiler.Compile<Product>(
    """
    price > 100 and category == "Books"
    """);
```

to produce the equivalent of:

```csharp
Expression<Func<Product, bool>> predicate =
    p => p.Price > 100 &&
         p.Category == "Books";
```

which means we can do:

```csharp
var result = db.Products.Where(predicate);
```

The query language initially supports only:

```text
price > 100
price >= 100
price == 100
category == "Books"
inStock == true

price > 100 and inStock == true

category == "Books" or category == "Games"

price > 100 and
(category == "Books" or category == "Games")
```

No functions. No arithmetic. No arrays. No null handling. No fancy syntax.

That's enough.

---

## 7. Then we can finally start programming

I'd make the implementation itself three relatively clean layers:

```text
             QueryCompiler
                   │
          "price > 100..."
                   │
        ┌──────────▼──────────┐
        │        Lexer        │
        │                     │
        │ text → tokens       │
        └──────────┬──────────┘
                   │
              List<Token>
                   │
        ┌──────────▼──────────┐
        │        Parser       │
        │                     │
        │ tokens → AST        │
        └──────────┬──────────┘
                   │
              Expression
                   │
        ┌──────────▼──────────┐
        │      Compiler       │
        │                     │
        │ AST → LINQ tree     │
        └──────────┬──────────┘
                   │
                   ▼
       Expression<Func<T,bool>>
```

There's also a nice conceptual distinction you can emphasize throughout the post:

**Lexer:** What are the words?

**Parser:** What do the words mean structurally?

**Compiler:** How do we turn that structure into something executable?

For example:

```text
price > 100
```

Lexer:

```text
Identifier  "price"
GreaterThan ">"
Number      "100"
```

Parser:

```text
ComparisonExpression
├── Left: IdentifierExpression("price")
├── Operator: GreaterThan
└── Right: NumberExpression(100)
```

Compiler:

```csharp
p => p.Price > 100m
```

That gives you a very natural point to begin the technical part of the article: **define `TokenKind` and write the lexer**.

And I'd deliberately *not* mention grammars, recursive descent, precedence, visitors, etc. too early. Let each compiler concept appear because the previous naive solution runs into a concrete problem. That progression is what can make this article stand out from a standard parser tutorial.
