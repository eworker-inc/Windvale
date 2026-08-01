using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Runtime;

namespace Windvale.Runtime.Native;

public sealed class Nativeˉhostˉservices
{
    public Nativeˉhostˉservices(
        TextWriter? standardˉoutput,
        IEnumerable<string>? authorizedˉcapabilities = null,
        Hostedˉresourceˉcontext? resources = null)
    {
        Standardˉoutput = standardˉoutput ?? resources?.Standardˉoutput;
        Diagnosticˉoutput = resources?.Diagnosticˉoutput;
        Resources = resources;
        Authorizedˉcapabilities = (authorizedˉcapabilities ?? [])
            .ToImmutableHashSet(StringComparer.Ordinal);
    }

    public TextWriter? Standardˉoutput { get; }

    public TextWriter? Diagnosticˉoutput { get; }

    public Hostedˉresourceˉcontext? Resources { get; }

    public ImmutableHashSet<string> Authorizedˉcapabilities { get; }

    internal bool Isˉauthorized(Nativeˉservice service) =>
        service switch
        {
            Nativeˉservice.Consoleˉwriteˉline =>
                Authorizedˉcapabilities.Contains(Capabilityˉcatalog.CONSOLE_WRITE_LINE),
            Nativeˉservice.Processˉargumentˉcount =>
                Authorizedˉcapabilities.Contains(Capabilityˉcatalog.PROCESS_ARGUMENT_COUNT),
            Nativeˉservice.Processˉargument =>
                Authorizedˉcapabilities.Contains(Capabilityˉcatalog.PROCESS_ARGUMENT),
            Nativeˉservice.Fileˉreadˉbytes =>
                Authorizedˉcapabilities.Contains(Capabilityˉcatalog.FILE_READ_BYTES),
            Nativeˉservice.Diagnosticˉwriteˉline =>
                Authorizedˉcapabilities.Contains(Capabilityˉcatalog.DIAGNOSTIC_WRITE_LINE),
            Nativeˉservice.Textˉutf8ˉisˉvalid or
                Nativeˉservice.Enumˉname or
                Nativeˉservice.Textˉconcat or
                Nativeˉservice.Textˉquote or
                Nativeˉservice.I32ˉformat or
                Nativeˉservice.U32ˉformat => true,
            _ => false,
        };

    internal bool Supports(Nativeˉservice service) =>
        service switch
        {
            Nativeˉservice.Consoleˉwriteˉline => Standardˉoutput is not null,
            Nativeˉservice.Processˉargumentˉcount or
                Nativeˉservice.Processˉargument => Resources is not null,
            Nativeˉservice.Fileˉreadˉbytes => Resources?.Fileˉreader is not null,
            Nativeˉservice.Diagnosticˉwriteˉline => Diagnosticˉoutput is not null,
            Nativeˉservice.Textˉutf8ˉisˉvalid or
                Nativeˉservice.Enumˉname or
                Nativeˉservice.Textˉconcat or
                Nativeˉservice.Textˉquote or
                Nativeˉservice.I32ˉformat or
                Nativeˉservice.U32ˉformat => true,
            _ => false,
        };
}
