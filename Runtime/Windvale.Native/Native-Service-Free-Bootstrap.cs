using System.Collections.Immutable;
using Windvale.Compiler.Native;

namespace Windvale.Runtime.Native;

internal static class Nativeˉserviceˉfreeˉbootstrap
{
    public static Nativeˉpublicationˉplan Planˉlayout(Nativeˉfragment fragment)
    {
        ArgumentNullException.ThrowIfNull(fragment);
        Nativeˉfragmentˉverifier.Verify(fragment);
        if (!fragment.Requiredˉservices.IsEmpty)
        {
            throw Invalidˉbootstrap(
                "The service-free bootstrap cannot bind runtime services.");
        }

        var Imageˉbytes = checked((fragment.Code.Length + 15) & ~15);
        if (Imageˉbytes > X64ˉnativeˉpublicationˉlayout.MAXIMUM_IMAGE_BYTES)
        {
            throw Invalidˉbootstrap(
                "The service-free bootstrap image exceeds its bounded extent.");
        }
        return new(fragment.Code.Length, Imageˉbytes, []);
    }

    public static Nativeˉpublicationˉlifetimeˉplan Planˉlifetime(int imageˉbytes)
    {
        if (imageˉbytes is < 1 or > X64ˉnativeˉpublicationˉlayout.MAXIMUM_IMAGE_BYTES)
        {
            throw Invalidˉbootstrap(
                "The service-free bootstrap lifetime extent is invalid.");
        }

        ImmutableArray<Nativeˉpublicationˉtransition> Transitions =
        [
            new(Nativeˉpublicationˉstate.Unallocated, Nativeˉpublicationˉaction.Allocateˉwritable, Nativeˉpublicationˉstate.Writable),
            new(Nativeˉpublicationˉstate.Writable, Nativeˉpublicationˉaction.Copyˉimage, Nativeˉpublicationˉstate.Copied),
            new(Nativeˉpublicationˉstate.Writable, Nativeˉpublicationˉaction.Release, Nativeˉpublicationˉstate.Released),
            new(Nativeˉpublicationˉstate.Copied, Nativeˉpublicationˉaction.Sealˉexecutable, Nativeˉpublicationˉstate.Executable),
            new(Nativeˉpublicationˉstate.Copied, Nativeˉpublicationˉaction.Release, Nativeˉpublicationˉstate.Released),
            new(Nativeˉpublicationˉstate.Executable, Nativeˉpublicationˉaction.Invoke, Nativeˉpublicationˉstate.Invoked),
            new(Nativeˉpublicationˉstate.Executable, Nativeˉpublicationˉaction.Release, Nativeˉpublicationˉstate.Released),
            new(Nativeˉpublicationˉstate.Invoked, Nativeˉpublicationˉaction.Release, Nativeˉpublicationˉstate.Released),
            new(Nativeˉpublicationˉstate.Released, Nativeˉpublicationˉaction.Complete, Nativeˉpublicationˉstate.Released),
        ];
        var Plan = new Nativeˉpublicationˉlifetimeˉplan(imageˉbytes, Transitions);
        X64ˉnativeˉpublicationˉlifetime.Verifyˉplan(Plan);
        return Plan;
    }

    private static InvalidOperationException Invalidˉbootstrap(string message) =>
        new($"The native service-free bootstrap is invalid. {message}");
}
