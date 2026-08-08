using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Runtime;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static void Nativeˉbyteˉentryˉinputˉruns()
    {
        const string Source = """
            module Nativeˉbyteˉentryˉinput profile portable;

            export fn Main(Input: bytes) -> bytes {
                let Length: u32 = Bytesˉlength(Input);
                if Length == 0u32 || Length == 4194304u32 {
                    return Input;
                }
                return Bytesˉconcat(
                    Bytesˉslice(Input, 1u32, Length - 1u32),
                    Bytesˉslice(Input, 0u32, 1u32)
                );
            }
            """;
        var Module = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(Source));
        var Reference = new Referenceˉruntime(
            Module,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults);
        var Fragment = X64ˉnativeˉbackend.Compile(Module).Fragment;
        var Shape = Nativeˉfragmentˉverifier.Verifyˉentryˉshape(Fragment);
        Equal(Nativeˉentryˉinputˉkind.Bytes, Shape.Input);
        Equal(Nativeˉentryˉresultˉkind.Descriptor, Shape.Result);

        var Parameterlessˉbytes = X64ˉnativeˉbackend.Compile(
            Moduleˉcodec.Readˉandˉverify(Compileˉsuccess("""
                module Nativeˉparameterlessˉbytes profile portable;
                data Value: bytes = [1, 2, 3];
                export fn Main() -> bytes { return Value; }
                """))).Fragment;
        Sequenceˉequal(
            new byte[] { 1, 2, 3 },
            X64ˉnativeˉexecutor.Executeˉbytes(Parameterlessˉbytes));
        Throwsˉnative(
            "WVN4011",
            () => _ = X64ˉnativeˉexecutor.Executeˉbytes(Parameterlessˉbytes, []));
        var Parameterlessˉscalar = X64ˉnativeˉbackend.Compile(
            Moduleˉcodec.Readˉandˉverify(Compileˉsuccess("""
                module Nativeˉparameterlessˉscalar profile portable;
                export fn Main() -> i32 { return 42; }
                """))).Fragment;
        Equal(42, X64ˉnativeˉexecutor.Executeˉi32(Parameterlessˉscalar));

        var Input = ImmutableArray.Create<byte>(0x10, 0x20, 0x30, 0x40);
        var Expected = Reference.Runˉmainˉbytes(Input).Bytes;
        Sequenceˉequal(new byte[] { 0x20, 0x30, 0x40, 0x10 }, Expected);
        Sequenceˉequal(Expected, X64ˉnativeˉexecutor.Executeˉbytes(Fragment, Input));

        var Empty = ImmutableArray<byte>.Empty;
        Sequenceˉequal(
            Reference.Runˉmainˉbytes(Empty).Bytes,
            X64ˉnativeˉexecutor.Executeˉbytes(Fragment, Empty));

        var Limit = ImmutableArray.CreateRange(
            new byte[Bytecodeˉlimits.MAX_BYTE_DATA_BYTES]);
        Sequenceˉequal(Limit, X64ˉnativeˉexecutor.Executeˉbytes(Fragment, Limit));

        Throwsˉnative("WVN4011", () => _ = X64ˉnativeˉexecutor.Executeˉbytes(Fragment));
        ImmutableArray<byte> Defaultˉinput = default;
        Throwsˉnative(
            "WVN4020",
            () => _ = X64ˉnativeˉexecutor.Executeˉbytes(Fragment, Defaultˉinput));
        Throwsˉnative(
            "WVN4020",
            () => _ = X64ˉnativeˉexecutor.Executeˉbytes(
                Fragment,
                ImmutableArray.CreateRange(
                    new byte[Bytecodeˉlimits.MAX_BYTE_DATA_BYTES + 1])));

        ReadOnlySpan<byte> Inputˉbridge = [
            0x4C, 0x8D, 0x41, Nativeˉcontract.VALUE_SLOT_BYTES,
        ];
        var Bridgeˉoffset = Fragment.Code.AsSpan().IndexOf(Inputˉbridge);
        True(Bridgeˉoffset >= 0, "The native byte entry omitted its exact input bridge.");
        var Corruptedˉcode = Fragment.Code.ToArray();
        Corruptedˉcode[Bridgeˉoffset + 3]++;
        Throwsˉnative(
            "WVN3030",
            () => _ = Nativeˉfragmentˉverifier.Verify(
                Fragment with { Code = Corruptedˉcode.ToImmutableArray() }));

        var Textˉinput = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess("""
            module Nativeˉtextˉentryˉinput profile portable;
            export fn Main(Input: text) -> bytes { return Textˉtoˉutf8(Input); }
            """));
        Throwsˉnative("WVN2002", () => _ = X64ˉnativeˉbackend.Compile(Textˉinput));
        var Scalarˉresult = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess("""
            module Nativeˉbyteˉentryˉscalar profile portable;
            export fn Main(Input: bytes) -> i32 { return 0; }
            """));
        Throwsˉnative("WVN2002", () => _ = X64ˉnativeˉbackend.Compile(Scalarˉresult));
    }

    private static void Nativeˉbyteˉdescriptorˉreturnsˉsurvive()
    {
        const string Source = """
            module Nativeˉbyteˉdescriptorˉreturns profile portable;

            data Values: bytes = [0, 1, 2, 3, 4, 5];

            fn Oneˉbyte(Index: u32) -> bytes {
                return Bytesˉslice(Values, Index, 1u32);
            }

            fn Append(Left: bytes, Right: bytes) -> bytes {
                return Bytesˉconcat(Left, Right);
            }

            fn Build() -> bytes {
                var Result: bytes = Textˉtoˉutf8("WVO1");
                Result = Append(Result, Oneˉbyte(1u32));
                Result = Append(Result, Oneˉbyte(2u32));
                Result = Append(Result, Bytesˉfromˉu32ˉlittle(3u32));
                return Result;
            }

            export fn Main() -> bytes {
                return Build();
            }
            """;
        var Module = Moduleˉcodec.Readˉandˉverify(Compileˉsuccess(Source));
        var Fragment = X64ˉnativeˉbackend.Compile(Module).Fragment;
        Equal(
            "57564F31010203000000",
            Convert.ToHexString(
                X64ˉnativeˉexecutor.Executeˉbytes(Fragment).AsSpan()).ToUpperInvariant());
    }
}
