using System.Buffers.Binary;
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
    ImmutableArray<byte> Executionˉbudgetˉbytes,
    ImmutableArray<byte> Executionˉbudgetˉdigest,
    ImmutableArray<byte> Resourceˉstoreˉbytes,
    ImmutableArray<byte> Resourceˉstoreˉdigest,
    ImmutableArray<byte> Directoryˉsnapshotˉbytes,
    ImmutableArray<byte> Directoryˉsnapshotˉdigest,
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
        "B43BC2457FD5B5622095BAD6D59AD3CD2AA045BDE1CC79576AFBB419BAC02FD7";
    private const string BOOT_RESOURCE_NAME = "boot:main.wvb";
    private const string BOOT_BUDGET_NAME = "boot:main.budget";
    private const string BOOT_CONFIGURATION_NAME = "boot:main.configuration";
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
            11);
        var Serviceˉlink = Linkˉcompiler.Link(
            [new(Serviceˉassembly.Objectˉbytes), new(Serviceˉobject)],
            new(0, Kernelˉprocessˉcontract.INIT_SERVICE_ENTRY_SYMBOL));
        Verifyˉlinkedˉimage(
            Serviceˉlink,
            "init/resource service",
            Kernelˉprocessˉcontract.INIT_CODE_PAGES * Kernelˉpagingˉcontract.PAGE_BYTES);
        var Serviceˉdigest = SHA256.HashData(Serviceˉcompilation.Moduleˉbytes.AsSpan()).ToImmutableArray();
        var Serviceˉidentity = Convert.ToHexString(Serviceˉdigest.AsSpan());
        if (!Serviceˉidentity.Equals(
                "0554D80340440BF8895F0BF066D355DA83337791F5404F2B72CA6DA214664467",
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"The init/resource service image has an unexpected WVB identity: {Serviceˉidentity}.");
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
        var Recordˉstorage = Nativeˉrecordˉstorageˉplanner.Measure(Interpreterˉnative.Module);
        var Executeˉmainˉindex = Interpreterˉnative.Module.Functions
            .Select((Function, Index) => (Function, Index))
            .Single(Entry => Entry.Function.Name == "Executeˉmain")
            .Index;
        var Executeˉmainˉframeˉslots = Recordˉstorage[Executeˉmainˉindex].Projectedˉframeˉcells;
        if (Executeˉmainˉframeˉslots != Kernelˉprocessˉcontract.CLIENT_INTERPRETER_FRAME_SLOTS)
        {
            throw new InvalidOperationException(
                $"The bytecode interpreter main frame uses {Executeˉmainˉframeˉslots} slots; " +
                $"expected {Kernelˉprocessˉcontract.CLIENT_INTERPRETER_FRAME_SLOTS}.");
        }
        var Nativeˉstackˉbytes = Measureˉnativeˉstackˉbytes(Interpreterˉnative.Module, "Main");
        if (Nativeˉstackˉbytes != Kernelˉprocessˉcontract.CLIENT_NATIVE_STACK_USED_BYTES ||
            Nativeˉstackˉbytes > Kernelˉprocessˉcontract.CLIENT_STACK_BYTES ||
            Nativeˉstackˉbytes <= Kernelˉprocessˉcontract.CLIENT_STACK_BYTES -
                Kernelˉpagingˉcontract.PAGE_BYTES)
        {
            throw new InvalidOperationException(
                $"The bytecode interpreter native stack requires {Nativeˉstackˉbytes} bytes; " +
                $"expected the minimal {Kernelˉprocessˉcontract.CLIENT_STACK_PAGES}-page " +
                $"envelope for {Kernelˉprocessˉcontract.CLIENT_NATIVE_STACK_USED_BYTES} bytes.");
        }
        var Interpreterˉobject = Kernelˉwvbˉadmission.Renameˉmainˉexport(
            Nativeˉobjectˉsink.Writeˉwvo(Interpreterˉnative.Fragment),
            Kernelˉprocessˉcontract.BYTECODE_INTERPRETER_MAIN_SYMBOL);
        var Interpreterˉdigest = SHA256.HashData(Interpreterˉcompilation.Moduleˉbytes.AsSpan()).ToImmutableArray();
        var Interpreterˉidentity = Convert.ToHexString(Interpreterˉdigest.AsSpan());
        if (!Interpreterˉidentity.Equals(
                "3669E94D712BD5A78F0061E29D8054ED3B54B687EFC9508114F79BB78AA8832F",
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
            userˉfault ? 3 : 4);
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
                "9CCFED0509E84BFC63979C6DC13170C14762EFBDAA448B4C5894325F31AA7761",
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The process image is not bound to the admitted WVB identity.");
        }
        var Executionˉbudgetˉbytes = ImmutableArray.Create<byte>(
            (byte)Kernelˉprocessˉcontract.EXECUTION_BUDGET, 0, 0, 0);
        var Executionˉbudgetˉdigest = SHA256.HashData(Executionˉbudgetˉbytes.AsSpan()).ToImmutableArray();
        var Resourceˉstoreˉbytes = Resourceˉstoreˉcodec.Write(
        [
            new(Kernelˉprocessˉcontract.MODULE_RESOURCE_ID, Resourceˉstoreˉkind.Wvbˉmodule,
                BOOT_RESOURCE_NAME, admission.Embeddedˉmoduleˉbytes),
            new(Kernelˉprocessˉcontract.BUDGET_RESOURCE_ID, Resourceˉstoreˉkind.U32ˉexecutionˉbudget,
                BOOT_BUDGET_NAME, Executionˉbudgetˉbytes),
            new(3, Resourceˉstoreˉkind.Opaqueˉbytes,
                BOOT_CONFIGURATION_NAME, [(byte)3, 5, 8, 13]),
        ]);
        var Resourceˉstoreˉdigest = SHA256.HashData(Resourceˉstoreˉbytes.AsSpan()).ToImmutableArray();
        var Directoryˉsnapshotˉbytes = Buildˉdirectoryˉsnapshot();
        var Directoryˉsnapshotˉdigest =
            SHA256.HashData(Directoryˉsnapshotˉbytes.AsSpan()).ToImmutableArray();
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
            Executionˉbudgetˉbytes,
            Executionˉbudgetˉdigest,
            Resourceˉstoreˉbytes,
            Resourceˉstoreˉdigest,
            Directoryˉsnapshotˉbytes,
            Directoryˉsnapshotˉdigest,
            Userˉassembly.Objectˉbytes,
            Userˉlink.Imageˉbytes);
    }

    private static ImmutableArray<byte> Buildˉdirectoryˉsnapshot()
    {
        var Fileˉbytes = new byte[3_072];
        for (var Index = 0; Index < Fileˉbytes.Length; Index++)
        {
            Fileˉbytes[Index] = checked((byte)(Index % 251));
        }
        var Snapshot = Directoryˉsnapshotˉcodec.Write([
            new(Directoryˉsnapshotˉkind.File, "kernel.wv", Fileˉbytes.ToImmutableArray()),
            new(Directoryˉsnapshotˉkind.Other, "folder", [])
        ]);
        if (Snapshot.Length != 3_184 ||
            !Convert.ToHexString(SHA256.HashData(Snapshot.AsSpan())).Equals(
                "0F793A41A701240B9CF41179DAFA252384B43CD23214646FF021D245657C235A",
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "The canonical Windvale OS directory snapshot has changed unexpectedly.");
        }
        return Snapshot;
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

    private static ulong Measureˉnativeˉstackˉbytes(Nativeˉmodule module, string entry)
    {
        var Entryˉindex = -1;
        for (var Index = 0; Index < module.Functions.Length; Index++)
        {
            if (module.Functions[Index].Name == entry)
            {
                Entryˉindex = Index;
                break;
            }
        }
        if (Entryˉindex < 0)
        {
            throw new InvalidOperationException($"The native module is missing stack entry '{entry}'.");
        }
        var Active = new bool[module.Functions.Length];
        var Memoized = new ulong[module.Functions.Length];
        var Recordˉstorage = Nativeˉrecordˉstorageˉplanner.Measure(module);
        return checked(sizeof(ulong) + Measureˉnativeˉstackˉpath(
            module, Recordˉstorage, Entryˉindex, Active, Memoized));
    }

    private static ulong Measureˉnativeˉstackˉpath(
        Nativeˉmodule module,
        ImmutableArray<Nativeˉfunctionˉrecordˉstorage> recordˉstorage,
        int functionˉindex,
        bool[] active,
        ulong[] memoized)
    {
        if (memoized[functionˉindex] != 0)
        {
            return memoized[functionˉindex];
        }
        if (active[functionˉindex])
        {
            throw new InvalidOperationException(
                "The bounded OS interpreter stack profile does not admit recursive native calls.");
        }
        active[functionˉindex] = true;
        var Function = module.Functions[functionˉindex];
        ulong Maximumˉcalleeˉbytes = 0;
        foreach (var Operation in Function.Blocks.SelectMany(Block => Block.Operations))
        {
            var Target = Operation switch
            {
                Nativeˉcall Call => Call.Function,
                Nativeˉvoidˉcall Call => Call.Function,
                _ => -1,
            };
            if (Target < 0)
            {
                continue;
            }
            Maximumˉcalleeˉbytes = Math.Max(
                Maximumˉcalleeˉbytes,
                Measureˉnativeˉstackˉpath(
                    module, recordˉstorage, Target, active, memoized));
        }
        active[functionˉindex] = false;
        var Frameˉslots = recordˉstorage[functionˉindex].Projectedˉframeˉcells;
        var Result = checked((ulong)Frameˉslots * Nativeˉcontract.VALUE_SLOT_BYTES +
            sizeof(ulong) + Maximumˉcalleeˉbytes);
        memoized[functionˉindex] = Result;
        return Result;
    }

    private static void Verifyˉpolicy(Verifiedˉmodule module)
    {
        if (module.Module is not
            {
                Name: "Processˉfoundation",
                Profile: Moduleˉprofile.Portable,
                Capabilities.Length: 0,
                Data.Length: 12,
                Functions.Length: 6,
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
                Data.Length: 2,
                Functions.Length: 23,
                Exports.Length: 1,
                Types.Length: 1,
            } ||
            module.Module.Capabilities[0].Name != Capabilityˉcatalog.FILE_READ_BYTES ||
            module.Module.Data.Count(Data => Data is Textˉdataˉdeclaration
                { Value: BOOT_RESOURCE_NAME or BOOT_BUDGET_NAME }) != 2 ||
            module.Functions.SelectMany(Function => Function.Instructions).All(Instruction =>
                Instruction.Opcode is not Opcode.Bytesˉreadˉi32ˉlittle) ||
            module.Functions.SelectMany(Function => Function.Instructions).Count(Instruction =>
                Instruction.Opcode is Opcode.Callˉcapability) != 2 ||
            module.Functions.SelectMany(Function => Function.Instructions).All(Instruction =>
                Instruction.Opcode is not Opcode.Branchˉfalse) ||
            module.Functions.SelectMany(Function => Function.Instructions).All(Instruction =>
                Instruction.Opcode is not Opcode.U8ˉequal) ||
            module.Functions.SelectMany(Function => Function.Instructions).All(Instruction =>
                Instruction.Opcode is not Opcode.Boolˉnot) ||
            module.Functions.SelectMany(Function => Function.Instructions).All(Instruction =>
                Instruction.Opcode is not Opcode.Call))
        {
            throw new InvalidOperationException(
                $"The Windvale bytecode interpreter violated '{Kernelˉprocessˉcontract.TARGET_NAME}': " +
                $"data={module.Module.Data.Length}, functions={module.Module.Functions.Length}, " +
                $"calls={module.Functions.SelectMany(Function => Function.Instructions).Count(Instruction => Instruction.Opcode is Opcode.Callˉcapability)}.");
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
        var Codeˉsections = Object.Sections
            .Select((Section, Index) => (Section, Index))
            .Where(Item => Item.Section.Kind == Objectˉsectionˉkind.Code)
            .ToArray();
        var Entry = Object.Symbols.SingleOrDefault(Symbol =>
            Symbol.Name == entryˉsymbol &&
            Symbol.Binding == Objectˉsymbolˉbinding.Export &&
            Symbol.Kind == Objectˉsymbolˉkind.Function);
        var Main = Object.Symbols.SingleOrDefault(Symbol =>
            Symbol.Name == mainˉsymbol &&
            Symbol.Binding == Objectˉsymbolˉbinding.Import &&
            Symbol.Kind == Objectˉsymbolˉkind.Function);
        if (Codeˉsections.Length != 1 ||
            Object.Sections.Any(Section => Section.Kind is not
                (Objectˉsectionˉkind.Code or Objectˉsectionˉkind.Readˉonlyˉdata)) ||
            Entry is null || Entry.Sectionˉindex != Codeˉsections[0].Index || Entry.Offset != 0 ||
            Main is null || Object.Relocations.IsEmpty ||
            Countˉsequence(Codeˉsections[0].Section.Data.AsSpan(), [0x0F, 0x05]) != expectedˉsyscalls)
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
        var Expectedˉleaf = Buildˉbootˉresourceˉserviceˉleaf();
        if (Stencil.Sections.Length != 1 ||
            Stencil.Sections[0] is not
            {
                Kind: Objectˉsectionˉkind.Readˉonlyˉdata,
                Data.Length: (int)Kernelˉprocessˉcontract.BOOT_RESOURCE_SERVICE_BYTES,
            } ||
            Stencil.Symbols.Length != 1 ||
            Stencil.Symbols[0] is not
            {
                Name: BOOT_RESOURCE_SERVICE_STENCIL_SYMBOL,
                Binding: Objectˉsymbolˉbinding.Export,
                Kind: Objectˉsymbolˉkind.Data,
                Sectionˉindex: 0,
                Offset: 0,
                Size: Kernelˉprocessˉcontract.BOOT_RESOURCE_SERVICE_BYTES,
            } ||
            !Stencil.Relocations.IsEmpty ||
            Expectedˉleaf.Length != Kernelˉprocessˉcontract.BOOT_RESOURCE_SERVICE_BYTES ||
            !Stencil.Sections[0].Data.AsSpan().SequenceEqual(Expectedˉleaf.AsSpan()) ||
            !Convert.ToHexString(SHA256.HashData(Stencil.Sections[0].Data.AsSpan())).Equals(
                BOOT_RESOURCE_SERVICE_STENCIL_SHA256,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "The Windvale OS boot-resource WVA stencil violated its fixed ABI-17 contract.");
        }
        return Objectˉcodec.Write(new Objectˉfile(
            Objectˉarchitecture.X86ˉ64,
            [new(".text.bresource", Objectˉsectionˉkind.Code, 16,
                Kernelˉprocessˉcontract.BOOT_RESOURCE_SERVICE_BYTES, Stencil.Sections[0].Data)],
            [new(BOOT_RESOURCE_SERVICE_SYMBOL, Objectˉsymbolˉbinding.Export,
                Objectˉsymbolˉkind.Function, 0, 0,
                Kernelˉprocessˉcontract.BOOT_RESOURCE_SERVICE_BYTES)],
            [])).ToImmutableArray();
    }

    private static ImmutableArray<byte> Buildˉbootˉresourceˉserviceˉleaf()
    {
        const byte CONDITION_BELOW = 0x82;
        const byte CONDITION_EQUAL = 0x84;
        const byte CONDITION_NOT_EQUAL = 0x85;
        const byte CONDITION_ABOVE = 0x87;
        const string MODULE_NAME = "boot_resource_module_name";
        const string TABLE = "boot_resource_table";
        const string MODULE_LENGTH = "boot_resource_module_length";
        const string SUCCESS = "boot_resource_success";
        const string INVALID = "boot_resource_invalid";
        const string MISSING = "boot_resource_missing";
        var Output = new X64ˉcodeˉbuilder();

        Output.Emit(0x41, 0xC7, 0x47, 0x40);
        Output.Emitˉu32(0);
        Output.Emit(0x41, 0x83, 0xF9, (byte)BOOT_RESOURCE_NAME.Length);
        Output.Jumpˉif(CONDITION_EQUAL, MODULE_NAME);
        Output.Emit(0x41, 0x83, 0xF9, (byte)BOOT_BUDGET_NAME.Length);
        Output.Jumpˉif(CONDITION_NOT_EQUAL, MISSING);
        Output.Emit(0x49, 0x8B, 0x00, 0x48, 0xBA);
        Output.Emitˉu64(BinaryPrimitives.ReadUInt64LittleEndian("boot:mai"u8));
        Output.Emit(0x48, 0x39, 0xD0);
        Output.Jumpˉif(CONDITION_NOT_EQUAL, MISSING);
        Output.Emit(0x49, 0x8B, 0x40, 0x08, 0x48, 0xBA);
        Output.Emitˉu64(BinaryPrimitives.ReadUInt64LittleEndian("n.budget"u8));
        Output.Emit(0x48, 0x39, 0xD0);
        Output.Jumpˉif(CONDITION_NOT_EQUAL, MISSING);
        Output.Emit(0xBA);
        Output.Emitˉu32(Kernelˉprocessˉcontract.BOOT_RESOURCE_SECOND_ENTRY_OFFSET);
        Output.Emit(0x41, 0xB9);
        Output.Emitˉu32(Kernelˉprocessˉcontract.BUDGET_RESOURCE_ID);
        Output.Jump(TABLE);

        Output.Mark(MODULE_NAME);
        Output.Emit(0x49, 0x8B, 0x00, 0x48, 0xBA);
        Output.Emitˉu64(BinaryPrimitives.ReadUInt64LittleEndian("boot:mai"u8));
        Output.Emit(0x48, 0x39, 0xD0);
        Output.Jumpˉif(CONDITION_NOT_EQUAL, MISSING);
        Output.Emit(0x49, 0x8B, 0x40, 0x05, 0x48, 0xBA);
        Output.Emitˉu64(BinaryPrimitives.ReadUInt64LittleEndian("main.wvb"u8));
        Output.Emit(0x48, 0x39, 0xD0);
        Output.Jumpˉif(CONDITION_NOT_EQUAL, MISSING);
        Output.Emit(0xBA);
        Output.Emitˉu32(Kernelˉprocessˉcontract.BOOT_RESOURCE_FIRST_ENTRY_OFFSET);
        Output.Emit(0x41, 0xB9);
        Output.Emitˉu32(Kernelˉprocessˉcontract.MODULE_RESOURCE_ID);

        Output.Mark(TABLE);
        Output.Emit(0x4D, 0x8B, 0x47, 0x60, 0x4D, 0x85, 0xC0);
        Output.Jumpˉif(CONDITION_EQUAL, MISSING);
        Output.Emit(0x48, 0xB8);
        Output.Emitˉu64(((ulong)Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_VERSION << 32) |
            Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_MAGIC);
        Output.Emit(0x49, 0x39, 0x00);
        Output.Jumpˉif(CONDITION_NOT_EQUAL, INVALID);
        Output.Emit(0x41, 0x83, 0x78, 0x08,
            (byte)Kernelˉprocessˉcontract.BOOT_RESOURCE_TABLE_BYTES);
        Output.Jumpˉif(CONDITION_NOT_EQUAL, INVALID);
        Output.Emit(0x41, 0x83, 0x78, 0x0C,
            (byte)Kernelˉprocessˉcontract.RESOURCE_COUNT);
        Output.Jumpˉif(CONDITION_NOT_EQUAL, INVALID);
        Output.Emit(0x4C, 0x01, 0xC2);
        Output.Emit(0x44, 0x39, 0x0A);
        Output.Jumpˉif(CONDITION_NOT_EQUAL, INVALID);
        Output.Emit(0x44, 0x39, 0x4A, 0x04);
        Output.Jumpˉif(CONDITION_NOT_EQUAL, INVALID);
        Output.Emit(0x83, 0x7A, 0x14, (byte)Kernelˉprocessˉcontract.RESOURCE_BASE_FLAGS);
        Output.Jumpˉif(CONDITION_NOT_EQUAL, INVALID);
        Output.Emit(0x48, 0x83, 0x7A, 0x18, 0x00);
        Output.Jumpˉif(CONDITION_NOT_EQUAL, INVALID);
        Output.Emit(0x48, 0x8B, 0x42, 0x08, 0x48, 0x85, 0xC0);
        Output.Jumpˉif(CONDITION_EQUAL, INVALID);
        Output.Emit(0x8B, 0x52, 0x10);
        Output.Emit(0x41, 0x83, 0xF9, (byte)Kernelˉprocessˉcontract.MODULE_RESOURCE_ID);
        Output.Jumpˉif(CONDITION_EQUAL, MODULE_LENGTH);
        Output.Emit(0x83, 0xFA, (byte)Kernelˉprocessˉcontract.EXECUTION_BUDGET_BYTES);
        Output.Jumpˉif(CONDITION_NOT_EQUAL, INVALID);
        Output.Jump(SUCCESS);

        Output.Mark(MODULE_LENGTH);
        Output.Emit(0x83, 0xFA, 12);
        Output.Jumpˉif(CONDITION_BELOW, INVALID);
        Output.Emit(0x81, 0xFA);
        Output.Emitˉu32(Kernelˉprocessˉcontract.MAXIMUM_RUNTIME_INPUT_BYTES);
        Output.Jumpˉif(CONDITION_ABOVE, INVALID);

        Output.Mark(SUCCESS);
        Output.Emit(0x48, 0x89, 0x01, 0x89, 0x51, 0x08, 0xC7, 0x41, 0x0C);
        Output.Emitˉu32(0);
        Output.Emit(0x31, 0xC0, 0xC3);

        Output.Mark(INVALID);
        Output.Emit(0x41, 0xC7, 0x47, 0x40);
        Output.Emitˉu32(6);
        Output.Emit(0xB8);
        Output.Emitˉu32(1);
        Output.Emit(0xC3);

        Output.Mark(MISSING);
        Output.Emit(0x41, 0xC7, 0x47, 0x40);
        Output.Emitˉu32(8);
        Output.Emit(0xB8);
        Output.Emitˉu32(1);
        Output.Emit(0xC3);
        return Output.Build();
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
