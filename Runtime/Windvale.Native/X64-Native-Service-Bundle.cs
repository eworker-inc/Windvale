using System.Collections.Immutable;
using System.Security.Cryptography;
using Windvale.Compiler.Native;

namespace Windvale.Runtime.Native;

public enum Nativeˉserviceˉplatform : uint
{
    Windows = 1,
    Linux = 2,
}

public enum Nativeˉserviceˉadapter : uint
{
    Windowsˉconsoleˉwrite = 1,
    Linuxˉconsoleˉwrite = 2,
    Argumentˉsnapshot = 3,
    Windowsˉfileˉinput = 4,
    Linuxˉfileˉinput = 5,
    Utf8 = 6,
    Windowsˉdiagnosticˉwrite = 7,
    Linuxˉdiagnosticˉwrite = 8,
    Enumˉmetadata = 9,
    Textˉconcat = 10,
    Textˉquote = 11,
    I32ˉformat = 12,
    U32ˉformat = 13,
    Windowsˉfileˉoutput = 14,
    Linuxˉfileˉoutput = 15,
}

public sealed record Nativeˉserviceˉbundleˉplacement(
    Nativeˉservice Service,
    Nativeˉserviceˉadapter Adapter,
    int Serviceˉtableˉoffset,
    int Imageˉoffset,
    int Codeˉbytes,
    string Sha256);

public sealed record Nativeˉserviceˉbundle(
    Nativeˉserviceˉplatform Platform,
    int Nativeˉimageˉbytes,
    ImmutableArray<byte> Imageˉbytes,
    ImmutableArray<Nativeˉserviceˉbundleˉplacement> Placements);

public static class X64ˉnativeˉserviceˉbundle
{
    public static Nativeˉserviceˉbundle Build(
        Nativeˉfragment fragment,
        Nativeˉserviceˉplatform platform)
    {
        return Build(fragment, platform, fragment.Requiredˉservices);
    }

    public static Nativeˉserviceˉbundle Buildˉhostedˉverifier(
        Nativeˉfragment fragment,
        Nativeˉserviceˉplatform platform)
    {
        Nativeˉfragmentˉverifier.Verify(fragment);
        ReadOnlySpan<Nativeˉservice> Expected =
        [
            Nativeˉservice.Consoleˉwriteˉline,
            Nativeˉservice.Processˉargumentˉcount,
            Nativeˉservice.Processˉargument,
            Nativeˉservice.Fileˉreadˉbytes,
            Nativeˉservice.Diagnosticˉwriteˉline,
        ];
        if (!fragment.Requiredˉservices.AsSpan().SequenceEqual(Expected))
        {
            throw new Nativeˉbackendˉexception(
                "WVN4018",
                "The hosted verifier requires its exact five-service authority profile.");
        }

        ImmutableArray<Nativeˉservice> Services =
        [
            Nativeˉservice.Consoleˉwriteˉline,
            Nativeˉservice.Processˉargumentˉcount,
            Nativeˉservice.Processˉargument,
            Nativeˉservice.Fileˉreadˉbytes,
            Nativeˉservice.Textˉutf8ˉisˉvalid,
            Nativeˉservice.Diagnosticˉwriteˉline,
        ];
        return Build(fragment, platform, Services);
    }

    public static Nativeˉserviceˉbundle Buildˉhostedˉinspector(
        Nativeˉfragment fragment,
        Nativeˉserviceˉplatform platform)
    {
        Nativeˉfragmentˉverifier.Verify(fragment);
        ReadOnlySpan<Nativeˉservice> Expected =
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
        if (!fragment.Requiredˉservices.AsSpan().SequenceEqual(Expected))
        {
            throw new Nativeˉbackendˉexception(
                "WVN4019",
                "The hosted WVB inspector requires its exact read-only service profile.");
        }

        return Build(fragment, platform, [.. Expected]);
    }

    public static Nativeˉserviceˉbundle Buildˉhostedˉrunner(
        Nativeˉfragment fragment,
        Nativeˉserviceˉplatform platform)
    {
        Nativeˉfragmentˉverifier.Verify(fragment);
        ReadOnlySpan<Nativeˉservice> Expected =
        [
            Nativeˉservice.Consoleˉwriteˉline,
            Nativeˉservice.Processˉargumentˉcount,
            Nativeˉservice.Processˉargument,
            Nativeˉservice.Fileˉreadˉbytes,
            Nativeˉservice.Textˉutf8ˉisˉvalid,
            Nativeˉservice.Diagnosticˉwriteˉline,
            Nativeˉservice.Textˉconcat,
            Nativeˉservice.I32ˉformat,
            Nativeˉservice.U32ˉformat,
        ];
        if (!fragment.Requiredˉservices.AsSpan().SequenceEqual(Expected))
        {
            throw new Nativeˉbackendˉexception(
                "WVN4020",
                "The hosted WVB runner requires its exact read-only execution service profile.");
        }

        return Build(fragment, platform, [.. Expected]);
    }

    private static Nativeˉserviceˉbundle Build(
        Nativeˉfragment fragment,
        Nativeˉserviceˉplatform platform,
        ImmutableArray<Nativeˉservice> requiredˉservices)
    {
        Nativeˉfragmentˉverifier.Verify(fragment);
        if (!Enum.IsDefined(platform))
        {
            throw new ArgumentOutOfRangeException(nameof(platform), platform, null);
        }

        var Services = requiredˉservices
            .Select(Service => Buildˉservice(Service, platform, fragment.Types))
            .ToImmutableArray();
        var Plan = X64ˉnativeˉpublicationˉlayout.Plan(
            fragment.Code.Length,
            [.. Services.Select(Item => new Nativeˉpublicationˉservice(
                Item.Service,
                Item.Code.Length))]);
        var Image = new byte[Plan.Imageˉbytes];
        fragment.Code.AsSpan().CopyTo(Image);
        var Placements = ImmutableArray.CreateBuilder<Nativeˉserviceˉbundleˉplacement>(
            Services.Length);
        var Previousˉserviceˉend = fragment.Code.Length;
        for (var Index = 0; Index < Services.Length; Index++)
        {
            var Service = Services[Index];
            var Placement = Plan.Placements[Index];
            if (Index != 0)
            {
                Image.AsSpan(
                    Previousˉserviceˉend,
                    Placement.Offset - Previousˉserviceˉend).Fill(0x90);
            }
            Service.Code.AsSpan().CopyTo(Image.AsSpan(Placement.Offset));
            Placements.Add(new(
                Service.Service,
                Service.Adapter,
                Serviceˉtableˉoffset(Service.Service),
                Placement.Offset,
                Placement.Size,
                Calculateˉsha256(Service.Code.AsSpan())));
            Previousˉserviceˉend = checked(Placement.Offset + Placement.Size);
        }

        return new(
            platform,
            fragment.Code.Length,
            Image.ToImmutableArray(),
            Placements.MoveToImmutable());
    }

    private static Nativeˉserviceˉcode Buildˉservice(
        Nativeˉservice service,
        Nativeˉserviceˉplatform platform,
        ImmutableArray<Windvale.Bytecode.Nominalˉtypeˉdeclaration> types)
    {
        ImmutableArray<byte> Code;
        Nativeˉserviceˉadapter Adapter;
        var Outputˉplatform = platform == Nativeˉserviceˉplatform.Windows
            ? Nativeˉoutputˉplatform.Windows
            : Nativeˉoutputˉplatform.Linux;
        var Fileˉplatform = platform == Nativeˉserviceˉplatform.Windows
            ? Nativeˉfileˉinputˉplatform.Windows
            : Nativeˉfileˉinputˉplatform.Linux;
        if (service is Nativeˉservice.Consoleˉwriteˉline or
            Nativeˉservice.Diagnosticˉwriteˉline)
        {
            Code = X64ˉnativeˉoutputˉservices.Build(service, Outputˉplatform);
            X64ˉnativeˉoutputˉservices.Verify(service, Outputˉplatform, Code.AsSpan());
            Adapter = (service, platform) switch
            {
                (Nativeˉservice.Consoleˉwriteˉline, Nativeˉserviceˉplatform.Windows) =>
                    Nativeˉserviceˉadapter.Windowsˉconsoleˉwrite,
                (Nativeˉservice.Consoleˉwriteˉline, Nativeˉserviceˉplatform.Linux) =>
                    Nativeˉserviceˉadapter.Linuxˉconsoleˉwrite,
                (Nativeˉservice.Diagnosticˉwriteˉline, Nativeˉserviceˉplatform.Windows) =>
                    Nativeˉserviceˉadapter.Windowsˉdiagnosticˉwrite,
                _ => Nativeˉserviceˉadapter.Linuxˉdiagnosticˉwrite,
            };
        }
        else if (service is Nativeˉservice.Processˉargumentˉcount or
            Nativeˉservice.Processˉargument)
        {
            Code = X64ˉnativeˉargumentˉservices.Build(service);
            X64ˉnativeˉargumentˉservices.Verify(service, Code.AsSpan());
            Adapter = Nativeˉserviceˉadapter.Argumentˉsnapshot;
        }
        else if (service == Nativeˉservice.Fileˉreadˉbytes)
        {
            Code = X64ˉnativeˉfileˉinputˉservice.Build(Fileˉplatform);
            X64ˉnativeˉfileˉinputˉservice.Verify(Fileˉplatform, Code.AsSpan());
            Adapter = platform == Nativeˉserviceˉplatform.Windows
                ? Nativeˉserviceˉadapter.Windowsˉfileˉinput
                : Nativeˉserviceˉadapter.Linuxˉfileˉinput;
        }
        else if (service == Nativeˉservice.Textˉutf8ˉisˉvalid)
        {
            Code = X64ˉnativeˉutf8ˉservice.Build();
            X64ˉnativeˉutf8ˉservice.Verify(Code.AsSpan());
            Adapter = Nativeˉserviceˉadapter.Utf8;
        }
        else if (service == Nativeˉservice.Fileˉwriteˉbytes)
        {
            Code = X64ˉnativeˉfileˉoutputˉservice.Build(Fileˉplatform);
            X64ˉnativeˉfileˉoutputˉservice.Verify(Fileˉplatform, Code.AsSpan());
            Adapter = platform == Nativeˉserviceˉplatform.Windows
                ? Nativeˉserviceˉadapter.Windowsˉfileˉoutput
                : Nativeˉserviceˉadapter.Linuxˉfileˉoutput;
        }
        else
        {
            Code = X64ˉnativeˉtextˉservices.Build(service, types);
            X64ˉnativeˉtextˉservices.Verify(service, Code.AsSpan(), types);
            Adapter = service switch
            {
                Nativeˉservice.Enumˉname => Nativeˉserviceˉadapter.Enumˉmetadata,
                Nativeˉservice.Textˉconcat => Nativeˉserviceˉadapter.Textˉconcat,
                Nativeˉservice.Textˉquote => Nativeˉserviceˉadapter.Textˉquote,
                Nativeˉservice.I32ˉformat => Nativeˉserviceˉadapter.I32ˉformat,
                Nativeˉservice.U32ˉformat => Nativeˉserviceˉadapter.U32ˉformat,
                _ => throw new Nativeˉbackendˉexception(
                    "WVN4010",
                    $"Unknown native service implementation '{service}'."),
            };
        }
        return new(service, Adapter, Code);
    }

    private static int Serviceˉtableˉoffset(Nativeˉservice service) => service switch
    {
        Nativeˉservice.Consoleˉwriteˉline =>
            Nativeˉserviceˉtableˉcontract.CONSOLE_WRITE_LINE_POINTER_OFFSET,
        Nativeˉservice.Processˉargumentˉcount =>
            Nativeˉserviceˉtableˉcontract.PROCESS_ARGUMENT_COUNT_POINTER_OFFSET,
        Nativeˉservice.Processˉargument =>
            Nativeˉserviceˉtableˉcontract.PROCESS_ARGUMENT_POINTER_OFFSET,
        Nativeˉservice.Fileˉreadˉbytes =>
            Nativeˉserviceˉtableˉcontract.FILE_READ_BYTES_POINTER_OFFSET,
        Nativeˉservice.Textˉutf8ˉisˉvalid =>
            Nativeˉserviceˉtableˉcontract.TEXT_UTF8_IS_VALID_POINTER_OFFSET,
        Nativeˉservice.Diagnosticˉwriteˉline =>
            Nativeˉserviceˉtableˉcontract.DIAGNOSTIC_WRITE_LINE_POINTER_OFFSET,
        Nativeˉservice.Enumˉname => Nativeˉserviceˉtableˉcontract.ENUM_NAME_POINTER_OFFSET,
        Nativeˉservice.Textˉconcat => Nativeˉserviceˉtableˉcontract.TEXT_CONCAT_POINTER_OFFSET,
        Nativeˉservice.Textˉquote => Nativeˉserviceˉtableˉcontract.TEXT_QUOTE_POINTER_OFFSET,
        Nativeˉservice.I32ˉformat => Nativeˉserviceˉtableˉcontract.I32_FORMAT_POINTER_OFFSET,
        Nativeˉservice.U32ˉformat => Nativeˉserviceˉtableˉcontract.U32_FORMAT_POINTER_OFFSET,
        Nativeˉservice.Fileˉwriteˉbytes =>
            Nativeˉserviceˉtableˉcontract.FILE_WRITE_BYTES_POINTER_OFFSET,
        _ => throw new ArgumentOutOfRangeException(nameof(service), service, null),
    };

    private static string Calculateˉsha256(ReadOnlySpan<byte> bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private sealed record Nativeˉserviceˉcode(
        Nativeˉservice Service,
        Nativeˉserviceˉadapter Adapter,
        ImmutableArray<byte> Code);
}
