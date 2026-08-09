using System.Collections.Immutable;
using Windvale.Assembler;

namespace Windvale.Bootstrap;

public sealed record Kernelˉprocessˉimageˉwvaˉobjects(
    ImmutableArray<byte> Initˉserviceˉshimˉobjectˉbytes,
    ImmutableArray<byte> Directoryˉserviceˉshimˉobjectˉbytes,
    ImmutableArray<byte> Bootˉresourceˉserviceˉobjectˉbytes,
    ImmutableArray<byte> Clientˉshimˉobjectˉbytes);

public static partial class Kernelˉprocessˉimage
{
    private static ImmutableArray<byte> Assembleˉwvaˉresource(
        string resource,
        string description)
    {
        var Assembly = Assemblyˉcompiler.Assemble(Loadˉsource(resource));
        if (!Assembly.Success)
        {
            throw new InvalidOperationException(
                $"The {description} did not assemble: {Assembly.Diagnostics[0].Code}: " +
                Assembly.Diagnostics[0].Message);
        }

        return Assembly.Objectˉbytes;
    }
}
