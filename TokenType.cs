namespace LINQ_ExpressionCompiler;

internal enum TokenType
{
    Unknown,
    Number,
    OperatorPlus,
    OperatorMinus,
    OperatorMultiply,
    OperatorDivide,
    Equal,
    EqualEqual,
    NotEqual,
    LessThan,
    GreaterThan,
    GreaterThanOrEqual,
    LessThanOrEqual,
    ParenthesisOpen,
    ParenthesisClose,
    Identifier,
    StringLiteral,
    Not,
    And,
    Or,
    EndOfInput,
}
