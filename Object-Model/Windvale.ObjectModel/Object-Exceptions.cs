namespace Windvale.ObjectModel;

public abstract class Objectˉexception : Exception
{
    protected Objectˉexception(string code, string message, int? byteˉoffset = null)
        : base(byteˉoffset is null ? $"{code}: {message}" : $"{code} at byte {byteˉoffset}: {message}")
    {
        Code = code;
        Byteˉoffset = byteˉoffset;
    }

    public string Code { get; }

    public int? Byteˉoffset { get; }
}

public sealed class Objectˉformatˉexception : Objectˉexception
{
    public Objectˉformatˉexception(string code, string message, int? byteˉoffset = null)
        : base(code, message, byteˉoffset)
    {
    }
}

public sealed class Objectˉverificationˉexception : Objectˉexception
{
    public Objectˉverificationˉexception(string code, string message, int? byteˉoffset = null)
        : base(code, message, byteˉoffset)
    {
    }
}
