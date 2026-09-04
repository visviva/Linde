namespace Linde.Binder;

[Serializable]
internal sealed class BinderException(string? message) : Exception(message) { }
