using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Compiler.Native;

namespace Windvale.Runtime.Native;

public static class X64ˉnativeˉfileˉoutputˉservice
{
    public const int WINDOWS_CANONICAL_SIZE = 787;
    public const int LINUX_CANONICAL_SIZE = 823;
    public const string WINDOWS_CANONICAL_SHA256 =
        "a331248b12fc5830587f6fd8ddf06a546859b8f57366e205032aa2c37db48bb1";
    public const string LINUX_CANONICAL_SHA256 =
        "fc688f2a84936dc1082fcb5654667a8a60b0581bff29b1868d48ef2d4af77422";

    public static ImmutableArray<byte> Build(Nativeˉfileˉinputˉplatform platform) =>
        platform switch
        {
            Nativeˉfileˉinputˉplatform.Windows => Buildˉwindows(),
            Nativeˉfileˉinputˉplatform.Linux => Buildˉlinux(),
            _ => throw new ArgumentOutOfRangeException(nameof(platform)),
        };

    public static void Verify(Nativeˉfileˉinputˉplatform platform, ReadOnlySpan<byte> code)
    {
        var Expected = Build(platform);
        var Actualˉhash = Convert.ToHexString(SHA256.HashData(code)).ToLowerInvariant();
        if (code.Length != Canonicalˉsize(platform) ||
            !StringComparer.Ordinal.Equals(Actualˉhash, Canonicalˉsha256(platform)) ||
            !code.SequenceEqual(Expected.AsSpan()))
        {
            throw new InvalidOperationException(
                $"Native {platform} file-output service identity is " +
                $"{code.Length} bytes / {Actualˉhash}; expected " +
                $"{Expected.Length} bytes / {Canonicalˉsha256(platform)}.");
        }
    }

    public static int Canonicalˉsize(Nativeˉfileˉinputˉplatform platform) =>
        platform switch
        {
            Nativeˉfileˉinputˉplatform.Windows => WINDOWS_CANONICAL_SIZE,
            Nativeˉfileˉinputˉplatform.Linux => LINUX_CANONICAL_SIZE,
            _ => throw new ArgumentOutOfRangeException(nameof(platform)),
        };

    public static string Canonicalˉsha256(Nativeˉfileˉinputˉplatform platform) =>
        platform switch
        {
            Nativeˉfileˉinputˉplatform.Windows => WINDOWS_CANONICAL_SHA256,
            Nativeˉfileˉinputˉplatform.Linux => LINUX_CANONICAL_SHA256,
            _ => throw new ArgumentOutOfRangeException(nameof(platform)),
        };

    private static ImmutableArray<byte> Buildˉwindows()
    {
        var Code = new Fileˉcodeˉbuilder("Windows");

        // Preserve Windvale counters and every Windows nonvolatile register used below.
        Code.Emit(0x48, 0x89, 0xE0);
        Code.Emit(0x48, 0x83, 0xE4, 0xF0);
        Code.Emit(0x48, 0x81, 0xEC, 0xB0, 0x00, 0x00, 0x00);
        Code.Emit(0x48, 0x89, 0x44, 0x24, 0x58);
        Code.Emit(0x4C, 0x89, 0x54, 0x24, 0x60);
        Code.Emit(0x4C, 0x89, 0x5C, 0x24, 0x68);
        Code.Emit(0x48, 0x89, 0x5C, 0x24, 0x70);
        Code.Emit(0x48, 0x89, 0x6C, 0x24, 0x78);
        Code.Emit(0x4C, 0x89, 0xA4, 0x24, 0x80, 0x00, 0x00, 0x00);
        Code.Emit(0x4C, 0x89, 0xAC, 0x24, 0x88, 0x00, 0x00, 0x00);
        Code.Emit(0x4C, 0x89, 0xB4, 0x24, 0x90, 0x00, 0x00, 0x00);
        Code.Emit(0x48, 0x89, 0xB4, 0x24, 0x98, 0x00, 0x00, 0x00);
        Code.Emit(0x48, 0x89, 0xBC, 0x24, 0xA0, 0x00, 0x00, 0x00);
        Code.Emit(0x49, 0x89, 0xCC);
        Code.Emit(0x89, 0x54, 0x24, 0x38);
        Code.Emit(0x4D, 0x89, 0xC5);
        Code.Emit(0x44, 0x89, 0x4C, 0x24, 0x3C);
        Code.Emit(0x41, 0xC7, 0x47,
            Nativeˉexecutionˉcontextˉcontract.SERVICE_FAILURE_DETAIL_OFFSET,
            0x00, 0x00, 0x00, 0x00);
        Code.Emit(0x49, 0x8B, 0x5F,
            Nativeˉexecutionˉcontextˉcontract.FILE_OUTPUT_TABLE_POINTER_OFFSET);

        // Validate the byte bound plus the nonempty, NUL-free resource name.
        Code.Emit(0x81, 0x7C, 0x24, 0x38, 0x00, 0x00, 0x40, 0x00);
        Code.Branch(0x87, "failure_too_large");
        Code.Emit(0x83, 0x7C, 0x24, 0x3C, 0x00);
        Code.Branch(0x84, "failure_invalid");
        Code.Emit(0x31, 0xC0);
        Code.Emit(0x4C, 0x89, 0xEF);
        Code.Emit(0x8B, 0x4C, 0x24, 0x3C);
        Code.Emit(0xF2, 0xAE);
        Code.Branch(0x84, "failure_invalid");

        // Convert strict UTF-8 into the execution-owned UTF-16 path scratch.
        Code.Emit(0xB9, 0xE9, 0xFD, 0x00, 0x00);
        Code.Emit(0xBA, 0x08, 0x00, 0x00, 0x00);
        Code.Emit(0x4D, 0x89, 0xE8);
        Code.Emit(0x44, 0x8B, 0x4C, 0x24, 0x3C);
        Code.Emit(0x48, 0x8B, 0x43,
            Nativeˉfileˉoutputˉtableˉcontract.SCRATCH_POINTER_OFFSET);
        Code.Emit(0x48, 0x89, 0x44, 0x24, 0x20);
        Code.Emit(0x8B, 0x43, Nativeˉfileˉoutputˉtableˉcontract.SCRATCH_BYTES_OFFSET);
        Code.Emit(0xD1, 0xE8);
        Code.Emit(0x83, 0xE8, 0x01);
        Code.Emit(0x48, 0x89, 0x44, 0x24, 0x28);
        Code.Emit(0xFF, 0x53,
            Nativeˉfileˉoutputˉtableˉcontract.WINDOWS_UTF8_TO_UTF16_POINTER_OFFSET);
        Code.Emit(0x85, 0xC0);
        Code.Branch(0x84, "failure_invalid");
        Code.Emit(0x48, 0x8B, 0x53,
            Nativeˉfileˉoutputˉtableˉcontract.SCRATCH_POINTER_OFFSET);
        Code.Emit(0x66, 0xC7, 0x04, 0x42, 0x00, 0x00);

        // Create or replace the file without sharing and retain the exact handle.
        Code.Emit(0x48, 0x89, 0xD1);
        Code.Emit(0xBA, 0x00, 0x00, 0x00, 0x40);
        Code.Emit(0x45, 0x31, 0xC0);
        Code.Emit(0x45, 0x31, 0xC9);
        Code.Emit(0x48, 0xC7, 0x44, 0x24, 0x20, 0x02, 0x00, 0x00, 0x00);
        Code.Emit(0x48, 0xC7, 0x44, 0x24, 0x28, 0x00, 0x00, 0x00, 0x08);
        Code.Emit(0x48, 0xC7, 0x44, 0x24, 0x30, 0x00, 0x00, 0x00, 0x00);
        Code.Emit(0xFF, 0x53, Nativeˉfileˉoutputˉtableˉcontract.WINDOWS_OPEN_POINTER_OFFSET);
        Code.Emit(0x48, 0x83, 0xF8, 0xFF);
        Code.Branch(0x84, "open_error");
        Code.Emit(0x49, 0x89, 0xC6);
        Code.Emit(0xC7, 0x44, 0x24, 0x40, 0x00, 0x00, 0x00, 0x00);

        // Complete partial writes; a successful zero-byte value still reaches durable flush.
        Code.Mark("write_loop");
        Code.Emit(0x8B, 0x44, 0x24, 0x40);
        Code.Emit(0x3B, 0x44, 0x24, 0x38);
        Code.Branch(0x84, "flush");
        Code.Emit(0x4C, 0x89, 0xF1);
        Code.Emit(0x4C, 0x89, 0xE2);
        Code.Emit(0x48, 0x01, 0xC2);
        Code.Emit(0x44, 0x8B, 0x44, 0x24, 0x38);
        Code.Emit(0x41, 0x29, 0xC0);
        Code.Emit(0x44, 0x89, 0x44, 0x24, 0x44);
        Code.Emit(0x4C, 0x8D, 0x4C, 0x24, 0x48);
        Code.Emit(0x48, 0xC7, 0x44, 0x24, 0x20, 0x00, 0x00, 0x00, 0x00);
        Code.Emit(0xFF, 0x53, Nativeˉfileˉoutputˉtableˉcontract.WINDOWS_WRITE_POINTER_OFFSET);
        Code.Emit(0x85, 0xC0);
        Code.Branch(0x84, "close_last_error");
        Code.Emit(0x8B, 0x44, 0x24, 0x48);
        Code.Emit(0x85, 0xC0);
        Code.Branch(0x84, "close_unavailable");
        Code.Emit(0x3B, 0x44, 0x24, 0x44);
        Code.Branch(0x87, "close_unavailable");
        Code.Emit(0x01, 0x44, 0x24, 0x40);
        Code.Jump("write_loop");

        Code.Mark("flush");
        Code.Emit(0x4C, 0x89, 0xF1);
        Code.Emit(0xFF, 0x53, Nativeˉfileˉoutputˉtableˉcontract.WINDOWS_FLUSH_POINTER_OFFSET);
        Code.Emit(0x85, 0xC0);
        Code.Branch(0x84, "close_last_error");
        Code.Emit(0x4C, 0x89, 0xF1);
        Code.Emit(0xFF, 0x53, Nativeˉfileˉoutputˉtableˉcontract.WINDOWS_CLOSE_POINTER_OFFSET);
        Code.Emit(0x85, 0xC0);
        Code.Branch(0x84, "failure_unavailable");
        Code.Emit(0x31, 0xC0);
        Code.Jump("return");

        Code.Mark("open_error");
        Emitˉwindowsˉlastˉerror(Code);
        Code.Jump("map_open_error");
        Code.Mark("close_last_error");
        Emitˉwindowsˉlastˉerror(Code);
        Code.Jump("map_close_error");
        Code.Mark("map_open_error");
        Emitˉwindowsˉerrorˉmapping(Code, close: false);
        Code.Mark("map_close_error");
        Emitˉwindowsˉerrorˉmapping(Code, close: true);

        Code.Mark("close_invalid");
        Emitˉstoreˉwindowsˉdetail(Code, Nativeˉserviceˉfailureˉdetail.Fileˉinvalidˉname);
        Code.Jump("close_failure");
        Code.Mark("close_not_found");
        Emitˉstoreˉwindowsˉdetail(Code, Nativeˉserviceˉfailureˉdetail.Fileˉnotˉfound);
        Code.Jump("close_failure");
        Code.Mark("close_denied");
        Emitˉstoreˉwindowsˉdetail(Code, Nativeˉserviceˉfailureˉdetail.Fileˉpermissionˉdenied);
        Code.Jump("close_failure");
        Code.Mark("close_too_large");
        Emitˉstoreˉwindowsˉdetail(Code, Nativeˉserviceˉfailureˉdetail.Fileˉtooˉlarge);
        Code.Jump("close_failure");
        Code.Mark("close_unavailable");
        Emitˉstoreˉwindowsˉdetail(Code, Nativeˉserviceˉfailureˉdetail.Fileˉunavailable);

        Code.Mark("close_failure");
        Code.Emit(0x4C, 0x89, 0xF1);
        Code.Emit(0xFF, 0x53, Nativeˉfileˉoutputˉtableˉcontract.WINDOWS_CLOSE_POINTER_OFFSET);
        Code.Emit(0x8B, 0x44, 0x24, 0x4C);
        Code.Emit(0x41, 0x89, 0x47,
            Nativeˉexecutionˉcontextˉcontract.SERVICE_FAILURE_DETAIL_OFFSET);
        Code.Emit(0xB8, 0x01, 0x00, 0x00, 0x00);
        Code.Jump("return");

        Emitˉdirectˉfailure(Code, "failure_invalid", Nativeˉserviceˉfailureˉdetail.Fileˉinvalidˉname);
        Emitˉdirectˉfailure(Code, "failure_not_found", Nativeˉserviceˉfailureˉdetail.Fileˉnotˉfound);
        Emitˉdirectˉfailure(Code, "failure_denied", Nativeˉserviceˉfailureˉdetail.Fileˉpermissionˉdenied);
        Emitˉdirectˉfailure(Code, "failure_unavailable", Nativeˉserviceˉfailureˉdetail.Fileˉunavailable);
        Emitˉdirectˉfailure(Code, "failure_too_large", Nativeˉserviceˉfailureˉdetail.Fileˉtooˉlarge);

        Code.Mark("return");
        Code.Emit(0x4C, 0x8B, 0xA4, 0x24, 0x80, 0x00, 0x00, 0x00);
        Code.Emit(0x4C, 0x8B, 0xAC, 0x24, 0x88, 0x00, 0x00, 0x00);
        Code.Emit(0x4C, 0x8B, 0xB4, 0x24, 0x90, 0x00, 0x00, 0x00);
        Code.Emit(0x48, 0x8B, 0xB4, 0x24, 0x98, 0x00, 0x00, 0x00);
        Code.Emit(0x48, 0x8B, 0xBC, 0x24, 0xA0, 0x00, 0x00, 0x00);
        Code.Emit(0x48, 0x8B, 0x5C, 0x24, 0x70);
        Code.Emit(0x48, 0x8B, 0x6C, 0x24, 0x78);
        Code.Emit(0x4C, 0x8B, 0x54, 0x24, 0x60);
        Code.Emit(0x4C, 0x8B, 0x5C, 0x24, 0x68);
        Code.Emit(0x48, 0x8B, 0x64, 0x24, 0x58);
        Code.Emit(0xC3);
        return Code.Finish();
    }

    private static ImmutableArray<byte> Buildˉlinux()
    {
        var Code = new Fileˉcodeˉbuilder("Linux");
        Code.Emit(0x48, 0x83, 0xEC, 0x60);
        Code.Emit(0x4C, 0x89, 0x14, 0x24);
        Code.Emit(0x4C, 0x89, 0x5C, 0x24, 0x08);
        Code.Emit(0x4C, 0x89, 0x7C, 0x24, 0x10);
        Code.Emit(0x48, 0x89, 0x4C, 0x24, 0x18);
        Code.Emit(0x89, 0x54, 0x24, 0x20);
        Code.Emit(0x4C, 0x89, 0x44, 0x24, 0x28);
        Code.Emit(0x44, 0x89, 0x4C, 0x24, 0x30);
        Code.Emit(0x41, 0xC7, 0x47,
            Nativeˉexecutionˉcontextˉcontract.SERVICE_FAILURE_DETAIL_OFFSET,
            0x00, 0x00, 0x00, 0x00);
        Code.Emit(0x49, 0x8B, 0x47,
            Nativeˉexecutionˉcontextˉcontract.FILE_OUTPUT_TABLE_POINTER_OFFSET);
        Code.Emit(0x48, 0x89, 0x44, 0x24, 0x38);

        Code.Emit(0x81, 0x7C, 0x24, 0x20, 0x00, 0x00, 0x40, 0x00);
        Code.Branch(0x87, "failure_too_large");
        Code.Emit(0x83, 0x7C, 0x24, 0x30, 0x00);
        Code.Branch(0x84, "failure_invalid");
        Code.Emit(0x31, 0xC0);
        Code.Emit(0x48, 0x8B, 0x7C, 0x24, 0x28);
        Code.Emit(0x8B, 0x4C, 0x24, 0x30);
        Code.Emit(0xF2, 0xAE);
        Code.Branch(0x84, "failure_invalid");
        Code.Emit(0x48, 0x8B, 0x44, 0x24, 0x38);
        Code.Emit(0x48, 0x8B, 0x78,
            Nativeˉfileˉoutputˉtableˉcontract.SCRATCH_POINTER_OFFSET);
        Code.Emit(0x48, 0x89, 0x7C, 0x24, 0x40);
        Code.Emit(0x48, 0x8B, 0x74, 0x24, 0x28);
        Code.Emit(0x8B, 0x4C, 0x24, 0x30);
        Code.Emit(0xF3, 0xA4);
        Code.Emit(0xC6, 0x07, 0x00);

        // openat(AT_FDCWD, path, O_WRONLY | O_CREAT | O_TRUNC | O_CLOEXEC, 0666)
        Code.Mark("open_call");
        Code.Emit(0xB8, 0x01, 0x01, 0x00, 0x00);
        Code.Emit(0x48, 0xC7, 0xC7, 0x9C, 0xFF, 0xFF, 0xFF);
        Code.Emit(0x48, 0x8B, 0x74, 0x24, 0x40);
        Code.Emit(0xBA, 0x41, 0x02, 0x08, 0x00);
        Code.Emit(0x41, 0xBA, 0xB6, 0x01, 0x00, 0x00);
        Code.Emit(0x0F, 0x05);
        Code.Emit(0x48, 0x83, 0xF8, 0xFC);
        Code.Branch(0x84, "open_call");
        Code.Emit(0x48, 0x85, 0xC0);
        Code.Branch(0x88, "open_error");
        Code.Emit(0x48, 0x89, 0x44, 0x24, 0x48);
        Code.Emit(0xC7, 0x44, 0x24, 0x50, 0x00, 0x00, 0x00, 0x00);

        Code.Mark("write_loop");
        Code.Emit(0x8B, 0x4C, 0x24, 0x50);
        Code.Emit(0x3B, 0x4C, 0x24, 0x20);
        Code.Branch(0x84, "flush_call");
        Code.Mark("write_call");
        Code.Emit(0xB8, 0x01, 0x00, 0x00, 0x00);
        Code.Emit(0x48, 0x8B, 0x7C, 0x24, 0x48);
        Code.Emit(0x48, 0x8B, 0x74, 0x24, 0x18);
        Code.Emit(0x8B, 0x4C, 0x24, 0x50);
        Code.Emit(0x48, 0x01, 0xCE);
        Code.Emit(0x8B, 0x54, 0x24, 0x20);
        Code.Emit(0x29, 0xCA);
        Code.Emit(0x89, 0x54, 0x24, 0x54);
        Code.Emit(0x0F, 0x05);
        Code.Emit(0x48, 0x83, 0xF8, 0xFC);
        Code.Branch(0x84, "write_call");
        Code.Emit(0x48, 0x85, 0xC0);
        Code.Branch(0x88, "write_error");
        Code.Branch(0x84, "close_unavailable");
        Code.Emit(0x3B, 0x44, 0x24, 0x54);
        Code.Branch(0x87, "close_unavailable");
        Code.Emit(0x01, 0x44, 0x24, 0x50);
        Code.Jump("write_loop");

        Code.Mark("flush_call");
        Code.Emit(0xB8, 0x4A, 0x00, 0x00, 0x00);
        Code.Emit(0x48, 0x8B, 0x7C, 0x24, 0x48);
        Code.Emit(0x0F, 0x05);
        Code.Emit(0x48, 0x83, 0xF8, 0xFC);
        Code.Branch(0x84, "flush_call");
        Code.Emit(0x48, 0x85, 0xC0);
        Code.Branch(0x88, "flush_error");
        Code.Emit(0xB8, 0x03, 0x00, 0x00, 0x00);
        Code.Emit(0x48, 0x8B, 0x7C, 0x24, 0x48);
        Code.Emit(0x0F, 0x05);
        Code.Emit(0x48, 0x85, 0xC0);
        Code.Branch(0x88, "failure_unavailable");
        Code.Emit(0x31, 0xC0);
        Code.Jump("return");

        Code.Mark("open_error");
        Emitˉlinuxˉerrorˉmapping(Code, close: false);
        Code.Mark("write_error");
        Emitˉlinuxˉerrorˉmapping(Code, close: true);
        Code.Mark("flush_error");
        Emitˉlinuxˉerrorˉmapping(Code, close: true);

        Code.Mark("close_invalid");
        Emitˉstoreˉdetail(Code, Nativeˉserviceˉfailureˉdetail.Fileˉinvalidˉname);
        Code.Jump("close_failure");
        Code.Mark("close_not_found");
        Emitˉstoreˉdetail(Code, Nativeˉserviceˉfailureˉdetail.Fileˉnotˉfound);
        Code.Jump("close_failure");
        Code.Mark("close_denied");
        Emitˉstoreˉdetail(Code, Nativeˉserviceˉfailureˉdetail.Fileˉpermissionˉdenied);
        Code.Jump("close_failure");
        Code.Mark("close_too_large");
        Emitˉstoreˉdetail(Code, Nativeˉserviceˉfailureˉdetail.Fileˉtooˉlarge);
        Code.Jump("close_failure");
        Code.Mark("close_unavailable");
        Emitˉstoreˉdetail(Code, Nativeˉserviceˉfailureˉdetail.Fileˉunavailable);

        Code.Mark("close_failure");
        Code.Emit(0xB8, 0x03, 0x00, 0x00, 0x00);
        Code.Emit(0x48, 0x8B, 0x7C, 0x24, 0x48);
        Code.Emit(0x0F, 0x05);
        Code.Emit(0x8B, 0x44, 0x24, 0x58);
        Code.Emit(0x41, 0x89, 0x47,
            Nativeˉexecutionˉcontextˉcontract.SERVICE_FAILURE_DETAIL_OFFSET);
        Code.Emit(0xB8, 0x01, 0x00, 0x00, 0x00);
        Code.Jump("return");

        Emitˉdirectˉfailure(Code, "failure_invalid", Nativeˉserviceˉfailureˉdetail.Fileˉinvalidˉname);
        Emitˉdirectˉfailure(Code, "failure_not_found", Nativeˉserviceˉfailureˉdetail.Fileˉnotˉfound);
        Emitˉdirectˉfailure(Code, "failure_denied", Nativeˉserviceˉfailureˉdetail.Fileˉpermissionˉdenied);
        Emitˉdirectˉfailure(Code, "failure_unavailable", Nativeˉserviceˉfailureˉdetail.Fileˉunavailable);
        Emitˉdirectˉfailure(Code, "failure_too_large", Nativeˉserviceˉfailureˉdetail.Fileˉtooˉlarge);

        Code.Mark("return");
        Code.Emit(0x4C, 0x8B, 0x14, 0x24);
        Code.Emit(0x4C, 0x8B, 0x5C, 0x24, 0x08);
        Code.Emit(0x4C, 0x8B, 0x7C, 0x24, 0x10);
        Code.Emit(0x48, 0x83, 0xC4, 0x60);
        Code.Emit(0xC3);
        return Code.Finish();
    }

    private static void Emitˉwindowsˉlastˉerror(Fileˉcodeˉbuilder code) =>
        code.Emit(0xFF, 0x53, Nativeˉfileˉoutputˉtableˉcontract.WINDOWS_LAST_ERROR_POINTER_OFFSET);

    private static void Emitˉwindowsˉerrorˉmapping(Fileˉcodeˉbuilder code, bool close)
    {
        var Prefix = close ? "close_" : "failure_";
        foreach (var Error in new[] { 123, 161, 206 })
        {
            code.Emit(0x3D);
            code.Emitˉu32((uint)Error);
            code.Branch(0x84, Prefix + "invalid");
        }
        foreach (var Error in new[] { 2, 3 })
        {
            code.Emit(0x83, 0xF8, (byte)Error);
            code.Branch(0x84, Prefix + "not_found");
        }
        code.Emit(0x83, 0xF8, 0x05);
        code.Branch(0x84, Prefix + "denied");
        code.Jump(Prefix + "unavailable");
    }

    private static void Emitˉlinuxˉerrorˉmapping(Fileˉcodeˉbuilder code, bool close)
    {
        var Prefix = close ? "close_" : "failure_";
        foreach (var Error in new[] { -22, -36 })
        {
            code.Emit(0x48, 0x83, 0xF8, unchecked((byte)Error));
            code.Branch(0x84, Prefix + "invalid");
        }
        foreach (var Error in new[] { -2, -20 })
        {
            code.Emit(0x48, 0x83, 0xF8, unchecked((byte)Error));
            code.Branch(0x84, Prefix + "not_found");
        }
        foreach (var Error in new[] { -1, -13, -21, -30 })
        {
            code.Emit(0x48, 0x83, 0xF8, unchecked((byte)Error));
            code.Branch(0x84, Prefix + "denied");
        }
        code.Emit(0x48, 0x83, 0xF8, unchecked((byte)-27));
        code.Branch(0x84, Prefix + "too_large");
        code.Jump(Prefix + "unavailable");
    }

    private static void Emitˉstoreˉdetail(
        Fileˉcodeˉbuilder code,
        Nativeˉserviceˉfailureˉdetail detail) =>
        code.Emit(0xC7, 0x44, 0x24, 0x58, (byte)detail, 0x00, 0x00, 0x00);

    private static void Emitˉstoreˉwindowsˉdetail(
        Fileˉcodeˉbuilder code,
        Nativeˉserviceˉfailureˉdetail detail) =>
        code.Emit(0xC7, 0x44, 0x24, 0x4C, (byte)detail, 0x00, 0x00, 0x00);

    private static void Emitˉdirectˉfailure(
        Fileˉcodeˉbuilder code,
        string label,
        Nativeˉserviceˉfailureˉdetail detail)
    {
        code.Mark(label);
        code.Emit(0x41, 0xC7, 0x47,
            Nativeˉexecutionˉcontextˉcontract.SERVICE_FAILURE_DETAIL_OFFSET,
            (byte)detail, 0x00, 0x00, 0x00);
        code.Emit(0xB8, 0x01, 0x00, 0x00, 0x00);
        code.Jump("return");
    }

    private sealed class Fileˉcodeˉbuilder(string platform)
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
            Span<byte> Encoded = stackalloc byte[sizeof(uint)];
            BinaryPrimitives.WriteUInt32LittleEndian(Encoded, value);
            Emit(Encoded);
        }

        public void Mark(string label)
        {
            if (!Labels.TryAdd(label, Bytes.Count))
            {
                throw new InvalidOperationException(
                    $"Duplicate native {platform} file-output label '{label}'.");
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
                        $"Native {platform} file-output label '{Patch.Label}' was not defined.");
                }
                BinaryPrimitives.WriteInt32LittleEndian(
                    Result.AsSpan(Patch.Offset),
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
