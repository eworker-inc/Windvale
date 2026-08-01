using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Runtime.Native;

public sealed class Nativeˉhostˉservices
{
    public Nativeˉhostˉservices(
        TextWriter? standardˉoutput,
        IEnumerable<string>? authorizedˉcapabilities = null)
    {
        Standardˉoutput = standardˉoutput;
        Authorizedˉcapabilities = (authorizedˉcapabilities ?? [])
            .ToImmutableHashSet(StringComparer.Ordinal);
    }

    public TextWriter? Standardˉoutput { get; }

    public ImmutableHashSet<string> Authorizedˉcapabilities { get; }

    internal bool Isˉauthorized(Nativeˉservice service) =>
        service switch
        {
            Nativeˉservice.Consoleˉwriteˉline =>
                Authorizedˉcapabilities.Contains(Capabilityˉcatalog.CONSOLE_WRITE_LINE),
            _ => false,
        };

    internal bool Supports(Nativeˉservice service) =>
        service switch
        {
            Nativeˉservice.Consoleˉwriteˉline => Standardˉoutput is not null,
            _ => false,
        };
}
