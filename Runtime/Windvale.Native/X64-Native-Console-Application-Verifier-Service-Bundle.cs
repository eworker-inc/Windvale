using Windvale.Compiler.Native;

namespace Windvale.Runtime.Native;

public static partial class X64ˉnativeˉserviceˉbundle
{
    public static Nativeˉserviceˉbundle Buildˉhostedˉconsoleˉapplicationˉverifier(
        Nativeˉfragment fragment,
        Nativeˉserviceˉplatform platform)
    {
        Nativeˉfragmentˉverifier.Verify(fragment);
        ReadOnlySpan<Nativeˉservice> Fragmentˉservices =
        [
            Nativeˉservice.Consoleˉwriteˉline,
            Nativeˉservice.Processˉargumentˉcount,
            Nativeˉservice.Processˉargument,
            Nativeˉservice.Fileˉreadˉbytes,
            Nativeˉservice.Diagnosticˉwriteˉline,
            Nativeˉservice.Textˉconcat,
            Nativeˉservice.U32ˉformat,
        ];
        if (!fragment.Requiredˉservices.AsSpan().SequenceEqual(Fragmentˉservices))
        {
            throw new Nativeˉbackendˉexception(
                "WVN4025",
                "The hosted console-application verifier requires its exact read-only " +
                $"service profile; actual services: {string.Join(", ", fragment.Requiredˉservices)}.");
        }

        ReadOnlySpan<Nativeˉservice> Bundleˉservices =
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
        ];
        return Build(fragment, platform, [.. Bundleˉservices]);
    }
}
