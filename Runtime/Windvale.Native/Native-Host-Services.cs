using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using Windvale.Bytecode;
using Windvale.Compiler.Native;
using Windvale.Runtime;

namespace Windvale.Runtime.Native;

public sealed class Nativeˉhostˉservices
{
    public Nativeˉhostˉservices(
        Nativeˉoutputˉchannel? standardˉoutput,
        IEnumerable<string>? authorizedˉcapabilities = null,
        Hostedˉresourceˉcontext? resources = null,
        Nativeˉoutputˉchannel? diagnosticˉoutput = null,
        Nativeˉfileˉinput? fileˉinput = null,
        Nativeˉfileˉoutput? fileˉoutput = null)
    {
        Standardˉoutput = standardˉoutput;
        Diagnosticˉoutput = diagnosticˉoutput;
        Fileˉinput = fileˉinput;
        Fileˉoutput = fileˉoutput;
        Resources = resources;
        Authorizedˉcapabilities = (authorizedˉcapabilities ?? [])
            .ToImmutableHashSet(StringComparer.Ordinal);
    }

    public Nativeˉoutputˉchannel? Standardˉoutput { get; }

    public Nativeˉoutputˉchannel? Diagnosticˉoutput { get; }

    public Nativeˉfileˉinput? Fileˉinput { get; }

    public Nativeˉfileˉoutput? Fileˉoutput { get; }

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
            Nativeˉservice.Fileˉwriteˉbytes =>
                Authorizedˉcapabilities.Contains(Capabilityˉcatalog.FILE_WRITE_BYTES),
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
            Nativeˉservice.Consoleˉwriteˉline => Standardˉoutput?.Isˉavailable == true,
            Nativeˉservice.Processˉargumentˉcount or
                Nativeˉservice.Processˉargument => Resources is not null,
            Nativeˉservice.Fileˉreadˉbytes => Fileˉinput?.Isˉavailable == true,
            Nativeˉservice.Fileˉwriteˉbytes => Fileˉoutput?.Isˉavailable == true,
            Nativeˉservice.Diagnosticˉwriteˉline => Diagnosticˉoutput?.Isˉavailable == true,
            Nativeˉservice.Textˉutf8ˉisˉvalid or
                Nativeˉservice.Enumˉname or
                Nativeˉservice.Textˉconcat or
                Nativeˉservice.Textˉquote or
                Nativeˉservice.I32ˉformat or
                Nativeˉservice.U32ˉformat => true,
            _ => false,
        };
}

public enum Nativeˉfileˉinputˉplatform : uint
{
    Windows = 1,
    Linux = 2,
}

public sealed class Nativeˉfileˉinput
{
    private Nativeˉfileˉinput(Nativeˉfileˉinputˉplatform platform)
    {
        Platform = platform;
    }

    internal Nativeˉfileˉinputˉplatform Platform { get; }

    internal bool Isˉavailable => Platform == Currentˉplatform();

    public static Nativeˉfileˉinput Hostˉfileˉsystem() => new(Currentˉplatform());

    internal static Nativeˉfileˉinputˉplatform Currentˉplatform()
    {
        if (OperatingSystem.IsWindows())
        {
            return Nativeˉfileˉinputˉplatform.Windows;
        }
        if (OperatingSystem.IsLinux())
        {
            return Nativeˉfileˉinputˉplatform.Linux;
        }
        throw new PlatformNotSupportedException(
            "The native file-input boundary supports Windows and Linux.");
    }
}

public sealed class Nativeˉfileˉoutput
{
    private Nativeˉfileˉoutput(Nativeˉfileˉinputˉplatform platform)
    {
        Platform = platform;
    }

    internal Nativeˉfileˉinputˉplatform Platform { get; }

    internal bool Isˉavailable => Platform == Nativeˉfileˉinput.Currentˉplatform();

    public static Nativeˉfileˉoutput Hostˉfileˉsystem() =>
        new(Nativeˉfileˉinput.Currentˉplatform());
}

public enum Nativeˉoutputˉplatform : uint
{
    Windows = 1,
    Linux = 2,
}

public sealed class Nativeˉoutputˉchannel
{
    private const int STD_OUTPUT_HANDLE = -11;
    private const int STD_ERROR_HANDLE = -12;

    private Nativeˉoutputˉchannel(
        SafeFileHandle handle,
        Nativeˉoutputˉplatform platform)
    {
        Handle = handle;
        Platform = platform;
    }

    internal SafeFileHandle Handle { get; }

    internal Nativeˉoutputˉplatform Platform { get; }

    internal bool Isˉavailable =>
        !Handle.IsClosed &&
        !Handle.IsInvalid &&
        (Platform != Nativeˉoutputˉplatform.Linux ||
            Handle.DangerousGetHandle().ToInt64() is >= 0 and <= int.MaxValue);

    public static Nativeˉoutputˉchannel Fromˉfileˉhandle(SafeFileHandle handle)
    {
        ArgumentNullException.ThrowIfNull(handle);
        return new(handle, Currentˉplatform());
    }

    public static Nativeˉoutputˉchannel Processˉstandardˉoutput() =>
        Processˉchannel(STD_OUTPUT_HANDLE, 1);

    public static Nativeˉoutputˉchannel Processˉdiagnosticˉoutput() =>
        Processˉchannel(STD_ERROR_HANDLE, 2);

    private static Nativeˉoutputˉchannel Processˉchannel(int windowsˉkind, int linuxˉdescriptor)
    {
        if (OperatingSystem.IsWindows())
        {
            return new(
                new SafeFileHandle(GetStdHandle(windowsˉkind), ownsHandle: false),
                Nativeˉoutputˉplatform.Windows);
        }
        if (OperatingSystem.IsLinux())
        {
            return new(
                new SafeFileHandle(new IntPtr(linuxˉdescriptor), ownsHandle: false),
                Nativeˉoutputˉplatform.Linux);
        }
        throw new PlatformNotSupportedException(
            "The native output channels support Windows and Linux.");
    }

    internal static Nativeˉoutputˉplatform Currentˉplatform()
    {
        if (OperatingSystem.IsWindows())
        {
            return Nativeˉoutputˉplatform.Windows;
        }
        if (OperatingSystem.IsLinux())
        {
            return Nativeˉoutputˉplatform.Linux;
        }
        throw new PlatformNotSupportedException(
            "The native output channels support Windows and Linux.");
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int standardˉhandle);
}
