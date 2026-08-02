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
    ImmutableArray<byte> Userˉshimˉobjectˉbytes,
    ImmutableArray<byte> Userˉimageˉbytes,
    ImmutableArray<byte> Moduleˉdigest);

public static class Kernelˉprocessˉimage
{
    private const string POLICY_RESOURCE = "Windvale.Os.Kernel.Process-Foundation.wv";
    private const string USER_RESOURCE = "Windvale.Os.Kernel.Process-User-Shim.wva";
    private const string USER_FAULT_RESOURCE = "Windvale.Os.Kernel.Process-User-Fault-Shim.wva";

    public static Kernelˉprocessˉimageˉartifacts Build(
        Kernelˉwvbˉadmissionˉartifacts admission,
        bool userˉfault)
    {
        ArgumentNullException.ThrowIfNull(admission);
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
        Verifyˉuserˉshim(Userˉassembly.Objectˉbytes, userˉfault);
        var Userˉlink = Linkˉcompiler.Link(
            [new(Userˉassembly.Objectˉbytes), new(admission.Embeddedˉnativeˉobjectˉbytes)],
            new(0, Kernelˉprocessˉcontract.USER_ENTRY_SYMBOL));
        if (!Userˉlink.Success || Userˉlink.Entryˉaddress != 0 || Userˉlink.Imageˉbytes.Length == 0 ||
            Userˉlink.Imageˉbytes.Length > (int)Kernelˉpagingˉcontract.PAGE_BYTES)
        {
            throw new InvalidOperationException(
                Userˉlink.Success
                    ? "The linked process image violated its one-page entry contract."
                    : $"The process image did not link: {Userˉlink.Diagnostics[0].Code}: {Userˉlink.Diagnostics[0].Message}");
        }

        var Digest = SHA256.HashData(admission.Embeddedˉmoduleˉbytes.AsSpan()).ToImmutableArray();
        if (!Convert.ToHexString(Digest.AsSpan()).Equals(
                "7F08EFBB20C6CC69C100F07407F759625B38C02A3F05BB4E8DABCC7BDD10C4E2",
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The process image is not bound to the admitted WVB identity.");
        }
        return new(
            Policyˉcompilation.Moduleˉbytes,
            Policyˉobject,
            Userˉassembly.Objectˉbytes,
            Userˉlink.Imageˉbytes,
            Digest);
    }

    private static void Verifyˉpolicy(Verifiedˉmodule module)
    {
        if (module.Module is not
            {
                Name: "Processˉfoundation",
                Profile: Moduleˉprofile.Portable,
                Capabilities.Length: 0,
                Data.Length: 2,
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

    private static void Verifyˉuserˉshim(ImmutableArray<byte> objectˉbytes, bool userˉfault)
    {
        var Object = Objectˉcodec.Readˉandˉverify(objectˉbytes.AsSpan()).Value;
        var Expectedˉsyscalls = userˉfault ? 2 : 3;
        if (Object.Sections.Length != 1 ||
            Object.Sections[0].Kind != Objectˉsectionˉkind.Code ||
            Object.Symbols.Length != 2 ||
            Object.Symbols[0] is not
            {
                Name: Kernelˉprocessˉcontract.USER_ENTRY_SYMBOL,
                Binding: Objectˉsymbolˉbinding.Export,
                Kind: Objectˉsymbolˉkind.Function,
                Sectionˉindex: 0,
                Offset: 0,
            } ||
            Object.Symbols[1] is not
            {
                Name: Kernelˉwvbˉadmissionˉcontract.EMBEDDED_MAIN_SYMBOL,
                Binding: Objectˉsymbolˉbinding.Import,
                Kind: Objectˉsymbolˉkind.Function,
            } ||
            Object.Relocations.Length != 1 ||
            Countˉsequence(Object.Sections[0].Data.AsSpan(), [0x0F, 0x05]) != Expectedˉsyscalls)
        {
            throw new InvalidOperationException("The process user WVA shim violated its fixed syscall contract.");
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
