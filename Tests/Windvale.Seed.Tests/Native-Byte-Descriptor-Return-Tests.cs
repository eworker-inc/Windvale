using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
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
