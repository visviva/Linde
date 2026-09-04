namespace Linde.Scanner;

[Serializable]
internal sealed class ScanException : Exception
{
    public ScanException() { }

    public ScanException(string? message)
        : base(message) { }

    public ScanException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
