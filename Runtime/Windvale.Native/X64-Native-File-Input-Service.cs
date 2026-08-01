using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Compiler.Native;

namespace Windvale.Runtime.Native;

public static class X64ˉnativeˉfileˉinputˉservice
{
    public const int WINDOWS_CANONICAL_SIZE = 1218;
    public const int LINUX_CANONICAL_SIZE = 996;
    public const string WINDOWS_CANONICAL_SHA256 =
        "3d2fffc028083cdc4cfd39e553dea603e9a1ae661bb5df3f14ca438c4d3e3cf8";
    public const string LINUX_CANONICAL_SHA256 =
        "55ae4524c463f064aee0964d7f9b64438701fb4375a97c53d11f2f17902c12cb";

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
        var Expectedˉsize = Canonicalˉsize(platform);
        var Expectedˉhash = Canonicalˉsha256(platform);
        var Actualˉhash = Convert.ToHexString(SHA256.HashData(code)).ToLowerInvariant();
        if (code.Length != Expectedˉsize ||
            !StringComparer.Ordinal.Equals(Actualˉhash, Expectedˉhash) ||
            !code.SequenceEqual(Expected.AsSpan()))
        {
            throw new InvalidOperationException(
                $"Native {platform} file-input service identity is " +
                $"{code.Length} bytes / {Actualˉhash}; expected " +
                $"{Expectedˉsize} bytes / {Expectedˉhash}.");
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

        // Preserve the Windvale counters plus every Windows nonvolatile register used below.
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
        Code.Emit(0x4C, 0x89, 0xC5);
        Code.Emit(0x49, 0x89, 0xCC);
        Code.Emit(0x44, 0x89, 0x4C, 0x24, 0x38);
        Code.Emit(0x41, 0xC7, 0x47,
            Nativeˉexecutionˉcontextˉcontract.SERVICE_FAILURE_DETAIL_OFFSET,
            0x00, 0x00, 0x00, 0x00);
        Code.Emit(0x49, 0x8B, 0x5F,
            Nativeˉexecutionˉcontextˉcontract.FILE_INPUT_TABLE_POINTER_OFFSET);
        Code.Emit(0x8B, 0x43, Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_COUNT_OFFSET);
        Code.Emit(0x89, 0x44, 0x24, 0x3C);
        Code.Emit(0xC7, 0x44, 0x24, 0x40, 0x00, 0x00, 0x00, 0x00);

        // Return an existing exact ordinal snapshot without reopening the resource.
        Code.Mark("cache_loop");
        Code.Emit(0x8B, 0x44, 0x24, 0x40);
        Code.Emit(0x3B, 0x44, 0x24, 0x3C);
        Code.Branch(0x83, "cache_miss");
        Code.Emit(0x48, 0x8B, 0x53,
            Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_TABLE_POINTER_OFFSET);
        Code.Emit(0x48, 0xC1, 0xE0, 0x05);
        Code.Emit(0x48, 0x01, 0xC2);
        Code.Emit(0x8B, 0x42, Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_NAME_LENGTH_OFFSET);
        Code.Emit(0x3B, 0x44, 0x24, 0x38);
        Code.Branch(0x85, "cache_next");
        Code.Emit(0x48, 0x8B, 0x32);
        Code.Emit(0x48, 0x89, 0xEF);
        Code.Emit(0x8B, 0x4C, 0x24, 0x38);
        Code.Emit(0xF3, 0xA6);
        Code.Branch(0x85, "cache_next");
        Emitˉwriteˉdescriptor(Code, recordˉregister: true);
        Code.Emit(0x31, 0xC0);
        Code.Jump("return");

        Code.Mark("cache_next");
        Code.Emit(0x83, 0x44, 0x24, 0x40, 0x01);
        Code.Jump("cache_loop");

        // Reject a 65th name, empty/oversized names, and embedded NUL before OS access.
        Code.Mark("cache_miss");
        Code.Emit(0x83, 0x7C, 0x24, 0x3C,
            (byte)Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_CAPACITY);
        Code.Branch(0x83, "failure_snapshot_limit");
        Code.Emit(0x83, 0x7C, 0x24, 0x38, 0x00);
        Code.Branch(0x84, "failure_invalid");
        Code.Emit(0x8B, 0x43, Nativeˉfileˉinputˉtableˉcontract.NAME_STRIDE_OFFSET);
        Code.Emit(0x39, 0x44, 0x24, 0x38);
        Code.Branch(0x87, "failure_invalid");
        Code.Emit(0x31, 0xC0);
        Code.Emit(0x48, 0x89, 0xEF);
        Code.Emit(0x8B, 0x4C, 0x24, 0x38);
        Code.Emit(0xF2, 0xAE);
        Code.Branch(0x84, "failure_invalid");

        // Convert the verified strict UTF-8 name into the execution-owned UTF-16 scratch.
        Code.Emit(0xB9, 0xE9, 0xFD, 0x00, 0x00);
        Code.Emit(0xBA, 0x08, 0x00, 0x00, 0x00);
        Code.Emit(0x49, 0x89, 0xE8);
        Code.Emit(0x44, 0x8B, 0x4C, 0x24, 0x38);
        Code.Emit(0x48, 0x8B, 0x43,
            Nativeˉfileˉinputˉtableˉcontract.SCRATCH_POINTER_OFFSET);
        Code.Emit(0x48, 0x89, 0x44, 0x24, 0x20);
        Code.Emit(0x8B, 0x43, Nativeˉfileˉinputˉtableˉcontract.SCRATCH_BYTES_OFFSET);
        Code.Emit(0xD1, 0xE8);
        Code.Emit(0x83, 0xE8, 0x01);
        Code.Emit(0x48, 0x89, 0x44, 0x24, 0x28);
        Code.Emit(0xFF, 0x53,
            Nativeˉfileˉinputˉtableˉcontract.WINDOWS_UTF8_TO_UTF16_POINTER_OFFSET);
        Code.Emit(0x85, 0xC0);
        Code.Branch(0x84, "failure_invalid");
        Code.Emit(0x48, 0x8B, 0x53,
            Nativeˉfileˉinputˉtableˉcontract.SCRATCH_POINTER_OFFSET);
        Code.Emit(0x66, 0xC7, 0x04, 0x42, 0x00, 0x00);

        // Open the path with the same read/share/sequential policy as the Stage 0 adapter.
        Code.Emit(0x48, 0x89, 0xD1);
        Code.Emit(0xBA, 0x00, 0x00, 0x00, 0x80);
        Code.Emit(0x41, 0xB8, 0x01, 0x00, 0x00, 0x00);
        Code.Emit(0x45, 0x31, 0xC9);
        Code.Emit(0x48, 0xC7, 0x44, 0x24, 0x20, 0x03, 0x00, 0x00, 0x00);
        Code.Emit(0x48, 0xC7, 0x44, 0x24, 0x28, 0x80, 0x00, 0x00, 0x08);
        Code.Emit(0x48, 0xC7, 0x44, 0x24, 0x30, 0x00, 0x00, 0x00, 0x00);
        Code.Emit(0xFF, 0x53, Nativeˉfileˉinputˉtableˉcontract.WINDOWS_OPEN_POINTER_OFFSET);
        Code.Emit(0x48, 0x83, 0xF8, 0xFF);
        Code.Branch(0x84, "open_error");
        Code.Emit(0x49, 0x89, 0xC5);

        // Reject a known oversized file before reading, then commit this snapshot's data slot.
        Code.Emit(0x4C, 0x89, 0xE9);
        Code.Emit(0x48, 0x8D, 0x54, 0x24, 0x50);
        Code.Emit(0xFF, 0x53, Nativeˉfileˉinputˉtableˉcontract.WINDOWS_SIZE_POINTER_OFFSET);
        Code.Emit(0x85, 0xC0);
        Code.Branch(0x84, "close_last_error");
        Code.Emit(0x48, 0x83, 0x7C, 0x24, 0x50, 0x00);
        Code.Branch(0x8C, "close_unavailable");
        Code.Emit(0x48, 0x81, 0x7C, 0x24, 0x50, 0x00, 0x00, 0x40, 0x00);
        Code.Branch(0x87, "close_too_large");
        Code.Emit(0x4C, 0x8B, 0x73,
            Nativeˉfileˉinputˉtableˉcontract.DATA_ARENA_POINTER_OFFSET);
        Code.Emit(0x8B, 0x44, 0x24, 0x3C);
        Code.Emit(0x48, 0xC1, 0xE0, 0x16);
        Code.Emit(0x49, 0x01, 0xC6);
        Code.Emit(0x4C, 0x89, 0xF1);
        Code.Emit(0xBA, 0x00, 0x00, 0x40, 0x00);
        Code.Emit(0x41, 0xB8, 0x00, 0x10, 0x00, 0x00);
        Code.Emit(0x41, 0xB9, 0x04, 0x00, 0x00, 0x00);
        Code.Emit(0xFF, 0x53,
            Nativeˉfileˉinputˉtableˉcontract.WINDOWS_COMMIT_POINTER_OFFSET);
        Code.Emit(0x4C, 0x39, 0xF0);
        Code.Branch(0x85, "close_unavailable");
        Code.Emit(0xC7, 0x44, 0x24, 0x44, 0x00, 0x00, 0x00, 0x00);

        // Read in bounded chunks, probing one extra byte at the exact limit.
        Code.Mark("read_loop");
        Code.Emit(0x8B, 0x44, 0x24, 0x44);
        Code.Emit(0x3D, 0x00, 0x00, 0x40, 0x00);
        Code.Branch(0x84, "overflow_probe");
        Code.Emit(0x41, 0xB8, 0x00, 0x00, 0x40, 0x00);
        Code.Emit(0x41, 0x29, 0xC0);
        Code.Emit(0x41, 0x81, 0xF8, 0x00, 0x00, 0x01, 0x00);
        Code.Branch(0x86, "read_request_ready");
        Code.Emit(0x41, 0xB8, 0x00, 0x00, 0x01, 0x00);
        Code.Mark("read_request_ready");
        Code.Emit(0x44, 0x89, 0x44, 0x24, 0x4C);
        Code.Emit(0x4C, 0x89, 0xE9);
        Code.Emit(0x4C, 0x89, 0xF2);
        Code.Emit(0x8B, 0x44, 0x24, 0x44);
        Code.Emit(0x48, 0x01, 0xC2);
        Code.Emit(0x4C, 0x8D, 0x4C, 0x24, 0x48);
        Code.Emit(0x48, 0xC7, 0x44, 0x24, 0x20, 0x00, 0x00, 0x00, 0x00);
        Code.Emit(0xFF, 0x53, Nativeˉfileˉinputˉtableˉcontract.WINDOWS_READ_POINTER_OFFSET);
        Code.Emit(0x85, 0xC0);
        Code.Branch(0x84, "close_last_error");
        Code.Emit(0x8B, 0x44, 0x24, 0x48);
        Code.Emit(0x85, 0xC0);
        Code.Branch(0x84, "success_close");
        Code.Emit(0x3B, 0x44, 0x24, 0x4C);
        Code.Branch(0x87, "close_unavailable");
        Code.Emit(0x01, 0x44, 0x24, 0x44);
        Code.Jump("read_loop");

        Code.Mark("overflow_probe");
        Code.Emit(0x4C, 0x89, 0xE9);
        Code.Emit(0x48, 0x8B, 0x53,
            Nativeˉfileˉinputˉtableˉcontract.SCRATCH_POINTER_OFFSET);
        Code.Emit(0x41, 0xB8, 0x01, 0x00, 0x00, 0x00);
        Code.Emit(0x4C, 0x8D, 0x4C, 0x24, 0x48);
        Code.Emit(0x48, 0xC7, 0x44, 0x24, 0x20, 0x00, 0x00, 0x00, 0x00);
        Code.Emit(0xFF, 0x53, Nativeˉfileˉinputˉtableˉcontract.WINDOWS_READ_POINTER_OFFSET);
        Code.Emit(0x85, 0xC0);
        Code.Branch(0x84, "close_last_error");
        Code.Emit(0x83, 0x7C, 0x24, 0x48, 0x00);
        Code.Branch(0x84, "success_close");
        Code.Jump("close_too_large");

        // Close the successful handle before publishing immutable cache metadata.
        Code.Mark("success_close");
        Code.Emit(0x4C, 0x89, 0xE9);
        Code.Emit(0xFF, 0x53, Nativeˉfileˉinputˉtableˉcontract.WINDOWS_CLOSE_POINTER_OFFSET);
        Code.Emit(0x85, 0xC0);
        Code.Branch(0x84, "failure_unavailable");

        // Commit/copy the exact UTF-8 name and publish one complete snapshot record.
        Code.Emit(0x48, 0x8B, 0x7B,
            Nativeˉfileˉinputˉtableˉcontract.NAME_ARENA_POINTER_OFFSET);
        Code.Emit(0x8B, 0x44, 0x24, 0x3C);
        Code.Emit(0x48, 0xC1, 0xE0, 0x14);
        Code.Emit(0x48, 0x01, 0xC7);
        Code.Emit(0x48, 0x89, 0xF9);
        Code.Emit(0x8B, 0x54, 0x24, 0x38);
        Code.Emit(0x41, 0xB8, 0x00, 0x10, 0x00, 0x00);
        Code.Emit(0x41, 0xB9, 0x04, 0x00, 0x00, 0x00);
        Code.Emit(0xFF, 0x53,
            Nativeˉfileˉinputˉtableˉcontract.WINDOWS_COMMIT_POINTER_OFFSET);
        Code.Emit(0x48, 0x39, 0xF8);
        Code.Branch(0x85, "failure_unavailable");
        Code.Emit(0x48, 0x89, 0x44, 0x24, 0x20);
        Code.Emit(0x48, 0x89, 0xEF);
        Code.Emit(0x48, 0x89, 0xEE);
        Code.Emit(0x48, 0x8B, 0x7C, 0x24, 0x20);
        Code.Emit(0x8B, 0x4C, 0x24, 0x38);
        Code.Emit(0xF3, 0xA4);
        Code.Emit(0x48, 0x8B, 0x53,
            Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_TABLE_POINTER_OFFSET);
        Code.Emit(0x8B, 0x44, 0x24, 0x3C);
        Code.Emit(0x48, 0xC1, 0xE0, 0x05);
        Code.Emit(0x48, 0x01, 0xC2);
        Code.Emit(0x48, 0x8B, 0x44, 0x24, 0x20);
        Code.Emit(0x48, 0x89, 0x02);
        Code.Emit(0x8B, 0x44, 0x24, 0x38);
        Code.Emit(0x89, 0x42, Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_NAME_LENGTH_OFFSET);
        Code.Emit(0xC7, 0x42,
            Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_NAME_RESERVED_OFFSET,
            0x00, 0x00, 0x00, 0x00);
        Code.Emit(0x4C, 0x89, 0x72,
            Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_DATA_POINTER_OFFSET);
        Code.Emit(0x8B, 0x44, 0x24, 0x44);
        Code.Emit(0x89, 0x42, Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_DATA_LENGTH_OFFSET);
        Code.Emit(0xC7, 0x42,
            Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_DATA_RESERVED_OFFSET,
            0x00, 0x00, 0x00, 0x00);
        Code.Emit(0x83, 0x43, Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_COUNT_OFFSET, 0x01);
        Code.Emit(0x4D, 0x89, 0x34, 0x24);
        Code.Emit(0x41, 0x89, 0x44, 0x24, Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET);
        Code.Emit(0x41, 0xC7, 0x44, 0x24, Nativeˉcontract.BORROWED_BYTES_RESERVED_OFFSET,
            0x00, 0x00, 0x00, 0x00);
        Code.Emit(0x31, 0xC0);
        Code.Jump("return");

        // Classify exact Windows path/open/read failures into stable Windvale details.
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
        Emitˉstoreˉdetail(Code, Nativeˉserviceˉfailureˉdetail.Fileˉinvalidˉname);
        Code.Jump("close_failure");
        Code.Mark("close_not_found");
        Emitˉstoreˉdetail(Code, Nativeˉserviceˉfailureˉdetail.Fileˉnotˉfound);
        Code.Jump("close_failure");
        Code.Mark("close_denied");
        Emitˉstoreˉdetail(Code, Nativeˉserviceˉfailureˉdetail.Fileˉpermissionˉdenied);
        Code.Jump("close_failure");
        Code.Mark("close_unavailable");
        Emitˉstoreˉdetail(Code, Nativeˉserviceˉfailureˉdetail.Fileˉunavailable);
        Code.Jump("close_failure");
        Code.Mark("close_too_large");
        Emitˉstoreˉdetail(Code, Nativeˉserviceˉfailureˉdetail.Fileˉtooˉlarge);

        Code.Mark("close_failure");
        Code.Emit(0x4C, 0x89, 0xE9);
        Code.Emit(0xFF, 0x53, Nativeˉfileˉinputˉtableˉcontract.WINDOWS_CLOSE_POINTER_OFFSET);
        Code.Emit(0x8B, 0x44, 0x24, 0x4C);
        Code.Emit(0x41, 0x89, 0x47,
            Nativeˉexecutionˉcontextˉcontract.SERVICE_FAILURE_DETAIL_OFFSET);
        Code.Emit(0xB8, 0x01, 0x00, 0x00, 0x00);
        Code.Jump("return");

        Emitˉdirectˉfailure(Code, "failure_invalid", Nativeˉserviceˉfailureˉdetail.Fileˉinvalidˉname);
        Emitˉdirectˉfailure(Code, "failure_not_found", Nativeˉserviceˉfailureˉdetail.Fileˉnotˉfound);
        Emitˉdirectˉfailure(Code, "failure_denied", Nativeˉserviceˉfailureˉdetail.Fileˉpermissionˉdenied);
        Emitˉdirectˉfailure(Code, "failure_unavailable", Nativeˉserviceˉfailureˉdetail.Fileˉunavailable);
        Emitˉdirectˉfailure(Code, "failure_snapshot_limit", Nativeˉserviceˉfailureˉdetail.Fileˉsnapshotˉlimit);

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
        Code.Emit(0x48, 0x81, 0xEC, 0x80, 0x00, 0x00, 0x00);
        Code.Emit(0x4C, 0x89, 0x14, 0x24);
        Code.Emit(0x4C, 0x89, 0x5C, 0x24, 0x08);
        Code.Emit(0x4C, 0x89, 0x7C, 0x24, 0x10);
        Code.Emit(0x4C, 0x89, 0x44, 0x24, 0x18);
        Code.Emit(0x48, 0x89, 0x4C, 0x24, 0x20);
        Code.Emit(0x44, 0x89, 0x4C, 0x24, 0x4C);
        Code.Emit(0x41, 0xC7, 0x47,
            Nativeˉexecutionˉcontextˉcontract.SERVICE_FAILURE_DETAIL_OFFSET,
            0x00, 0x00, 0x00, 0x00);
        Code.Emit(0x49, 0x8B, 0x47,
            Nativeˉexecutionˉcontextˉcontract.FILE_INPUT_TABLE_POINTER_OFFSET);
        Code.Emit(0x48, 0x89, 0x44, 0x24, 0x28);
        Code.Emit(0x8B, 0x40, Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_COUNT_OFFSET);
        Code.Emit(0x89, 0x44, 0x24, 0x30);
        Code.Emit(0xC7, 0x44, 0x24, 0x34, 0x00, 0x00, 0x00, 0x00);

        Code.Mark("cache_loop");
        Code.Emit(0x8B, 0x44, 0x24, 0x34);
        Code.Emit(0x3B, 0x44, 0x24, 0x30);
        Code.Branch(0x83, "cache_miss");
        Code.Emit(0x48, 0x8B, 0x54, 0x24, 0x28);
        Code.Emit(0x48, 0x8B, 0x52,
            Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_TABLE_POINTER_OFFSET);
        Code.Emit(0x48, 0xC1, 0xE0, 0x05);
        Code.Emit(0x48, 0x01, 0xC2);
        Code.Emit(0x8B, 0x42, Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_NAME_LENGTH_OFFSET);
        Code.Emit(0x3B, 0x44, 0x24, 0x4C);
        Code.Branch(0x85, "cache_next");
        Code.Emit(0x48, 0x8B, 0x32);
        Code.Emit(0x48, 0x8B, 0x7C, 0x24, 0x18);
        Code.Emit(0x8B, 0x4C, 0x24, 0x4C);
        Code.Emit(0xF3, 0xA6);
        Code.Branch(0x85, "cache_next");
        Emitˉwriteˉdescriptorˉlinux(Code);
        Code.Emit(0x31, 0xC0);
        Code.Jump("return");

        Code.Mark("cache_next");
        Code.Emit(0x83, 0x44, 0x24, 0x34, 0x01);
        Code.Jump("cache_loop");

        Code.Mark("cache_miss");
        Code.Emit(0x83, 0x7C, 0x24, 0x30,
            (byte)Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_CAPACITY);
        Code.Branch(0x83, "failure_snapshot_limit");
        Code.Emit(0x83, 0x7C, 0x24, 0x4C, 0x00);
        Code.Branch(0x84, "failure_invalid");
        Code.Emit(0x48, 0x8B, 0x44, 0x24, 0x28);
        Code.Emit(0x8B, 0x50, Nativeˉfileˉinputˉtableˉcontract.NAME_STRIDE_OFFSET);
        Code.Emit(0x39, 0x54, 0x24, 0x4C);
        Code.Branch(0x87, "failure_invalid");
        Code.Emit(0x31, 0xC0);
        Code.Emit(0x48, 0x8B, 0x7C, 0x24, 0x18);
        Code.Emit(0x8B, 0x4C, 0x24, 0x4C);
        Code.Emit(0xF2, 0xAE);
        Code.Branch(0x84, "failure_invalid");
        Code.Emit(0x48, 0x8B, 0x44, 0x24, 0x28);
        Code.Emit(0x48, 0x8B, 0x78,
            Nativeˉfileˉinputˉtableˉcontract.SCRATCH_POINTER_OFFSET);
        Code.Emit(0x48, 0x89, 0x7C, 0x24, 0x60);
        Code.Emit(0x48, 0x8B, 0x74, 0x24, 0x18);
        Code.Emit(0x8B, 0x4C, 0x24, 0x4C);
        Code.Emit(0xF3, 0xA4);
        Code.Emit(0xC6, 0x07, 0x00);

        // openat(AT_FDCWD, path, O_RDONLY | O_CLOEXEC, 0)
        Code.Emit(0xB8, 0x01, 0x01, 0x00, 0x00);
        Code.Emit(0x48, 0xC7, 0xC7, 0x9C, 0xFF, 0xFF, 0xFF);
        Code.Emit(0x48, 0x8B, 0x74, 0x24, 0x60);
        Code.Emit(0xBA, 0x00, 0x00, 0x08, 0x00);
        Code.Emit(0x45, 0x31, 0xD2);
        Code.Emit(0x0F, 0x05);
        Code.Emit(0x48, 0x85, 0xC0);
        Code.Branch(0x88, "open_error");
        Code.Emit(0x48, 0x89, 0x44, 0x24, 0x38);
        Code.Emit(0x48, 0x8B, 0x44, 0x24, 0x28);
        Code.Emit(0x48, 0x8B, 0x40,
            Nativeˉfileˉinputˉtableˉcontract.DATA_ARENA_POINTER_OFFSET);
        Code.Emit(0x8B, 0x4C, 0x24, 0x30);
        Code.Emit(0x48, 0xC1, 0xE1, 0x16);
        Code.Emit(0x48, 0x01, 0xC8);
        Code.Emit(0x48, 0x89, 0x44, 0x24, 0x40);
        Code.Emit(0xC7, 0x44, 0x24, 0x48, 0x00, 0x00, 0x00, 0x00);

        Code.Mark("read_loop");
        Code.Emit(0x8B, 0x44, 0x24, 0x48);
        Code.Emit(0x3D, 0x00, 0x00, 0x40, 0x00);
        Code.Branch(0x84, "overflow_probe");
        Code.Emit(0xBA, 0x00, 0x00, 0x40, 0x00);
        Code.Emit(0x29, 0xC2);
        Code.Emit(0x81, 0xFA, 0x00, 0x00, 0x01, 0x00);
        Code.Branch(0x86, "read_request_ready");
        Code.Emit(0xBA, 0x00, 0x00, 0x01, 0x00);
        Code.Mark("read_request_ready");
        Code.Emit(0x89, 0x54, 0x24, 0x68);
        Code.Mark("read_call");
        Code.Emit(0x31, 0xC0);
        Code.Emit(0x48, 0x8B, 0x7C, 0x24, 0x38);
        Code.Emit(0x48, 0x8B, 0x74, 0x24, 0x40);
        Code.Emit(0x8B, 0x4C, 0x24, 0x48);
        Code.Emit(0x48, 0x01, 0xCE);
        Code.Emit(0x8B, 0x54, 0x24, 0x68);
        Code.Emit(0x0F, 0x05);
        Code.Emit(0x48, 0x83, 0xF8, 0xFC);
        Code.Branch(0x84, "read_call");
        Code.Emit(0x48, 0x85, 0xC0);
        Code.Branch(0x88, "read_error");
        Code.Branch(0x84, "success_close");
        Code.Emit(0x8B, 0x54, 0x24, 0x68);
        Code.Emit(0x48, 0x39, 0xD0);
        Code.Branch(0x87, "close_unavailable");
        Code.Emit(0x01, 0x44, 0x24, 0x48);
        Code.Jump("read_loop");

        Code.Mark("overflow_probe");
        Code.Mark("overflow_call");
        Code.Emit(0x31, 0xC0);
        Code.Emit(0x48, 0x8B, 0x7C, 0x24, 0x38);
        Code.Emit(0x48, 0x8B, 0x74, 0x24, 0x60);
        Code.Emit(0xBA, 0x01, 0x00, 0x00, 0x00);
        Code.Emit(0x0F, 0x05);
        Code.Emit(0x48, 0x83, 0xF8, 0xFC);
        Code.Branch(0x84, "overflow_call");
        Code.Emit(0x48, 0x85, 0xC0);
        Code.Branch(0x88, "read_error");
        Code.Branch(0x84, "success_close");
        Code.Jump("close_too_large");

        Code.Mark("success_close");
        Code.Emit(0xB8, 0x03, 0x00, 0x00, 0x00);
        Code.Emit(0x48, 0x8B, 0x7C, 0x24, 0x38);
        Code.Emit(0x0F, 0x05);
        Code.Emit(0x48, 0x85, 0xC0);
        Code.Branch(0x88, "failure_unavailable");

        // Cache the successful exact name and immutable byte descriptor.
        Code.Emit(0x48, 0x8B, 0x44, 0x24, 0x28);
        Code.Emit(0x48, 0x8B, 0x78,
            Nativeˉfileˉinputˉtableˉcontract.NAME_ARENA_POINTER_OFFSET);
        Code.Emit(0x8B, 0x4C, 0x24, 0x30);
        Code.Emit(0x48, 0xC1, 0xE1, 0x14);
        Code.Emit(0x48, 0x01, 0xCF);
        Code.Emit(0x48, 0x89, 0x7C, 0x24, 0x60);
        Code.Emit(0x48, 0x8B, 0x74, 0x24, 0x18);
        Code.Emit(0x8B, 0x4C, 0x24, 0x4C);
        Code.Emit(0xF3, 0xA4);
        Code.Emit(0x48, 0x8B, 0x50,
            Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_TABLE_POINTER_OFFSET);
        Code.Emit(0x8B, 0x4C, 0x24, 0x30);
        Code.Emit(0x48, 0xC1, 0xE1, 0x05);
        Code.Emit(0x48, 0x01, 0xCA);
        Code.Emit(0x48, 0x8B, 0x7C, 0x24, 0x60);
        Code.Emit(0x48, 0x89, 0x3A);
        Code.Emit(0x8B, 0x4C, 0x24, 0x4C);
        Code.Emit(0x89, 0x4A, Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_NAME_LENGTH_OFFSET);
        Code.Emit(0xC7, 0x42,
            Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_NAME_RESERVED_OFFSET,
            0x00, 0x00, 0x00, 0x00);
        Code.Emit(0x48, 0x8B, 0x7C, 0x24, 0x40);
        Code.Emit(0x48, 0x89, 0x7A,
            Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_DATA_POINTER_OFFSET);
        Code.Emit(0x8B, 0x4C, 0x24, 0x48);
        Code.Emit(0x89, 0x4A, Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_DATA_LENGTH_OFFSET);
        Code.Emit(0xC7, 0x42,
            Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_DATA_RESERVED_OFFSET,
            0x00, 0x00, 0x00, 0x00);
        Code.Emit(0x83, 0x40, Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_COUNT_OFFSET, 0x01);
        Code.Emit(0x48, 0x8B, 0x54, 0x24, 0x20);
        Code.Emit(0x48, 0x89, 0x3A);
        Code.Emit(0x89, 0x4A, Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET);
        Code.Emit(0xC7, 0x42, Nativeˉcontract.BORROWED_BYTES_RESERVED_OFFSET,
            0x00, 0x00, 0x00, 0x00);
        Code.Emit(0x31, 0xC0);
        Code.Jump("return");

        Code.Mark("open_error");
        Emitˉlinuxˉerrorˉmapping(Code, close: false);
        Code.Mark("read_error");
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
        Code.Mark("close_unavailable");
        Emitˉstoreˉdetail(Code, Nativeˉserviceˉfailureˉdetail.Fileˉunavailable);
        Code.Jump("close_failure");
        Code.Mark("close_too_large");
        Emitˉstoreˉdetail(Code, Nativeˉserviceˉfailureˉdetail.Fileˉtooˉlarge);

        Code.Mark("close_failure");
        Code.Emit(0xB8, 0x03, 0x00, 0x00, 0x00);
        Code.Emit(0x48, 0x8B, 0x7C, 0x24, 0x38);
        Code.Emit(0x0F, 0x05);
        Code.Emit(0x8B, 0x44, 0x24, 0x4C);
        Code.Emit(0x41, 0x89, 0x47,
            Nativeˉexecutionˉcontextˉcontract.SERVICE_FAILURE_DETAIL_OFFSET);
        Code.Emit(0xB8, 0x01, 0x00, 0x00, 0x00);
        Code.Jump("return");

        Emitˉdirectˉfailure(Code, "failure_invalid", Nativeˉserviceˉfailureˉdetail.Fileˉinvalidˉname);
        Emitˉdirectˉfailure(Code, "failure_not_found", Nativeˉserviceˉfailureˉdetail.Fileˉnotˉfound);
        Emitˉdirectˉfailure(Code, "failure_denied", Nativeˉserviceˉfailureˉdetail.Fileˉpermissionˉdenied);
        Emitˉdirectˉfailure(Code, "failure_unavailable", Nativeˉserviceˉfailureˉdetail.Fileˉunavailable);
        Emitˉdirectˉfailure(Code, "failure_snapshot_limit", Nativeˉserviceˉfailureˉdetail.Fileˉsnapshotˉlimit);

        Code.Mark("return");
        Code.Emit(0x4C, 0x8B, 0x14, 0x24);
        Code.Emit(0x4C, 0x8B, 0x5C, 0x24, 0x08);
        Code.Emit(0x4C, 0x8B, 0x7C, 0x24, 0x10);
        Code.Emit(0x48, 0x81, 0xC4, 0x80, 0x00, 0x00, 0x00);
        Code.Emit(0xC3);
        return Code.Finish();
    }

    private static void Emitˉwriteˉdescriptor(Fileˉcodeˉbuilder code, bool recordˉregister)
    {
        if (!recordˉregister)
        {
            throw new ArgumentOutOfRangeException(nameof(recordˉregister));
        }
        code.Emit(0x48, 0x8B, 0x42,
            Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_DATA_POINTER_OFFSET);
        code.Emit(0x49, 0x89, 0x04, 0x24);
        code.Emit(0x8B, 0x42, Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_DATA_LENGTH_OFFSET);
        code.Emit(0x41, 0x89, 0x44, 0x24, Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET);
        code.Emit(0x41, 0xC7, 0x44, 0x24, Nativeˉcontract.BORROWED_BYTES_RESERVED_OFFSET,
            0x00, 0x00, 0x00, 0x00);
    }

    private static void Emitˉwriteˉdescriptorˉlinux(Fileˉcodeˉbuilder code)
    {
        code.Emit(0x48, 0x8B, 0x42,
            Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_DATA_POINTER_OFFSET);
        code.Emit(0x48, 0x8B, 0x7C, 0x24, 0x20);
        code.Emit(0x48, 0x89, 0x07);
        code.Emit(0x8B, 0x42, Nativeˉfileˉinputˉtableˉcontract.SNAPSHOT_DATA_LENGTH_OFFSET);
        code.Emit(0x89, 0x47, Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET);
        code.Emit(0xC7, 0x47, Nativeˉcontract.BORROWED_BYTES_RESERVED_OFFSET,
            0x00, 0x00, 0x00, 0x00);
    }

    private static void Emitˉwindowsˉlastˉerror(Fileˉcodeˉbuilder code) =>
        code.Emit(0xFF, 0x93, 0x80, 0x00, 0x00, 0x00);

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
        foreach (var Error in new[] { -1, -13, -21 })
        {
            code.Emit(0x48, 0x83, 0xF8, unchecked((byte)Error));
            code.Branch(0x84, Prefix + "denied");
        }
        code.Jump(Prefix + "unavailable");
    }

    private static void Emitˉstoreˉdetail(
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
                    $"Duplicate native {platform} file-input label '{label}'.");
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
                        $"Unknown native {platform} file-input label '{Patch.Label}'.");
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
