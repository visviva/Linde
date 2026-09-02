namespace LINQ_ExpressionCompiler
{
    [Serializable]
    internal class CompileException : Exception
    {
        public CompileException() { }

        public CompileException(string? message)
            : base(message) { }

        public CompileException(string? message, Exception? innerException)
            : base(message, innerException) { }
    }
}
