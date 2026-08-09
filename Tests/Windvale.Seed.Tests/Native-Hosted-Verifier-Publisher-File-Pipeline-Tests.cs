using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.Runtime;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private static void Nativeˉhostedˉverifierˉpublisherˉfileˉpipelineˉruns()
    {
        var Repository = Findˉrepositoryˉroot();
        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-publisher-file-pipeline-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Newˉtools = new (string Name, int Bytes, string Sha256)[]
            {
                ("Metadata-Producer", 53_009,
                    "74de7ca9a0c959c782837d6674c30db1dcccb07ed258d50017c968ad38d503bc"),
                ("Object-Instantiation", 21_724,
                    "410e4f93c24a2f7cac168298e1e3f2bc3d62f9738c36227b69805ad65591b341"),
                ("Windows-Imports", 10_464,
                    "63b87f2618c9fd413238a9a2919bc6cdb1c769e72f4dca2de47c1c7e1c697a29"),
                ("Linux-Materialization", 16_600,
                    "84bec5e36d1ae61f05b28c506b8285526022ec05990153bb0079beb61badeacc"),
                ("Windows-Materialization", 18_658,
                    "2c9092e5781cadf6a675168415c73ed65303737f6134a5d0bb9a59d874a7cbd2"),
            };
            var Toolˉmodules = new Dictionary<string, Verifiedˉmodule>(
                StringComparer.Ordinal);
            foreach (var Tool in Newˉtools)
            {
                var Output = Path.Combine(Directoryˉpath, Tool.Name + ".wvb");
                var Build = Runˉnativeˉfrontˉdoor(
                    Repository,
                    Path.Combine(
                        Repository,
                        $"Windvale-Native-Hosted-Verifier-Publisher-{Tool.Name}-Tool.wvproj"),
                    Output);
                Equal(0, Build.Exitˉcode);
                Equal(string.Empty, Build.Error);
                var Bytes = File.ReadAllBytes(Output);
                Equal(Tool.Bytes, Bytes.Length);
                Equal(Tool.Sha256, Moduleˉdigest.Calculateˉsha256(Bytes));
                Toolˉmodules.Add(Tool.Name, Moduleˉcodec.Readˉandˉverify(Bytes));
            }

            var Candidateˉroot = Path.Combine(
                Repository,
                "Artifacts",
                "Native-Hosted-Verifier-Publisher-Construction-Candidate");
            foreach (var Name in new[]
                {
                    "Identity-Request",
                    "Structure-Request",
                    "Construction-Request",
                    "Target-Request",
                })
            {
                Toolˉmodules.Add(
                    Name,
                    Moduleˉcodec.Readˉandˉverify(File.ReadAllBytes(Path.Combine(
                        Candidateˉroot,
                        $"Publisher-{Name}-Tool.wvb"))));
            }

            int Run(string name, params string[] arguments)
            {
                var Module = Toolˉmodules[name];
                var Native = X64ˉnativeˉbackend.Compile(Module).Fragment;
                var Resources = new Hostedˉresourceˉcontext(
                    arguments.ToImmutableArray(),
                    TextWriter.Null,
                    TextWriter.Null);
                var Services = new Nativeˉhostˉservices(
                    Nativeˉoutputˉchannel.Processˉstandardˉoutput(),
                    Module.Module.Capabilities.Select(Item => Item.Name),
                    Resources,
                    Nativeˉoutputˉchannel.Processˉdiagnosticˉoutput(),
                    Nativeˉfileˉinput.Hostˉfileˉsystem(),
                    Nativeˉfileˉoutput.Hostˉfileˉsystem());
                return X64ˉnativeˉexecutor.Executeˉi32(
                    Native,
                    maximumˉinstructions: 48_000_000_000,
                    hostˉservices: Services);
            }

            var Publisherˉmoduleˉpath = Path.Combine(
                Repository,
                "Artifacts",
                "Native-Hosted-Verifier-Application-Publisher-Candidate",
                "Hosted-Verifier-Application-Publisher.wvb");
            var Publisherˉmodule = Moduleˉcodec.Readˉandˉverify(
                File.ReadAllBytes(Publisherˉmoduleˉpath));
            var Publisherˉfragment = X64ˉnativeˉbackend.Compile(Publisherˉmodule).Fragment;
            var Nativeˉentry = Publisherˉfragment.Symbols.Single(Symbol =>
                Symbol.Binding == Nativeˉsymbolˉbinding.Export &&
                Symbol.Kind == Nativeˉsymbolˉkind.Function &&
                Symbol.Name == "Main").Offset;
            var Publisherˉobjectˉpath = Path.Combine(Candidateˉroot, "Publisher.wvo");
            var Sha256ˉobjectˉpath = Path.Combine(
                Repository,
                "Linker",
                "Reference",
                "Consumers",
                "X64-Wvb-Publication-Sha256.wvo");

            foreach (var Platform in Enum.GetValues<Nativeˉserviceˉplatform>())
            {
                var Windows = Platform == Nativeˉserviceˉplatform.Windows;
                var Target = Windows ? "1" : "2";
                var Prefix = Path.Combine(Directoryˉpath, Target + "-");
                var Startupˉobjectˉpath = Path.Combine(
                    Repository,
                    "Linker",
                    "Reference",
                    "Consumers",
                    $"{(Windows ? "Windows" : "Linux")}-X64-Wvb-Publisher.wvo");
                var Adapterˉobjectˉpath = Path.Combine(
                    Repository,
                    "Linker",
                    "Reference",
                    "Consumers",
                    $"{(Windows ? "Windows" : "Linux")}-X64-Wvb-Publication-Adapter.wvo");
                var Metadataˉpath = Prefix + "Metadata.wvvp";
                var Identityˉpath = Prefix + "Identity.wvpi";
                var Structureˉpath = Prefix + "Structure.wvps";
                var Constructionˉpath = Prefix + "Construction.wvcr";
                var Targetsˉpath = Prefix + "Targets.wvpt";
                var Objectsˉpath = Prefix + "Objects.wvio";
                var Importsˉpath = Prefix + "Imports.wvim";
                var Baseˉpath = Prefix + (Windows ? "Base.exe" : "Base.elf");
                var Outputˉpath = Prefix + (Windows ? "Publisher.exe" : "Publisher.elf");

                var Bundle = X64ˉnativeˉserviceˉbundle.Buildˉhostedˉverifier(
                    Publisherˉfragment,
                    Platform);
                var Base = Windows
                    ? Windowsˉhostedˉverifierˉapplicationˉbuilder.Build(
                        Publisherˉmodule.Module.Capabilities,
                        Bundle,
                        Nativeˉentry)
                    : Linuxˉhostedˉverifierˉapplicationˉbuilder.Build(
                        Publisherˉmodule.Module.Capabilities,
                        Bundle,
                        Nativeˉentry);
                File.WriteAllBytes(Baseˉpath, Base.AsSpan());

                Equal(0, Run(
                    "Metadata-Producer",
                    Target,
                    Publisherˉmoduleˉpath,
                    Startupˉobjectˉpath,
                    Metadataˉpath));
                Equal(
                    Windows
                        ? "40e73f9c4ac9e27c9dea7f9bed8217be125159f89cb2ea314a91bc66da389b74"
                        : "393253dab73387a0c96fd33c278b350fe43e5466a243eabe3f62a6652c946035",
                    Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Metadataˉpath)))
                        .ToLowerInvariant());
                Equal(0, Run(
                    "Identity-Request",
                    Target,
                    Publisherˉmoduleˉpath,
                    Publisherˉobjectˉpath,
                    Startupˉobjectˉpath,
                    Adapterˉobjectˉpath,
                    Sha256ˉobjectˉpath,
                    Metadataˉpath,
                    Identityˉpath));
                Equal(0, Run("Structure-Request", Identityˉpath, Structureˉpath));
                Equal(0, Run("Construction-Request", Structureˉpath, Constructionˉpath));
                Equal(0, Run("Target-Request", Structureˉpath, Targetsˉpath));
                Equal(0, Run(
                    "Object-Instantiation",
                    Constructionˉpath,
                    Targetsˉpath,
                    Startupˉobjectˉpath,
                    Adapterˉobjectˉpath,
                    Sha256ˉobjectˉpath,
                    Objectsˉpath));

                if (Windows)
                {
                    Equal(0, Run("Windows-Imports", Importsˉpath));
                    Equal(0, Run(
                        "Windows-Materialization",
                        Baseˉpath,
                        Constructionˉpath,
                        Objectsˉpath,
                        Metadataˉpath,
                        Importsˉpath,
                        Outputˉpath));
                }
                else
                {
                    Equal(0, Run(
                        "Linux-Materialization",
                        Baseˉpath,
                        Constructionˉpath,
                        Objectsˉpath,
                        Metadataˉpath,
                        Outputˉpath));
                }

                Sequenceˉequal(
                    File.ReadAllBytes(Path.Combine(
                        Repository,
                        "Artifacts",
                        "Native-Hosted-Verifier-Application-Publisher-Candidate",
                        Windows
                            ? "windows-x64-wvhostverifierpublish.exe"
                            : "linux-x64-wvhostverifierpublish.elf")),
                    File.ReadAllBytes(Outputˉpath));

                byte[] Sentinel = [0x57, 0x56, 0x50, 0x46];
                var Corruptˉbaseˉpath = Prefix + (Windows ? "Corrupt.exe" : "Corrupt.elf");
                var Corruptˉbase = File.ReadAllBytes(Baseˉpath);
                Corruptˉbase[0] ^= 1;
                File.WriteAllBytes(Corruptˉbaseˉpath, Corruptˉbase);
                File.WriteAllBytes(Outputˉpath, Sentinel);
                Equal(2, Windows
                    ? Run(
                        "Windows-Materialization",
                        Corruptˉbaseˉpath,
                        Constructionˉpath,
                        Objectsˉpath,
                        Metadataˉpath,
                        Importsˉpath,
                        Outputˉpath)
                    : Run(
                        "Linux-Materialization",
                        Corruptˉbaseˉpath,
                        Constructionˉpath,
                        Objectsˉpath,
                        Metadataˉpath,
                        Outputˉpath));
                Sequenceˉequal(Sentinel, File.ReadAllBytes(Outputˉpath));
                if (Windows)
                {
                    var Corruptˉmoduleˉpath = Prefix + "Corrupt.wvb";
                    var Corruptˉmodule = File.ReadAllBytes(Publisherˉmoduleˉpath);
                    Corruptˉmodule[0] ^= 1;
                    File.WriteAllBytes(Corruptˉmoduleˉpath, Corruptˉmodule);
                    File.WriteAllBytes(Outputˉpath, Sentinel);
                    Equal(2, Run(
                        "Metadata-Producer",
                        Target,
                        Corruptˉmoduleˉpath,
                        Startupˉobjectˉpath,
                        Outputˉpath));
                    Sequenceˉequal(Sentinel, File.ReadAllBytes(Outputˉpath));
                }
                File.WriteAllBytes(Outputˉpath, Sentinel);
                var Corruptˉtargets = File.ReadAllBytes(Targetsˉpath);
                Array.Clear(Corruptˉtargets, 16, 4);
                File.WriteAllBytes(Targetsˉpath, Corruptˉtargets);
                Equal(2, Run(
                    "Object-Instantiation",
                    Constructionˉpath,
                    Targetsˉpath,
                    Startupˉobjectˉpath,
                    Adapterˉobjectˉpath,
                    Sha256ˉobjectˉpath,
                    Outputˉpath));
                Sequenceˉequal(Sentinel, File.ReadAllBytes(Outputˉpath));
                Equal(64, Run(
                    "Object-Instantiation",
                    Constructionˉpath,
                    Constructionˉpath,
                    Startupˉobjectˉpath,
                    Adapterˉobjectˉpath,
                    Sha256ˉobjectˉpath,
                    Constructionˉpath));
                Equal(416, File.ReadAllBytes(Constructionˉpath).Length);
            }
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }
}
