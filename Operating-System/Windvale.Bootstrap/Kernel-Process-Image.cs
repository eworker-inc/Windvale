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
    ImmutableArray<byte> Bootˉresourceˉserviceˉstencilˉobjectˉbytes,
    ImmutableArray<byte> Bootˉresourceˉserviceˉobjectˉbytes,
    uint Bootˉresourceˉserviceˉoffset,
    ImmutableArray<byte> Admittedˉprogramˉbytes,
    ImmutableArray<byte> Admittedˉprogramˉdigest,
    ImmutableArray<byte> Clientˉshimˉobjectˉbytes,
    ImmutableArray<byte> Clientˉimageˉbytes);

public static class Kernelˉprocessˉimage
{
    private const string POLICY_RESOURCE = "Windvale.Os.Kernel.Process-Foundation.wv";
    private const string INIT_SERVICE_RESOURCE = "Windvale.Os.Kernel.Init-Resource-Service.wv";
    private const string INIT_SERVICE_SHIM_RESOURCE = "Windvale.Os.Kernel.Init-Resource-Service-Shim.wva";
    private const string INTERPRETER_RESOURCE = "Windvale.Os.Runtime.Bytecode-Interpreter.wv";
    private const string BOOT_RESOURCE_SERVICE_RESOURCE = "Windvale.Os.Runtime.Boot-Resource-Service.wva";
    private const string BOOT_RESOURCE_SERVICE_SYMBOL = "Windvale_os_boot_resource_read_bytes";
    private const string BOOT_RESOURCE_SERVICE_STENCIL_SYMBOL =
        "Windvale_os_boot_resource_read_bytes_stencil";
    private const string BOOT_RESOURCE_SERVICE_STENCIL_SHA256 =
        "8FCCEE8F5FC7369F88C5FA018D8B05EEB4C6B0317C7E1A5AD9D7CB88B95B2422";
    private const string BOOT_RESOURCE_NAME = "boot:main.wvb";
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
        var Interpreterˉcompilation = Seedˉcompiler.Compile(
            Interpreterˉsource, "Bytecode-Interpreter.wv");
        if (!Interpreterˉcompilation.Success)
        {
            throw new InvalidOperationException(
                $"The Windvale bytecode interpreter did not compile: {Interpreterˉcompilation.Diagnostics[0]}");
        }
        var Interpreterˉmodule = Moduleˉcodec.Readˉandˉverify(Interpreterˉcompilation.Moduleˉbytes.AsSpan());
        Verifyˉinterpreter(Interpreterˉmodule);
        var Interpreterˉnative = X64ˉnativeˉbackend.Compile(Interpreterˉmodule);
        var Interpreterˉobject = Kernelˉwvbˉadmission.Renameˉmainˉexport(
            Nativeˉobjectˉsink.Writeˉwvo(Interpreterˉnative.Fragment),
            Kernelˉprocessˉcontract.BYTECODE_INTERPRETER_MAIN_SYMBOL);
        var Interpreterˉdigest = SHA256.HashData(Interpreterˉcompilation.Moduleˉbytes.AsSpan()).ToImmutableArray();
        var Interpreterˉidentity = Convert.ToHexString(Interpreterˉdigest.AsSpan());
        if (!Interpreterˉidentity.Equals(
                "25A223346C6357290680476A39A4E67821E5EFC9420933A90486F993AEF46BF2",
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"The user-space interpreter has an unexpected WVB identity: {Interpreterˉidentity}.");
        }

        var Bootˉresourceˉassembly = Assemblyˉcompiler.Assemble(
            Loadˉsource(BOOT_RESOURCE_SERVICE_RESOURCE));
        if (!Bootˉresourceˉassembly.Success)
        {
            throw new InvalidOperationException(
                $"The boot-resource service did not assemble: " +
                $"{Bootˉresourceˉassembly.Diagnostics[0].Code}: " +
                Bootˉresourceˉassembly.Diagnostics[0].Message);
        }
        var Bootˉresourceˉserviceˉobject = Publishˉbootˉresourceˉservice(
            Bootˉresourceˉassembly.Objectˉbytes);

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
            [
                new(Userˉassembly.Objectˉbytes),
                new(Interpreterˉobject),
                new(Bootˉresourceˉserviceˉobject),
            ],
            new(0, Kernelˉprocessˉcontract.USER_ENTRY_SYMBOL));
        Verifyˉlinkedˉimage(Userˉlink, "bytecode-interpreter client", Kernelˉprocessˉcontract.CLIENT_CODE_BYTES);
        var Bootˉresourceˉserviceˉoffset = Readˉlinkedˉserviceˉoffset(
            Userˉlink, Bootˉresourceˉserviceˉobject, BOOT_RESOURCE_SERVICE_SYMBOL);

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
            Bootˉresourceˉassembly.Objectˉbytes,
            Bootˉresourceˉserviceˉobject,
            Bootˉresourceˉserviceˉoffset,
            admission.Embeddedˉmoduleˉbytes,
            Admittedˉprogramˉdigest,
            Userˉassembly.Objectˉbytes,
            Userˉlink.Imageˉbytes);
    }

    // Stage 0 test seam: compile the interpreter once; tests vary only the
    // immutable resource supplied through its declared runtime capability.
    public static ImmutableArray<byte> Compileˉinterpreterˉmodule()
    {
        var Compilation = Seedˉcompiler.Compile(
            Loadˉsource(INTERPRETER_RESOURCE),
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

    private static void Verifyˉinterpreter(Verifiedˉmodule module)
    {
        if (module.Module is not
            {
                Name: "Bytecodeˉinterpreter",
                Profile: Moduleˉprofile.Hosted,
                Capabilities.Length: 1,
                Data.Length: 1,
                Functions.Length: 8,
                Exports.Length: 1,
                Types.Length: 0,
            } ||
            module.Module.Capabilities[0].Name != Capabilityˉcatalog.FILE_READ_BYTES ||
            module.Module.Data[0] is not Textˉdataˉdeclaration { Value: BOOT_RESOURCE_NAME } ||
            module.Functions.SelectMany(Function => Function.Instructions).All(Instruction =>
                Instruction.Opcode is not Opcode.Bytesˉreadˉi32ˉlittle) ||
            module.Functions.SelectMany(Function => Function.Instructions).Count(Instruction =>
                Instruction.Opcode is Opcode.Callˉcapability) != 1 ||
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

    private static ImmutableArray<byte> Publishˉbootˉresourceˉservice(
        ImmutableArray<byte> stencilˉobjectˉbytes)
    {
        var Stencil = Objectˉcodec.Readˉandˉverify(stencilˉobjectˉbytes.AsSpan()).Value;
        if (Stencil.Sections.Length != 1 ||
            Stencil.Sections[0] is not
            {
                Kind: Objectˉsectionˉkind.Readˉonlyˉdata,
                Data.Length: 199,
            } ||
            Stencil.Symbols.Length != 1 ||
            Stencil.Symbols[0] is not
            {
                Name: BOOT_RESOURCE_SERVICE_STENCIL_SYMBOL,
                Binding: Objectˉsymbolˉbinding.Export,
                Kind: Objectˉsymbolˉkind.Data,
                Sectionˉindex: 0,
                Offset: 0,
                Size: 199,
            } ||
            !Stencil.Relocations.IsEmpty ||
            !Convert.ToHexString(SHA256.HashData(Stencil.Sections[0].Data.AsSpan())).Equals(
                BOOT_RESOURCE_SERVICE_STENCIL_SHA256,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "The Windvale OS boot-resource WVA stencil violated its fixed ABI-16 contract.");
        }
        return Objectˉcodec.Write(new Objectˉfile(
            Objectˉarchitecture.X86ˉ64,
            [new(".text.bresource", Objectˉsectionˉkind.Code, 16, 199, Stencil.Sections[0].Data)],
            [new(BOOT_RESOURCE_SERVICE_SYMBOL, Objectˉsymbolˉbinding.Export,
                Objectˉsymbolˉkind.Function, 0, 0, 199)],
            [])).ToImmutableArray();
    }

    private static uint Readˉlinkedˉserviceˉoffset(
        Linkˉresult link,
        ImmutableArray<byte> objectˉbytes,
        string symbol)
    {
        var Map = new UTF8Encoding(false, true).GetString(link.Mapˉbytes.AsSpan());
        var Marker = $" kind=function name={symbol} address=";
        var Markerˉoffset = Map.IndexOf(Marker, StringComparison.Ordinal);
        if (Markerˉoffset < 0)
        {
            throw new InvalidOperationException(
                "The linked client image is missing its boot-resource service leaf.");
        }
        var Addressˉstart = Markerˉoffset + Marker.Length;
        var Addressˉend = Map.IndexOf(' ', Addressˉstart);
        if (Addressˉend < 0 ||
            !uint.TryParse(Map.AsSpan(Addressˉstart, Addressˉend - Addressˉstart), out var Address) ||
            Address > (uint)link.Imageˉbytes.Length)
        {
            throw new InvalidOperationException(
                "The linked boot-resource service address is malformed or outside the client image.");
        }
        var Object = Objectˉcodec.Readˉandˉverify(objectˉbytes.AsSpan()).Value;
        var Leaf = Object.Sections[0].Data;
        if ((uint)Leaf.Length > (uint)link.Imageˉbytes.Length - Address ||
            !link.Imageˉbytes.AsSpan((int)Address, Leaf.Length).SequenceEqual(Leaf.AsSpan()))
        {
            throw new InvalidOperationException(
                "The linked boot-resource service bytes do not match their verified WVA leaf.");
        }
        return Address;
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

    private static string Loadˉsource(string resource)
    {
        using var Stream = typeof(Kernelˉprocessˉimage).Assembly.GetManifestResourceStream(resource) ??
            throw new InvalidOperationException($"Embedded process source '{resource}' is missing.");
        using var Reader = new StreamReader(Stream, new UTF8Encoding(false, true), false);
        return Reader.ReadToEnd();
    }
}
