namespace Linde.Scanner;

internal enum TokenType
{
    Unknown,
    Number,
    OperatorPlus,
    OperatorMinus,
    OperatorMultiply,
    OperatorDivide,
    Equal,
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
    True,
    False,
    EndOfInput,
}
