using System.Buffers.Binary;
using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private const string NATIVE_FRAGMENT_ARTIFACT_SOURCE = """
        module Nativeˉfragmentˉartifact profile hosted;
        capability console.write_line;

        record Nativeˉartifactˉvalue {
            Choice: Nativeˉartifactˉstate;
            Count: u32;
        }

        enum Nativeˉartifactˉstate {
            Ready = 3;
            Done = 7;
        }

        export fn Main() -> i32 {
            let Value = Nativeˉartifactˉvalue {
                Choice: Nativeˉartifactˉstate.Ready,
                Count: 42u32
            };
            console.write_line(Enumˉname(Value.Choice));
            if Value.Choice == Nativeˉartifactˉstate.Ready { return 0; }
            return 1;
        }
        """;

    private static void Nativeˉfragmentˉartifactsˉroundˉtrip()
    {
        var Module = Moduleˉcodec.Readˉandˉverify(
            Compileˉsuccess(NATIVE_FRAGMENT_ARTIFACT_SOURCE));
        var Fragment = X64ˉnativeˉbackend.Compile(Module).Fragment;
        True(Fragment.Types.Length >= 2, "The artifact fixture omitted nominal metadata.");
        Sequenceˉequal(
            [Nativeˉservice.Consoleˉwriteˉline, Nativeˉservice.Enumˉname],
            Fragment.Requiredˉservices);

        var First = Nativeˉfragmentˉartifactˉcodec.Write(Fragment);
        var Second = Nativeˉfragmentˉartifactˉcodec.Write(Fragment);
        Sequenceˉequal(First, Second);
        var Roundˉtrip = Nativeˉfragmentˉartifactˉcodec.Readˉandˉverify(First);
        Equal(Fragment.Target, Roundˉtrip.Target);
        Equal(Fragment.Abiˉversion, Roundˉtrip.Abiˉversion);
        Equal(Fragment.Architecture, Roundˉtrip.Architecture);
        Equal(Fragment.Alignment, Roundˉtrip.Alignment);
        Sequenceˉequal(Fragment.Code, Roundˉtrip.Code);
        Sequenceˉequal(Fragment.Symbols, Roundˉtrip.Symbols);
        Sequenceˉequal(Fragment.Patches, Roundˉtrip.Patches);
        Equalˉnativeˉfragmentˉtypes(Fragment.Types, Roundˉtrip.Types);
        Sequenceˉequal(Fragment.Requiredˉservices, Roundˉtrip.Requiredˉservices);
        Sequenceˉequal(First, Nativeˉfragmentˉartifactˉcodec.Write(Roundˉtrip));

        var Maximumˉname = $"N{new string('a', Bytecodeˉlimits.MAX_NAME_BYTES - 1)}";
        var Boundaryˉtypes = Fragment.Types.SetItem(
            0,
            Fragment.Types[0] switch
            {
                Recordˉtypeˉdeclaration Record => Record with { Name = Maximumˉname },
                Enumˉtypeˉdeclaration Enum => Enum with { Name = Maximumˉname },
                _ => throw new InvalidOperationException("The artifact fixture type is unsupported."),
            });
        var Boundary = Fragment with { Types = Boundaryˉtypes };
        var Boundaryˉroundˉtrip = Nativeˉfragmentˉartifactˉcodec.Readˉandˉverify(
            Nativeˉfragmentˉartifactˉcodec.Write(Boundary));
        Equal(Maximumˉname, Boundaryˉroundˉtrip.Types[0].Name);

        var Oversizedˉname = $"N{new string('a', Bytecodeˉlimits.MAX_NAME_BYTES)}";
        var Oversizedˉtypes = Fragment.Types.SetItem(
            0,
            Fragment.Types[0] switch
            {
                Recordˉtypeˉdeclaration Record => Record with { Name = Oversizedˉname },
                Enumˉtypeˉdeclaration Enum => Enum with { Name = Oversizedˉname },
                _ => throw new InvalidOperationException("The artifact fixture type is unsupported."),
            });
        Throwsˉnativeˉfragmentˉartifact(
            "WNF2002",
            () => _ = Nativeˉfragmentˉartifactˉcodec.Write(
                Fragment with { Types = Oversizedˉtypes }));

        Throwsˉnativeˉfragmentˉartifact(
            "WNF1001",
            () => _ = Nativeˉfragmentˉartifactˉcodec.Read(
                new byte[Nativeˉfragmentˉartifactˉcontract.MAXIMUM_ARTIFACT_BYTES + 1]));
        Throwsˉnativeˉfragmentˉartifact(
            "WNF1002",
            () => _ = Nativeˉfragmentˉartifactˉcodec.Read([]));

        var Badˉmagic = First.ToArray();
        Badˉmagic[0] ^= 0x01;
        Throwsˉnativeˉfragmentˉartifact(
            "WNF1003",
            () => _ = Nativeˉfragmentˉartifactˉcodec.Read(Badˉmagic));

        var Badˉversion = First.ToArray();
        Badˉversion[4]++;
        Throwsˉnativeˉfragmentˉartifact(
            "WNF1004",
            () => _ = Nativeˉfragmentˉartifactˉcodec.Read(Badˉversion));

        var Badˉlength = First.ToArray();
        BinaryPrimitives.WriteUInt32LittleEndian(Badˉlength.AsSpan(8), 1);
        Throwsˉnativeˉfragmentˉartifact(
            "WNF1005",
            () => _ = Nativeˉfragmentˉartifactˉcodec.Read(Badˉlength));

        var Missingˉcode = First.ToArray();
        BinaryPrimitives.WriteUInt32LittleEndian(Missingˉcode.AsSpan(12), 0);
        Throwsˉnativeˉfragmentˉartifact(
            "WNF1009",
            () => _ = Nativeˉfragmentˉartifactˉcodec.Read(Missingˉcode));

        var Excessˉsymbols = First.ToArray();
        BinaryPrimitives.WriteUInt32LittleEndian(Excessˉsymbols.AsSpan(16), uint.MaxValue);
        Throwsˉnativeˉfragmentˉartifact(
            "WNF1007",
            () => _ = Nativeˉfragmentˉartifactˉcodec.Read(Excessˉsymbols));

        var Badˉflags = First.ToArray();
        Badˉflags[41] = 1;
        Throwsˉnativeˉfragmentˉartifact(
            "WNF1009",
            () => _ = Nativeˉfragmentˉartifactˉcodec.Read(Badˉflags));

        var Badˉtarget = First.ToArray();
        Badˉtarget[Nativeˉfragmentˉartifactˉcontract.HEADER_BYTES] = 0xFF;
        Throwsˉnativeˉfragmentˉartifact(
            "WNF1010",
            () => _ = Nativeˉfragmentˉartifactˉcodec.Read(Badˉtarget));

        var Truncated = First[..^1];
        BinaryPrimitives.WriteUInt32LittleEndian(Truncated.AsSpan(8), checked((uint)Truncated.Length));
        Throwsˉnativeˉfragmentˉartifact(
            "WNF1002",
            () => _ = Nativeˉfragmentˉartifactˉcodec.Read(Truncated));

        var Trailing = new byte[First.Length + 1];
        First.CopyTo(Trailing, 0);
        BinaryPrimitives.WriteUInt32LittleEndian(Trailing.AsSpan(8), checked((uint)Trailing.Length));
        Throwsˉnativeˉfragmentˉartifact(
            "WNF1011",
            () => _ = Nativeˉfragmentˉartifactˉcodec.Read(Trailing));

        var Badˉservice = First.ToArray();
        Badˉservice[^1] = byte.MaxValue;
        Throwsˉnativeˉfragmentˉartifact(
            "WNF1009",
            () => _ = Nativeˉfragmentˉartifactˉcodec.Read(Badˉservice));

        var Badˉshape = First.ToArray();
        var Fieldˉnameˉoffset = Badˉshape.AsSpan().IndexOf("Choice"u8);
        True(Fieldˉnameˉoffset > 0, "The artifact fixture field name is unavailable.");
        Badˉshape[Fieldˉnameˉoffset + "Choice"u8.Length + 1] = (byte)Valueˉtype.U8;
        Throwsˉnativeˉfragmentˉartifact(
            "WNF1009",
            () => _ = Nativeˉfragmentˉartifactˉcodec.Read(Badˉshape));

        var Corruptedˉcode = First.ToArray();
        var Targetˉbytes = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(
            Corruptedˉcode.AsSpan(44, sizeof(uint))));
        Corruptedˉcode[Nativeˉfragmentˉartifactˉcontract.HEADER_BYTES + Targetˉbytes] ^= 0x01;
        Throwsˉnative(
            "WVN3030",
            () => _ = Nativeˉfragmentˉartifactˉcodec.Readˉandˉverify(Corruptedˉcode));
    }

    private static void Throwsˉnativeˉfragmentˉartifact(string code, Action action)
    {
        try
        {
            action();
        }
        catch (Nativeˉfragmentˉartifactˉexception Exception)
        {
            Equal(code, Exception.Code);
            return;
        }
        throw new InvalidOperationException(
            $"Expected native-fragment artifact failure {code}.");
    }

    private static void Equalˉnativeˉfragmentˉtypes(
        IReadOnlyList<Nominalˉtypeˉdeclaration> expected,
        IReadOnlyList<Nominalˉtypeˉdeclaration> actual)
    {
        Equal(expected.Count, actual.Count);
        for (var Typeˉindex = 0; Typeˉindex < expected.Count; Typeˉindex++)
        {
            Equal(expected[Typeˉindex].Name, actual[Typeˉindex].Name);
            Equal(expected[Typeˉindex].Kind, actual[Typeˉindex].Kind);
            switch (expected[Typeˉindex], actual[Typeˉindex])
            {
                case (Recordˉtypeˉdeclaration Expected, Recordˉtypeˉdeclaration Actual):
                    Equal(Expected.Fields.Length, Actual.Fields.Length);
                    for (var Field = 0; Field < Expected.Fields.Length; Field++)
                    {
                        Equal(Expected.Fields[Field].Name, Actual.Fields[Field].Name);
                        Equal(Expected.Fields[Field].Type, Actual.Fields[Field].Type);
                    }
                    break;
                case (Enumˉtypeˉdeclaration Expected, Enumˉtypeˉdeclaration Actual):
                    Equal(Expected.Members.Length, Actual.Members.Length);
                    for (var Member = 0; Member < Expected.Members.Length; Member++)
                    {
                        Equal(Expected.Members[Member].Name, Actual.Members[Member].Name);
                        Equal(Expected.Members[Member].Value, Actual.Members[Member].Value);
                    }
                    break;
                default:
                    throw new InvalidOperationException(
                        "Native-fragment nominal metadata changed representation.");
            }
        }
    }
}
