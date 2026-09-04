namespace Linde.Compiler;

[Serializable]
internal sealed class CompileException(string? message) : Exception(message) { }
