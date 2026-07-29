namespace Windvale.Bytecode;

public abstract class Bytecodeˉexception : Exception
{
    protected Bytecodeˉexception(string code, string message, int? byteˉoffset = null)
        : base(byteˉoffset is null ? $"{code}: {message}" : $"{code} at byte {byteˉoffset}: {message}")
    {
        Code = code;
        Byteˉoffset = byteˉoffset;
    }

    public string Code { get; }

    public int? Byteˉoffset { get; }
}

public sealed class Moduleˉformatˉexception : Bytecodeˉexception
{
    public Moduleˉformatˉexception(string code, string message, int? byteˉoffset = null)
        : base(code, message, byteˉoffset)
    {
    }
}

public sealed class Moduleˉverificationˉexception : Bytecodeˉexception
{
    public Moduleˉverificationˉexception(string code, string message, int? byteˉoffset = null)
        : base(code, message, byteˉoffset)
    {
    }
}
