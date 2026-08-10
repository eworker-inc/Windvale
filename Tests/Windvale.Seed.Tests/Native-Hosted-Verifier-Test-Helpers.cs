using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Linker;
using Windvale.Runtime.Native;

namespace Windvale.Seed.Tests;

internal static partial class Program
{
    private const int CONSOLE_APPLICATION_VERIFIER_WVB_BYTES = 105_006;
    private const string CONSOLE_APPLICATION_VERIFIER_WVB_SHA256 =
        "1dcd5f2aeebd974649e64c90d9f473e1e75f7d13dbcde2814de1dded72cf2c0c";

    private static (
        Verifiedˉmodule Module,
        Nativeˉfragment Fragment,
        uint Entry) Loadˉconsoleˉapplicationˉverifierˉfixture(
            string repository)
    {
        var Bytes = File.ReadAllBytes(Path.Combine(
            repository,
            "Artifacts",
            "Native-Console-Application-Verifier-Candidate",
            "Console-Application-Verifier.wvb"));
        Equal(CONSOLE_APPLICATION_VERIFIER_WVB_BYTES, Bytes.Length);
        Equal(
            CONSOLE_APPLICATION_VERIFIER_WVB_SHA256,
            Moduleˉdigest.Calculateˉsha256(Bytes));
        var Module = Moduleˉcodec.Readˉandˉverify(Bytes);
        var Fragment = X64ˉnativeˉbackend.Compile(Module).Fragment;
        Sequenceˉequal(
            [
                Nativeˉservice.Consoleˉwriteˉline,
                Nativeˉservice.Processˉargumentˉcount,
                Nativeˉservice.Processˉargument,
                Nativeˉservice.Fileˉreadˉbytes,
                Nativeˉservice.Diagnosticˉwriteˉline,
                Nativeˉservice.Textˉconcat,
                Nativeˉservice.U32ˉformat,
            ],
            Fragment.Requiredˉservices);
        Sequenceˉequal(
            [
                Nativeˉservice.Consoleˉwriteˉline,
                Nativeˉservice.Processˉargumentˉcount,
                Nativeˉservice.Processˉargument,
                Nativeˉservice.Fileˉreadˉbytes,
                Nativeˉservice.Textˉutf8ˉisˉvalid,
                Nativeˉservice.Diagnosticˉwriteˉline,
                Nativeˉservice.Enumˉname,
                Nativeˉservice.Textˉconcat,
                Nativeˉservice.Textˉquote,
                Nativeˉservice.I32ˉformat,
                Nativeˉservice.U32ˉformat,
            ],
            Hostedˉverifierˉapplicationˉmetadata.Requiredˉservices(
                Hostedˉverifierˉapplicationˉprofile
                    .Consoleˉapplicationˉverifier));
        var Entry = Fragment.Symbols.Single(Symbol =>
            Symbol.Binding == Nativeˉsymbolˉbinding.Export &&
            Symbol.Kind == Nativeˉsymbolˉkind.Function &&
            Symbol.Name == "Main").Offset;
        return (Module, Fragment, Entry);
    }
}
