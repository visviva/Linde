![Linde](./assets/Linde3.png)

# Linde

Linde is a small expression-language compiler. It turns a text predicate into a strongly typed LINQ
expression and uses that expression to filter an in-memory collection.

## How does it work

Given a predicate such as:

```csharp
(category == "Books" && price > 130) && instock
```

Linde processes it in three stages:

1. The scanner converts the source text into tokens.
2. The parser builds an abstract syntax tree (AST), applying operator precedence.
3. The compiler maps the AST to an `Expression<Func<T, bool>>`.

The sample then compiles that expression into a delegate and passes it to LINQ's `Where` method.
Property lookup is case-insensitive, so `category` resolves to the sample record's `Category`
property.

## Run the example

Install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0), then run:

```console
dotnet run
```

The console output includes the sample collection, tokens, AST, generated LINQ expression, and
matching products. Change `customExpression` in `Program.cs` to experiment with another predicate.

## Supported syntax

The demo recognizes:

- Property identifiers, integer numbers, quoted strings, and `true` / `false`
- Arithmetic operators: `+`, `-`, `*`, `/`
- Comparisons: `==`, `!=`, `<`, `<=`, `>`, `>=`
- Boolean operators: `&&`, `||`, `!` (or `and`, `or`, `not`)
- Parentheses for explicit grouping

Only expressions that produce a boolean predicate can be compiled.

## Project layout

```text
scanner/                    Tokenization
parser/                     Parsing, syntax nodes, and AST printing
compiler/                   LINQ expression-tree generation
LindeExpressionCompiler.cs  Pipeline orchestration
Program.cs                  Runnable product-filtering example
```

## License

Licensed under the [MIT License](LICENSE).
