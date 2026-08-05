using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.ObjectModel;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private const int NATIVE_AOT_LINK_MAP_BYTES = 630;
    private const string NATIVE_AOT_LINK_MAP_SHA256 =
        "857710249807d2fed4da847729d0244f08ccdc70156c043fdaa0516de394e2dc";
    private const int NATIVE_AOT_IMAGE_BYTES = 406;
    private const string NATIVE_AOT_IMAGE_SHA256 =
        "7c05565142850adab1d63d999479977a23ef50c7264c03ee55ce5b323df26408";
    private const int WINDOWS_NATIVE_AOT_APPLICATION_BYTES = 2_560;
    private const string WINDOWS_NATIVE_AOT_APPLICATION_SHA256 =
        "8f2c3389dafa40c0231a0f5aeead3db5570697d54874f324a81f84a2d5b16eb6";
    private const int LINUX_NATIVE_AOT_APPLICATION_BYTES = 8_304;
    private const string LINUX_NATIVE_AOT_APPLICATION_SHA256 =
        "fe525b84b9bf902677a5c7beb36872dfd72e7d6d0f12bfb5c95d491c4e1cd3f7";

    private static void Nativeˉsourceˉtoˉaotˉfrontˉdoorˉcomposes()
    {
        const string Expectedˉloweringˉreport =
            "native x64 status=Valid abi=22 code-bytes=406 object-bytes=479\n";
        const string Expectedˉmap =
            "windvale-link-map 1\n" +
            "target name=flat-x86-64-v1 architecture=x86-64 base-address=1048576 image-bytes=406\n" +
            "entry name=Main address=1048576\n" +
            "image sha256=7c05565142850adab1d63d999479977a23ef50c7264c03ee55ce5b323df26408\n" +
            "inputs count=1\n" +
            "input index=0 sha256=0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5\n" +
            "sections count=1\n" +
            "section index=0 input=0 source-index=0 kind=code name=.text image-offset=0 address=1048576 memory-bytes=406 data-bytes=406 alignment=16\n" +
            "defined-symbols count=1\n" +
            "symbol index=0 input=0 source-index=0 binding=export kind=function name=Main address=1048576 size=406\n" +
            "imports count=0\n" +
            "relocations count=0\n";

        Equal(NATIVE_AOT_LINK_MAP_BYTES, System.Text.Encoding.UTF8.GetByteCount(Expectedˉmap));
        Equal(
            NATIVE_AOT_LINK_MAP_SHA256,
            Objectˉdigest.Calculateˉsha256(
                System.Text.Encoding.UTF8.GetBytes(Expectedˉmap).AsSpan()));

        var Repository = Findˉrepositoryˉroot();
        var Directoryˉpath = Path.Combine(
            Path.GetTempPath(),
            $"windvale-native-aot-composition-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Directoryˉpath);
        try
        {
            var Wvbˉpath = Path.Combine(Directoryˉpath, "Return-42.wvb");
            var Wvoˉpath = Path.Combine(Directoryˉpath, "Return-42.wvo");
            var Imageˉpath = Path.Combine(Directoryˉpath, "Return-42.bin");
            var Applicationˉpath = Path.Combine(
                Directoryˉpath,
                OperatingSystem.IsWindows() ? "Return-42.exe" : "Return-42.elf");

            var Build = Runˉnativeˉfrontˉdoor(
                Repository,
                Path.Combine(
                    Repository,
                    "Windvale-Native-Test-Wvb-To-Wvo-Return-42.wvproj"),
                Wvbˉpath);
            Equal(0, Build.Exitˉcode);
            Contains(Build.Output, "build status=Published verification=compiler-aligned");
            Contains(Build.Output, "publication status=Complete bytes=0x000000ae");
            Equal(string.Empty, Build.Error);
            var Wvb = File.ReadAllBytes(Wvbˉpath);
            Equal(WVB_TO_WVO_FIXTURE_WVB_BYTES, Wvb.Length);
            Equal(WVB_TO_WVO_FIXTURE_WVB_SHA256, Moduleˉdigest.Calculateˉsha256(Wvb));
            var Verifiedˉwvb = Moduleˉcodec.Readˉandˉverify(Wvb);
            Equal("Wvbˉtoˉwvoˉfixture", Verifiedˉwvb.Module.Name);
            Equal(Moduleˉprofile.Portable, Verifiedˉwvb.Module.Profile);

            var Lowerer = Buildˉcurrentˉhostˉwvbˉtoˉwvoˉapplication();
            var Linker = Buildˉcurrentˉhostˉwvˉlinkerˉapplication();
            var Packager = Buildˉcurrentˉhostˉconsoleˉpackagerˉapplication();

            if (OperatingSystem.IsWindows())
            {
                var Loadedˉlowerer = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                Equal(0, Executeˉwindowsˉapplication(
                    Lowerer,
                    Expectedˉloweringˉreport,
                    [Wvbˉpath, Wvoˉpath],
                    loadedˉmodules: Loadedˉlowerer));
                Requireˉnoˉdotnetˉmodules(Loadedˉlowerer);
            }
            else
            {
                var Loadedˉlowerer = new HashSet<string>(StringComparer.Ordinal);
                Equal(0, Executeˉlinuxˉapplication(
                    Lowerer,
                    Expectedˉloweringˉreport,
                    [Wvbˉpath, Wvoˉpath],
                    loadedˉmappings: Loadedˉlowerer));
                Requireˉnoˉdotnetˉmodules(Loadedˉlowerer);
            }

            var Wvo = File.ReadAllBytes(Wvoˉpath);
            Equal(WVB_TO_WVO_FIXTURE_OBJECT_BYTES, Wvo.Length);
            Equal(WVB_TO_WVO_FIXTURE_OBJECT_SHA256, Objectˉdigest.Calculateˉsha256(Wvo));
            var Verifiedˉwvo = Objectˉcodec.Readˉandˉverify(Wvo);
            Equal(1, Verifiedˉwvo.Value.Sections.Length);
            Equal(1, Verifiedˉwvo.Value.Symbols.Length);
            Equal(0, Verifiedˉwvo.Value.Relocations.Length);
            var Entry = Verifiedˉwvo.Value.Symbols.Single();
            Equal("Main", Entry.Name);
            Equal(Objectˉsymbolˉbinding.Export, Entry.Binding);
            Equal(0u, Entry.Offset);

            var Linkˉarguments = new[]
            {
                Linkˉcontract.DEFAULT_BASE_ADDRESS.ToString(
                    System.Globalization.CultureInfo.InvariantCulture),
                "Main",
                Imageˉpath,
                Wvoˉpath,
            };
            if (OperatingSystem.IsWindows())
            {
                var Loadedˉlinker = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                Equal(0, Executeˉwindowsˉapplication(
                    Linker,
                    Expectedˉmap,
                    Linkˉarguments,
                    loadedˉmodules: Loadedˉlinker));
                Requireˉnoˉdotnetˉmodules(Loadedˉlinker);
            }
            else
            {
                var Loadedˉlinker = new HashSet<string>(StringComparer.Ordinal);
                Equal(0, Executeˉlinuxˉapplication(
                    Linker,
                    Expectedˉmap,
                    Linkˉarguments,
                    loadedˉmappings: Loadedˉlinker));
                Requireˉnoˉdotnetˉmodules(Loadedˉlinker);
            }

            var Image = File.ReadAllBytes(Imageˉpath);
            Equal(NATIVE_AOT_IMAGE_BYTES, Image.Length);
            Equal(NATIVE_AOT_IMAGE_SHA256, Objectˉdigest.Calculateˉsha256(Image));

            var Target = OperatingSystem.IsWindows()
                ? Windowsˉconsoleˉapplicationˉcontract.TARGET_NAME
                : Linuxˉconsoleˉapplicationˉcontract.TARGET_NAME;
            var Applicationˉbytes = OperatingSystem.IsWindows()
                ? WINDOWS_NATIVE_AOT_APPLICATION_BYTES
                : LINUX_NATIVE_AOT_APPLICATION_BYTES;
            var Expectedˉpackageˉreport =
                $"package status=Valid target={Target} native-image-bytes=406 " +
                $"entry-offset=0 application-bytes={Applicationˉbytes}\n";
            var Packageˉarguments = new[]
            {
                Target,
                Imageˉpath,
                "0",
                Applicationˉpath,
            };
            if (OperatingSystem.IsWindows())
            {
                var Loadedˉpackager = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                Equal(0, Executeˉwindowsˉapplication(
                    Packager,
                    Expectedˉpackageˉreport,
                    Packageˉarguments,
                    loadedˉmodules: Loadedˉpackager));
                Requireˉnoˉdotnetˉmodules(Loadedˉpackager);
            }
            else
            {
                var Loadedˉpackager = new HashSet<string>(StringComparer.Ordinal);
                Equal(0, Executeˉlinuxˉapplication(
                    Packager,
                    Expectedˉpackageˉreport,
                    Packageˉarguments,
                    loadedˉmappings: Loadedˉpackager));
                Requireˉnoˉdotnetˉmodules(Loadedˉpackager);
            }

            var Application = File.ReadAllBytes(Applicationˉpath);
            Equal(Applicationˉbytes, Application.Length);
            Equal(
                OperatingSystem.IsWindows()
                    ? WINDOWS_NATIVE_AOT_APPLICATION_SHA256
                    : LINUX_NATIVE_AOT_APPLICATION_SHA256,
                Objectˉdigest.Calculateˉsha256(Application));
            if (OperatingSystem.IsWindows())
            {
                var Verified = Windowsˉconsoleˉapplicationˉverifier.Verify(Application);
                Sequenceˉequal(Image, Verified.Nativeˉimageˉbytes);
                Equal(0u, Verified.Nativeˉentryˉoffset);
                var Loadedˉapplication = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                Equal(42, Executeˉwindowsˉapplication(
                    Application.ToImmutableArray(),
                    loadedˉmodules: Loadedˉapplication));
                Requireˉnoˉdotnetˉmodules(Loadedˉapplication);
            }
            else
            {
                var Verified = Linuxˉconsoleˉapplicationˉverifier.Verify(Application);
                Sequenceˉequal(Image, Verified.Nativeˉimageˉbytes);
                Equal(0u, Verified.Nativeˉentryˉoffset);
                var Loadedˉapplication = new HashSet<string>(StringComparer.Ordinal);
                Equal(42, Executeˉlinuxˉapplication(
                    Application.ToImmutableArray(),
                    loadedˉmappings: Loadedˉapplication));
                Requireˉnoˉdotnetˉmodules(Loadedˉapplication);
            }
        }
        finally
        {
            Directory.Delete(Directoryˉpath, recursive: true);
        }
    }

    private static ImmutableArray<byte> Buildˉcurrentˉhostˉwvbˉtoˉwvoˉapplication()
    {
        var Module = Moduleˉcodec.Readˉandˉverify(Compileˉwvbˉtoˉwvoˉtoolˉsuccess());
        var Fragment = X64ˉnativeˉbackend.Compile(Module).Fragment;
        if (OperatingSystem.IsWindows())
        {
            var Result = Hostedˉwvbˉtoˉwvoˉapplicationˉwriter.Writeˉwindows(
                Fragment,
                Module.Module.Capabilities,
                Module.Module.Name);
            True(Result.Success, Result.Diagnostics.IsEmpty
                ? "The Windows WVB-to-WVO writer failed without a diagnostic."
                : Result.Diagnostics[0].Message);
            Equal(WINDOWS_WVB_TO_WVO_APPLICATION_BYTES, Result.Imageˉbytes.Length);
            Equal(
                WINDOWS_WVB_TO_WVO_APPLICATION_SHA256,
                Objectˉdigest.Calculateˉsha256(Result.Imageˉbytes.AsSpan()));
            return Result.Imageˉbytes;
        }

        var Linux = Hostedˉwvbˉtoˉwvoˉapplicationˉwriter.Writeˉlinux(
            Fragment,
            Module.Module.Capabilities,
            Module.Module.Name);
        True(Linux.Success, Linux.Diagnostics.IsEmpty
            ? "The Linux WVB-to-WVO writer failed without a diagnostic."
            : Linux.Diagnostics[0].Message);
        Equal(LINUX_WVB_TO_WVO_APPLICATION_BYTES, Linux.Imageˉbytes.Length);
        Equal(
            LINUX_WVB_TO_WVO_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Linux.Imageˉbytes.AsSpan()));
        return Linux.Imageˉbytes;
    }

    private static ImmutableArray<byte> Buildˉcurrentˉhostˉwvˉlinkerˉapplication()
    {
        var Module = Moduleˉcodec.Readˉandˉverify(Compileˉwvˉlinkerˉsuccess());
        var Fragment = X64ˉnativeˉbackend.Compile(Module).Fragment;
        if (OperatingSystem.IsWindows())
        {
            var Result = Hostedˉwvˉlinkerˉapplicationˉwriter.Writeˉwindows(
                Fragment,
                Module.Module.Capabilities,
                Module.Module.Name);
            True(Result.Success, Result.Diagnostics.IsEmpty
                ? "The Windows Windvale linker writer failed without a diagnostic."
                : Result.Diagnostics[0].Message);
            Equal(WINDOWS_WV_LINKER_APPLICATION_BYTES, Result.Imageˉbytes.Length);
            Equal(
                WINDOWS_WV_LINKER_APPLICATION_SHA256,
                Objectˉdigest.Calculateˉsha256(Result.Imageˉbytes.AsSpan()));
            return Result.Imageˉbytes;
        }

        var Linux = Hostedˉwvˉlinkerˉapplicationˉwriter.Writeˉlinux(
            Fragment,
            Module.Module.Capabilities,
            Module.Module.Name);
        True(Linux.Success, Linux.Diagnostics.IsEmpty
            ? "The Linux Windvale linker writer failed without a diagnostic."
            : Linux.Diagnostics[0].Message);
        Equal(LINUX_WV_LINKER_APPLICATION_BYTES, Linux.Imageˉbytes.Length);
        Equal(
            LINUX_WV_LINKER_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Linux.Imageˉbytes.AsSpan()));
        return Linux.Imageˉbytes;
    }

    private static ImmutableArray<byte> Buildˉcurrentˉhostˉconsoleˉpackagerˉapplication()
    {
        var Module = Moduleˉcodec.Readˉandˉverify(Compileˉconsoleˉpackagerˉsuccess());
        var Fragment = X64ˉnativeˉbackend.Compile(Module).Fragment;
        if (OperatingSystem.IsWindows())
        {
            var Result = Hostedˉconsoleˉpackagerˉapplicationˉwriter.Writeˉwindows(
                Fragment,
                Module.Module.Capabilities,
                Module.Module.Name);
            True(Result.Success, Result.Diagnostics.IsEmpty
                ? "The Windows console-packager writer failed without a diagnostic."
                : Result.Diagnostics[0].Message);
            Equal(WINDOWS_CONSOLE_PACKAGER_APPLICATION_BYTES, Result.Imageˉbytes.Length);
            Equal(
                WINDOWS_CONSOLE_PACKAGER_APPLICATION_SHA256,
                Objectˉdigest.Calculateˉsha256(Result.Imageˉbytes.AsSpan()));
            return Result.Imageˉbytes;
        }

        var Linux = Hostedˉconsoleˉpackagerˉapplicationˉwriter.Writeˉlinux(
            Fragment,
            Module.Module.Capabilities,
            Module.Module.Name);
        True(Linux.Success, Linux.Diagnostics.IsEmpty
            ? "The Linux console-packager writer failed without a diagnostic."
            : Linux.Diagnostics[0].Message);
        Equal(LINUX_CONSOLE_PACKAGER_APPLICATION_BYTES, Linux.Imageˉbytes.Length);
        Equal(
            LINUX_CONSOLE_PACKAGER_APPLICATION_SHA256,
            Objectˉdigest.Calculateˉsha256(Linux.Imageˉbytes.AsSpan()));
        return Linux.Imageˉbytes;
    }

    private static void Requireˉnoˉdotnetˉmodules(IEnumerable<string> modules)
    {
        Equal(0, modules.Count(Name =>
            Name.Equals("clr.dll", StringComparison.OrdinalIgnoreCase) ||
            Name.Equals("mscoree.dll", StringComparison.OrdinalIgnoreCase) ||
            Name.Equals("mscorwks.dll", StringComparison.OrdinalIgnoreCase) ||
            Name.Contains("dotnet", StringComparison.OrdinalIgnoreCase) ||
            Name.Contains("coreclr", StringComparison.OrdinalIgnoreCase) ||
            Name.Contains("hostfxr", StringComparison.OrdinalIgnoreCase) ||
            Name.Contains("hostpolicy", StringComparison.OrdinalIgnoreCase)));
    }
}
