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
    ImmutableArray<byte> Clientˉshimˉobjectˉbytes,
    ImmutableArray<byte> Clientˉimageˉbytes,
    ImmutableArray<byte> Clientˉdigest);

public static class Kernelˉprocessˉimage
{
    private const string POLICY_RESOURCE = "Windvale.Os.Kernel.Process-Foundation.wv";
    private const string INIT_SERVICE_RESOURCE = "Windvale.Os.Kernel.Init-Resource-Service.wv";
    private const string INIT_SERVICE_SHIM_RESOURCE = "Windvale.Os.Kernel.Init-Resource-Service-Shim.wva";
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
        Verifyˉlinkedˉimage(Serviceˉlink, "init/resource service");
        var Serviceˉdigest = SHA256.HashData(Serviceˉcompilation.Moduleˉbytes.AsSpan()).ToImmutableArray();
        if (!Convert.ToHexString(Serviceˉdigest.AsSpan()).Equals(
                "478DFCD36FED7C8063CFB3F53A6A1362BDA5353656339B730BE573A1BE8F95B0",
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The init/resource service image has an unexpected WVB identity.");
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
            Kernelˉwvbˉadmissionˉcontract.EMBEDDED_MAIN_SYMBOL,
            userˉfault ? 1 : 2);
        var Userˉlink = Linkˉcompiler.Link(
            [new(Userˉassembly.Objectˉbytes), new(admission.Embeddedˉnativeˉobjectˉbytes)],
            new(0, Kernelˉprocessˉcontract.USER_ENTRY_SYMBOL));
        Verifyˉlinkedˉimage(Userˉlink, "client");

        var Clientˉdigest = SHA256.HashData(admission.Embeddedˉmoduleˉbytes.AsSpan()).ToImmutableArray();
        if (!Convert.ToHexString(Clientˉdigest.AsSpan()).Equals(
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
            Userˉassembly.Objectˉbytes,
            Userˉlink.Imageˉbytes,
            Clientˉdigest);
    }

    private static void Verifyˉpolicy(Verifiedˉmodule module)
    {
        if (module.Module is not
            {
                Name: "Processˉfoundation",
                Profile: Moduleˉprofile.Portable,
                Capabilities.Length: 0,
                Data.Length: 4,
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

    private static void Verifyˉlinkedˉimage(Linkˉresult link, string role)
    {
        if (!link.Success || link.Entryˉaddress != 0 || link.Imageˉbytes.Length == 0 ||
            link.Imageˉbytes.Length > (int)Kernelˉpagingˉcontract.PAGE_BYTES)
        {
            throw new InvalidOperationException(
                link.Success
                    ? $"The linked {role} image violated its one-page entry contract."
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

    private static string Loadˉsource(string resource)
    {
        using var Stream = typeof(Kernelˉprocessˉimage).Assembly.GetManifestResourceStream(resource) ??
            throw new InvalidOperationException($"Embedded process source '{resource}' is missing.");
        using var Reader = new StreamReader(Stream, new UTF8Encoding(false, true), false);
        return Reader.ReadToEnd();
    }
}
