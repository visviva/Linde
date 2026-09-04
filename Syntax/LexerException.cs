namespace Linde.Syntax;

[Serializable]
internal sealed class LexerException(string? message) : Exception(message) { }
