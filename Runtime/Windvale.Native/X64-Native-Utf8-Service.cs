using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;

namespace Windvale.Runtime.Native;

public static class X64ˉnativeˉutf8ˉservice
{
    public const int CANONICAL_SIZE = 800;
    public const string CANONICAL_SHA256 =
        "4c3d2e370d62c8d2f54a3c453f39b94cf46ddabd6db3c2f3d6b65f0713b68aaf";

    // ABI-10 supplies a proven immutable range in R8/R9D and a verified bool cell in RCX.
    // This leaf returns status zero in EAX and never changes R10, R11, or R15.
    public static ImmutableArray<byte> Build()
    {
        var Code = new Serviceˉcodeˉbuilder();

        Code.Emit(0xC7, 0x01, 0x00, 0x00, 0x00, 0x00);
        Code.Emit(0x31, 0xC0);

        Code.Mark("loop");
        Code.Emit(0x44, 0x39, 0xC8);
        Code.Branch(0x83, "valid");
        Code.Emit(0x41, 0x0F, 0xB6, 0x14, 0x00);
        Code.Compareˉedx(0x7F);
        Code.Branch(0x86, "one");
        Code.Compareˉedx(0xC2);
        Code.Branch(0x82, "invalid");
        Code.Compareˉedx(0xDF);
        Code.Branch(0x86, "two");
        Code.Compareˉedx(0xE0);
        Code.Branch(0x84, "three_e0");
        Code.Compareˉedx(0xEC);
        Code.Branch(0x86, "three_standard");
        Code.Compareˉedx(0xED);
        Code.Branch(0x84, "three_ed");
        Code.Compareˉedx(0xEF);
        Code.Branch(0x86, "three_standard");
        Code.Compareˉedx(0xF0);
        Code.Branch(0x84, "four_f0");
        Code.Compareˉedx(0xF3);
        Code.Branch(0x86, "four_standard");
        Code.Compareˉedx(0xF4);
        Code.Branch(0x84, "four_f4");
        Code.Jump("invalid");

        Code.Mark("one");
        Code.Emit(0xFF, 0xC0);
        Code.Jump("loop");

        Code.Mark("two");
        Code.Requireˉremaining(2, "invalid");
        Code.Loadˉbyte(1);
        Code.Requireˉcontinuation("invalid");
        Code.Advance(2, "loop");

        Code.Mark("three_e0");
        Code.Requireˉremaining(3, "invalid");
        Code.Loadˉbyte(1);
        Code.Requireˉrange(0xA0, 0xBF, "invalid");
        Code.Loadˉbyte(2);
        Code.Requireˉcontinuation("invalid");
        Code.Advance(3, "loop");

        Code.Mark("three_standard");
        Code.Requireˉremaining(3, "invalid");
        Code.Loadˉbyte(1);
        Code.Requireˉcontinuation("invalid");
        Code.Loadˉbyte(2);
        Code.Requireˉcontinuation("invalid");
        Code.Advance(3, "loop");

        Code.Mark("three_ed");
        Code.Requireˉremaining(3, "invalid");
        Code.Loadˉbyte(1);
        Code.Requireˉrange(0x80, 0x9F, "invalid");
        Code.Loadˉbyte(2);
        Code.Requireˉcontinuation("invalid");
        Code.Advance(3, "loop");

        Code.Mark("four_f0");
        Code.Requireˉremaining(4, "invalid");
        Code.Loadˉbyte(1);
        Code.Requireˉrange(0x90, 0xBF, "invalid");
        Code.Loadˉbyte(2);
        Code.Requireˉcontinuation("invalid");
        Code.Loadˉbyte(3);
        Code.Requireˉcontinuation("invalid");
        Code.Advance(4, "loop");

        Code.Mark("four_standard");
        Code.Requireˉremaining(4, "invalid");
        Code.Loadˉbyte(1);
        Code.Requireˉcontinuation("invalid");
        Code.Loadˉbyte(2);
        Code.Requireˉcontinuation("invalid");
        Code.Loadˉbyte(3);
        Code.Requireˉcontinuation("invalid");
        Code.Advance(4, "loop");

        Code.Mark("four_f4");
        Code.Requireˉremaining(4, "invalid");
        Code.Loadˉbyte(1);
        Code.Requireˉrange(0x80, 0x8F, "invalid");
        Code.Loadˉbyte(2);
        Code.Requireˉcontinuation("invalid");
        Code.Loadˉbyte(3);
        Code.Requireˉcontinuation("invalid");
        Code.Advance(4, "loop");

        Code.Mark("valid");
        Code.Emit(0xC7, 0x01, 0x01, 0x00, 0x00, 0x00);
        Code.Emit(0x31, 0xC0, 0xC3);

        Code.Mark("invalid");
        Code.Emit(0x31, 0xC0, 0xC3);

        return Code.Finish();
    }

    public static void Verify(ReadOnlySpan<byte> code)
    {
        var Hash = Convert.ToHexString(SHA256.HashData(code)).ToLowerInvariant();
        if (code.Length != CANONICAL_SIZE ||
            !StringComparer.Ordinal.Equals(Hash, CANONICAL_SHA256))
        {
            throw new InvalidOperationException(
                $"Native UTF-8 service identity is {code.Length} bytes / {Hash}; " +
                $"expected {CANONICAL_SIZE} bytes / {CANONICAL_SHA256}.");
        }
    }

    private sealed class Serviceˉcodeˉbuilder
    {
        private readonly List<byte> Bytes = [];
        private readonly Dictionary<string, int> Labels = new(StringComparer.Ordinal);
        private readonly List<(int Offset, string Label)> Patches = [];

        public void Emit(params ReadOnlySpan<byte> bytes)
        {
            foreach (var Value in bytes)
            {
                Bytes.Add(Value);
            }
        }

        public void Mark(string label)
        {
            if (!Labels.TryAdd(label, Bytes.Count))
            {
                throw new InvalidOperationException($"Duplicate native UTF-8 service label '{label}'.");
            }
        }

        public void Branch(byte condition, string label)
        {
            Emit(0x0F, condition);
            Patches.Add((Bytes.Count, label));
            Emit(0x00, 0x00, 0x00, 0x00);
        }

        public void Jump(string label)
        {
            Emit(0xE9);
            Patches.Add((Bytes.Count, label));
            Emit(0x00, 0x00, 0x00, 0x00);
        }

        public void Compareˉedx(uint value)
        {
            Emit(0x81, 0xFA);
            Emitˉu32(value);
        }

        public void Requireˉremaining(byte length, string invalidˉlabel)
        {
            Emit(0x89, 0xC2);
            Emit(0x83, 0xC2, length);
            Emit(0x44, 0x39, 0xCA);
            Branch(0x87, invalidˉlabel);
        }

        public void Loadˉbyte(byte offset) =>
            Emit(0x41, 0x0F, 0xB6, 0x54, 0x00, offset);

        public void Requireˉcontinuation(string invalidˉlabel) =>
            Requireˉrange(0x80, 0xBF, invalidˉlabel);

        public void Requireˉrange(uint minimum, uint maximum, string invalidˉlabel)
        {
            Compareˉedx(minimum);
            Branch(0x82, invalidˉlabel);
            Compareˉedx(maximum);
            Branch(0x87, invalidˉlabel);
        }

        public void Advance(byte length, string loopˉlabel)
        {
            Emit(0x83, 0xC0, length);
            Jump(loopˉlabel);
        }

        public ImmutableArray<byte> Finish()
        {
            var Result = Bytes.ToArray();
            foreach (var Patch in Patches)
            {
                if (!Labels.TryGetValue(Patch.Label, out var Target))
                {
                    throw new InvalidOperationException(
                        $"Unknown native UTF-8 service label '{Patch.Label}'.");
                }
                BinaryPrimitives.WriteInt32LittleEndian(
                    Result.AsSpan(Patch.Offset, sizeof(int)),
                    checked(Target - (Patch.Offset + sizeof(int))));
            }
            return Result.ToImmutableArray();
        }

        private void Emitˉu32(uint value)
        {
            var Value = new byte[sizeof(uint)];
            BinaryPrimitives.WriteUInt32LittleEndian(Value, value);
            Emit(Value);
        }
    }
}
