using System.Buffers.Binary;
using System.Collections.Immutable;

namespace Windvale.Bootstrap;

internal sealed class X64ˉcodeˉbuilder
{
    private readonly List<byte> Output = [];
    private readonly Dictionary<string, int> Labels = new(StringComparer.Ordinal);
    private readonly List<Relativeˉfixup> Fixups = [];

    public void Emit(params byte[] bytes) => Output.AddRange(bytes);

    public void Emitˉu32(uint value)
    {
        Span<byte> Bytes = stackalloc byte[sizeof(uint)];
        BinaryPrimitives.WriteUInt32LittleEndian(Bytes, value);
        foreach (var Value in Bytes)
        {
            Output.Add(Value);
        }
    }

    public void Emitˉu64(ulong value)
    {
        Span<byte> Bytes = stackalloc byte[sizeof(ulong)];
        BinaryPrimitives.WriteUInt64LittleEndian(Bytes, value);
        foreach (var Value in Bytes)
        {
            Output.Add(Value);
        }
    }

    public void Mark(string label)
    {
        if (!Labels.TryAdd(label, Output.Count))
        {
            throw new InvalidOperationException($"Duplicate x86-64 bootstrap label '{label}'.");
        }
    }

    public void Jump(string label)
    {
        Output.Add(0xE9);
        Addˉfixup(label);
    }

    public void Jumpˉif(byte conditionˉopcode, string label)
    {
        Output.Add(0x0F);
        Output.Add(conditionˉopcode);
        Addˉfixup(label);
    }

    public uint Emitˉcallˉplaceholder()
    {
        Output.Add(0xE8);
        var Displacementˉoffset = checked((uint)Output.Count);
        Output.AddRange([0, 0, 0, 0]);
        return Displacementˉoffset;
    }

    public ImmutableArray<byte> Build()
    {
        var Result = Output.ToArray();
        foreach (var Fixup in Fixups)
        {
            if (!Labels.TryGetValue(Fixup.Label, out var Target))
            {
                throw new InvalidOperationException($"Undefined x86-64 bootstrap label '{Fixup.Label}'.");
            }

            var Displacement = checked(Target - (Fixup.Displacementˉoffset + sizeof(int)));
            BinaryPrimitives.WriteInt32LittleEndian(
                Result.AsSpan(Fixup.Displacementˉoffset, sizeof(int)),
                Displacement);
        }
        return Result.ToImmutableArray();
    }

    private void Addˉfixup(string label)
    {
        var Offset = Output.Count;
        Output.AddRange([0, 0, 0, 0]);
        Fixups.Add(new(Offset, label));
    }

    private sealed record Relativeˉfixup(
        int Displacementˉoffset,
        string Label);
}
