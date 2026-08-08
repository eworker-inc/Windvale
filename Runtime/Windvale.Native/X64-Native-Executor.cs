using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Runtime;

namespace Windvale.Runtime.Native;

public static class X64ˉnativeˉexecutor
{
    public static int Executeˉi32(
        Nativeˉfragment fragment,
        string entry = "Main",
        long maximumˉinstructions = Nativeˉcontract.DEFAULT_MAXIMUM_INSTRUCTIONS,
        int maximumˉcallˉdepth = Nativeˉcontract.DEFAULT_MAXIMUM_CALL_DEPTH,
        Nativeˉhostˉservices? hostˉservices = null) =>
        Executeˉentry(
            fragment,
            Nativeˉentryˉinputˉkind.None,
            Nativeˉentryˉresultˉkind.Scalar,
            [],
            entry,
            maximumˉinstructions,
            maximumˉcallˉdepth,
            hostˉservices).Scalar;

    public static ImmutableArray<byte> Executeˉbytes(
        Nativeˉfragment fragment,
        string entry = "Main",
        long maximumˉinstructions = Nativeˉcontract.DEFAULT_MAXIMUM_INSTRUCTIONS,
        int maximumˉcallˉdepth = Nativeˉcontract.DEFAULT_MAXIMUM_CALL_DEPTH,
        Nativeˉhostˉservices? hostˉservices = null) =>
        Executeˉentry(
            fragment,
            Nativeˉentryˉinputˉkind.None,
            Nativeˉentryˉresultˉkind.Descriptor,
            [],
            entry,
            maximumˉinstructions,
            maximumˉcallˉdepth,
            hostˉservices).Bytes;

    public static ImmutableArray<byte> Executeˉbytes(
        Nativeˉfragment fragment,
        ImmutableArray<byte> input,
        string entry = "Main",
        long maximumˉinstructions = Nativeˉcontract.DEFAULT_MAXIMUM_INSTRUCTIONS,
        int maximumˉcallˉdepth = Nativeˉcontract.DEFAULT_MAXIMUM_CALL_DEPTH,
        Nativeˉhostˉservices? hostˉservices = null) =>
        Executeˉentry(
            fragment,
            Nativeˉentryˉinputˉkind.Bytes,
            Nativeˉentryˉresultˉkind.Descriptor,
            input,
            entry,
            maximumˉinstructions,
            maximumˉcallˉdepth,
            hostˉservices).Bytes;

    internal static Nativeˉexecutionˉmeasurement Measureˉi32(
        Nativeˉfragment fragment,
        string entry = "Main",
        long maximumˉinstructions = Nativeˉcontract.DEFAULT_MAXIMUM_INSTRUCTIONS,
        int maximumˉcallˉdepth = Nativeˉcontract.DEFAULT_MAXIMUM_CALL_DEPTH,
        Nativeˉhostˉservices? hostˉservices = null)
    {
        var Outcome = Executeˉentry(
            fragment,
            Nativeˉentryˉinputˉkind.None,
            Nativeˉentryˉresultˉkind.Scalar,
            [],
            entry,
            maximumˉinstructions,
            maximumˉcallˉdepth,
            hostˉservices);
        return new(Outcome.Scalar, Outcome.Recordˉarenaˉused, Outcome.Textˉarenaˉused);
    }

    private static Nativeˉexecutionˉoutcome Executeˉentry(
        Nativeˉfragment fragment,
        Nativeˉentryˉinputˉkind expectedˉinput,
        Nativeˉentryˉresultˉkind expectedˉresult,
        ImmutableArray<byte> input,
        string entry = "Main",
        long maximumˉinstructions = Nativeˉcontract.DEFAULT_MAXIMUM_INSTRUCTIONS,
        int maximumˉcallˉdepth = Nativeˉcontract.DEFAULT_MAXIMUM_CALL_DEPTH,
        Nativeˉhostˉservices? hostˉservices = null)
    {
        var Actualˉshape = Nativeˉfragmentˉverifier.Verifyˉentryˉshape(fragment);
        ArgumentNullException.ThrowIfNull(entry);
        if (Actualˉshape.Input != expectedˉinput || Actualˉshape.Result != expectedˉresult)
        {
            throw new Nativeˉbackendˉexception(
                "WVN4011",
                $"Native entry '{entry}' has shape {Actualˉshape.Input} -> {Actualˉshape.Result}; " +
                    $"the selected executor requires {expectedˉinput} -> {expectedˉresult}.");
        }
        if (expectedˉinput == Nativeˉentryˉinputˉkind.Bytes &&
            (input.IsDefault || input.Length > Bytecodeˉlimits.MAX_BYTE_DATA_BYTES))
        {
            throw new Nativeˉbackendˉexception(
                "WVN4020",
                $"Native entry byte input must be initialized and no larger than " +
                    $"{Bytecodeˉlimits.MAX_BYTE_DATA_BYTES} bytes.");
        }
        if (maximumˉinstructions <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumˉinstructions),
                "The maximum instruction count must be positive.");
        }
        if (maximumˉcallˉdepth <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumˉcallˉdepth),
                "The maximum call depth must be positive.");
        }
        if (RuntimeInformation.ProcessArchitecture != Architecture.X64)
        {
            throw new PlatformNotSupportedException("The first native executor requires an x86-64 process.");
        }
        Requireˉservices(fragment, hostˉservices);
        var Entry = fragment.Symbols.SingleOrDefault(Symbol =>
            Symbol.Binding == Nativeˉsymbolˉbinding.Export &&
            Symbol.Kind == Nativeˉsymbolˉkind.Function &&
            StringComparer.Ordinal.Equals(Symbol.Name, entry));
        if (Entry is null || Entry.Size == 0)
        {
            throw new Nativeˉbackendˉexception("WVN4001", $"Native entry '{entry}' is missing or empty.");
        }

        var Requiresˉarguments = fragment.Requiredˉservices.Contains(
            Nativeˉservice.Processˉargumentˉcount) ||
            fragment.Requiredˉservices.Contains(Nativeˉservice.Processˉargument);
        using var Buffers = new Nativeˉexecutionˉbuffers(
            hostˉservices?.Resources,
            Requiresˉarguments);
        var Entryˉinput = expectedˉinput == Nativeˉentryˉinputˉkind.Bytes
            ? Buffers.Prepareˉentryˉinput(input)
            : default;
        var Requiresˉconsole = fragment.Requiredˉservices.Contains(
            Nativeˉservice.Consoleˉwriteˉline);
        var Requiresˉdiagnostic = fragment.Requiredˉservices.Contains(
            Nativeˉservice.Diagnosticˉwriteˉline);
        using var Output = new Nativeˉoutputˉcontext(
            hostˉservices,
            Requiresˉconsole,
            Requiresˉdiagnostic);
        var Requiresˉfileˉinput = fragment.Requiredˉservices.Contains(
            Nativeˉservice.Fileˉreadˉbytes);
        using var Fileˉinput = new Nativeˉfileˉinputˉcontext(
            hostˉservices,
            Requiresˉfileˉinput);
        var Requiresˉfileˉoutput = fragment.Requiredˉservices.Contains(
            Nativeˉservice.Fileˉwriteˉbytes);
        using var Fileˉoutput = new Nativeˉfileˉoutputˉcontext(
            hostˉservices,
            Requiresˉfileˉoutput);
        var Address = IntPtr.Zero;
        var Context = IntPtr.Zero;
        var Resultˉcell = IntPtr.Zero;
        var Serviceˉcode = new List<(Nativeˉservice Service, ImmutableArray<byte> Code)>();
        foreach (var Service in fragment.Requiredˉservices)
        {
            ImmutableArray<byte> Nativeˉserviceˉcode;
            if (Service is Nativeˉservice.Consoleˉwriteˉline or
                Nativeˉservice.Diagnosticˉwriteˉline)
            {
                Nativeˉserviceˉcode = X64ˉnativeˉoutputˉservices.Build(
                    Service,
                    Output.Platform);
                X64ˉnativeˉoutputˉservices.Verify(
                    Service,
                    Output.Platform,
                    Nativeˉserviceˉcode.AsSpan());
            }
            else if (Service is Nativeˉservice.Processˉargumentˉcount or
                Nativeˉservice.Processˉargument)
            {
                Nativeˉserviceˉcode = X64ˉnativeˉargumentˉservices.Build(Service);
                X64ˉnativeˉargumentˉservices.Verify(Service, Nativeˉserviceˉcode.AsSpan());
            }
            else if (Service == Nativeˉservice.Textˉutf8ˉisˉvalid)
            {
                Nativeˉserviceˉcode = X64ˉnativeˉutf8ˉservice.Build();
                X64ˉnativeˉutf8ˉservice.Verify(Nativeˉserviceˉcode.AsSpan());
            }
            else if (Service == Nativeˉservice.Fileˉreadˉbytes)
            {
                Nativeˉserviceˉcode = X64ˉnativeˉfileˉinputˉservice.Build(
                    Fileˉinput.Platform);
                X64ˉnativeˉfileˉinputˉservice.Verify(
                    Fileˉinput.Platform,
                    Nativeˉserviceˉcode.AsSpan());
            }
            else if (Service == Nativeˉservice.Fileˉwriteˉbytes)
            {
                Nativeˉserviceˉcode = X64ˉnativeˉfileˉoutputˉservice.Build(
                    Fileˉoutput.Platform);
                X64ˉnativeˉfileˉoutputˉservice.Verify(
                    Fileˉoutput.Platform,
                    Nativeˉserviceˉcode.AsSpan());
            }
            else if (Service is Nativeˉservice.Enumˉname or
                Nativeˉservice.Textˉconcat or
                Nativeˉservice.Textˉquote or
                Nativeˉservice.I32ˉformat or
                Nativeˉservice.U32ˉformat)
            {
                Nativeˉserviceˉcode = X64ˉnativeˉtextˉservices.Build(Service, fragment.Types);
                X64ˉnativeˉtextˉservices.Verify(
                    Service,
                    Nativeˉserviceˉcode.AsSpan(),
                    fragment.Types);
            }
            else
            {
                throw new Nativeˉbackendˉexception(
                    "WVN4010",
                    $"Unknown native service implementation '{Service}'.");
            }
            Serviceˉcode.Add((Service, Nativeˉserviceˉcode));
        }
        var Publicationˉplan = X64ˉnativeˉpublicationˉlayout.Plan(
            fragment.Code.Length,
            Serviceˉcode
                .Select(Item => new Nativeˉpublicationˉservice(Item.Service, Item.Code.Length))
                .ToImmutableArray());
        var Allocationˉbytes = Publicationˉplan.Imageˉbytes;
        var Serviceˉoffsets = Publicationˉplan.Placements.ToDictionary(
            Placement => Placement.Service,
            Placement => Placement.Offset);
        using var Executableˉimage = Nativeˉexecutableˉimage.Allocateˉwritable(
            X64ˉnativeˉpublicationˉlifetime.Plan(Allocationˉbytes));
        var Serviceˉtable = IntPtr.Zero;
        var Serviceˉfailureˉdetail = Nativeˉserviceˉfailureˉdetail.None;
        var Resultˉbytes = ImmutableArray<byte>.Empty;
        uint Recordˉarenaˉused = 0;
        uint Textˉarenaˉused = 0;
        ulong Outcome;
        try
        {
            var Linkedˉcode = new byte[Allocationˉbytes];
            fragment.Code.CopyTo(Linkedˉcode);
            var Previousˉserviceˉend = fragment.Code.Length;
            for (var Index = 0; Index < Serviceˉcode.Count; Index++)
            {
                var Placement = Publicationˉplan.Placements[Index];
                if (Index != 0)
                {
                    Array.Fill(
                        Linkedˉcode,
                        (byte)0x90,
                        Previousˉserviceˉend,
                        Placement.Offset - Previousˉserviceˉend);
                }
                Serviceˉcode[Index].Code.CopyTo(Linkedˉcode, Placement.Offset);
                Previousˉserviceˉend = checked(Placement.Offset + Placement.Size);
            }
            Executableˉimage.Copyˉimage(Linkedˉcode);
            Executableˉimage.Sealˉexecutable();
            Address = Executableˉimage.Executableˉaddress;

            if (Serviceˉcode.Count != 0)
            {
                Serviceˉtable = Marshal.AllocHGlobal(checked((int)Nativeˉserviceˉtableˉcontract.SIZE));
                var Tableˉbytes = new byte[checked((int)Nativeˉserviceˉtableˉcontract.SIZE)];
                BinaryPrimitives.WriteUInt32LittleEndian(
                    Tableˉbytes.AsSpan(Nativeˉserviceˉtableˉcontract.FORMAT_VERSION_OFFSET),
                    Nativeˉserviceˉtableˉcontract.FORMAT_VERSION);
                BinaryPrimitives.WriteUInt32LittleEndian(
                    Tableˉbytes.AsSpan(Nativeˉserviceˉtableˉcontract.SIZE_OFFSET),
                    Nativeˉserviceˉtableˉcontract.SIZE);
                foreach (var Service in fragment.Requiredˉservices)
                {
                    BinaryPrimitives.WriteUInt64LittleEndian(
                        Tableˉbytes.AsSpan(Serviceˉtableˉpointerˉoffset(Service)),
                        checked((ulong)(Address.ToInt64() + Serviceˉoffsets[Service])));
                }
                Marshal.Copy(Tableˉbytes, 0, Serviceˉtable, Tableˉbytes.Length);
            }

            Context = Marshal.AllocHGlobal(checked((int)Nativeˉexecutionˉcontextˉcontract.SIZE));
            var Contextˉbytes = new byte[checked((int)Nativeˉexecutionˉcontextˉcontract.SIZE)];
            BinaryPrimitives.WriteUInt32LittleEndian(
                Contextˉbytes.AsSpan(Nativeˉexecutionˉcontextˉcontract.FORMAT_VERSION_OFFSET),
                Nativeˉexecutionˉcontextˉcontract.FORMAT_VERSION);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Contextˉbytes.AsSpan(Nativeˉexecutionˉcontextˉcontract.SIZE_OFFSET),
                Nativeˉexecutionˉcontextˉcontract.SIZE);
            BinaryPrimitives.WriteUInt64LittleEndian(
                Contextˉbytes.AsSpan(Nativeˉexecutionˉcontextˉcontract.INSTRUCTION_BUDGET_OFFSET),
                checked((ulong)maximumˉinstructions));
            BinaryPrimitives.WriteUInt64LittleEndian(
                Contextˉbytes.AsSpan(Nativeˉexecutionˉcontextˉcontract.CALL_DEPTH_BUDGET_OFFSET),
                checked((ulong)maximumˉcallˉdepth));
            BinaryPrimitives.WriteUInt64LittleEndian(
                Contextˉbytes.AsSpan(Nativeˉexecutionˉcontextˉcontract.SERVICE_TABLE_POINTER_OFFSET),
                Serviceˉtable == IntPtr.Zero ? 0 : checked((ulong)Serviceˉtable.ToInt64()));
            BinaryPrimitives.WriteUInt64LittleEndian(
                Contextˉbytes.AsSpan(Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_POINTER_OFFSET),
                checked((ulong)Buffers.Recordˉarena.Address.ToInt64()));
            BinaryPrimitives.WriteUInt32LittleEndian(
                Contextˉbytes.AsSpan(Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_LENGTH_OFFSET),
                checked((uint)Buffers.Recordˉarena.Length));
            BinaryPrimitives.WriteUInt32LittleEndian(
                Contextˉbytes.AsSpan(Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_USED_OFFSET),
                0);
            BinaryPrimitives.WriteUInt64LittleEndian(
                Contextˉbytes.AsSpan(Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_POINTER_OFFSET),
                checked((ulong)Buffers.Textˉarena.Address.ToInt64()));
            BinaryPrimitives.WriteUInt32LittleEndian(
                Contextˉbytes.AsSpan(Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_LENGTH_OFFSET),
                checked((uint)Buffers.Textˉarena.Length));
            BinaryPrimitives.WriteUInt32LittleEndian(
                Contextˉbytes.AsSpan(Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET),
                0);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Contextˉbytes.AsSpan(Nativeˉexecutionˉcontextˉcontract.SERVICE_FAILURE_DETAIL_OFFSET),
                (uint)Nativeˉserviceˉfailureˉdetail.None);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Contextˉbytes.AsSpan(Nativeˉexecutionˉcontextˉcontract.RESERVED_OFFSET),
                0);
            BinaryPrimitives.WriteUInt64LittleEndian(
                Contextˉbytes.AsSpan(
                    Nativeˉexecutionˉcontextˉcontract.ARGUMENT_TABLE_POINTER_OFFSET),
                Buffers.Argumentˉtable.Address == IntPtr.Zero
                    ? 0
                    : checked((ulong)Buffers.Argumentˉtable.Address.ToInt64()));
            BinaryPrimitives.WriteUInt32LittleEndian(
                Contextˉbytes.AsSpan(Nativeˉexecutionˉcontextˉcontract.ARGUMENT_COUNT_OFFSET),
                Buffers.Argumentˉcount);
            BinaryPrimitives.WriteUInt32LittleEndian(
                Contextˉbytes.AsSpan(Nativeˉexecutionˉcontextˉcontract.ARGUMENT_RESERVED_OFFSET),
                0);
            BinaryPrimitives.WriteUInt64LittleEndian(
                Contextˉbytes.AsSpan(Nativeˉexecutionˉcontextˉcontract.OUTPUT_TABLE_POINTER_OFFSET),
                Output.Address == IntPtr.Zero ? 0 : checked((ulong)Output.Address.ToInt64()));
            BinaryPrimitives.WriteUInt64LittleEndian(
                Contextˉbytes.AsSpan(
                    Nativeˉexecutionˉcontextˉcontract.FILE_INPUT_TABLE_POINTER_OFFSET),
                Fileˉinput.Address == IntPtr.Zero
                    ? 0
                    : checked((ulong)Fileˉinput.Address.ToInt64()));
            BinaryPrimitives.WriteUInt64LittleEndian(
                Contextˉbytes.AsSpan(
                    Nativeˉexecutionˉcontextˉcontract.FILE_OUTPUT_TABLE_POINTER_OFFSET),
                Fileˉoutput.Address == IntPtr.Zero
                    ? 0
                    : checked((ulong)Fileˉoutput.Address.ToInt64()));
            Marshal.Copy(Contextˉbytes, 0, Context, Contextˉbytes.Length);

            if (expectedˉresult == Nativeˉentryˉresultˉkind.Descriptor)
            {
                var Bridgeˉbytes = new byte[
                    expectedˉinput == Nativeˉentryˉinputˉkind.Bytes
                        ? 2 * Nativeˉcontract.VALUE_SLOT_BYTES
                        : Nativeˉcontract.VALUE_SLOT_BYTES];
                if (expectedˉinput == Nativeˉentryˉinputˉkind.Bytes)
                {
                    var Descriptor = Bridgeˉbytes.AsSpan(Nativeˉcontract.VALUE_SLOT_BYTES);
                    BinaryPrimitives.WriteUInt64LittleEndian(
                        Descriptor[Nativeˉcontract.BORROWED_BYTES_POINTER_OFFSET..],
                        checked((ulong)Entryˉinput.Address.ToInt64()));
                    BinaryPrimitives.WriteUInt32LittleEndian(
                        Descriptor[Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET..],
                        checked((uint)Entryˉinput.Length));
                    BinaryPrimitives.WriteUInt32LittleEndian(
                        Descriptor[Nativeˉcontract.BORROWED_BYTES_RESERVED_OFFSET..],
                        0);
                }
                Resultˉcell = Marshal.AllocHGlobal(Bridgeˉbytes.Length);
                Marshal.Copy(Bridgeˉbytes, 0, Resultˉcell, Bridgeˉbytes.Length);
            }
            var Entryˉaddress = checked(Address.ToInt64() + Entry.Offset);
            var Function = Marshal.GetDelegateForFunctionPointer<Nativeˉentry>(new(Entryˉaddress));
            var Contextˉpointer = checked((ulong)Context.ToInt64());
            var Resultˉpointer = Resultˉcell == IntPtr.Zero
                ? 0UL
                : checked((ulong)Resultˉcell.ToInt64());
            Outcome = Executableˉimage.Invoke(_ => Function(
                Resultˉpointer,
                Contextˉpointer,
                Contextˉpointer,
                Resultˉpointer,
                0,
                0));
            Recordˉarenaˉused = unchecked((uint)Marshal.ReadInt32(
                Context,
                Nativeˉexecutionˉcontextˉcontract.RECORD_ARENA_USED_OFFSET));
            Textˉarenaˉused = unchecked((uint)Marshal.ReadInt32(
                Context,
                Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET));
            Fileˉinput.Verifyˉcompleted();
            Fileˉoutput.Verifyˉcompleted();
            Serviceˉfailureˉdetail = (Nativeˉserviceˉfailureˉdetail)unchecked((uint)Marshal.ReadInt32(
                Context,
                Nativeˉexecutionˉcontextˉcontract.SERVICE_FAILURE_DETAIL_OFFSET));
            if ((uint)(Outcome >> 32) == 0 &&
                expectedˉresult == Nativeˉentryˉresultˉkind.Descriptor)
            {
                Resultˉbytes = Readˉverifiedˉbyteˉresult(
                    fragment,
                    Address,
                    Context,
                    Buffers.Textˉarena,
                    Entryˉinput,
                    Resultˉcell,
                    entry);
            }
        }
        finally
        {
            if (Resultˉcell != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(Resultˉcell);
            }
            if (Context != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(Context);
            }
            if (Serviceˉtable != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(Serviceˉtable);
            }
        }

        var Status = (uint)(Outcome >> 32);
        if (Status == 0)
        {
            return new(
                unchecked((int)(uint)Outcome),
                Resultˉbytes,
                Recordˉarenaˉused,
                Textˉarenaˉused);
        }
        if (Status == 1)
        {
            throw new Nativeˉtrapˉexception(
                "WVR3007",
                $"Integer overflow in native entry '{entry}'.");
        }
        if (Status == 2)
        {
            throw new Nativeˉtrapˉexception(
                "WVR3011",
                $"The native instruction limit {maximumˉinstructions} was exceeded in entry '{entry}'.");
        }
        if (Status == 3)
        {
            throw new Nativeˉtrapˉexception(
                "WVR3004",
                $"The native call-depth limit {maximumˉcallˉdepth} was exceeded in entry '{entry}'.");
        }
        if (Status == 4)
        {
            throw new Nativeˉtrapˉexception(
                "WVR3005",
                $"A native static-data index was outside its immutable array in entry '{entry}'.");
        }
        if (Status == 5)
        {
            if (Serviceˉfailureˉdetail == Nativeˉserviceˉfailureˉdetail.Textˉvalueˉlimit)
            {
                throw new Nativeˉtrapˉexception(
                    "WVR3012",
                    $"A native text result exceeded the {Bytecodeˉlimits.MAX_UTF8_VALUE_BYTES}-byte value limit in entry '{entry}'.");
            }
            if (Serviceˉfailureˉdetail == Nativeˉserviceˉfailureˉdetail.Textˉarenaˉexhausted)
            {
                throw new Nativeˉtrapˉexception(
                    "WVR3018",
                    $"The native text arena exhausted its {Nativeˉcontract.MAXIMUM_TEXT_ARENA_BYTES}-byte limit in entry '{entry}'.");
            }
            if (Serviceˉfailureˉdetail ==
                Nativeˉserviceˉfailureˉdetail.Argumentˉindexˉoutˉofˉrange)
            {
                throw new Nativeˉtrapˉexception(
                    "WVR3020",
                    $"A native argument index was outside the supplied execution snapshot in entry '{entry}'.");
            }
            if (Serviceˉfailureˉdetail == Nativeˉserviceˉfailureˉdetail.Outputˉwriteˉfailed)
            {
                throw new Nativeˉtrapˉexception(
                    "WVR3029",
                    $"A native output channel rejected a write in entry '{entry}'.");
            }
            if (Serviceˉfailureˉdetail == Nativeˉserviceˉfailureˉdetail.Fileˉinvalidˉname)
            {
                throw new Nativeˉtrapˉexception(
                    "WVR3021",
                    $"A native file request used an invalid resource name in entry '{entry}'.");
            }
            if (Serviceˉfailureˉdetail == Nativeˉserviceˉfailureˉdetail.Fileˉnotˉfound)
            {
                throw new Nativeˉtrapˉexception(
                    "WVR3022",
                    $"A native file request could not find its resource in entry '{entry}'.");
            }
            if (Serviceˉfailureˉdetail ==
                Nativeˉserviceˉfailureˉdetail.Fileˉpermissionˉdenied)
            {
                throw new Nativeˉtrapˉexception(
                    "WVR3023",
                    $"A native file request was denied in entry '{entry}'.");
            }
            if (Serviceˉfailureˉdetail == Nativeˉserviceˉfailureˉdetail.Fileˉunavailable)
            {
                throw new Nativeˉtrapˉexception(
                    "WVR3024",
                    $"A native file request failed at the host boundary in entry '{entry}'.");
            }
            if (Serviceˉfailureˉdetail == Nativeˉserviceˉfailureˉdetail.Fileˉtooˉlarge)
            {
                throw new Nativeˉtrapˉexception(
                    "WVR3025",
                    $"A native file exceeded the {Bytecodeˉlimits.MAX_BYTE_DATA_BYTES}-byte limit in entry '{entry}'.");
            }
            if (Serviceˉfailureˉdetail ==
                Nativeˉserviceˉfailureˉdetail.Fileˉsnapshotˉlimit)
            {
                throw new Nativeˉtrapˉexception(
                    "WVR3028",
                    $"Native file snapshots exceeded the per-execution limit in entry '{entry}'.");
            }
            if (Serviceˉfailureˉdetail == Nativeˉserviceˉfailureˉdetail.Bytesˉvalueˉlimit)
            {
                throw new Nativeˉtrapˉexception(
                    "WVR3015",
                    $"A native byte result exceeded the {Bytecodeˉlimits.MAX_BYTE_DATA_BYTES}-byte value limit in entry '{entry}'.");
            }
            if (Serviceˉfailureˉdetail ==
                Nativeˉserviceˉfailureˉdetail.Bytesˉu16ˉoutˉofˉrange)
            {
                throw new Nativeˉtrapˉexception(
                    "WVR3016",
                    $"Bytesˉfromˉu16ˉlittle received a native value above {ushort.MaxValue} in entry '{entry}'.");
            }
            throw new Nativeˉtrapˉexception(
                "WVR3013",
                $"A native runtime service rejected its request in entry '{entry}'.");
        }
        if (Status == 6)
        {
            throw new Nativeˉtrapˉexception(
                "WVR3008",
                $"A native byte slice or fixed-width read was outside its immutable source in entry '{entry}'.");
        }
        if (Status == 7)
        {
            throw new Nativeˉtrapˉexception(
                "WVR3017",
                $"The native record arena exhausted its {Nativeˉcontract.MAXIMUM_RECORD_ARENA_BYTES}-byte limit in entry '{entry}'.");
        }
        if (Status == 8)
        {
            throw new Nativeˉtrapˉexception(
                "WVR3014",
                $"Textˉfromˉutf8 received an invalid UTF-8 byte sequence in native entry '{entry}'.");
        }
        if (Status == 9)
        {
            throw new Nativeˉtrapˉexception(
                "WVR3032",
                $"Integer division by zero in native entry '{entry}'.");
        }
        if (Status == 10)
        {
            throw new Nativeˉtrapˉexception(
                "WVR3033",
                $"Invalid integer shift count in native entry '{entry}'.");
        }
        throw new Nativeˉbackendˉexception(
            "WVN4005",
            $"Native entry '{entry}' returned unknown status {Status}.");
    }

    private static ImmutableArray<byte> Readˉverifiedˉbyteˉresult(
        Nativeˉfragment fragment,
        IntPtr executableˉaddress,
        IntPtr context,
        Nativeˉborrowedˉbuffer arena,
        Nativeˉborrowedˉbuffer entryˉinput,
        IntPtr resultˉcell,
        string entry)
    {
        var Descriptor = new byte[Nativeˉcontract.VALUE_SLOT_BYTES];
        Marshal.Copy(resultˉcell, Descriptor, 0, Descriptor.Length);
        var Pointer = BinaryPrimitives.ReadUInt64LittleEndian(
            Descriptor.AsSpan(Nativeˉcontract.BORROWED_BYTES_POINTER_OFFSET));
        var Length = BinaryPrimitives.ReadUInt32LittleEndian(
            Descriptor.AsSpan(Nativeˉcontract.BORROWED_BYTES_LENGTH_OFFSET));
        var Reserved = BinaryPrimitives.ReadUInt32LittleEndian(
            Descriptor.AsSpan(Nativeˉcontract.BORROWED_BYTES_RESERVED_OFFSET));
        if (Reserved != 0 || Length > Bytecodeˉlimits.MAX_BYTE_DATA_BYTES)
        {
            throw Invalidˉbyteˉresult(entry);
        }
        if (Length == 0 && Pointer == 0)
        {
            return [];
        }

        var Arenaˉused = unchecked((uint)Marshal.ReadInt32(
            context,
            Nativeˉexecutionˉcontextˉcontract.TEXT_ARENA_USED_OFFSET));
        if (Arenaˉused > arena.Length)
        {
            throw Invalidˉbyteˉresult(entry);
        }
        var Arenaˉstart = checked((ulong)arena.Address.ToInt64());
        var Isˉarenaˉresult = Isˉinside(Pointer, Length, Arenaˉstart, Arenaˉused);
        var Imageˉstart = checked((ulong)executableˉaddress.ToInt64());
        var Isˉstaticˉresult = fragment.Symbols
            .Where(Symbol => Symbol.Kind == Nativeˉsymbolˉkind.Data)
            .Any(Symbol => Isˉinside(
                Pointer,
                Length,
                checked(Imageˉstart + Symbol.Offset),
                Symbol.Size));
        var Isˉinputˉresult = entryˉinput.Address != IntPtr.Zero && Isˉinside(
            Pointer,
            Length,
            checked((ulong)entryˉinput.Address.ToInt64()),
            checked((uint)entryˉinput.Length));
        if (!Isˉarenaˉresult && !Isˉstaticˉresult && !Isˉinputˉresult)
        {
            throw Invalidˉbyteˉresult(entry);
        }

        var Bytes = new byte[checked((int)Length)];
        if (Bytes.Length != 0)
        {
            Marshal.Copy(new IntPtr(checked((long)Pointer)), Bytes, 0, Bytes.Length);
        }
        return Bytes.ToImmutableArray();
    }

    private static bool Isˉinside(ulong pointer, uint length, ulong start, uint available)
    {
        if (pointer < start)
        {
            return false;
        }
        var Offset = pointer - start;
        return Offset <= available && length <= available - Offset;
    }

    private static Nativeˉbackendˉexception Invalidˉbyteˉresult(string entry) =>
        new(
            "WVN4012",
            $"Native entry '{entry}' returned a byte descriptor outside its verified immutable data, entry input, or execution arena.");

    private static void Requireˉservices(
        Nativeˉfragment fragment,
        Nativeˉhostˉservices? hostˉservices)
    {
        foreach (var Service in fragment.Requiredˉservices)
        {
            if (Service is Nativeˉservice.Textˉutf8ˉisˉvalid or
                Nativeˉservice.Enumˉname or
                Nativeˉservice.Textˉconcat or
                Nativeˉservice.Textˉquote or
                Nativeˉservice.I32ˉformat or
                Nativeˉservice.U32ˉformat)
            {
                continue;
            }
            if (hostˉservices is null || !hostˉservices.Isˉauthorized(Service))
            {
                throw new Nativeˉtrapˉexception(
                    "WVR3010",
                    $"Native service '{Service}' was required but not authorized.");
            }
            if (!hostˉservices.Supports(Service))
            {
                throw new Nativeˉtrapˉexception(
                    "WVR3001",
                    $"The host does not implement native service '{Service}'.");
            }
        }
    }

    private static int Serviceˉtableˉpointerˉoffset(Nativeˉservice service) =>
        service switch
        {
            Nativeˉservice.Consoleˉwriteˉline =>
                Nativeˉserviceˉtableˉcontract.CONSOLE_WRITE_LINE_POINTER_OFFSET,
            Nativeˉservice.Processˉargumentˉcount =>
                Nativeˉserviceˉtableˉcontract.PROCESS_ARGUMENT_COUNT_POINTER_OFFSET,
            Nativeˉservice.Processˉargument =>
                Nativeˉserviceˉtableˉcontract.PROCESS_ARGUMENT_POINTER_OFFSET,
            Nativeˉservice.Fileˉreadˉbytes =>
                Nativeˉserviceˉtableˉcontract.FILE_READ_BYTES_POINTER_OFFSET,
            Nativeˉservice.Fileˉwriteˉbytes =>
                Nativeˉserviceˉtableˉcontract.FILE_WRITE_BYTES_POINTER_OFFSET,
            Nativeˉservice.Textˉutf8ˉisˉvalid =>
                Nativeˉserviceˉtableˉcontract.TEXT_UTF8_IS_VALID_POINTER_OFFSET,
            Nativeˉservice.Diagnosticˉwriteˉline =>
                Nativeˉserviceˉtableˉcontract.DIAGNOSTIC_WRITE_LINE_POINTER_OFFSET,
            Nativeˉservice.Enumˉname => Nativeˉserviceˉtableˉcontract.ENUM_NAME_POINTER_OFFSET,
            Nativeˉservice.Textˉconcat => Nativeˉserviceˉtableˉcontract.TEXT_CONCAT_POINTER_OFFSET,
            Nativeˉservice.Textˉquote => Nativeˉserviceˉtableˉcontract.TEXT_QUOTE_POINTER_OFFSET,
            Nativeˉservice.I32ˉformat => Nativeˉserviceˉtableˉcontract.I32_FORMAT_POINTER_OFFSET,
            Nativeˉservice.U32ˉformat => Nativeˉserviceˉtableˉcontract.U32_FORMAT_POINTER_OFFSET,
            _ => throw new Nativeˉbackendˉexception("WVN4010", "Unknown native service table entry."),
        };

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate ulong Nativeˉentry(
        ulong windowsˉpadding,
        ulong windowsˉcontext,
        ulong systemˉvˉcontext,
        ulong windowsˉpaddingˉfour,
        ulong systemˉvˉpadding,
        ulong systemˉvˉpaddingˉsix);

    private readonly record struct Nativeˉexecutionˉoutcome(
        int Scalar,
        ImmutableArray<byte> Bytes,
        uint Recordˉarenaˉused,
        uint Textˉarenaˉused);

    internal readonly record struct Nativeˉexecutionˉmeasurement(
        int Scalar,
        uint Recordˉarenaˉused,
        uint Textˉarenaˉused);

}
