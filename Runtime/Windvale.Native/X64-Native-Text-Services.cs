using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Runtime.Native;

public static class X64ˉnativeˉtextˉservices
{
    public const int TEXT_CONCAT_CANONICAL_SIZE = 249;
    public const string TEXT_CONCAT_CANONICAL_SHA256 =
        "75c5588117e1f5f58a593a23aae6156a3a68a6302df5f50153b977bccbaaa3a0";
    public const int I32_FORMAT_CANONICAL_SIZE = 225;
    public const string I32_FORMAT_CANONICAL_SHA256 =
        "c33758106e8d7cd31bbed8ef1e789a8e355c52736c119c75493154a4184fa41e";
    public const int U32_FORMAT_CANONICAL_SIZE = 191;
    public const string U32_FORMAT_CANONICAL_SHA256 =
        "b98f2d55e30bb7369e233f94e4ade5f3e8917a7730114446f1ebc81f353e1e43";

    // ABI-11 supplies the text arena and service-failure detail through R15's context.
    // These leaves return zero on success or one after publishing an exact failure detail.
    public static ImmutableArray<byte> Build(Nativeˉservice service) => service switch
    {
        Nativeˉservice.Textˉconcat => Buildˉtextˉconcat(),
        Nativeˉservice.I32ˉformat => Buildˉintegerˉformat(isˉsigned: true),
        Nativeˉservice.U32ˉformat => Buildˉintegerˉformat(isˉsigned: false),
        _ => throw new ArgumentOutOfRangeException(
            nameof(service),
            service,
            "The requested service is not an ABI-11 native text leaf."),
    };

    public static void Verify(Nativeˉservice service, ReadOnlySpan<byte> code)
    {
        var (Expectedˉsize, Expectedˉhash) = service switch
        {
            Nativeˉservice.Textˉconcat =>
                (TEXT_CONCAT_CANONICAL_SIZE, TEXT_CONCAT_CANONICAL_SHA256),
            Nativeˉservice.I32ˉformat =>
                (I32_FORMAT_CANONICAL_SIZE, I32_FORMAT_CANONICAL_SHA256),
            Nativeˉservice.U32ˉformat =>
                (U32_FORMAT_CANONICAL_SIZE, U32_FORMAT_CANONICAL_SHA256),
            _ => throw new ArgumentOutOfRangeException(
                nameof(service),
                service,
                "The requested service is not an ABI-11 native text leaf."),
        };
        var Hash = Convert.ToHexString(SHA256.HashData(code)).ToLowerInvariant();
        if (code.Length != Expectedˉsize || !StringComparer.Ordinal.Equals(Hash, Expectedˉhash))
        {
            throw new InvalidOperationException(
                $"Native {service} service identity is {code.Length} bytes / {Hash}; " +
                $"expected {Expectedˉsize} bytes / {Expectedˉhash}.");
        }
    }

    private static ImmutableArray<byte> Buildˉintegerˉformat(bool isˉsigned)
    {
        var Code = new Serviceˉcodeˉbuilder();
        Code.Emit(0x48, 0x83, 0xEC, 0x20);
        Code.Emit(0x4C, 0x89, 0x0C, 0x24);
        Code.Emit(0x41, 0xC7, 0x47, Nativeˉexecutionˉcontextˉcontract.SERVICE_FAILURE_DETAIL_OFFSET,
            0x00, 0x00, 0x00, 0x00);
        Code.Emit(0x44, 0x89, 0xC0);
        if (isˉsigned)
        {
            Code.Emit(0x89, 0xC1);
            Code.Emit(0xC1, 0xF9, 0x1F);
            Code.Emit(0x89, 0x4C, 0x24, 0x10);
            Code.Emit(0x31, 0xC8);
            Code.Emit(0x29, 0xC8);
        }
        Code.Emit(0x4C, 0x8D, 0x4C, 0x24, 0x20);
        Code.Emit(0x45, 0x31, 0xC0);

        Code.Mark("digit");
        Code.Emit(0x31, 0xD2);
        Code.Emit(0xB9, 0x0A, 0x00, 0x00, 0x00);
        Code.Emit(0xF7, 0xF1);
        Code.Emit(0x80, 0xC2, 0x30);
        Code.Emit(0x49, 0xFF, 0xC9);
        Code.Emit(0x41, 0x88, 0x11);
        Code.Emit(0x41, 0xFF, 0xC0);
        Code.Emit(0x85, 0xC0);
        Code.Branch(0x85, "digit");

        if (isˉsigned)
        {
            Code.Emit(0x83, 0x7C, 0x24, 0x10, 0x00);
            Code.Branch(0x84, "length_ready");
            Code.Emit(0x49, 0xFF, 0xC9);
            Code.Emit(0x41, 0xC6, 0x01, 0x2D);
            Code.Emit(0x41, 0xFF, 0xC0);
        }

        Code.Mark("length_ready");
        Code.Emit(0x44, 0x89, 0x44, 0x24, 0x08);
        Code.Emit(0x41, 0x8B, 0x4F, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET);
        Code.Emit(0x89, 0x4C, 0x24, 0x0C);
        Code.Emit(0x89, 0xC8);
        Code.Emit(0x44, 0x01, 0xC0);
        Code.Branch(0x82, "arena_failure");
        Code.Emit(0x41, 0x3B, 0x47, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_LENGTH_OFFSET);
        Code.Branch(0x87, "arena_failure");
        Code.Emit(0x41, 0x89, 0x47, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET);
        Code.Emit(0x49, 0x8B, 0x57, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_POINTER_OFFSET);
        Code.Emit(0x8B, 0x4C, 0x24, 0x0C);
        Code.Emit(0x48, 0x01, 0xCA);

        Code.Mark("copy");
        Code.Emit(0x45, 0x85, 0xC0);
        Code.Branch(0x84, "written");
        Code.Emit(0x41, 0x8A, 0x01);
        Code.Emit(0x88, 0x02);
        Code.Emit(0x49, 0xFF, 0xC1);
        Code.Emit(0x48, 0xFF, 0xC2);
        Code.Emit(0x41, 0xFF, 0xC8);
        Code.Branch(0x85, "copy");

        Code.Mark("written");
        Code.Emit(0x48, 0x8B, 0x0C, 0x24);
        Code.Emit(0x49, 0x8B, 0x47, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_POINTER_OFFSET);
        Code.Emit(0x8B, 0x54, 0x24, 0x0C);
        Code.Emit(0x48, 0x01, 0xD0);
        Code.Emit(0x48, 0x89, 0x01);
        Code.Emit(0x8B, 0x44, 0x24, 0x08);
        Code.Emit(0x89, 0x41, 0x08);
        Code.Emit(0xC7, 0x41, 0x0C, 0x00, 0x00, 0x00, 0x00);
        Code.Emit(0x31, 0xC0);
        Code.Emit(0x48, 0x83, 0xC4, 0x20);
        Code.Emit(0xC3);

        Code.Mark("arena_failure");
        Code.Emit(0x41, 0xC7, 0x47, Nativeˉexecutionˉcontextˉcontract.SERVICE_FAILURE_DETAIL_OFFSET,
            (byte)Nativeˉserviceˉfailureˉdetail.Textˉarenaˉexhausted, 0x00, 0x00, 0x00);
        Code.Emit(0xB8, 0x01, 0x00, 0x00, 0x00);
        Code.Emit(0x48, 0x83, 0xC4, 0x20);
        Code.Emit(0xC3);
        return Code.Finish();
    }

    private static ImmutableArray<byte> Buildˉtextˉconcat()
    {
        var Code = new Serviceˉcodeˉbuilder();
        Code.Emit(0x48, 0x83, 0xEC, 0x20);
        Code.Emit(0x4C, 0x89, 0x04, 0x24);
        Code.Emit(0x4C, 0x89, 0x4C, 0x24, 0x08);
        Code.Emit(0x48, 0x89, 0x4C, 0x24, 0x10);
        Code.Emit(0x41, 0xC7, 0x47, Nativeˉexecutionˉcontextˉcontract.SERVICE_FAILURE_DETAIL_OFFSET,
            0x00, 0x00, 0x00, 0x00);
        Code.Emit(0x41, 0x8B, 0x40, Nativeˉcontract.BORROWED_TEXT_LENGTH_OFFSET);
        Code.Emit(0x41, 0x8B, 0x49, Nativeˉcontract.BORROWED_TEXT_LENGTH_OFFSET);
        Code.Emit(0x01, 0xC1);
        Code.Branch(0x82, "value_failure");
        Code.Emit(0x81, 0xF9);
        Code.Emitˉu32(Bytecodeˉlimits.MAX_UTF8_VALUE_BYTES);
        Code.Branch(0x87, "value_failure");
        Code.Emit(0x89, 0x4C, 0x24, 0x18);
        Code.Emit(0x41, 0x8B, 0x47, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET);
        Code.Emit(0x89, 0x44, 0x24, 0x1C);
        Code.Emit(0x01, 0xC1);
        Code.Branch(0x82, "arena_failure");
        Code.Emit(0x41, 0x3B, 0x4F, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_LENGTH_OFFSET);
        Code.Branch(0x87, "arena_failure");
        Code.Emit(0x41, 0x89, 0x4F, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET);
        Code.Emit(0x4D, 0x8B, 0x4F, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_POINTER_OFFSET);
        Code.Emit(0x8B, 0x54, 0x24, 0x1C);
        Code.Emit(0x49, 0x01, 0xD1);

        Code.Emit(0x4C, 0x8B, 0x04, 0x24);
        Code.Emit(0x41, 0x8B, 0x48, Nativeˉcontract.BORROWED_TEXT_LENGTH_OFFSET);
        Code.Emit(0x4D, 0x8B, 0x00);
        Code.Mark("left_copy");
        Code.Emit(0x85, 0xC9);
        Code.Branch(0x84, "right_start");
        Code.Emit(0x41, 0x8A, 0x00);
        Code.Emit(0x41, 0x88, 0x01);
        Code.Emit(0x49, 0xFF, 0xC0);
        Code.Emit(0x49, 0xFF, 0xC1);
        Code.Emit(0xFF, 0xC9);
        Code.Branch(0x85, "left_copy");

        Code.Mark("right_start");
        Code.Emit(0x4C, 0x8B, 0x44, 0x24, 0x08);
        Code.Emit(0x41, 0x8B, 0x48, Nativeˉcontract.BORROWED_TEXT_LENGTH_OFFSET);
        Code.Emit(0x4D, 0x8B, 0x00);
        Code.Mark("right_copy");
        Code.Emit(0x85, 0xC9);
        Code.Branch(0x84, "written");
        Code.Emit(0x41, 0x8A, 0x00);
        Code.Emit(0x41, 0x88, 0x01);
        Code.Emit(0x49, 0xFF, 0xC0);
        Code.Emit(0x49, 0xFF, 0xC1);
        Code.Emit(0xFF, 0xC9);
        Code.Branch(0x85, "right_copy");

        Code.Mark("written");
        Code.Emit(0x48, 0x8B, 0x4C, 0x24, 0x10);
        Code.Emit(0x49, 0x8B, 0x47, Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_POINTER_OFFSET);
        Code.Emit(0x8B, 0x54, 0x24, 0x1C);
        Code.Emit(0x48, 0x01, 0xD0);
        Code.Emit(0x48, 0x89, 0x01);
        Code.Emit(0x8B, 0x44, 0x24, 0x18);
        Code.Emit(0x89, 0x41, 0x08);
        Code.Emit(0xC7, 0x41, 0x0C, 0x00, 0x00, 0x00, 0x00);
        Code.Emit(0x31, 0xC0);
        Code.Emit(0x48, 0x83, 0xC4, 0x20);
        Code.Emit(0xC3);

        Code.Mark("value_failure");
        Code.Emit(0x41, 0xC7, 0x47, Nativeˉexecutionˉcontextˉcontract.SERVICE_FAILURE_DETAIL_OFFSET,
            (byte)Nativeˉserviceˉfailureˉdetail.Textˉvalueˉlimit, 0x00, 0x00, 0x00);
        Code.Jump("failure");
        Code.Mark("arena_failure");
        Code.Emit(0x41, 0xC7, 0x47, Nativeˉexecutionˉcontextˉcontract.SERVICE_FAILURE_DETAIL_OFFSET,
            (byte)Nativeˉserviceˉfailureˉdetail.Textˉarenaˉexhausted, 0x00, 0x00, 0x00);
        Code.Mark("failure");
        Code.Emit(0xB8, 0x01, 0x00, 0x00, 0x00);
        Code.Emit(0x48, 0x83, 0xC4, 0x20);
        Code.Emit(0xC3);
        return Code.Finish();
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

        public void Emitˉu32(uint value)
        {
            Span<byte> Value = stackalloc byte[sizeof(uint)];
            BinaryPrimitives.WriteUInt32LittleEndian(Value, value);
            Emit(Value);
        }

        public void Mark(string label)
        {
            if (!Labels.TryAdd(label, Bytes.Count))
            {
                throw new InvalidOperationException($"Duplicate native text-service label '{label}'.");
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

        public ImmutableArray<byte> Finish()
        {
            var Result = Bytes.ToArray();
            foreach (var Patch in Patches)
            {
                if (!Labels.TryGetValue(Patch.Label, out var Target))
                {
                    throw new InvalidOperationException(
                        $"Unknown native text-service label '{Patch.Label}'.");
                }
                BinaryPrimitives.WriteInt32LittleEndian(
                    Result.AsSpan(Patch.Offset, sizeof(int)),
                    checked(Target - (Patch.Offset + sizeof(int))));
            }
            return Result.ToImmutableArray();
        }
    }
}
