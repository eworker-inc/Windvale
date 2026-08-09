using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.Compiler.Native;
using Windvale.Linker;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static ImmutableArray<byte> Buildˉhostedˉstartupˉresponse(
        Consoleˉapplicationˉtarget target,
        ImmutableArray<byte> plan)
    {
        uint Read(int offset) => BinaryPrimitives.ReadUInt32LittleEndian(
            plan.AsSpan()[offset..]);
        var Targets = ImmutableArray.CreateBuilder<uint>(checked((int)(Read(100) / 4)));
        for (var Offset = 128; Offset < checked(128 + (int)Read(100)); Offset += 4)
        {
            Targets.Add(BinaryPrimitives.ReadUInt32LittleEndian(
                plan.AsSpan()[Offset..]));
        }
        var Object = target == Consoleˉapplicationˉtarget.Windowsˉx64
            ? Nativeˉhostedˉstartupˉinstantiator.Readˉobject(
                typeof(Windowsˉhostedˉcompilerˉstartup),
                "Windvale.Linker.Windows-X64-Hosted-Compiler.wvo",
                Windowsˉhostedˉcompilerˉstartup.WVO_BYTES,
                Windowsˉhostedˉcompilerˉstartup.WVO_SHA256)
            : Nativeˉhostedˉstartupˉinstantiator.Readˉobject(
                typeof(Linuxˉhostedˉcompilerˉstartup),
                "Windvale.Linker.Linux-X64-Hosted-Compiler.wvo",
                Linuxˉhostedˉcompilerˉstartup.WVO_BYTES,
                Linuxˉhostedˉcompilerˉstartup.WVO_SHA256);
        var Inputs = new Nativeˉhostedˉstartupˉinputs(
            Read(80),
            Read(44),
            target == Consoleˉapplicationˉtarget.Windowsˉx64 ? 40u : 26u,
            Targets.ToImmutable(),
            Object);
        return Nativeˉhostedˉstartupˉinstantiator.Buildˉwithˉwindvale(
            Nativeˉhostedˉstartupˉinstantiator.Buildˉrequest(Inputs));
    }

    private static void Verifyˉhostedˉcontainerˉsourceˉmanifest(
        byte[] manifest,
        ImmutableArray<byte> plan,
        ImmutableArray<byte[]>.Builder resources)
    {
        uint Read(int offset) => BinaryPrimitives.ReadUInt32LittleEndian(
            manifest.AsSpan(offset));
        uint Plan(int offset) => BinaryPrimitives.ReadUInt32LittleEndian(
            plan.AsSpan()[offset..]);
        Equal(0x4753_5657u, Read(0));
        Equal(1u, Read(4));
        Equal((uint)manifest.Length, Read(8));
        Equal((uint)resources.Count, Read(12));
        Equal(6u, Read(16));
        Equal(Plan(28), Read(24));
        var Logical = 0u;
        for (var Index = 0; Index < resources.Count; Index++)
        {
            var Record = 32 + Index * 20;
            Equal((uint)Index, Read(Record));
            Equal(Logical, Read(Record + 4));
            Equal(0u, Read(Record + 8));
            Equal((uint)resources[Index].Length, Read(Record + 12));
            Equal((uint)resources[Index].Length, Read(Record + 16));
            Logical += (uint)resources[Index].Length;
        }
        Equal(Logical, Read(20));
        var Regionˉbase = 32 + resources.Count * 20;
        var Regionˉlogical = 0u;
        for (var Index = 0; Index < 6; Index++)
        {
            var Record = Regionˉbase + Index * 16;
            var Planˉoffset = Index == 0 ? 32 : 32 + Index * 8;
            var Imageˉoffset = Index == 0 ? 0u : Plan(Planˉoffset);
            var Regionˉbytes = Plan(Planˉoffset + 4);
            if (Regionˉbytes == 0 && Index == 3)
            {
                Imageˉoffset = Plan(48) + Plan(52);
            }
            else if (Regionˉbytes == 0 && Index == 5)
            {
                Imageˉoffset = Plan(64) + Plan(68);
            }
            Equal((uint)Index, Read(Record));
            Equal(Regionˉlogical, Read(Record + 4));
            Equal(Imageˉoffset, Read(Record + 8));
            Equal(Regionˉbytes, Read(Record + 12));
            Regionˉlogical += Regionˉbytes;
        }
        Equal(Logical, Regionˉlogical);
    }

    private static int Executeˉhostedˉcontainerˉsourceˉset(
        ImmutableArray<byte> application,
        string[] arguments,
        string expectedˉoutput,
        ISet<string>? loaded = null,
        string expectedˉerror = "") =>
        OperatingSystem.IsWindows()
            ? Executeˉwindowsˉapplication(
                application,
                expectedˉoutput,
                arguments,
                loadedˉmodules: loaded,
                expectedˉerror: expectedˉerror)
            : Executeˉlinuxˉapplication(
                application,
                expectedˉoutput,
                arguments,
                loadedˉmappings: loaded,
                expectedˉerror: expectedˉerror);
}
