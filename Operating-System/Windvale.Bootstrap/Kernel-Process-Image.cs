using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using Windvale.Assembler;
using Windvale.Bytecode;
using Windvale.Compiler;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.ObjectModel;

namespace Windvale.Bootstrap;

public sealed record Kernelˉprocessˉimageˉartifacts(
    ImmutableArray<byte> Policyˉmoduleˉbytes,
    ImmutableArray<byte> Policyˉnativeˉobjectˉbytes,
    ImmutableArray<byte> Initˉserviceˉmoduleˉbytes,
    ImmutableArray<byte> Initˉserviceˉnativeˉobjectˉbytes,
    ImmutableArray<byte> Initˉserviceˉshimˉobjectˉbytes,
    ImmutableArray<byte> Initˉserviceˉimageˉbytes,
    ImmutableArray<byte> Initˉserviceˉdigest,
    ImmutableArray<byte> Interpreterˉmoduleˉbytes,
    ImmutableArray<byte> Interpreterˉnativeˉobjectˉbytes,
    ImmutableArray<byte> Interpreterˉdigest,
    ImmutableArray<byte> Admittedˉprogramˉdigest,
    ImmutableArray<byte> Clientˉshimˉobjectˉbytes,
    ImmutableArray<byte> Clientˉimageˉbytes);

public static class Kernelˉprocessˉimage
{
    private const string POLICY_RESOURCE = "Windvale.Os.Kernel.Process-Foundation.wv";
    private const string INIT_SERVICE_RESOURCE = "Windvale.Os.Kernel.Init-Resource-Service.wv";
    private const string INIT_SERVICE_SHIM_RESOURCE = "Windvale.Os.Kernel.Init-Resource-Service-Shim.wva";
    private const string INTERPRETER_RESOURCE = "Windvale.Os.Runtime.Bytecode-Interpreter.wv";
    private const string INTERPRETER_DATA_PREFIX = "data Admittedˉmodule: bytes = [";
    private const int MAXIMUM_INTERPRETED_MODULE_BYTES = 4_096;
    private const string USER_RESOURCE = "Windvale.Os.Kernel.Process-User-Shim.wva";
    private const string USER_FAULT_RESOURCE = "Windvale.Os.Kernel.Process-User-Fault-Shim.wva";

    public static Kernelˉprocessˉimageˉartifacts Build(
        Kernelˉwvbˉadmissionˉartifacts admission,
        bool userˉfault)
    {
        ArgumentNullException.ThrowIfNull(admission);

        var Serviceˉcompilation = Seedˉcompiler.Compile(
            Loadˉsource(INIT_SERVICE_RESOURCE), "Init-Resource-Service.wv");
        if (!Serviceˉcompilation.Success)
        {
            throw new InvalidOperationException(
                $"The Windvale init/resource service did not compile: {Serviceˉcompilation.Diagnostics[0]}");
        }
        var Serviceˉmodule = Moduleˉcodec.Readˉandˉverify(Serviceˉcompilation.Moduleˉbytes.AsSpan());
        Verifyˉservice(Serviceˉmodule);
        var Serviceˉnative = X64ˉnativeˉbackend.Compile(Serviceˉmodule);
        var Serviceˉobject = Kernelˉwvbˉadmission.Renameˉmainˉexport(
            Nativeˉobjectˉsink.Writeˉwvo(Serviceˉnative.Fragment),
            Kernelˉprocessˉcontract.INIT_SERVICE_MAIN_SYMBOL);
        var Serviceˉassembly = Assemblyˉcompiler.Assemble(Loadˉsource(INIT_SERVICE_SHIM_RESOURCE));
        if (!Serviceˉassembly.Success)
        {
            throw new InvalidOperationException(
                $"The init/resource service shim did not assemble: {Serviceˉassembly.Diagnostics[0].Code}: " +
                Serviceˉassembly.Diagnostics[0].Message);
        }
        Verifyˉshim(
            Serviceˉassembly.Objectˉbytes,
            Kernelˉprocessˉcontract.INIT_SERVICE_ENTRY_SYMBOL,
            Kernelˉprocessˉcontract.INIT_SERVICE_MAIN_SYMBOL,
            2);
        var Serviceˉlink = Linkˉcompiler.Link(
            [new(Serviceˉassembly.Objectˉbytes), new(Serviceˉobject)],
            new(0, Kernelˉprocessˉcontract.INIT_SERVICE_ENTRY_SYMBOL));
        Verifyˉlinkedˉimage(Serviceˉlink, "init/resource service", Kernelˉpagingˉcontract.PAGE_BYTES);
        var Serviceˉdigest = SHA256.HashData(Serviceˉcompilation.Moduleˉbytes.AsSpan()).ToImmutableArray();
        if (!Convert.ToHexString(Serviceˉdigest.AsSpan()).Equals(
                "478DFCD36FED7C8063CFB3F53A6A1362BDA5353656339B730BE573A1BE8F95B0",
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The init/resource service image has an unexpected WVB identity.");
        }

        var Interpreterˉsource = Loadˉsource(INTERPRETER_RESOURCE);
        if (!StringComparer.Ordinal.Equals(
                Interpreterˉsource,
                Injectˉinterpreterˉmodule(Interpreterˉsource, admission.Embeddedˉmoduleˉbytes)))
        {
            throw new InvalidOperationException(
                "Bytecode-Interpreter.wv does not embed the exact WVB admitted by the kernel policy.");
        }
        var Interpreterˉcompilation = Seedˉcompiler.Compile(
            Interpreterˉsource, "Bytecode-Interpreter.wv");
        if (!Interpreterˉcompilation.Success)
        {
            throw new InvalidOperationException(
                $"The Windvale bytecode interpreter did not compile: {Interpreterˉcompilation.Diagnostics[0]}");
        }
        var Interpreterˉmodule = Moduleˉcodec.Readˉandˉverify(Interpreterˉcompilation.Moduleˉbytes.AsSpan());
        Verifyˉinterpreter(Interpreterˉmodule, admission.Embeddedˉmoduleˉbytes);
        var Interpreterˉnative = X64ˉnativeˉbackend.Compile(Interpreterˉmodule);
        var Interpreterˉobject = Kernelˉwvbˉadmission.Renameˉmainˉexport(
            Nativeˉobjectˉsink.Writeˉwvo(Interpreterˉnative.Fragment),
            Kernelˉprocessˉcontract.BYTECODE_INTERPRETER_MAIN_SYMBOL);
        var Interpreterˉdigest = SHA256.HashData(Interpreterˉcompilation.Moduleˉbytes.AsSpan()).ToImmutableArray();
        var Interpreterˉidentity = Convert.ToHexString(Interpreterˉdigest.AsSpan());
        if (!Interpreterˉidentity.Equals(
                "909E624DF86E614B6F7DCAA61E75FFA685467015015BFAFD7B0772EE41A89920",
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"The user-space interpreter has an unexpected WVB identity: {Interpreterˉidentity}.");
        }

        var Policyˉcompilation = Seedˉcompiler.Compile(Loadˉsource(POLICY_RESOURCE), "Process-Foundation.wv");
        if (!Policyˉcompilation.Success)
        {
            throw new InvalidOperationException(
                $"The Windvale process policy did not compile: {Policyˉcompilation.Diagnostics[0]}");
        }
        var Policyˉmodule = Moduleˉcodec.Readˉandˉverify(Policyˉcompilation.Moduleˉbytes.AsSpan());
        Verifyˉpolicy(Policyˉmodule);
        var Policyˉnative = X64ˉnativeˉbackend.Compile(Policyˉmodule);
        var Policyˉobject = Kernelˉwvbˉadmission.Renameˉmainˉexport(
            Nativeˉobjectˉsink.Writeˉwvo(Policyˉnative.Fragment),
            Kernelˉprocessˉcontract.POLICY_SYMBOL);

        var Userˉassembly = Assemblyˉcompiler.Assemble(Loadˉsource(userˉfault ? USER_FAULT_RESOURCE : USER_RESOURCE));
        if (!Userˉassembly.Success)
        {
            throw new InvalidOperationException(
                $"The process user shim did not assemble: {Userˉassembly.Diagnostics[0].Code}: " +
                Userˉassembly.Diagnostics[0].Message);
        }
        Verifyˉshim(
            Userˉassembly.Objectˉbytes,
            Kernelˉprocessˉcontract.USER_ENTRY_SYMBOL,
            Kernelˉprocessˉcontract.BYTECODE_INTERPRETER_MAIN_SYMBOL,
            userˉfault ? 1 : 2);
        var Userˉlink = Linkˉcompiler.Link(
            [new(Userˉassembly.Objectˉbytes), new(Interpreterˉobject)],
            new(0, Kernelˉprocessˉcontract.USER_ENTRY_SYMBOL));
        Verifyˉlinkedˉimage(Userˉlink, "bytecode-interpreter client", Kernelˉprocessˉcontract.CLIENT_CODE_BYTES);

        var Admittedˉprogramˉdigest = SHA256.HashData(admission.Embeddedˉmoduleˉbytes.AsSpan()).ToImmutableArray();
        if (!Convert.ToHexString(Admittedˉprogramˉdigest.AsSpan()).Equals(
                "7F08EFBB20C6CC69C100F07407F759625B38C02A3F05BB4E8DABCC7BDD10C4E2",
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The process image is not bound to the admitted WVB identity.");
        }
        return new(
            Policyˉcompilation.Moduleˉbytes,
            Policyˉobject,
            Serviceˉcompilation.Moduleˉbytes,
            Serviceˉobject,
            Serviceˉassembly.Objectˉbytes,
            Serviceˉlink.Imageˉbytes,
            Serviceˉdigest,
            Interpreterˉcompilation.Moduleˉbytes,
            Interpreterˉobject,
            Interpreterˉdigest,
            Admittedˉprogramˉdigest,
            Userˉassembly.Objectˉbytes,
            Userˉlink.Imageˉbytes);
    }

    // Stage 0 test seam: change only the interpreter input while retaining the
    // Windvale-owned bounded decoder and its deterministic failure results.
    public static ImmutableArray<byte> Compileˉinterpreterˉmodule(ImmutableArray<byte> programˉbytes)
    {
        if (programˉbytes.IsDefault || programˉbytes.Length > MAXIMUM_INTERPRETED_MODULE_BYTES)
        {
            throw new ArgumentOutOfRangeException(nameof(programˉbytes));
        }
        var Compilation = Seedˉcompiler.Compile(
            Injectˉinterpreterˉmodule(Loadˉsource(INTERPRETER_RESOURCE), programˉbytes),
            "Bytecode-Interpreter.wv");
        if (!Compilation.Success)
        {
            throw new InvalidOperationException(
                $"The Windvale bytecode interpreter test module did not compile: {Compilation.Diagnostics[0]}");
        }
        return Compilation.Moduleˉbytes;
    }

    private static void Verifyˉpolicy(Verifiedˉmodule module)
    {
        if (module.Module is not
            {
                Name: "Processˉfoundation",
                Profile: Moduleˉprofile.Portable,
                Capabilities.Length: 0,
                Data.Length: 6,
                Functions.Length: 3,
                Exports.Length: 1,
                Types.Length: 0,
            } ||
            module.Module.Data.Any(Data => Data is not Bytesˉdataˉdeclaration) ||
            module.Functions.SelectMany(Function => Function.Instructions).All(Instruction =>
                Instruction.Opcode is not Opcode.Bytesˉreadˉu8) ||
            module.Functions.SelectMany(Function => Function.Instructions).All(Instruction =>
                Instruction.Opcode is not Opcode.Call))
        {
            throw new InvalidOperationException(
                $"The Windvale process policy violated '{Kernelˉprocessˉcontract.TARGET_NAME}'.");
        }
    }

    private static void Verifyˉinterpreter(
        Verifiedˉmodule module,
        ImmutableArray<byte> admittedˉmoduleˉbytes)
    {
        if (module.Module is not
            {
                Name: "Bytecodeˉinterpreter",
                Profile: Moduleˉprofile.Portable,
                Capabilities.Length: 0,
                Data.Length: 1,
                Functions.Length: 8,
                Exports.Length: 1,
                Types.Length: 0,
            } ||
            module.Module.Data[0] is not Bytesˉdataˉdeclaration Program ||
            !Program.Values.AsSpan().SequenceEqual(admittedˉmoduleˉbytes.AsSpan()) ||
            module.Functions.SelectMany(Function => Function.Instructions).All(Instruction =>
                Instruction.Opcode is not Opcode.Bytesˉreadˉi32ˉlittle) ||
            module.Functions.SelectMany(Function => Function.Instructions).All(Instruction =>
                Instruction.Opcode is not Opcode.Branchˉfalse) ||
            module.Functions.SelectMany(Function => Function.Instructions).All(Instruction =>
                Instruction.Opcode is not Opcode.Call))
        {
            throw new InvalidOperationException(
                $"The Windvale bytecode interpreter violated '{Kernelˉprocessˉcontract.TARGET_NAME}'.");
        }
    }

    private static void Verifyˉservice(Verifiedˉmodule module)
    {
        if (module.Module is not
            {
                Name: "Initˉresourceˉservice",
                Profile: Moduleˉprofile.Portable,
                Capabilities.Length: 0,
                Data.Length: 0,
                Functions.Length: 1,
                Exports.Length: 1,
                Types.Length: 0,
            })
        {
            throw new InvalidOperationException(
                $"The Windvale init/resource service violated '{Kernelˉprocessˉcontract.TARGET_NAME}'.");
        }
    }

    private static void Verifyˉshim(
        ImmutableArray<byte> objectˉbytes,
        string entryˉsymbol,
        string mainˉsymbol,
        int expectedˉsyscalls)
    {
        var Object = Objectˉcodec.Readˉandˉverify(objectˉbytes.AsSpan()).Value;
        if (Object.Sections.Length != 1 ||
            Object.Sections[0].Kind != Objectˉsectionˉkind.Code ||
            Object.Symbols.Length != 2 ||
            Object.Symbols[0] is not
            {
                Name: var Entryˉname,
                Binding: Objectˉsymbolˉbinding.Export,
                Kind: Objectˉsymbolˉkind.Function,
                Sectionˉindex: 0,
                Offset: 0,
            } ||
            Object.Symbols[1] is not
            {
                Name: var Mainˉname,
                Binding: Objectˉsymbolˉbinding.Import,
                Kind: Objectˉsymbolˉkind.Function,
            } ||
            Entryˉname != entryˉsymbol ||
            Mainˉname != mainˉsymbol ||
            Object.Relocations.Length != 1 ||
            Countˉsequence(Object.Sections[0].Data.AsSpan(), [0x0F, 0x05]) != expectedˉsyscalls)
        {
            throw new InvalidOperationException("A process WVA shim violated its fixed syscall contract.");
        }
    }

    private static void Verifyˉlinkedˉimage(Linkˉresult link, string role, ulong maximumˉbytes)
    {
        if (!link.Success || link.Entryˉaddress != 0 || link.Imageˉbytes.Length == 0 ||
            (ulong)link.Imageˉbytes.Length > maximumˉbytes)
        {
            throw new InvalidOperationException(
                link.Success
                    ? $"The linked {role} image has {link.Imageˉbytes.Length} bytes and violated its " +
                        $"{maximumˉbytes}-byte RX extent."
                    : $"The {role} image did not link: {link.Diagnostics[0].Code}: {link.Diagnostics[0].Message}");
        }
    }

    private static int Countˉsequence(ReadOnlySpan<byte> source, ReadOnlySpan<byte> sequence)
    {
        var Count = 0;
        for (var Index = 0; Index <= source.Length - sequence.Length; Index++)
        {
            if (source.Slice(Index, sequence.Length).SequenceEqual(sequence))
            {
                Count++;
            }
        }
        return Count;
    }

    private static string Injectˉinterpreterˉmodule(
        string source,
        ImmutableArray<byte> moduleˉbytes)
    {
        var Valuesˉstart = source.IndexOf(INTERPRETER_DATA_PREFIX, StringComparison.Ordinal);
        if (Valuesˉstart < 0)
        {
            throw new InvalidOperationException(
                "Bytecode-Interpreter.wv is missing its admitted-module declaration.");
        }
        Valuesˉstart += INTERPRETER_DATA_PREFIX.Length;
        var Valuesˉend = source.IndexOf("];", Valuesˉstart, StringComparison.Ordinal);
        if (Valuesˉend < 0 ||
            source.IndexOf(INTERPRETER_DATA_PREFIX, Valuesˉstart, StringComparison.Ordinal) >= 0)
        {
            throw new InvalidOperationException(
                "Bytecode-Interpreter.wv has an ambiguous admitted-module declaration.");
        }
        return string.Concat(
            source.AsSpan(0, Valuesˉstart),
            string.Join(", ", moduleˉbytes),
            source.AsSpan(Valuesˉend));
    }

    private static string Loadˉsource(string resource)
    {
        using var Stream = typeof(Kernelˉprocessˉimage).Assembly.GetManifestResourceStream(resource) ??
            throw new InvalidOperationException($"Embedded process source '{resource}' is missing.");
        using var Reader = new StreamReader(Stream, new UTF8Encoding(false, true), false);
        return Reader.ReadToEnd();
    }
}
