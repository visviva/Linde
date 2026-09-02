namespace Linde.Compiler;

[Serializable]
internal class CompileException(string? message) : Exception(message) { }
