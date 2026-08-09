using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.ObjectModel;
using Windvale.Runtime;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static void Windvaleˉuefiˉapplicationˉconstructionˉruns()
    {
        Sourceˉmoduleˉinput Source(string path, string resource) =>
            new(path, Readˉembeddedˉsource($"Windvale.Seed.Tests.{resource}"));

        var Byteˉconstruction = Source(
            "Foundation/Byte-Construction.wv",
            "Byte-Construction.wv");
        var Verificationˉinput = Source(
            "Linker/Windvale/Uefi-Application-Verification-Core.wv",
            "Uefi-Application-Verification-Core.wv");
        var Verificationˉbridgeˉinput = Source(
            "Linker/Windvale/Uefi-Application-Verification-Bridge.wv",
            "Uefi-Application-Verification-Bridge.wv");
        var Constructionˉinput = Source(
            "Linker/Windvale/Uefi-Application-Construction-Core.wv",
            "Uefi-Application-Construction-Core.wv");

        var Verificationˉcompilation = Seedˉcompiler.Compileˉmodules(
            Verificationˉbridgeˉinput,
            [Verificationˉinput]);
        True(
            Verificationˉcompilation.Success,
            "The Windvale UEFI verifier did not compile: " +
                string.Join(" | ", Verificationˉcompilation.Diagnostics));
        var Constructionˉcompilation = Seedˉcompiler.Compileˉmodules(
            Constructionˉinput,
            [Byteˉconstruction, Verificationˉinput]);
        True(
            Constructionˉcompilation.Success,
            "The Windvale UEFI constructor did not compile: " +
                string.Join(" | ", Constructionˉcompilation.Diagnostics));

        var Verificationˉmodule = Moduleˉcodec.Readˉandˉverify(
            Verificationˉcompilation.Moduleˉbytes.AsSpan());
        var Constructionˉmodule = Moduleˉcodec.Readˉandˉverify(
            Constructionˉcompilation.Moduleˉbytes.AsSpan());
        Equal(Moduleˉprofile.Portable, Verificationˉmodule.Module.Profile);
        Equal(Moduleˉprofile.Portable, Constructionˉmodule.Module.Profile);
        True(
            Verificationˉmodule.Module.Capabilities.IsEmpty,
            "The Windvale UEFI verifier requires a capability.");
        True(
            Constructionˉmodule.Module.Capabilities.IsEmpty,
            "The Windvale UEFI constructor requires a capability.");

        var Verificationˉruntime = new Referenceˉruntime(
            Verificationˉmodule,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults);
        var Constructionˉruntime = new Referenceˉruntime(
            Constructionˉmodule,
            new Referenceˉcapabilityˉhost(TextWriter.Null),
            Runtimeˉoptions.Portableˉdefaults);
        var Verificationˉnative = X64ˉnativeˉbackend.Compile(Verificationˉmodule).Fragment;
        var Constructionˉnative = X64ˉnativeˉbackend.Compile(Constructionˉmodule).Fragment;
        Equal(
            new Nativeˉentryˉshape(
                Nativeˉentryˉinputˉkind.Bytes,
                Nativeˉentryˉresultˉkind.Descriptor),
            Nativeˉfragmentˉverifier.Verifyˉentryˉshape(Verificationˉnative));
        Equal(
            new Nativeˉentryˉshape(
                Nativeˉentryˉinputˉkind.Bytes,
                Nativeˉentryˉresultˉkind.Descriptor),
            Nativeˉfragmentˉverifier.Verifyˉentryˉshape(Constructionˉnative));

        static ImmutableArray<byte> Request(ImmutableArray<byte> code, uint entryˉoffset)
        {
            var Result = new byte[checked(16 + code.Length)];
            BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(), 1_381_324_375);
            BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(4), 1);
            BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8), (uint)Result.Length);
            BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(12), entryˉoffset);
            code.AsSpan().CopyTo(Result.AsSpan(16));
            return Result.ToImmutableArray();
        }

        static ImmutableArray<byte> Linkˉapplication(
            ImmutableArray<byte> code,
            uint entryˉoffset)
        {
            var Objectˉbytes = Objectˉcodec.Write(new Objectˉfile(
                Objectˉarchitecture.X86ˉ64,
                [new(".text", Objectˉsectionˉkind.Code, 16, (uint)code.Length, code)],
                [new(
                    "Main",
                    Objectˉsymbolˉbinding.Export,
                    Objectˉsymbolˉkind.Function,
                    0,
                    entryˉoffset,
                    (uint)code.Length - entryˉoffset)],
                [])).ToImmutableArray();
            var Linked = Linkˉcompiler.Link([new(Objectˉbytes)], new(0, "Main"));
            True(Linked.Success, "The UEFI differential fixture did not link.");
            var Application = Uefiˉapplicationˉwriter.Write(Linked);
            True(
                Application.Success,
                Application.Diagnostics.IsEmpty
                    ? "The Stage 0 UEFI writer rejected the differential fixture."
                    : Application.Diagnostics[0].Message);
            return Application.Imageˉbytes;
        }

        ImmutableArray<byte> Construct(ImmutableArray<byte> code, uint entryˉoffset)
        {
            var Input = Request(code, entryˉoffset);
            var Interpreted = Constructionˉruntime.Runˉmainˉbytes(Input).Bytes;
            var Native = X64ˉnativeˉexecutor.Executeˉbytes(Constructionˉnative, Input);
            Sequenceˉequal(Interpreted, Native);
            Sequenceˉequal(
                Interpreted,
                Constructionˉruntime.Runˉmainˉbytes(Input).Bytes);
            Equal(1_129_666_135u, BinaryPrimitives.ReadUInt32LittleEndian(Interpreted.AsSpan()));
            Equal(1u, BinaryPrimitives.ReadUInt32LittleEndian(Interpreted.AsSpan()[4..]));
            Equal((uint)Interpreted.Length, BinaryPrimitives.ReadUInt32LittleEndian(Interpreted.AsSpan()[8..]));
            Equal(0u, BinaryPrimitives.ReadUInt32LittleEndian(Interpreted.AsSpan()[12..]));
            Equal(entryˉoffset, BinaryPrimitives.ReadUInt32LittleEndian(Interpreted.AsSpan()[20..]));
            Equal((uint)Interpreted.Length - 32u, BinaryPrimitives.ReadUInt32LittleEndian(Interpreted.AsSpan()[24..]));
            Equal(0u, BinaryPrimitives.ReadUInt32LittleEndian(Interpreted.AsSpan()[28..]));
            return Interpreted[32..];
        }

        ImmutableArray<byte> Verify(ImmutableArray<byte> image, uint status)
        {
            var Interpreted = Verificationˉruntime.Runˉmainˉbytes(image).Bytes;
            var Native = X64ˉnativeˉexecutor.Executeˉbytes(Verificationˉnative, image);
            Sequenceˉequal(Interpreted, Native);
            Equal(1_448_433_239u, BinaryPrimitives.ReadUInt32LittleEndian(Interpreted.AsSpan()));
            Equal(1u, BinaryPrimitives.ReadUInt32LittleEndian(Interpreted.AsSpan()[4..]));
            Equal((uint)Interpreted.Length, BinaryPrimitives.ReadUInt32LittleEndian(Interpreted.AsSpan()[8..]));
            Equal(status, BinaryPrimitives.ReadUInt32LittleEndian(Interpreted.AsSpan()[12..]));
            Equal((uint)Interpreted.Length - 32u, BinaryPrimitives.ReadUInt32LittleEndian(Interpreted.AsSpan()[24..]));
            Equal(0u, BinaryPrimitives.ReadUInt32LittleEndian(Interpreted.AsSpan()[28..]));
            return Interpreted[32..];
        }

        ImmutableArray<byte> Tinyˉcode = [0x31, 0xC0, 0xC3];
        var Tiny = Construct(Tinyˉcode, 0);
        Equal(1_536, Tiny.Length);
        Sequenceˉequal(Linkˉapplication(Tinyˉcode, 0), Tiny);
        Sequenceˉequal(Tinyˉcode, Verify(Tiny, 0));

        ImmutableArray<byte> Offsetˉcode = [0x90, 0xC3, 3, 5, 8, 13];
        var Offset = Construct(Offsetˉcode, 1);
        Sequenceˉequal(Linkˉapplication(Offsetˉcode, 1), Offset);
        Sequenceˉequal(Offsetˉcode, Verify(Offset, 0));
        var Offsetˉverification = Verificationˉruntime.Runˉmainˉbytes(Offset).Bytes;
        Equal(1u, BinaryPrimitives.ReadUInt32LittleEndian(Offsetˉverification.AsSpan()[20..]));

        static ImmutableArray<byte> Mutate(ImmutableArray<byte> input, int offset)
        {
            var Result = input.ToArray();
            Result[offset] ^= 1;
            return Result.ToImmutableArray();
        }

        Equal(0, Verify(Tiny[..^1], 1).Length);
        Equal(0, Verify(Mutate(Tiny, 0x00), 2).Length);
        Equal(0, Verify(Mutate(Tiny, 0x80), 3).Length);
        Equal(0, Verify(Mutate(Tiny, 0x98), 4).Length);
        Equal(0, Verify(Mutate(Tiny, 0x188), 5).Length);
        Equal(0, Verify(Mutate(Tiny, 0x400), 6).Length);
        Equal(0, Verify(Mutate(Tiny, 0x203), 7).Length);
        Equal(0, Verify(Mutate(Tiny, 0x40C), 7).Length);
        Equal(0, Verify(Tiny.Add(0), 1).Length);

        void Expectˉconstructionˉfailure(ImmutableArray<byte> request, uint status)
        {
            var Interpreted = Constructionˉruntime.Runˉmainˉbytes(request).Bytes;
            Sequenceˉequal(
                Interpreted,
                X64ˉnativeˉexecutor.Executeˉbytes(Constructionˉnative, request));
            Equal(32, Interpreted.Length);
            Equal(status, BinaryPrimitives.ReadUInt32LittleEndian(Interpreted.AsSpan()[12..]));
        }

        var Validˉrequest = Request(Tinyˉcode, 0);
        Expectˉconstructionˉfailure(Validˉrequest[..16], 1);
        Expectˉconstructionˉfailure(Mutate(Validˉrequest, 0), 2);
        Expectˉconstructionˉfailure(Mutate(Validˉrequest, 4), 3);
        Expectˉconstructionˉfailure(Mutate(Validˉrequest, 8), 1);
        Expectˉconstructionˉfailure(Request(Tinyˉcode, (uint)Tinyˉcode.Length), 4);

        var Repository = Findˉrepositoryˉroot();
        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-uefi-application-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Verificationˉpath = Path.Combine(Directoryˉpath, "Verification.wvb");
            var Nativeˉverification = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Windvale-Native-Uefi-Application-Verification.wvproj"),
                Verificationˉpath);
            Equal(0, Nativeˉverification.Exitˉcode);
            Equal(string.Empty, Nativeˉverification.Error);
            Sequenceˉequal(
                Verificationˉcompilation.Moduleˉbytes,
                File.ReadAllBytes(Verificationˉpath));

            var Constructionˉpath = Path.Combine(Directoryˉpath, "Construction.wvb");
            var Nativeˉconstruction = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Windvale-Native-Uefi-Application-Construction.wvproj"),
                Constructionˉpath);
            Equal(0, Nativeˉconstruction.Exitˉcode);
            Equal(string.Empty, Nativeˉconstruction.Error);
            Sequenceˉequal(
                Constructionˉcompilation.Moduleˉbytes,
                File.ReadAllBytes(Constructionˉpath));
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }
}
