using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Windvale.Bytecode;

namespace Windvale.Runtime;

public sealed class Referenceˉruntime
{
    private static readonly UTF8Encoding STRICT_UTF8 = new(false, true);

    private readonly Verifiedˉmodule Verifiedˉmodule;
    private readonly ICapabilityˉhost Capabilityˉhost;
    private readonly Runtimeˉoptions Options;
    private readonly ImmutableArray<Dictionary<int, int>> Instructionˉindices;
    private readonly long[]? Functionˉsteps;
    private readonly long[]? Functionˉrecordˉfields;
    private readonly long[,]? Functionˉdynamicˉvalueˉcounts;
    private readonly long[,]? Functionˉdynamicˉvalueˉbytes;
    private readonly Runtimeˉdynamicˉlifetimeˉtracker? Dynamicˉlifetime;
    private long Executedˉinstructions;

    public Referenceˉruntime(
        Verifiedˉmodule verifiedˉmodule,
        ICapabilityˉhost capabilityˉhost,
        Runtimeˉoptions options)
    {
        ArgumentNullException.ThrowIfNull(verifiedˉmodule);
        ArgumentNullException.ThrowIfNull(capabilityˉhost);
        ArgumentNullException.ThrowIfNull(options);
        if (options.Maximumˉinstructions <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                "The maximum instruction count must be positive.");
        }

        if (options.Maximumˉcallˉdepth <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                "The maximum call depth must be positive.");
        }
        if (options.Collectˉdynamicˉallocatorˉtrace &&
            (options.Dynamicˉallocatorˉarenaˉbytes < Runtimeˉdynamicˉallocatorˉtraceˉstate.ALIGNMENT_BYTES ||
             options.Dynamicˉallocatorˉarenaˉbytes % Runtimeˉdynamicˉallocatorˉtraceˉstate.ALIGNMENT_BYTES != 0))
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                $"The diagnostic dynamic arena must be a positive multiple of " +
                $"{Runtimeˉdynamicˉallocatorˉtraceˉstate.ALIGNMENT_BYTES} bytes.");
        }

        Verifiedˉmodule = verifiedˉmodule;
        Capabilityˉhost = capabilityˉhost;
        Options = options;
        Instructionˉindices = verifiedˉmodule.Functions
            .Select(Function => Function.Instructions
                .Select((Instruction, Index) => (Instruction.Offset, Index))
                .ToDictionary(Item => Item.Offset, Item => Item.Index))
            .ToImmutableArray();
        Functionˉsteps = options.Collectˉfunctionˉsteps
            ? new long[verifiedˉmodule.Functions.Length]
            : null;
        Functionˉrecordˉfields = options.Collectˉfunctionˉrecordˉfields
            ? new long[verifiedˉmodule.Functions.Length]
            : null;
        if (options.Collectˉfunctionˉdynamicˉvalues)
        {
            var Kinds = Enum.GetValues<Runtimeˉdynamicˉvalueˉkind>().Length;
            Functionˉdynamicˉvalueˉcounts = new long[verifiedˉmodule.Functions.Length, Kinds];
            Functionˉdynamicˉvalueˉbytes = new long[verifiedˉmodule.Functions.Length, Kinds];
        }
        Dynamicˉlifetime = options.Collectˉdynamicˉvalueˉlifetime ||
            options.Collectˉdynamicˉallocatorˉtrace
            ? new Runtimeˉdynamicˉlifetimeˉtracker(
                options.Collectˉdynamicˉallocatorˉtrace
                    ? options.Dynamicˉallocatorˉarenaˉbytes
                    : null)
            : null;
    }

    public ImmutableArray<Runtimeˉfunctionˉsteps> Readˉfunctionˉsteps()
    {
        if (Functionˉsteps is null)
        {
            return [];
        }

        return Verifiedˉmodule.Functions
            .Select((Function, Index) => new Runtimeˉfunctionˉsteps(
                Index,
                Function.Declaration.Name,
                Functionˉsteps[Index]))
            .Where(Item => Item.Executedˉinstructions > 0)
            .OrderByDescending(Item => Item.Executedˉinstructions)
            .ThenBy(Item => Item.Functionˉindex)
            .ToImmutableArray();
    }

    public ImmutableArray<Runtimeˉfunctionˉrecordˉfields> Readˉfunctionˉrecordˉfields()
    {
        if (Functionˉrecordˉfields is null)
        {
            return [];
        }

        return Verifiedˉmodule.Functions
            .Select((Function, Index) => new Runtimeˉfunctionˉrecordˉfields(
                Index,
                Function.Declaration.Name,
                Functionˉrecordˉfields[Index]))
            .Where(Item => Item.Constructedˉfields > 0)
            .OrderByDescending(Item => Item.Constructedˉfields)
            .ThenBy(Item => Item.Functionˉindex)
            .ToImmutableArray();
    }

    public ImmutableArray<Runtimeˉfunctionˉdynamicˉvalues> Readˉfunctionˉdynamicˉvalues()
    {
        if (Functionˉdynamicˉvalueˉcounts is null ||
            Functionˉdynamicˉvalueˉbytes is null)
        {
            return [];
        }

        var Report = ImmutableArray.CreateBuilder<Runtimeˉfunctionˉdynamicˉvalues>();
        foreach (var Kind in Enum.GetValues<Runtimeˉdynamicˉvalueˉkind>())
        {
            var Kindˉindex = (int)Kind;
            for (var Functionˉindex = 0;
                Functionˉindex < Verifiedˉmodule.Functions.Length;
                Functionˉindex++)
            {
                var Count = Functionˉdynamicˉvalueˉcounts[Functionˉindex, Kindˉindex];
                if (Count == 0)
                {
                    continue;
                }
                Report.Add(new(
                    Functionˉindex,
                    Verifiedˉmodule.Functions[Functionˉindex].Declaration.Name,
                    Kind,
                    Count,
                    Functionˉdynamicˉvalueˉbytes[Functionˉindex, Kindˉindex]));
            }
        }

        return Report
            .OrderByDescending(Item => Item.Constructedˉbytes)
            .ThenByDescending(Item => Item.Constructedˉvalues)
            .ThenBy(Item => Item.Functionˉindex)
            .ThenBy(Item => Item.Kind)
            .ToImmutableArray();
    }

    public Runtimeˉdynamicˉvalueˉlifetime? Readˉdynamicˉvalueˉlifetime()
    {
        if (Dynamicˉlifetime is null)
        {
            return null;
        }

        var Peakˉfunctionˉindex = Dynamicˉlifetime.Peakˉoperationˉfunctionˉindex;
        return new(
            Dynamicˉlifetime.Constructedˉvalues,
            Dynamicˉlifetime.Constructedˉbytes,
            Dynamicˉlifetime.Peakˉliveˉvalues,
            Dynamicˉlifetime.Peakˉliveˉbytes,
            Dynamicˉlifetime.Peakˉoperationˉvalues,
            Dynamicˉlifetime.Peakˉoperationˉbytes,
            Peakˉfunctionˉindex,
            Peakˉfunctionˉindex < 0
                ? null
                : Verifiedˉmodule.Functions[Peakˉfunctionˉindex].Declaration.Name,
            Dynamicˉlifetime.Peakˉoperationˉkind,
            Dynamicˉlifetime.Liveˉvalues,
            Dynamicˉlifetime.Liveˉbytes);
    }

    public Runtimeˉdynamicˉallocatorˉtrace? Readˉdynamicˉallocatorˉtrace() =>
        Dynamicˉlifetime?.Readˉallocatorˉtrace();

    public Runtimeˉresult Runˉmain()
    {
        var Mainˉexport = Prepareˉmain(Valueˉtype.I32, "fn() -> i32");
        try
        {
            var Result = Executeˉfunction(Mainˉexport.Targetˉindex, null, 0, 1);
            return new(Result!.Value.I32ˉvalue, Executedˉinstructions);
        }
        finally
        {
            Dynamicˉlifetime?.Completeˉrun();
        }
    }

    public Runtimeˉbytesˉresult Runˉmainˉbytes()
    {
        var Mainˉexport = Prepareˉmain(Valueˉtype.Bytes, "fn() -> bytes");
        try
        {
            var Result = Executeˉfunction(Mainˉexport.Targetˉindex, null, 0, 1);
            return new(Result!.Value.Bytesˉvalue.Toˉimmutableˉarray(), Executedˉinstructions);
        }
        finally
        {
            Dynamicˉlifetime?.Completeˉrun();
        }
    }

    public Runtimeˉbytesˉresult Runˉmainˉbytes(ImmutableArray<byte> input)
    {
        var Mainˉexport = Prepareˉmain(
            [Valueˉtype.Bytes],
            Valueˉtype.Bytes,
            "fn(bytes) -> bytes");
        using var Arguments = new Runtimeˉstack(1, Dynamicˉlifetime);
        Arguments.Push(Runtimeˉvalue.Fromˉbytes(input));
        try
        {
            var Result = Executeˉfunction(Mainˉexport.Targetˉindex, Arguments, 1, 1);
            return new(Result!.Value.Bytesˉvalue.Toˉimmutableˉarray(), Executedˉinstructions);
        }
        finally
        {
            Dynamicˉlifetime?.Completeˉrun();
        }
    }

    public Runtimeˉtextˉresult Runˉmainˉtext(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var Mainˉexport = Prepareˉmain(
            [Valueˉtype.Text],
            Valueˉtype.Text,
            "fn(text) -> text");
        using var Arguments = new Runtimeˉstack(1, Dynamicˉlifetime);
        Arguments.Push(Runtimeˉvalue.Fromˉtext(input));
        try
        {
            var Result = Executeˉfunction(Mainˉexport.Targetˉindex, Arguments, 1, 1);
            return new(Result!.Value.Textˉvalue!, Executedˉinstructions);
        }
        finally
        {
            Dynamicˉlifetime?.Completeˉrun();
        }
    }

    private Exportˉdeclaration Prepareˉmain(Valueˉshape returnˉtype, string signature)
    {
        return Prepareˉmain([], returnˉtype, signature);
    }

    private Exportˉdeclaration Prepareˉmain(
        ImmutableArray<Valueˉshape> parameterˉtypes,
        Valueˉshape returnˉtype,
        string signature)
    {
        Requireˉauthorizedˉcapabilities();
        var Mainˉexport = Verifiedˉmodule.Module.Exports
            .FirstOrDefault(Export => StringComparer.Ordinal.Equals(Export.Name, "Main"));
        if (Mainˉexport is null)
        {
            throw new Runtimeˉexception("WVR3002", "The module does not export 'Main'.");
        }

        var Mainˉfunction = Verifiedˉmodule.Module.Functions[Mainˉexport.Targetˉindex];
        if (!Mainˉfunction.Parameterˉtypes.SequenceEqual(parameterˉtypes) ||
            Mainˉfunction.Returnˉtype != returnˉtype)
        {
            throw new Runtimeˉexception(
                "WVR3003",
                $"The exported Main function must have signature {signature}.");
        }

        Executedˉinstructions = 0;
        if (Functionˉsteps is not null)
        {
            Array.Clear(Functionˉsteps);
        }
        if (Functionˉrecordˉfields is not null)
        {
            Array.Clear(Functionˉrecordˉfields);
        }
        if (Functionˉdynamicˉvalueˉcounts is not null)
        {
            Array.Clear(Functionˉdynamicˉvalueˉcounts);
            Array.Clear(Functionˉdynamicˉvalueˉbytes!);
        }
        Dynamicˉlifetime?.Reset();
        return Mainˉexport;
    }

    private Runtimeˉvalue? Executeˉfunction(
        int functionˉindex,
        Runtimeˉstack? argumentˉstack,
        int argumentˉcount,
        int callˉdepth)
    {
        if (callˉdepth > Options.Maximumˉcallˉdepth)
        {
            throw new Runtimeˉexception("WVR3004", "The maximum call depth was exceeded.");
        }

        var Verifiedˉfunction = Verifiedˉmodule.Functions[functionˉindex];
        var Function = Verifiedˉfunction.Declaration;
        if (argumentˉcount != Function.Parameterˉtypes.Length)
        {
            throw new InvalidOperationException(
                $"Function '{Function.Name}' received {argumentˉcount} arguments; " +
                $"expected {Function.Parameterˉtypes.Length}.");
        }

        var Localˉcount = checked(Function.Parameterˉtypes.Length + Function.Localˉtypes.Length);
        // Frame storage is bounded by verified declarations and cleared before it returns to the pool.
        var Locals = ArrayPool<Runtimeˉvalue>.Shared.Rent(Math.Max(1, Localˉcount));
        try
        {
            if (argumentˉcount != 0)
            {
                if (argumentˉstack is null)
                {
                    throw new InvalidOperationException(
                        $"Function '{Function.Name}' has no argument source.");
                }

                argumentˉstack.Popˉargumentsˉto(Locals, argumentˉcount);
            }

            for (var Index = 0; Index < Function.Localˉtypes.Length; Index++)
            {
                Locals[Function.Parameterˉtypes.Length + Index] = Runtimeˉvalue.Default(
                    Function.Localˉtypes[Index],
                    Verifiedˉmodule.Module.Types);
            }

            return Executeˉfunctionˉbody(functionˉindex, Locals, callˉdepth);
        }
        finally
        {
            if (Dynamicˉlifetime is not null)
            {
                for (var Index = 0; Index < Localˉcount; Index++)
                {
                    Dynamicˉlifetime.Removeˉroots(Locals[Index]);
                }
            }
            ArrayPool<Runtimeˉvalue>.Shared.Return(Locals, clearArray: true);
        }
    }

    private Runtimeˉvalue? Executeˉfunctionˉbody(
        int functionˉindex,
        Runtimeˉvalue[] Locals,
        int callˉdepth)
    {
        var Verifiedˉfunction = Verifiedˉmodule.Functions[functionˉindex];
        var Function = Verifiedˉfunction.Declaration;
        using var Stack = new Runtimeˉstack(Function.Maximumˉstackˉdepth, Dynamicˉlifetime);
        var Instructionˉindex = 0;
        while (true)
        {
            Countˉinstruction(functionˉindex);
            var Instruction = Verifiedˉfunction.Instructions[Instructionˉindex];
            var Advance = true;

            try
            {
                switch (Instruction.Opcode)
                {
                    case Opcode.I32ˉconst:
                        Stack.Push(Runtimeˉvalue.Fromˉi32(Instruction.Signedˉoperand));
                        break;
                    case Opcode.I64ˉconst:
                        Stack.Push(Runtimeˉvalue.Fromˉi64(Instruction.Signedˉwideˉoperand));
                        break;
                    case Opcode.Boolˉconst:
                        Stack.Push(Runtimeˉvalue.Fromˉbool(Instruction.Unsignedˉoperand == 1));
                        break;
                    case Opcode.U8ˉconst:
                        Stack.Push(Runtimeˉvalue.Fromˉu8(checked((byte)Instruction.Unsignedˉoperand)));
                        break;
                    case Opcode.U32ˉconst:
                        Stack.Push(Runtimeˉvalue.Fromˉu32(Instruction.Unsignedˉoperand));
                        break;
                    case Opcode.U64ˉconst:
                        Stack.Push(Runtimeˉvalue.Fromˉu64(Instruction.Unsignedˉwideˉoperand));
                        break;
                    case Opcode.Textˉconst:
                        var Text = (Textˉdataˉdeclaration)Verifiedˉmodule.Module.Data[(int)Instruction.Unsignedˉoperand];
                        Stack.Push(Runtimeˉvalue.Fromˉtext(Text.Value));
                        break;
                    case Opcode.Bytesˉconst:
                        var Bytes = (Bytesˉdataˉdeclaration)Verifiedˉmodule.Module.Data[(int)Instruction.Unsignedˉoperand];
                        Stack.Push(Runtimeˉvalue.Fromˉbytes(Bytes.Values));
                        break;
                    case Opcode.Localˉload:
                        Stack.Push(Locals[(int)Instruction.Unsignedˉoperand]);
                        break;
                    case Opcode.Localˉstore:
                        var Localˉindex = (int)Instruction.Unsignedˉoperand;
                        var Localˉvalue = Stack.Pop();
                        Dynamicˉlifetime?.Removeˉroots(Locals[Localˉindex]);
                        Locals[Localˉindex] = Localˉvalue;
                        Dynamicˉlifetime?.Addˉroots(Localˉvalue);
                        break;
                    case Opcode.Dataˉlength:
                        var Lengthˉdata = (I32ˉarrayˉdataˉdeclaration)Verifiedˉmodule.Module.Data[(int)Instruction.Unsignedˉoperand];
                        Stack.Push(Runtimeˉvalue.Fromˉi32(Lengthˉdata.Values.Length));
                        break;
                    case Opcode.Dataˉloadˉi32:
                        var Array = (I32ˉarrayˉdataˉdeclaration)Verifiedˉmodule.Module.Data[(int)Instruction.Unsignedˉoperand];
                        var Elementˉindex = Stack.Pop().I32ˉvalue;
                        if ((uint)Elementˉindex >= (uint)Array.Values.Length)
                        {
                            throw new Runtimeˉexception(
                                "WVR3005",
                                $"Index {Elementˉindex} is outside data '{Array.Name}' with length {Array.Values.Length}.");
                        }

                        Stack.Push(Runtimeˉvalue.Fromˉi32(Array.Values[Elementˉindex]));
                        break;
                    case Opcode.Bytesˉlength:
                        Stack.Push(Runtimeˉvalue.Fromˉu32(checked((uint)Stack.Pop().Bytesˉvalue.Length)));
                        break;
                    case Opcode.Bytesˉslice:
                        var Sliceˉlength = Stack.Pop().U32ˉvalue;
                        var Sliceˉoffset = Stack.Pop().U32ˉvalue;
                        var Sliceˉsourceˉvalue = Stack.Pop();
                        var Sliceˉsource = Sliceˉsourceˉvalue.Bytesˉvalue;
                        Stack.Push(Runtimeˉvalue.Fromˉbytes(
                            Sliceˉbytes(Sliceˉsource, Sliceˉoffset, Sliceˉlength),
                            Sliceˉsourceˉvalue.Dynamicˉroots));
                        break;
                    case Opcode.Bytesˉreadˉu8:
                        var U8ˉoffset = Stack.Pop().U32ˉvalue;
                        var U8ˉsource = Stack.Pop().Bytesˉvalue;
                        Stack.Push(Runtimeˉvalue.Fromˉu8(
                            U8ˉsource.Storage.Read(Requireˉbyteˉrange(
                                U8ˉsource,
                                U8ˉoffset,
                                sizeof(byte)))));
                        break;
                    case Opcode.Bytesˉreadˉu16ˉlittle:
                        var U16ˉoffset = Stack.Pop().U32ˉvalue;
                        var U16ˉsource = Stack.Pop().Bytesˉvalue;
                        var U16ˉabsolute = Requireˉbyteˉrange(U16ˉsource, U16ˉoffset, sizeof(ushort));
                        Stack.Push(Runtimeˉvalue.Fromˉu32(Readˉu16(U16ˉsource.Storage, U16ˉabsolute)));
                        break;
                    case Opcode.Bytesˉreadˉu32ˉlittle:
                        var U32ˉoffset = Stack.Pop().U32ˉvalue;
                        var U32ˉsource = Stack.Pop().Bytesˉvalue;
                        var U32ˉabsolute = Requireˉbyteˉrange(U32ˉsource, U32ˉoffset, sizeof(uint));
                        Stack.Push(Runtimeˉvalue.Fromˉu32(Readˉu32(U32ˉsource.Storage, U32ˉabsolute)));
                        break;
                    case Opcode.Bytesˉreadˉi32ˉlittle:
                        var Readˉi32ˉoffset = Stack.Pop().U32ˉvalue;
                        var Readˉi32ˉsource = Stack.Pop().Bytesˉvalue;
                        var Readˉi32ˉabsolute = Requireˉbyteˉrange(
                            Readˉi32ˉsource,
                            Readˉi32ˉoffset,
                            sizeof(int));
                        Stack.Push(Runtimeˉvalue.Fromˉi32(Readˉi32(
                            Readˉi32ˉsource.Storage,
                            Readˉi32ˉabsolute)));
                        break;
                    case Opcode.I32ˉadd:
                        Applyˉi32ˉbinary(Stack, (Left, Right) => checked(Left + Right));
                        break;
                    case Opcode.I32ˉsubtract:
                        Applyˉi32ˉbinary(Stack, (Left, Right) => checked(Left - Right));
                        break;
                    case Opcode.I32ˉmultiply:
                        Applyˉi32ˉbinary(Stack, (Left, Right) => checked(Left * Right));
                        break;
                    case Opcode.I32ˉnegate:
                        Stack.Push(Runtimeˉvalue.Fromˉi32(checked(-Stack.Pop().I32ˉvalue)));
                        break;
                    case Opcode.I64ˉadd:
                        Applyˉi64ˉbinary(Stack, (Left, Right) => checked(Left + Right));
                        break;
                    case Opcode.I64ˉsubtract:
                        Applyˉi64ˉbinary(Stack, (Left, Right) => checked(Left - Right));
                        break;
                    case Opcode.I64ˉmultiply:
                        Applyˉi64ˉbinary(Stack, (Left, Right) => checked(Left * Right));
                        break;
                    case Opcode.I64ˉnegate:
                        Stack.Push(Runtimeˉvalue.Fromˉi64(checked(-Stack.Pop().I64ˉvalue)));
                        break;
                    case Opcode.U32ˉadd:
                        Applyˉu32ˉbinary(Stack, (Left, Right) => checked(Left + Right));
                        break;
                    case Opcode.U32ˉsubtract:
                        Applyˉu32ˉbinary(Stack, (Left, Right) => checked(Left - Right));
                        break;
                    case Opcode.U32ˉmultiply:
                        Applyˉu32ˉbinary(Stack, (Left, Right) => checked(Left * Right));
                        break;
                    case Opcode.U64ˉadd:
                        Applyˉu64ˉbinary(Stack, (Left, Right) => checked(Left + Right));
                        break;
                    case Opcode.U64ˉsubtract:
                        Applyˉu64ˉbinary(Stack, (Left, Right) => checked(Left - Right));
                        break;
                    case Opcode.U64ˉmultiply:
                        Applyˉu64ˉbinary(Stack, (Left, Right) => checked(Left * Right));
                        break;
                    case Opcode.I32ˉequal:
                        Applyˉi32ˉcomparison(Stack, (Left, Right) => Left == Right);
                        break;
                    case Opcode.I32ˉnotˉequal:
                        Applyˉi32ˉcomparison(Stack, (Left, Right) => Left != Right);
                        break;
                    case Opcode.I32ˉless:
                        Applyˉi32ˉcomparison(Stack, (Left, Right) => Left < Right);
                        break;
                    case Opcode.I32ˉlessˉequal:
                        Applyˉi32ˉcomparison(Stack, (Left, Right) => Left <= Right);
                        break;
                    case Opcode.I32ˉgreater:
                        Applyˉi32ˉcomparison(Stack, (Left, Right) => Left > Right);
                        break;
                    case Opcode.I32ˉgreaterˉequal:
                        Applyˉi32ˉcomparison(Stack, (Left, Right) => Left >= Right);
                        break;
                    case Opcode.I64ˉequal:
                        Applyˉi64ˉcomparison(Stack, (Left, Right) => Left == Right);
                        break;
                    case Opcode.I64ˉnotˉequal:
                        Applyˉi64ˉcomparison(Stack, (Left, Right) => Left != Right);
                        break;
                    case Opcode.I64ˉless:
                        Applyˉi64ˉcomparison(Stack, (Left, Right) => Left < Right);
                        break;
                    case Opcode.I64ˉlessˉequal:
                        Applyˉi64ˉcomparison(Stack, (Left, Right) => Left <= Right);
                        break;
                    case Opcode.I64ˉgreater:
                        Applyˉi64ˉcomparison(Stack, (Left, Right) => Left > Right);
                        break;
                    case Opcode.I64ˉgreaterˉequal:
                        Applyˉi64ˉcomparison(Stack, (Left, Right) => Left >= Right);
                        break;
                    case Opcode.Boolˉequal:
                        Applyˉboolˉcomparison(Stack, (Left, Right) => Left == Right);
                        break;
                    case Opcode.Boolˉnotˉequal:
                        Applyˉboolˉcomparison(Stack, (Left, Right) => Left != Right);
                        break;
                    case Opcode.Boolˉnot:
                        Stack.Push(Runtimeˉvalue.Fromˉbool(!Stack.Pop().Boolˉvalue));
                        break;
                    case Opcode.U32ˉequal:
                        Applyˉu32ˉcomparison(Stack, (Left, Right) => Left == Right);
                        break;
                    case Opcode.U32ˉnotˉequal:
                        Applyˉu32ˉcomparison(Stack, (Left, Right) => Left != Right);
                        break;
                    case Opcode.U32ˉless:
                        Applyˉu32ˉcomparison(Stack, (Left, Right) => Left < Right);
                        break;
                    case Opcode.U32ˉlessˉequal:
                        Applyˉu32ˉcomparison(Stack, (Left, Right) => Left <= Right);
                        break;
                    case Opcode.U32ˉgreater:
                        Applyˉu32ˉcomparison(Stack, (Left, Right) => Left > Right);
                        break;
                    case Opcode.U32ˉgreaterˉequal:
                        Applyˉu32ˉcomparison(Stack, (Left, Right) => Left >= Right);
                        break;
                    case Opcode.U64ˉequal:
                        Applyˉu64ˉcomparison(Stack, (Left, Right) => Left == Right);
                        break;
                    case Opcode.U64ˉnotˉequal:
                        Applyˉu64ˉcomparison(Stack, (Left, Right) => Left != Right);
                        break;
                    case Opcode.U64ˉless:
                        Applyˉu64ˉcomparison(Stack, (Left, Right) => Left < Right);
                        break;
                    case Opcode.U64ˉlessˉequal:
                        Applyˉu64ˉcomparison(Stack, (Left, Right) => Left <= Right);
                        break;
                    case Opcode.U64ˉgreater:
                        Applyˉu64ˉcomparison(Stack, (Left, Right) => Left > Right);
                        break;
                    case Opcode.U64ˉgreaterˉequal:
                        Applyˉu64ˉcomparison(Stack, (Left, Right) => Left >= Right);
                        break;
                    case Opcode.U8ˉequal:
                        Applyˉu8ˉcomparison(Stack, (Left, Right) => Left == Right);
                        break;
                    case Opcode.U8ˉnotˉequal:
                        Applyˉu8ˉcomparison(Stack, (Left, Right) => Left != Right);
                        break;
                    case Opcode.Enumˉconst:
                        var Enumˉtype = (Enumˉtypeˉdeclaration)Verifiedˉmodule.Module.Types[
                            (int)Instruction.Unsignedˉoperand];
                        Stack.Push(Runtimeˉvalue.Fromˉenum(
                            (int)Instruction.Unsignedˉoperand,
                            Enumˉtype.Members[(int)Instruction.Secondˉunsignedˉoperand].Value));
                        break;
                    case Opcode.Enumˉequal:
                        Applyˉenumˉcomparison(Stack, (Left, Right) => Left == Right);
                        break;
                    case Opcode.Enumˉnotˉequal:
                        Applyˉenumˉcomparison(Stack, (Left, Right) => Left != Right);
                        break;
                    case Opcode.Enumˉname:
                        var Enumˉvalue = Stack.Pop();
                        var Namedˉenum = (Enumˉtypeˉdeclaration)Verifiedˉmodule.Module.Types[
                            Enumˉvalue.Type.Nominalˉtypeˉindex];
                        var Enumˉname = Namedˉenum.Members.Single(
                            Member => Member.Value == Enumˉvalue.Enumˉvalue).Name;
                        var Enumˉnameˉroots = Recordˉdynamicˉvalue(
                            functionˉindex,
                            Runtimeˉdynamicˉvalueˉkind.Enumˉname,
                            STRICT_UTF8.GetByteCount(Enumˉname));
                        Stack.Push(Runtimeˉvalue.Fromˉtext(Enumˉname, Enumˉnameˉroots));
                        break;
                    case Opcode.I32ˉformat:
                        var I32ˉformatted = Stack.Pop().I32ˉvalue.ToString(CultureInfo.InvariantCulture);
                        var I32ˉformattedˉroots = Recordˉdynamicˉvalue(
                            functionˉindex,
                            Runtimeˉdynamicˉvalueˉkind.I32ˉformat,
                            I32ˉformatted.Length);
                        Stack.Push(Runtimeˉvalue.Fromˉtext(I32ˉformatted, I32ˉformattedˉroots));
                        break;
                    case Opcode.I64ˉformat:
                        var I64ˉformatted = Stack.Pop().I64ˉvalue.ToString(CultureInfo.InvariantCulture);
                        var I64ˉformattedˉroots = Recordˉdynamicˉvalue(
                            functionˉindex,
                            Runtimeˉdynamicˉvalueˉkind.I64ˉformat,
                            I64ˉformatted.Length);
                        Stack.Push(Runtimeˉvalue.Fromˉtext(I64ˉformatted, I64ˉformattedˉroots));
                        break;
                    case Opcode.U8ˉformat:
                        var U8ˉformatted = Stack.Pop().U8ˉvalue.ToString(CultureInfo.InvariantCulture);
                        var U8ˉformattedˉroots = Recordˉdynamicˉvalue(
                            functionˉindex,
                            Runtimeˉdynamicˉvalueˉkind.U8ˉformat,
                            U8ˉformatted.Length);
                        Stack.Push(Runtimeˉvalue.Fromˉtext(U8ˉformatted, U8ˉformattedˉroots));
                        break;
                    case Opcode.U32ˉformat:
                        var U32ˉformatted = Stack.Pop().U32ˉvalue.ToString(CultureInfo.InvariantCulture);
                        var U32ˉformattedˉroots = Recordˉdynamicˉvalue(
                            functionˉindex,
                            Runtimeˉdynamicˉvalueˉkind.U32ˉformat,
                            U32ˉformatted.Length);
                        Stack.Push(Runtimeˉvalue.Fromˉtext(U32ˉformatted, U32ˉformattedˉroots));
                        break;
                    case Opcode.U64ˉformat:
                        var U64ˉformatted = Stack.Pop().U64ˉvalue.ToString(CultureInfo.InvariantCulture);
                        var U64ˉformattedˉroots = Recordˉdynamicˉvalue(
                            functionˉindex,
                            Runtimeˉdynamicˉvalueˉkind.U64ˉformat,
                            U64ˉformatted.Length);
                        Stack.Push(Runtimeˉvalue.Fromˉtext(U64ˉformatted, U64ˉformattedˉroots));
                        break;
                    case Opcode.U32ˉfromˉu8:
                        Stack.Push(Runtimeˉvalue.Fromˉu32(Stack.Pop().U8ˉvalue));
                        break;
                    case Opcode.Textˉconcat:
                        var Rightˉtextˉvalue = Stack.Pop();
                        var Leftˉtextˉvalue = Stack.Pop();
                        var Rightˉtext = Rightˉtextˉvalue.Textˉvalue!;
                        var Leftˉtext = Leftˉtextˉvalue.Textˉvalue!;
                        var Utf8ˉlength = checked(
                            Encoding.UTF8.GetByteCount(Leftˉtext) + Encoding.UTF8.GetByteCount(Rightˉtext));
                        if (Utf8ˉlength > Bytecodeˉlimits.MAX_UTF8_VALUE_BYTES)
                        {
                            throw new Runtimeˉexception(
                                "WVR3012",
                                $"Text concatenation result {Utf8ˉlength} exceeds the UTF-8 value limit.");
                        }

                        var Concatenatedˉtextˉroots = Recordˉdynamicˉvalue(
                            functionˉindex,
                            Runtimeˉdynamicˉvalueˉkind.Textˉconcat,
                            Utf8ˉlength,
                            Leftˉtextˉvalue,
                            Rightˉtextˉvalue);
                        Stack.Push(Runtimeˉvalue.Fromˉtext(
                            string.Concat(Leftˉtext, Rightˉtext),
                            Concatenatedˉtextˉroots));
                        break;
                    case Opcode.Textˉutf8ˉisˉvalid:
                        var Utf8ˉcandidate = Stack.Pop().Bytesˉvalue;
                        Stack.Push(Runtimeˉvalue.Fromˉbool(Isˉvalidˉutf8(Utf8ˉcandidate)));
                        break;
                    case Opcode.Textˉfromˉutf8:
                        var Utf8ˉsourceˉvalue = Stack.Pop();
                        var Utf8ˉsource = Utf8ˉsourceˉvalue.Bytesˉvalue;
                        if (Utf8ˉsource.Length > Bytecodeˉlimits.MAX_UTF8_VALUE_BYTES)
                        {
                            throw new Runtimeˉexception(
                                "WVR3012",
                                $"Decoded text result exceeds the UTF-8 value limit {Bytecodeˉlimits.MAX_UTF8_VALUE_BYTES}.");
                        }

                        try
                        {
                            Stack.Push(Runtimeˉvalue.Fromˉtext(
                                STRICT_UTF8.GetString(Utf8ˉsource.Toˉarray()),
                                Utf8ˉsourceˉvalue.Dynamicˉroots));
                        }
                        catch (DecoderFallbackException)
                        {
                            throw new Runtimeˉexception(
                                "WVR3014",
                                "Textˉfromˉutf8 received an invalid UTF-8 byte sequence.");
                        }

                        break;
                    case Opcode.Textˉquote:
                        var Quoteˉsource = Stack.Pop();
                        var Quotedˉtext = Quoteˉtext(Quoteˉsource.Textˉvalue!);
                        var Quotedˉtextˉroots = Recordˉdynamicˉvalue(
                            functionˉindex,
                            Runtimeˉdynamicˉvalueˉkind.Textˉquote,
                            STRICT_UTF8.GetByteCount(Quotedˉtext),
                            Quoteˉsource);
                        Stack.Push(Runtimeˉvalue.Fromˉtext(Quotedˉtext, Quotedˉtextˉroots));
                        break;
                    case Opcode.Bytesˉconcat:
                        var Rightˉbytesˉvalue = Stack.Pop();
                        var Leftˉbytesˉvalue = Stack.Pop();
                        var Rightˉbytes = Rightˉbytesˉvalue.Bytesˉvalue;
                        var Leftˉbytes = Leftˉbytesˉvalue.Bytesˉvalue;
                        var Concatenatedˉbytes = Concatˉbytes(Leftˉbytes, Rightˉbytes);
                        var Concatenatedˉbytesˉroots = Recordˉdynamicˉvalue(
                            functionˉindex,
                            Runtimeˉdynamicˉvalueˉkind.Bytesˉconcat,
                            Concatenatedˉbytes.Length,
                            Leftˉbytesˉvalue,
                            Rightˉbytesˉvalue);
                        Stack.Push(Runtimeˉvalue.Fromˉbytes(
                            Concatenatedˉbytes,
                            Concatenatedˉbytesˉroots));
                        break;
                    case Opcode.Bytesˉfromˉu8:
                        var U8ˉbyte = Stack.Pop().U8ˉvalue;
                        var U8ˉbytesˉroots = Recordˉdynamicˉvalue(
                            functionˉindex,
                            Runtimeˉdynamicˉvalueˉkind.Bytesˉfromˉu8,
                            sizeof(byte));
                        Stack.Push(Runtimeˉvalue.Fromˉbytes(
                            ImmutableArray.Create(U8ˉbyte),
                            U8ˉbytesˉroots));
                        break;
                    case Opcode.Bytesˉfromˉu16ˉlittle:
                        var U16ˉvalue = Stack.Pop().U32ˉvalue;
                        if (U16ˉvalue > ushort.MaxValue)
                        {
                            throw new Runtimeˉexception(
                                "WVR3016",
                                $"Bytesˉfromˉu16ˉlittle received {U16ˉvalue}; the maximum is {ushort.MaxValue}.");
                        }

                        var U16ˉbytes = new byte[sizeof(ushort)];
                        BinaryPrimitives.WriteUInt16LittleEndian(U16ˉbytes, (ushort)U16ˉvalue);
                        var U16ˉbytesˉroots = Recordˉdynamicˉvalue(
                            functionˉindex,
                            Runtimeˉdynamicˉvalueˉkind.Bytesˉfromˉu16ˉlittle,
                            U16ˉbytes.Length);
                        Stack.Push(Runtimeˉvalue.Fromˉbytes(
                            ImmutableArray.Create(U16ˉbytes),
                            U16ˉbytesˉroots));
                        break;
                    case Opcode.Bytesˉfromˉu32ˉlittle:
                        var U32ˉbytes = new byte[sizeof(uint)];
                        BinaryPrimitives.WriteUInt32LittleEndian(U32ˉbytes, Stack.Pop().U32ˉvalue);
                        var U32ˉbytesˉroots = Recordˉdynamicˉvalue(
                            functionˉindex,
                            Runtimeˉdynamicˉvalueˉkind.Bytesˉfromˉu32ˉlittle,
                            U32ˉbytes.Length);
                        Stack.Push(Runtimeˉvalue.Fromˉbytes(
                            ImmutableArray.Create(U32ˉbytes),
                            U32ˉbytesˉroots));
                        break;
                    case Opcode.Bytesˉfromˉi32ˉlittle:
                        var I32ˉbytes = new byte[sizeof(int)];
                        BinaryPrimitives.WriteInt32LittleEndian(I32ˉbytes, Stack.Pop().I32ˉvalue);
                        var I32ˉbytesˉroots = Recordˉdynamicˉvalue(
                            functionˉindex,
                            Runtimeˉdynamicˉvalueˉkind.Bytesˉfromˉi32ˉlittle,
                            I32ˉbytes.Length);
                        Stack.Push(Runtimeˉvalue.Fromˉbytes(
                            ImmutableArray.Create(I32ˉbytes),
                            I32ˉbytesˉroots));
                        break;
                    case Opcode.Bytesˉsha256ˉhex:
                        Stack.Push(Runtimeˉvalue.Fromˉtext(
                            Convert.ToHexStringLower(SHA256.HashData(Stack.Pop().Bytesˉvalue.Toˉarray()))));
                        break;
                    case Opcode.Textˉtoˉutf8:
                        var Utf8ˉtext = Stack.Pop();
                        try
                        {
                            Stack.Push(Runtimeˉvalue.Fromˉbytes(
                                ImmutableArray.Create(STRICT_UTF8.GetBytes(Utf8ˉtext.Textˉvalue!)),
                                Utf8ˉtext.Dynamicˉroots));
                        }
                        catch (EncoderFallbackException)
                        {
                            throw new Runtimeˉexception(
                                "WVR3014",
                                "Textˉtoˉutf8 received an invalid Unicode value.");
                        }

                        break;
                    case Opcode.Recordˉcreate:
                        var Recordˉtype = (Recordˉtypeˉdeclaration)Verifiedˉmodule.Module.Types[
                            (int)Instruction.Unsignedˉoperand];
                        if (Functionˉrecordˉfields is not null)
                        {
                            Functionˉrecordˉfields[functionˉindex] = checked(
                                Functionˉrecordˉfields[functionˉindex] +
                                Recordˉtype.Fields.Length);
                        }
                        var Recordˉfields = Popˉarguments(Stack, Recordˉtype.Fields.Length);
                        Stack.Push(Runtimeˉvalue.Fromˉrecord(
                            (int)Instruction.Unsignedˉoperand,
                            Recordˉfields,
                            Dynamicˉlifetime is null
                                ? null
                                : Runtimeˉdynamicˉrootˉset.Combine(Recordˉfields)));
                        break;
                    case Opcode.Recordˉfield:
                        var Record = Stack.Pop().Recordˉvalue!;
                        Stack.Push(Record.Fields[(int)Instruction.Unsignedˉoperand]);
                        break;
                    case Opcode.Jump:
                        Instructionˉindex = Instructionˉindices[functionˉindex][(int)Instruction.Unsignedˉoperand];
                        Advance = false;
                        break;
                    case Opcode.Branchˉfalse:
                        if (!Stack.Pop().Boolˉvalue)
                        {
                            Instructionˉindex = Instructionˉindices[functionˉindex][(int)Instruction.Unsignedˉoperand];
                            Advance = false;
                        }

                        break;
                    case Opcode.Call:
                        var Calledˉfunction = Verifiedˉmodule.Module.Functions[(int)Instruction.Unsignedˉoperand];
                        var Callˉresult = Executeˉfunction(
                            (int)Instruction.Unsignedˉoperand,
                            Stack,
                            Calledˉfunction.Parameterˉtypes.Length,
                            callˉdepth + 1);
                        if (Callˉresult is not null)
                        {
                            Stack.Push(Callˉresult.Value);
                        }

                        break;
                    case Opcode.Callˉcapability:
                        var Capability = Verifiedˉmodule.Module.Capabilities[(int)Instruction.Unsignedˉoperand];
                        var Capabilityˉarguments = Popˉarguments(Stack, Capability.Parameterˉtypes.Length);
                        if (Dynamicˉlifetime is not null)
                        {
                            foreach (var Argument in Capabilityˉarguments)
                            {
                                Dynamicˉlifetime.Addˉroots(Argument);
                            }
                        }
                        Runtimeˉvalue? Capabilityˉresult;
                        try
                        {
                            if (StringComparer.Ordinal.Equals(
                                    Capability.Name,
                                    Capabilityˉcatalog.FILESYSTEM_DIRECTORY_READ_V1) &&
                                Readˉonlyˉdirectoryˉcontract.Tryˉrejectˉrequest(
                                    Capabilityˉarguments[0].Textˉvalue!,
                                    Capabilityˉarguments[1].U32ˉvalue,
                                    Capabilityˉarguments[2].U32ˉvalue,
                                    out var Rejection))
                            {
                                Capabilityˉresult = Runtimeˉvalue.Fromˉbytes(Rejection);
                            }
                            else
                            {
                                Capabilityˉresult = Capabilityˉhost.Invoke(
                                    Capability,
                                    Capabilityˉarguments);
                            }
                        }
                        finally
                        {
                            if (Dynamicˉlifetime is not null)
                            {
                                foreach (var Argument in Capabilityˉarguments)
                                {
                                    Dynamicˉlifetime.Removeˉroots(Argument);
                                }
                            }
                        }
                        Validateˉcapabilityˉresult(
                            Capability,
                            Capabilityˉresult,
                            Capabilityˉarguments,
                            Function.Name,
                            Instruction.Offset);
                        if (Capabilityˉresult is not null)
                        {
                            Stack.Push(Capabilityˉresult.Value);
                        }

                        break;
                    case Opcode.Pop:
                        Stack.Pop();
                        break;
                    case Opcode.Return:
                        return Function.Returnˉtype == Valueˉtype.Void ? null : Stack.Pop();
                    default:
                        throw new Runtimeˉexception(
                            "WVR3006",
                            $"The verified opcode '{Instruction.Opcode}' is not implemented by the runtime.");
                }
            }
            catch (OverflowException)
            {
                throw new Runtimeˉexception(
                    "WVR3007",
                    $"Integer overflow in function '{Function.Name}' at bytecode offset {Instruction.Offset}.");
            }

            if (Advance)
            {
                Instructionˉindex++;
            }
        }
    }

    private void Requireˉauthorizedˉcapabilities()
    {
        foreach (var Capability in Verifiedˉmodule.Module.Capabilities)
        {
            if (!Options.Authorizedˉcapabilities.Contains(Capability.Name))
            {
                throw new Runtimeˉexception(
                    "WVR3010",
                    $"Capability '{Capability.Name}' was declared but not authorized.");
            }

            if (!Capabilityˉhost.Supports(Capability.Name))
            {
                throw new Runtimeˉexception(
                    "WVR3001",
                    $"The host does not implement capability '{Capability.Name}'.");
            }
        }
    }

    private static void Validateˉcapabilityˉresult(
        Capabilityˉdeclaration capability,
        Runtimeˉvalue? result,
        ImmutableArray<Runtimeˉvalue> arguments,
        string functionˉname,
        int offset)
    {
        if (capability.Returnˉtype == Valueˉtype.Void)
        {
            if (result is not null)
            {
                Failˉcapabilityˉresult(capability, functionˉname, offset, "returned a value for void");
            }

            return;
        }

        if (result is null)
        {
            Failˉcapabilityˉresult(capability, functionˉname, offset, "returned no value");
        }

        var Value = result!.Value;
        if (Value.Type != capability.Returnˉtype)
        {
            Failˉcapabilityˉresult(
                capability,
                functionˉname,
                offset,
                $"returned {Value.Type} instead of {capability.Returnˉtype}");
        }

        if (Value.Type.Kind == Valueˉtype.Text)
        {
            if (Value.Textˉvalue is null)
            {
                Failˉcapabilityˉresult(capability, functionˉname, offset, "returned null text");
            }

            int Utf8ˉlength;
            try
            {
                Utf8ˉlength = STRICT_UTF8.GetByteCount(Value.Textˉvalue!);
            }
            catch (EncoderFallbackException)
            {
                Failˉcapabilityˉresult(capability, functionˉname, offset, "returned invalid Unicode text");
                return;
            }

            if (Utf8ˉlength > Bytecodeˉlimits.MAX_UTF8_VALUE_BYTES)
            {
                Failˉcapabilityˉresult(
                    capability,
                    functionˉname,
                    offset,
                    $"returned {Utf8ˉlength} UTF-8 bytes; the text limit is {Bytecodeˉlimits.MAX_UTF8_VALUE_BYTES}");
            }
        }

        if (Value.Type.Kind == Valueˉtype.Bytes &&
            Value.Bytesˉvalue.Length > Bytecodeˉlimits.MAX_BYTE_DATA_BYTES)
        {
            Failˉcapabilityˉresult(
                capability,
                functionˉname,
                offset,
                $"returned {Value.Bytesˉvalue.Length} bytes; the byte-value limit is {Bytecodeˉlimits.MAX_BYTE_DATA_BYTES}");
        }

        if (StringComparer.Ordinal.Equals(
                capability.Name,
                Capabilityˉcatalog.FILESYSTEM_DIRECTORY_READ_V1) &&
            Value.Type.Kind == Valueˉtype.Bytes)
        {
            Readˉonlyˉdirectoryˉcontract.Verifyˉresponse(
                Value.Bytesˉvalue.Toˉarray(),
                arguments[0].Textˉvalue!,
                arguments[1].U32ˉvalue,
                arguments[2].U32ˉvalue);
        }
    }

    private static void Failˉcapabilityˉresult(
        Capabilityˉdeclaration capability,
        string functionˉname,
        int offset,
        string reason)
    {
        throw new Runtimeˉexception(
            "WVR3013",
            $"Capability '{capability.Name}' {reason} in function '{functionˉname}' at bytecode offset {offset}.");
    }

    private void Countˉinstruction(int functionˉindex)
    {
        if (Executedˉinstructions >= Options.Maximumˉinstructions)
        {
            throw new Runtimeˉexception(
                "WVR3011",
                $"The instruction limit {Options.Maximumˉinstructions} was exceeded.");
        }

        Executedˉinstructions++;
        if (Functionˉsteps is not null)
        {
            Functionˉsteps[functionˉindex]++;
        }
    }

    private Runtimeˉdynamicˉrootˉset? Recordˉdynamicˉvalue(
        int functionˉindex,
        Runtimeˉdynamicˉvalueˉkind kind,
        int bytes,
        Runtimeˉvalue? firstˉinput = null,
        Runtimeˉvalue? secondˉinput = null)
    {
        if (bytes < 0)
        {
            throw new InvalidOperationException("A dynamic value cannot have a negative byte length.");
        }

        if (Functionˉdynamicˉvalueˉcounts is not null &&
            Functionˉdynamicˉvalueˉbytes is not null)
        {
            var Kindˉindex = (int)kind;
            Functionˉdynamicˉvalueˉcounts[functionˉindex, Kindˉindex] = checked(
                Functionˉdynamicˉvalueˉcounts[functionˉindex, Kindˉindex] + 1);
            Functionˉdynamicˉvalueˉbytes[functionˉindex, Kindˉindex] = checked(
                Functionˉdynamicˉvalueˉbytes[functionˉindex, Kindˉindex] + bytes);
        }

        return Dynamicˉlifetime?.Allocate(
            functionˉindex,
            kind,
            bytes,
            firstˉinput,
            secondˉinput);
    }

    private static ImmutableArray<Runtimeˉvalue> Popˉarguments(
        Runtimeˉstack stack,
        int count)
    {
        var Arguments = new Runtimeˉvalue[count];
        for (var Index = count - 1; Index >= 0; Index--)
        {
            Arguments[Index] = stack.Pop();
        }

        return [.. Arguments];
    }

    private static void Applyˉi32ˉbinary(
        Runtimeˉstack stack,
        Func<int, int, int> operation)
    {
        var Right = stack.Pop().I32ˉvalue;
        var Left = stack.Pop().I32ˉvalue;
        stack.Push(Runtimeˉvalue.Fromˉi32(operation(Left, Right)));
    }

    private static void Applyˉi32ˉcomparison(
        Runtimeˉstack stack,
        Func<int, int, bool> operation)
    {
        var Right = stack.Pop().I32ˉvalue;
        var Left = stack.Pop().I32ˉvalue;
        stack.Push(Runtimeˉvalue.Fromˉbool(operation(Left, Right)));
    }

    private static void Applyˉi64ˉbinary(
        Runtimeˉstack stack,
        Func<long, long, long> operation)
    {
        var Right = stack.Pop().I64ˉvalue;
        var Left = stack.Pop().I64ˉvalue;
        stack.Push(Runtimeˉvalue.Fromˉi64(operation(Left, Right)));
    }

    private static void Applyˉi64ˉcomparison(
        Runtimeˉstack stack,
        Func<long, long, bool> operation)
    {
        var Right = stack.Pop().I64ˉvalue;
        var Left = stack.Pop().I64ˉvalue;
        stack.Push(Runtimeˉvalue.Fromˉbool(operation(Left, Right)));
    }

    private static void Applyˉu32ˉbinary(
        Runtimeˉstack stack,
        Func<uint, uint, uint> operation)
    {
        var Right = stack.Pop().U32ˉvalue;
        var Left = stack.Pop().U32ˉvalue;
        stack.Push(Runtimeˉvalue.Fromˉu32(operation(Left, Right)));
    }

    private static void Applyˉu32ˉcomparison(
        Runtimeˉstack stack,
        Func<uint, uint, bool> operation)
    {
        var Right = stack.Pop().U32ˉvalue;
        var Left = stack.Pop().U32ˉvalue;
        stack.Push(Runtimeˉvalue.Fromˉbool(operation(Left, Right)));
    }

    private static void Applyˉu64ˉbinary(
        Runtimeˉstack stack,
        Func<ulong, ulong, ulong> operation)
    {
        var Right = stack.Pop().U64ˉvalue;
        var Left = stack.Pop().U64ˉvalue;
        stack.Push(Runtimeˉvalue.Fromˉu64(operation(Left, Right)));
    }

    private static void Applyˉu64ˉcomparison(
        Runtimeˉstack stack,
        Func<ulong, ulong, bool> operation)
    {
        var Right = stack.Pop().U64ˉvalue;
        var Left = stack.Pop().U64ˉvalue;
        stack.Push(Runtimeˉvalue.Fromˉbool(operation(Left, Right)));
    }

    private static void Applyˉu8ˉcomparison(
        Runtimeˉstack stack,
        Func<byte, byte, bool> operation)
    {
        var Right = stack.Pop().U8ˉvalue;
        var Left = stack.Pop().U8ˉvalue;
        stack.Push(Runtimeˉvalue.Fromˉbool(operation(Left, Right)));
    }

    private static void Applyˉboolˉcomparison(
        Runtimeˉstack stack,
        Func<bool, bool, bool> operation)
    {
        var Right = stack.Pop().Boolˉvalue;
        var Left = stack.Pop().Boolˉvalue;
        stack.Push(Runtimeˉvalue.Fromˉbool(operation(Left, Right)));
    }

    private static void Applyˉenumˉcomparison(
        Runtimeˉstack stack,
        Func<int, int, bool> operation)
    {
        var Right = stack.Pop().Enumˉvalue;
        var Left = stack.Pop().Enumˉvalue;
        stack.Push(Runtimeˉvalue.Fromˉbool(operation(Left, Right)));
    }

    private sealed class Runtimeˉstack : IDisposable
    {
        private Runtimeˉvalue[] Values;
        private readonly int Capacity;
        private readonly Runtimeˉdynamicˉlifetimeˉtracker? Dynamicˉlifetime;
        private int Count;

        public Runtimeˉstack(
            int capacity,
            Runtimeˉdynamicˉlifetimeˉtracker? dynamicˉlifetime)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            Capacity = capacity;
            Dynamicˉlifetime = dynamicˉlifetime;
            Values = ArrayPool<Runtimeˉvalue>.Shared.Rent(Math.Max(1, capacity));
        }

        public void Push(Runtimeˉvalue value)
        {
            if (Count >= Capacity)
            {
                throw new InvalidOperationException("The verified operand stack exceeded its declared depth.");
            }

            Values[Count++] = value;
            Dynamicˉlifetime?.Addˉroots(value);
        }

        public Runtimeˉvalue Pop()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("The verified operand stack underflowed.");
            }

            var Index = --Count;
            var Value = Values[Index];
            Values[Index] = default;
            Dynamicˉlifetime?.Removeˉroots(Value);
            return Value;
        }

        public void Popˉargumentsˉto(Runtimeˉvalue[] destination, int count)
        {
            ArgumentNullException.ThrowIfNull(destination);
            if (count < 0 || count > Count || count > destination.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            var Start = Count - count;
            // Arguments already occupy canonical left-to-right order below the stack top.
            Array.Copy(Values, Start, destination, 0, count);
            Array.Clear(Values, Start, count);
            Count = Start;
        }

        public void Dispose()
        {
            var Rentedˉvalues = Values;
            if (Rentedˉvalues.Length == 0)
            {
                return;
            }

            if (Dynamicˉlifetime is not null)
            {
                for (var Index = 0; Index < Count; Index++)
                {
                    Dynamicˉlifetime.Removeˉroots(Rentedˉvalues[Index]);
                }
            }
            Values = [];
            Count = 0;
            ArrayPool<Runtimeˉvalue>.Shared.Return(Rentedˉvalues, clearArray: true);
        }
    }

    private sealed class Runtimeˉdynamicˉlifetimeˉtracker
    {
        private readonly Runtimeˉdynamicˉallocatorˉtraceˉstate? Allocator;
        private readonly List<Runtimeˉdynamicˉrootˉset> Pendingˉreleases = [];

        public Runtimeˉdynamicˉlifetimeˉtracker(int? allocatorˉarenaˉbytes)
        {
            Allocator = allocatorˉarenaˉbytes is { } Arenaˉbytes
                ? new Runtimeˉdynamicˉallocatorˉtraceˉstate(Arenaˉbytes)
                : null;
        }

        public long Constructedˉvalues { get; private set; }

        public long Constructedˉbytes { get; private set; }

        public long Liveˉvalues { get; private set; }

        public long Liveˉbytes { get; private set; }

        public long Peakˉliveˉvalues { get; private set; }

        public long Peakˉliveˉbytes { get; private set; }

        public long Peakˉoperationˉvalues { get; private set; }

        public long Peakˉoperationˉbytes { get; private set; }

        public int Peakˉoperationˉfunctionˉindex { get; private set; } = -1;

        public Runtimeˉdynamicˉvalueˉkind? Peakˉoperationˉkind { get; private set; }

        public Runtimeˉdynamicˉrootˉset Allocate(
            int functionˉindex,
            Runtimeˉdynamicˉvalueˉkind kind,
            int bytes,
            Runtimeˉvalue? firstˉinput,
            Runtimeˉvalue? secondˉinput)
        {
            Constructedˉvalues = checked(Constructedˉvalues + 1);
            Constructedˉbytes = checked(Constructedˉbytes + bytes);

            var Operationˉvalues = checked(Liveˉvalues + 1);
            var Operationˉbytes = checked(Liveˉbytes + bytes);
            Addˉunrootedˉinputs(
                firstˉinput?.Dynamicˉroots,
                secondˉinput?.Dynamicˉroots,
                ref Operationˉvalues,
                ref Operationˉbytes);
            if (Operationˉbytes > Peakˉoperationˉbytes ||
                (Operationˉbytes == Peakˉoperationˉbytes &&
                 Operationˉvalues > Peakˉoperationˉvalues))
            {
                Peakˉoperationˉvalues = Operationˉvalues;
                Peakˉoperationˉbytes = Operationˉbytes;
                Peakˉoperationˉfunctionˉindex = functionˉindex;
                Peakˉoperationˉkind = kind;
            }

            var Backing = Runtimeˉdynamicˉrootˉset.Createˉbacking(bytes);
            if (Allocator is not null)
            {
                Flushˉpending(
                    firstˉinput?.Dynamicˉroots,
                    secondˉinput?.Dynamicˉroots);
                Allocator.Allocate(Backing);
            }
            return Backing;
        }

        public void Addˉroots(Runtimeˉvalue value)
        {
            var Roots = value.Dynamicˉroots;
            if (Roots is null)
            {
                return;
            }

            if (Roots.Isˉbacking)
            {
                Addˉbacking(Roots);
            }
            else
            {
                foreach (var Backing in Roots.Backingˉmembers)
                {
                    Addˉbacking(Backing);
                }
            }

            if (Liveˉbytes > Peakˉliveˉbytes ||
                (Liveˉbytes == Peakˉliveˉbytes && Liveˉvalues > Peakˉliveˉvalues))
            {
                Peakˉliveˉvalues = Liveˉvalues;
                Peakˉliveˉbytes = Liveˉbytes;
            }
        }

        public void Removeˉroots(Runtimeˉvalue value)
        {
            var Roots = value.Dynamicˉroots;
            if (Roots is null)
            {
                return;
            }

            if (Roots.Isˉbacking)
            {
                Removeˉbacking(Roots);
            }
            else
            {
                foreach (var Backing in Roots.Backingˉmembers)
                {
                    Removeˉbacking(Backing);
                }
            }
        }

        public void Reset()
        {
            if (Liveˉvalues != 0 || Liveˉbytes != 0)
            {
                throw new InvalidOperationException(
                    "Dynamic-value lifetime tracking retained roots after execution completed.");
            }

            Constructedˉvalues = 0;
            Constructedˉbytes = 0;
            Peakˉliveˉvalues = 0;
            Peakˉliveˉbytes = 0;
            Peakˉoperationˉvalues = 0;
            Peakˉoperationˉbytes = 0;
            Peakˉoperationˉfunctionˉindex = -1;
            Peakˉoperationˉkind = null;
            Pendingˉreleases.Clear();
            Allocator?.Reset();
        }

        public void Completeˉrun()
        {
            Flushˉpending(null, null);
        }

        public Runtimeˉdynamicˉallocatorˉtrace? Readˉallocatorˉtrace() =>
            Allocator?.Read();

        private static void Addˉunrootedˉinputs(
            Runtimeˉdynamicˉrootˉset? first,
            Runtimeˉdynamicˉrootˉset? second,
            ref long values,
            ref long bytes)
        {
            if (first is not null)
            {
                if (first.Isˉbacking)
                {
                    Addˉunrootedˉbacking(first, ref values, ref bytes);
                }
                else
                {
                    foreach (var Backing in first.Backingˉmembers)
                    {
                        Addˉunrootedˉbacking(Backing, ref values, ref bytes);
                    }
                }
            }

            if (second is null)
            {
                return;
            }

            if (second.Isˉbacking)
            {
                if (!Containsˉbacking(first, second))
                {
                    Addˉunrootedˉbacking(second, ref values, ref bytes);
                }
                return;
            }

            foreach (var Backing in second.Backingˉmembers)
            {
                if (!Containsˉbacking(first, Backing))
                {
                    Addˉunrootedˉbacking(Backing, ref values, ref bytes);
                }
            }
        }

        private void Addˉbacking(Runtimeˉdynamicˉrootˉset backing)
        {
            var Previousˉcount = backing.Rootˉcount;
            backing.Rootˉcount = checked(Previousˉcount + 1);
            if (Previousˉcount != 0)
            {
                return;
            }

            if (Allocator is not null)
            {
                if (!backing.Isˉallocatorˉallocated)
                {
                    throw new InvalidOperationException(
                        "Dynamic-value allocator tracking observed a revived released backing.");
                }
                Pendingˉreleases.Remove(backing);
            }

            Liveˉvalues = checked(Liveˉvalues + 1);
            Liveˉbytes = checked(Liveˉbytes + backing.Backingˉbytes);
        }

        private void Removeˉbacking(Runtimeˉdynamicˉrootˉset backing)
        {
            if (backing.Rootˉcount <= 0)
            {
                throw new InvalidOperationException(
                    "Dynamic-value lifetime tracking observed an unbalanced root removal.");
            }

            if (--backing.Rootˉcount != 0)
            {
                return;
            }

            Liveˉvalues--;
            Liveˉbytes -= backing.Backingˉbytes;
            if (Allocator is not null)
            {
                Pendingˉreleases.Add(backing);
            }
        }

        private static void Addˉunrootedˉbacking(
            Runtimeˉdynamicˉrootˉset backing,
            ref long values,
            ref long bytes)
        {
            if (backing.Rootˉcount != 0)
            {
                return;
            }

            values = checked(values + 1);
            bytes = checked(bytes + backing.Backingˉbytes);
        }

        private static bool Containsˉbacking(
            Runtimeˉdynamicˉrootˉset? roots,
            Runtimeˉdynamicˉrootˉset backing)
        {
            if (roots is null)
            {
                return false;
            }
            if (roots.Isˉbacking)
            {
                return ReferenceEquals(roots, backing);
            }

            return roots.Backingˉmembers.Contains(backing);
        }

        private void Flushˉpending(
            Runtimeˉdynamicˉrootˉset? firstˉinput,
            Runtimeˉdynamicˉrootˉset? secondˉinput)
        {
            if (Allocator is null)
            {
                return;
            }

            for (var Index = Pendingˉreleases.Count - 1; Index >= 0; Index--)
            {
                var Backing = Pendingˉreleases[Index];
                if (Backing.Rootˉcount != 0)
                {
                    Pendingˉreleases.RemoveAt(Index);
                    continue;
                }
                if (Containsˉbacking(firstˉinput, Backing) ||
                    Containsˉbacking(secondˉinput, Backing))
                {
                    continue;
                }

                Allocator.Release(Backing);
                Pendingˉreleases.RemoveAt(Index);
            }
        }
    }

    private sealed class Runtimeˉdynamicˉallocatorˉtraceˉstate
    {
        public const int HEADER_BYTES = 16;
        public const int ALIGNMENT_BYTES = 16;

        private readonly int Arenaˉbytes;
        private readonly List<Runtimeˉdynamicˉfreeˉspan> Freeˉspans = [];
        private long Allocations;
        private long Reusedˉallocations;
        private long Allocatedˉpayloadˉbytes;
        private long Allocatedˉchargedˉbytes;
        private int Allocatedˉblocks;
        private long Peakˉpayloadˉbytes;
        private long Peakˉchargedˉbytes;
        private int Peakˉblocks;
        private int Maximumˉaddressedˉbytes;
        private int Peakˉexternalˉfragmentationˉbytes;
        private int Maximumˉfreeˉspans;
        private long Failedˉallocations;
        private int Firstˉfailureˉpayloadˉbytes;
        private int Firstˉfailureˉchargedˉbytes;
        private int Firstˉfailureˉlargestˉfreeˉspanˉbytes;

        public Runtimeˉdynamicˉallocatorˉtraceˉstate(int arenaˉbytes)
        {
            Arenaˉbytes = arenaˉbytes;
            Initializeˉfreeˉspan();
        }

        public void Allocate(Runtimeˉdynamicˉrootˉset backing)
        {
            if (!backing.Isˉbacking || backing.Isˉallocatorˉallocated)
            {
                throw new InvalidOperationException(
                    "Dynamic allocator tracing requires one unallocated backing leaf.");
            }

            var Chargedˉbytes = Charge(backing.Backingˉbytes);
            var Spanˉindex = -1;
            for (var Index = 0; Index < Freeˉspans.Count; Index++)
            {
                if (Freeˉspans[Index].Bytes >= Chargedˉbytes)
                {
                    Spanˉindex = Index;
                    break;
                }
            }

            if (Spanˉindex < 0)
            {
                Failedˉallocations = checked(Failedˉallocations + 1);
                if (Failedˉallocations == 1)
                {
                    Firstˉfailureˉpayloadˉbytes = backing.Backingˉbytes;
                    Firstˉfailureˉchargedˉbytes = Chargedˉbytes;
                    Firstˉfailureˉlargestˉfreeˉspanˉbytes = Largestˉfreeˉspan();
                }
                throw new Runtimeˉexception(
                    "WVR3018",
                    $"The diagnostic first-fit dynamic arena exhausted its " +
                    $"{Arenaˉbytes}-byte limit while placing a {Chargedˉbytes}-byte block.");
            }

            var Span = Freeˉspans[Spanˉindex];
            var Offset = Span.Offset;
            if (Span.Bytes == Chargedˉbytes)
            {
                Freeˉspans.RemoveAt(Spanˉindex);
            }
            else
            {
                Freeˉspans[Spanˉindex] = new(
                    checked(Span.Offset + Chargedˉbytes),
                    Span.Bytes - Chargedˉbytes);
            }

            if (Offset < Maximumˉaddressedˉbytes)
            {
                Reusedˉallocations = checked(Reusedˉallocations + 1);
            }
            backing.Allocatorˉoffset = Offset;
            backing.Allocatorˉchargedˉbytes = Chargedˉbytes;
            backing.Isˉallocatorˉallocated = true;
            Allocations = checked(Allocations + 1);
            Allocatedˉpayloadˉbytes = checked(
                Allocatedˉpayloadˉbytes + backing.Backingˉbytes);
            Allocatedˉchargedˉbytes = checked(Allocatedˉchargedˉbytes + Chargedˉbytes);
            Allocatedˉblocks = checked(Allocatedˉblocks + 1);
            Maximumˉaddressedˉbytes = Math.Max(
                Maximumˉaddressedˉbytes,
                checked(Offset + Chargedˉbytes));
            if (Allocatedˉchargedˉbytes > Peakˉchargedˉbytes ||
                (Allocatedˉchargedˉbytes == Peakˉchargedˉbytes &&
                 Allocatedˉpayloadˉbytes > Peakˉpayloadˉbytes))
            {
                Peakˉpayloadˉbytes = Allocatedˉpayloadˉbytes;
                Peakˉchargedˉbytes = Allocatedˉchargedˉbytes;
                Peakˉblocks = Allocatedˉblocks;
            }
            Updateˉfreeˉmetrics();
        }

        public void Release(Runtimeˉdynamicˉrootˉset backing)
        {
            if (!backing.Isˉbacking || !backing.Isˉallocatorˉallocated ||
                backing.Allocatorˉoffset < 0 || backing.Allocatorˉchargedˉbytes <= 0)
            {
                throw new InvalidOperationException(
                    "Dynamic allocator tracing observed an invalid backing release.");
            }

            var Released = new Runtimeˉdynamicˉfreeˉspan(
                backing.Allocatorˉoffset,
                backing.Allocatorˉchargedˉbytes);
            var Insertˉindex = 0;
            while (Insertˉindex < Freeˉspans.Count &&
                   Freeˉspans[Insertˉindex].Offset < Released.Offset)
            {
                Insertˉindex++;
            }
            Freeˉspans.Insert(Insertˉindex, Released);
            Coalesceˉat(Insertˉindex);

            Allocatedˉpayloadˉbytes -= backing.Backingˉbytes;
            Allocatedˉchargedˉbytes -= backing.Allocatorˉchargedˉbytes;
            Allocatedˉblocks--;
            backing.Allocatorˉoffset = -1;
            backing.Allocatorˉchargedˉbytes = 0;
            backing.Isˉallocatorˉallocated = false;
            Updateˉfreeˉmetrics();
        }

        public Runtimeˉdynamicˉallocatorˉtrace Read() => new(
            Arenaˉbytes,
            HEADER_BYTES,
            ALIGNMENT_BYTES,
            Allocations,
            Reusedˉallocations,
            Peakˉpayloadˉbytes,
            Peakˉchargedˉbytes,
            Peakˉblocks,
            Maximumˉaddressedˉbytes,
            Peakˉexternalˉfragmentationˉbytes,
            Maximumˉfreeˉspans,
            Failedˉallocations,
            Firstˉfailureˉpayloadˉbytes,
            Firstˉfailureˉchargedˉbytes,
            Firstˉfailureˉlargestˉfreeˉspanˉbytes,
            Allocatedˉblocks,
            Allocatedˉchargedˉbytes);

        public void Reset()
        {
            if (Allocatedˉblocks != 0 || Allocatedˉpayloadˉbytes != 0 ||
                Allocatedˉchargedˉbytes != 0 || Freeˉspans.Count != 1 ||
                Freeˉspans[0] != new Runtimeˉdynamicˉfreeˉspan(0, Arenaˉbytes))
            {
                throw new InvalidOperationException(
                    "Dynamic allocator tracing did not recover its complete coalesced arena.");
            }

            Allocations = 0;
            Reusedˉallocations = 0;
            Peakˉpayloadˉbytes = 0;
            Peakˉchargedˉbytes = 0;
            Peakˉblocks = 0;
            Maximumˉaddressedˉbytes = 0;
            Peakˉexternalˉfragmentationˉbytes = 0;
            Maximumˉfreeˉspans = 1;
            Failedˉallocations = 0;
            Firstˉfailureˉpayloadˉbytes = 0;
            Firstˉfailureˉchargedˉbytes = 0;
            Firstˉfailureˉlargestˉfreeˉspanˉbytes = 0;
        }

        private int Charge(int payloadˉbytes)
        {
            if (payloadˉbytes < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(payloadˉbytes));
            }
            var Withˉheader = checked(payloadˉbytes + HEADER_BYTES);
            return checked(
                ((Withˉheader + ALIGNMENT_BYTES - 1) / ALIGNMENT_BYTES) * ALIGNMENT_BYTES);
        }

        private void Coalesceˉat(int index)
        {
            if (index > 0)
            {
                var Previous = Freeˉspans[index - 1];
                var Current = Freeˉspans[index];
                if (checked(Previous.Offset + Previous.Bytes) == Current.Offset)
                {
                    Freeˉspans[index - 1] = new(
                        Previous.Offset,
                        checked(Previous.Bytes + Current.Bytes));
                    Freeˉspans.RemoveAt(index);
                    index--;
                }
            }

            if (index + 1 < Freeˉspans.Count)
            {
                var Current = Freeˉspans[index];
                var Next = Freeˉspans[index + 1];
                if (checked(Current.Offset + Current.Bytes) == Next.Offset)
                {
                    Freeˉspans[index] = new(
                        Current.Offset,
                        checked(Current.Bytes + Next.Bytes));
                    Freeˉspans.RemoveAt(index + 1);
                }
            }
        }

        private void Initializeˉfreeˉspan()
        {
            Freeˉspans.Clear();
            Freeˉspans.Add(new(0, Arenaˉbytes));
            Maximumˉfreeˉspans = 1;
        }

        private int Largestˉfreeˉspan()
        {
            var Largest = 0;
            foreach (var Span in Freeˉspans)
            {
                Largest = Math.Max(Largest, Span.Bytes);
            }
            return Largest;
        }

        private void Updateˉfreeˉmetrics()
        {
            var Totalˉfree = checked(Arenaˉbytes - (int)Allocatedˉchargedˉbytes);
            var Externalˉfragmentation = checked(Totalˉfree - Largestˉfreeˉspan());
            Peakˉexternalˉfragmentationˉbytes = Math.Max(
                Peakˉexternalˉfragmentationˉbytes,
                Externalˉfragmentation);
            Maximumˉfreeˉspans = Math.Max(Maximumˉfreeˉspans, Freeˉspans.Count);
        }

        private readonly record struct Runtimeˉdynamicˉfreeˉspan(int Offset, int Bytes);
    }

    private static Runtimeˉbyteˉslice Sliceˉbytes(
        Runtimeˉbyteˉslice source,
        uint offset,
        uint length)
    {
        if (offset > (uint)source.Length || length > (uint)source.Length - offset)
        {
            throw new Runtimeˉexception(
                "WVR3008",
                $"Byte slice offset {offset} and length {length} exceed source length {source.Length}.");
        }

        return new(
            source.Storage,
            checked(source.Offset + (int)offset),
            checked((int)length));
    }

    private static bool Isˉvalidˉutf8(Runtimeˉbyteˉslice value)
    {
        try
        {
            _ = STRICT_UTF8.GetCharCount(value.Toˉarray());
            return true;
        }
        catch (DecoderFallbackException)
        {
            return false;
        }
    }

    private static Runtimeˉbyteˉslice Concatˉbytes(
        Runtimeˉbyteˉslice left,
        Runtimeˉbyteˉslice right)
    {
        var Length = checked(left.Length + right.Length);
        if (Length > Bytecodeˉlimits.MAX_BYTE_DATA_BYTES)
        {
            throw new Runtimeˉexception(
                "WVR3015",
                $"Concatenated byte result exceeds the byte-value limit {Bytecodeˉlimits.MAX_BYTE_DATA_BYTES}.");
        }

        var Leftˉnode = Runtimeˉbyteˉnode.Slice(left.Storage, left.Offset, left.Length);
        var Rightˉnode = Runtimeˉbyteˉnode.Slice(right.Storage, right.Offset, right.Length);
        return new(Runtimeˉbyteˉnode.Concat(Leftˉnode, Rightˉnode), 0, Length);
    }

    private static string Quoteˉtext(string value)
    {
        var Outputˉlength = 2;
        foreach (var Character in value)
        {
            Outputˉlength = checked(Outputˉlength + Character switch
            {
                '"' or '\\' or '\b' or '\f' or '\n' or '\r' or '\t' => 2,
                >= ' ' and <= '~' => 1,
                _ => 6,
            });
            if (Outputˉlength > Bytecodeˉlimits.MAX_UTF8_VALUE_BYTES)
            {
                throw new Runtimeˉexception(
                    "WVR3012",
                    $"Quoted text result exceeds the UTF-8 value limit {Bytecodeˉlimits.MAX_UTF8_VALUE_BYTES}.");
            }
        }

        var Result = new StringBuilder(Outputˉlength);
        Result.Append('"');
        foreach (var Character in value)
        {
            switch (Character)
            {
                case '"':
                    Result.Append("\\\"");
                    break;
                case '\\':
                    Result.Append("\\\\");
                    break;
                case '\b':
                    Result.Append("\\b");
                    break;
                case '\f':
                    Result.Append("\\f");
                    break;
                case '\n':
                    Result.Append("\\n");
                    break;
                case '\r':
                    Result.Append("\\r");
                    break;
                case '\t':
                    Result.Append("\\t");
                    break;
                case >= ' ' and <= '~':
                    Result.Append(Character);
                    break;
                default:
                    Result.Append("\\u");
                    Result.Append(((ushort)Character).ToString("X4", CultureInfo.InvariantCulture));
                    break;
            }
        }

        Result.Append('"');
        return Result.ToString();
    }

    private static int Requireˉbyteˉrange(
        Runtimeˉbyteˉslice source,
        uint offset,
        int width)
    {
        if (offset > (uint)source.Length || width > source.Length - (int)offset)
        {
            throw new Runtimeˉexception(
                "WVR3008",
                $"A {width}-byte read at offset {offset} exceeds source length {source.Length}.");
        }

        return checked(source.Offset + (int)offset);
    }

    private static ushort Readˉu16(Runtimeˉbyteˉnode source, int offset)
    {
        Span<byte> Buffer = stackalloc byte[sizeof(ushort)];
        source.Copyˉto(Buffer, offset, Buffer.Length);
        return BinaryPrimitives.ReadUInt16LittleEndian(Buffer);
    }

    private static uint Readˉu32(Runtimeˉbyteˉnode source, int offset)
    {
        Span<byte> Buffer = stackalloc byte[sizeof(uint)];
        source.Copyˉto(Buffer, offset, Buffer.Length);
        return BinaryPrimitives.ReadUInt32LittleEndian(Buffer);
    }

    private static int Readˉi32(Runtimeˉbyteˉnode source, int offset)
    {
        Span<byte> Buffer = stackalloc byte[sizeof(int)];
        source.Copyˉto(Buffer, offset, Buffer.Length);
        return BinaryPrimitives.ReadInt32LittleEndian(Buffer);
    }
}
