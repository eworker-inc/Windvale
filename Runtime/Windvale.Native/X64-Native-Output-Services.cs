using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Compiler.Native;

namespace Windvale.Runtime.Native;

public static class X64ˉnativeˉoutputˉservices
{
    public const int WINDOWS_CANONICAL_SIZE = 258;
    public const int LINUX_CANONICAL_SIZE = 213;
    public const string WINDOWS_CONSOLE_SHA256 =
        "10f3a500aca7f0236cdf9f6c20658591df88bc612e677264cdaa0bcef59a0a48";
    public const string WINDOWS_DIAGNOSTIC_SHA256 =
        "1b4068c01b2050c3055c78eb82303c71b8488e8766f7b628fab10ffb23e5ffe2";
    public const string LINUX_CONSOLE_SHA256 =
        "c5ea073a24c46dd634b1a67a7e7041d476dbce856d058aa8adc2c4e680d3d226";
    public const string LINUX_DIAGNOSTIC_SHA256 =
        "1c81018143fa9b708373eaceda62722ca40fb1e11b20808f765fe5ece33406fe";

    public static ImmutableArray<byte> Build(
        Nativeˉservice service,
        Nativeˉoutputˉplatform platform)
    {
        if (service is not Nativeˉservice.Consoleˉwriteˉline and
            not Nativeˉservice.Diagnosticˉwriteˉline)
        {
            throw new ArgumentOutOfRangeException(
                nameof(service),
                "Only native output services have an output leaf.");
        }

        return platform switch
        {
            Nativeˉoutputˉplatform.Windows => Buildˉwindows(service),
            Nativeˉoutputˉplatform.Linux => Buildˉlinux(service),
            _ => throw new ArgumentOutOfRangeException(
                nameof(platform),
                "The native output service supports Windows and Linux."),
        };
    }

    public static void Verify(
        Nativeˉservice service,
        Nativeˉoutputˉplatform platform,
        ReadOnlySpan<byte> code)
    {
        var Expected = Build(service, platform);
        var Expectedˉsize = Canonicalˉsize(platform);
        var Expectedˉsha256 = Canonicalˉsha256(service, platform);
        var Actualˉsha256 = Convert.ToHexString(SHA256.HashData(code)).ToLowerInvariant();
        if (code.Length != Expectedˉsize ||
            !StringComparer.Ordinal.Equals(Actualˉsha256, Expectedˉsha256) ||
            !code.SequenceEqual(Expected.AsSpan()))
        {
            throw new InvalidOperationException(
                $"Native {platform} {service} service identity is not canonical.");
        }
    }

    public static int Canonicalˉsize(Nativeˉoutputˉplatform platform) =>
        platform switch
        {
            Nativeˉoutputˉplatform.Windows => WINDOWS_CANONICAL_SIZE,
            Nativeˉoutputˉplatform.Linux => LINUX_CANONICAL_SIZE,
            _ => throw new ArgumentOutOfRangeException(nameof(platform)),
        };

    public static string Canonicalˉsha256(
        Nativeˉservice service,
        Nativeˉoutputˉplatform platform) =>
        (service, platform) switch
        {
            (Nativeˉservice.Consoleˉwriteˉline, Nativeˉoutputˉplatform.Windows) =>
                WINDOWS_CONSOLE_SHA256,
            (Nativeˉservice.Diagnosticˉwriteˉline, Nativeˉoutputˉplatform.Windows) =>
                WINDOWS_DIAGNOSTIC_SHA256,
            (Nativeˉservice.Consoleˉwriteˉline, Nativeˉoutputˉplatform.Linux) =>
                LINUX_CONSOLE_SHA256,
            (Nativeˉservice.Diagnosticˉwriteˉline, Nativeˉoutputˉplatform.Linux) =>
                LINUX_DIAGNOSTIC_SHA256,
            _ => throw new ArgumentOutOfRangeException(nameof(service)),
        };

    private static ImmutableArray<byte> Buildˉwindows(Nativeˉservice service)
    {
        var Targetˉfieldˉoffset = Targetˉoffset(service);
        var Code = new Outputˉcodeˉbuilder();
        Code.Emit(0x48, 0x89, 0xE0);
        Code.Emit(0x48, 0x83, 0xE4, 0xF0);
        Code.Emit(0x48, 0x83, 0xEC, 0x70);
        Code.Emit(0x48, 0x89, 0x44, 0x24, 0x28);
        Code.Emit(0x4C, 0x89, 0x54, 0x24, 0x30);
        Code.Emit(0x4C, 0x89, 0x5C, 0x24, 0x38);
        Code.Emit(0x4C, 0x89, 0x7C, 0x24, 0x40);
        Code.Emit(0x4C, 0x89, 0x44, 0x24, 0x48);
        Code.Emit(0x44, 0x89, 0x4C, 0x24, 0x50);
        Code.Emit(0x41, 0xC7, 0x47,
            Nativeˉexecutionˉcontextˉcontract.SERVICE_FAILURE_DETAIL_OFFSET,
            0x00, 0x00, 0x00, 0x00);
        Code.Emit(0x49, 0x8B, 0x47,
            Nativeˉexecutionˉcontextˉcontract.OUTPUT_TABLE_POINTER_OFFSET);
        Code.Emit(0x48, 0x8B, 0x48, Targetˉfieldˉoffset);
        Code.Emit(0x48, 0x89, 0x4C, 0x24, 0x58);
        Code.Emit(0x48, 0x8B, 0x40,
            Nativeˉoutputˉtableˉcontract.WRITE_FUNCTION_POINTER_OFFSET);
        Code.Emit(0x48, 0x89, 0x44, 0x24, 0x60);

        Code.Mark("text");
        Code.Emit(0x83, 0x7C, 0x24, 0x50, 0x00);
        Code.Branch(0x84, "newline");
        Emitˉwindowsˉcall(Code, bufferˉoffset: 0x48, lengthˉfromˉstack: true);
        Code.Emit(0x85, 0xC0);
        Code.Branch(0x84, "failure");
        Code.Emit(0x8B, 0x44, 0x24, 0x54);
        Code.Emit(0x85, 0xC0);
        Code.Branch(0x84, "failure");
        Code.Emit(0x3B, 0x44, 0x24, 0x50);
        Code.Branch(0x87, "failure");
        Code.Emit(0x48, 0x01, 0x44, 0x24, 0x48);
        Code.Emit(0x29, 0x44, 0x24, 0x50);
        Code.Jump("text");

        Code.Mark("newline");
        Code.Emit(0xC6, 0x44, 0x24, 0x68, 0x0A);
        Emitˉwindowsˉcall(Code, bufferˉoffset: 0x68, lengthˉfromˉstack: false);
        Code.Emit(0x85, 0xC0);
        Code.Branch(0x84, "failure");
        Code.Emit(0x83, 0x7C, 0x24, 0x54, 0x01);
        Code.Branch(0x85, "failure");
        Code.Emit(0x31, 0xC0);
        Code.Jump("return");

        Code.Mark("failure");
        Code.Emit(0x41, 0xC7, 0x47,
            Nativeˉexecutionˉcontextˉcontract.SERVICE_FAILURE_DETAIL_OFFSET,
            (byte)Nativeˉserviceˉfailureˉdetail.Outputˉwriteˉfailed,
            0x00, 0x00, 0x00);
        Code.Emit(0xB8, 0x01, 0x00, 0x00, 0x00);

        Code.Mark("return");
        Code.Emit(0x4C, 0x8B, 0x54, 0x24, 0x30);
        Code.Emit(0x4C, 0x8B, 0x5C, 0x24, 0x38);
        Code.Emit(0x4C, 0x8B, 0x7C, 0x24, 0x40);
        Code.Emit(0x48, 0x8B, 0x64, 0x24, 0x28);
        Code.Emit(0xC3);
        return Code.Finish();
    }

    private static void Emitˉwindowsˉcall(
        Outputˉcodeˉbuilder code,
        byte bufferˉoffset,
        bool lengthˉfromˉstack)
    {
        code.Emit(0x48, 0x8B, 0x4C, 0x24, 0x58);
        if (lengthˉfromˉstack)
        {
            code.Emit(0x48, 0x8B, 0x54, 0x24, bufferˉoffset);
            code.Emit(0x44, 0x8B, 0x44, 0x24, 0x50);
        }
        else
        {
            code.Emit(0x48, 0x8D, 0x54, 0x24, bufferˉoffset);
            code.Emit(0x41, 0xB8, 0x01, 0x00, 0x00, 0x00);
        }
        code.Emit(0x4C, 0x8D, 0x4C, 0x24, 0x54);
        code.Emit(0x48, 0xC7, 0x44, 0x24, 0x20, 0x00, 0x00, 0x00, 0x00);
        code.Emit(0xFF, 0x54, 0x24, 0x60);
    }

    private static ImmutableArray<byte> Buildˉlinux(Nativeˉservice service)
    {
        var Targetˉfieldˉoffset = Targetˉoffset(service);
        var Code = new Outputˉcodeˉbuilder();
        Code.Emit(0x48, 0x83, 0xEC, 0x30);
        Code.Emit(0x4C, 0x89, 0x14, 0x24);
        Code.Emit(0x4C, 0x89, 0x5C, 0x24, 0x08);
        Code.Emit(0x4C, 0x89, 0x7C, 0x24, 0x10);
        Code.Emit(0x4C, 0x89, 0x44, 0x24, 0x18);
        Code.Emit(0x44, 0x89, 0x4C, 0x24, 0x20);
        Code.Emit(0x41, 0xC7, 0x47,
            Nativeˉexecutionˉcontextˉcontract.SERVICE_FAILURE_DETAIL_OFFSET,
            0x00, 0x00, 0x00, 0x00);
        Code.Emit(0x49, 0x8B, 0x47,
            Nativeˉexecutionˉcontextˉcontract.OUTPUT_TABLE_POINTER_OFFSET);
        Code.Emit(0x48, 0x8B, 0x78, Targetˉfieldˉoffset);
        Code.Emit(0x48, 0x89, 0x7C, 0x24, 0x28);

        Code.Mark("text");
        Code.Emit(0x83, 0x7C, 0x24, 0x20, 0x00);
        Code.Branch(0x84, "newline");
        Code.Emit(0xB8, 0x01, 0x00, 0x00, 0x00);
        Code.Emit(0x48, 0x8B, 0x7C, 0x24, 0x28);
        Code.Emit(0x48, 0x8B, 0x74, 0x24, 0x18);
        Code.Emit(0x8B, 0x54, 0x24, 0x20);
        Code.Emit(0x0F, 0x05);
        Code.Emit(0x48, 0x83, 0xF8, 0xFC);
        Code.Branch(0x84, "text");
        Code.Emit(0x48, 0x85, 0xC0);
        Code.Branch(0x8E, "failure");
        Code.Emit(0x8B, 0x54, 0x24, 0x20);
        Code.Emit(0x48, 0x39, 0xD0);
        Code.Branch(0x87, "failure");
        Code.Emit(0x48, 0x01, 0x44, 0x24, 0x18);
        Code.Emit(0x29, 0x44, 0x24, 0x20);
        Code.Jump("text");

        Code.Mark("newline");
        Code.Emit(0xC6, 0x44, 0x24, 0x24, 0x0A);
        Code.Mark("newline_call");
        Code.Emit(0xB8, 0x01, 0x00, 0x00, 0x00);
        Code.Emit(0x48, 0x8B, 0x7C, 0x24, 0x28);
        Code.Emit(0x48, 0x8D, 0x74, 0x24, 0x24);
        Code.Emit(0xBA, 0x01, 0x00, 0x00, 0x00);
        Code.Emit(0x0F, 0x05);
        Code.Emit(0x48, 0x83, 0xF8, 0xFC);
        Code.Branch(0x84, "newline_call");
        Code.Emit(0x48, 0x83, 0xF8, 0x01);
        Code.Branch(0x85, "failure");
        Code.Emit(0x31, 0xC0);
        Code.Jump("return");

        Code.Mark("failure");
        Code.Emit(0x41, 0xC7, 0x47,
            Nativeˉexecutionˉcontextˉcontract.SERVICE_FAILURE_DETAIL_OFFSET,
            (byte)Nativeˉserviceˉfailureˉdetail.Outputˉwriteˉfailed,
            0x00, 0x00, 0x00);
        Code.Emit(0xB8, 0x01, 0x00, 0x00, 0x00);

        Code.Mark("return");
        Code.Emit(0x4C, 0x8B, 0x14, 0x24);
        Code.Emit(0x4C, 0x8B, 0x5C, 0x24, 0x08);
        Code.Emit(0x4C, 0x8B, 0x7C, 0x24, 0x10);
        Code.Emit(0x48, 0x83, 0xC4, 0x30);
        Code.Emit(0xC3);
        return Code.Finish();
    }

    private static byte Targetˉoffset(Nativeˉservice service) =>
        service switch
        {
            Nativeˉservice.Consoleˉwriteˉline =>
                Nativeˉoutputˉtableˉcontract.CONSOLE_TARGET_OFFSET,
            Nativeˉservice.Diagnosticˉwriteˉline =>
                Nativeˉoutputˉtableˉcontract.DIAGNOSTIC_TARGET_OFFSET,
            _ => throw new ArgumentOutOfRangeException(nameof(service)),
        };

    private sealed class Outputˉcodeˉbuilder
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
                throw new InvalidOperationException(
                    $"Duplicate native output-service label '{label}'.");
            }
        }

        public void Branch(byte condition, string label)
        {
            Emit(0x0F, condition);
            Reference(label);
        }

        public void Jump(string label)
        {
            Emit(0xE9);
            Reference(label);
        }

        public ImmutableArray<byte> Finish()
        {
            var Result = Bytes.ToArray();
            foreach (var Patch in Patches)
            {
                if (!Labels.TryGetValue(Patch.Label, out var Target))
                {
                    throw new InvalidOperationException(
                        $"Unknown native output-service label '{Patch.Label}'.");
                }
                BinaryPrimitives.WriteInt32LittleEndian(
                    Result.AsSpan(Patch.Offset, sizeof(int)),
                    checked(Target - (Patch.Offset + sizeof(int))));
            }
            return Result.ToImmutableArray();
        }

        private void Reference(string label)
        {
            Patches.Add((Bytes.Count, label));
            Emit(0x00, 0x00, 0x00, 0x00);
        }
    }
}
