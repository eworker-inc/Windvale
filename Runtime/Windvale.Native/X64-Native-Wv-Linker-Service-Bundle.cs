using Windvale.Compiler.Native;

namespace Windvale.Runtime.Native;

public static partial class X64ˉnativeˉserviceˉbundle
{
    public static Nativeˉserviceˉbundle Buildˉhostedˉwvˉlinker(
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
            Nativeˉservice.Textˉutf8ˉisˉvalid,
            Nativeˉservice.Diagnosticˉwriteˉline,
            Nativeˉservice.Enumˉname,
            Nativeˉservice.Textˉconcat,
            Nativeˉservice.U32ˉformat,
            Nativeˉservice.Fileˉwriteˉbytes,
        ];
        if (!fragment.Requiredˉservices.AsSpan().SequenceEqual(Fragmentˉservices))
        {
            throw new Nativeˉbackendˉexception(
                "WVN4022",
                "The hosted Windvale linker requires its exact read/write service profile; " +
                $"actual services: {string.Join(", ", fragment.Requiredˉservices)}.");
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
            Nativeˉservice.U32ˉformat,
            Nativeˉservice.Fileˉwriteˉbytes,
        ];
        return Build(fragment, platform, [.. Bundleˉservices]);
    }
}
