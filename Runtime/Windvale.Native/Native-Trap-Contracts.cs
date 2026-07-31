namespace Windvale.Runtime.Native;

public sealed class Nativeˉtrapˉexception(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
