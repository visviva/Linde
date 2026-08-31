# Grammar Evaluation

I need to find the grammar of the language that is implicitly defined here. Making the grammar
explicit makes writing the parser much more easier.

## Easy Example of Language Usage

```csharp
100 + 200 * 3 != 7
```

The resulting AST will be:

```text
!=
 ├─ 7
 └─ +
    ├─ 100
    └─ *
       ├─ 200
       └─ 3
```

So the resulting precedence of operators according to this example will be:

1. Notequal
1. Sum
1. Product

With the higher the precedence number, the higher priority of the precedence.

## More complicated example

```csharp
-5 <= 7 && true || -price == 5
```

Here the resulting AST is:

```
||
 ├─ &&
 │   ├─ true
 │   └─ <=
 │      ├─ -
 │      │  └─ 5
 │      └─ 7
 └─ ==
     ├─ -
     │  └─ price
     └─ 5
```

So precedence is now:

1. OR
1. AND
1. ==
1. <=
1. unary -

## Complex Example using all Operators

```csharp
(
    price > 100
    and price >= 101
    and price < 1000
    and price <= 999
    and category == "Books"
    and category != "Games"
)
or inStock == true
```

This time with parentheses it is interesting:

```text
or
├─ and
│  ├─ and
│  │  ├─ and
│  │  │  ├─ and
│  │  │  │  ├─ and
│  │  │  │  │  ├─ >
│  │  │  │  │  │  ├─ price
│  │  │  │  │  │  └─ 100
│  │  │  │  │  └─ >=
│  │  │  │  │     ├─ price
│  │  │  │  │     └─ 101
│  │  │  │  └─ <
│  │  │  │     ├─ price
│  │  │  │     └─ 1000
│  │  │  └─ <=
│  │  │     ├─ price
│  │  │     └─ 999
│  │  └─ ==
│  │     ├─ category
│  │     └─ "Books"
│  └─ !=
│     ├─ category
│     └─ "Games"
└─ ==
   ├─ inStock
   └─ true
```

The parentheses make the entire chain of `and` expressions the left operand of `or`. They do not
need their own AST node because their purpose is to control how the tree is built.

## Operator Precedence

From lowest to highest, the operator precedence is:

1. `or`, `||`
1. `and`, `&&`
1. `==`, `!=`
1. `<`, `<=`, `>`, `>=`
1. `+`, `-`
1. `*`, `/`
1. Unary `-`, `not`, `!`

Parentheses override this precedence by forcing the enclosed expression to be parsed first.

## Operator Precedence Rules

To sum up, the operator precedence for all operators, from lowest to highest, is:

1. **Logical OR:** `or`, `||`
1. **Logical AND:** `and`, `&&`
1. **Equality:** `==`, `!=`
1. **Comparison:** `<`, `<=`, `>`, `>=`
1. **Additive:** `+`, `-`
1. **Multiplicative:** `*`, `/`
1. **Unary:** `-`, `not`, `!`

Operators on the same level have equal precedence. Binary operators are left-associative, while
unary operators are right-associative. Parentheses take precedence over these rules and cause the
expression inside them to be parsed as a single operand.

## Full Grammar

```text
Expression -> OR

OR-Condition -> AND-Condition (("||" | "OR") AND-Conditions)*

AND-Condition -> Equality (("&&" | "AND") Equality)*

Equality -> Comparison (("==" | "!=") Comparison)*

Comparison -> Sum (("<" | ">" | "<=" | ">=") Sum)*

Sum -> Product ("+" Product)*

Product -> Unary ("*" Unary) *

Unary -> ("-" | "!" | "NOT") Unary
       | Primary

Primary -> Number
         | Identifier
         | StringLiteral
         | "True"
         | "False"
         | "(" Expression ")"
```
