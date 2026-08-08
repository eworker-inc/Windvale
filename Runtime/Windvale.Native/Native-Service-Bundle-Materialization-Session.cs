using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Runtime.Native;

internal static class Nativeˉserviceˉbundleˉmaterializationˉsession
{
    private const uint REQUEST_MAGIC = 0x5153_5657;
    private const uint RESPONSE_MAGIC = 0x4953_5657;
    private const uint FORMAT_VERSION = 2;
    private const int REQUEST_HEADER_BYTES = 32;
    private const int RESPONSE_HEADER_BYTES = 40;
    private const int MAXIMUM_PLAN_BYTES =
        X64ˉnativeˉpublicationˉlayout.REQUEST_HEADER_BYTES +
        X64ˉnativeˉpublicationˉlayout.MAXIMUM_SERVICES *
        X64ˉnativeˉpublicationˉlayout.SERVICE_RECORD_BYTES;

    internal const int MAXIMUM_SEGMENT_BYTES =
        Bytecodeˉlimits.MAX_BYTE_DATA_BYTES - REQUEST_HEADER_BYTES - MAXIMUM_PLAN_BYTES;

    public static ImmutableArray<byte> Build(
        ImmutableArray<byte> fragment,
        ImmutableArray<Nativeˉserviceˉcode> services,
        Nativeˉpublicationˉplan plan)
    {
        Validateˉinput(fragment, services, plan);
        var Result = ImmutableArray.CreateBuilder<byte>(plan.Imageˉbytes);
        var Segmentˉoffset = 0;
        while (Segmentˉoffset < plan.Imageˉbytes)
        {
            var Request = Buildˉrequest(fragment, services, plan, Segmentˉoffset);
            var Response = X64ˉnativeˉserviceˉbundleˉmaterialization.Buildˉwithˉwindvale(
                Request);
            var Segment = Verifyˉresponse(
                fragment,
                services,
                plan,
                Segmentˉoffset,
                Request.Length,
                Response);
            Result.AddRange(Segment);
            Segmentˉoffset = checked(Segmentˉoffset + Segment.Length);
        }
        if (Result.Count != plan.Imageˉbytes)
        {
            throw Invalidˉresponse("The segmented Windvale service bundle is incomplete.");
        }
        return Result.MoveToImmutable();
    }

    internal static ImmutableArray<ImmutableArray<byte>> Buildˉrequests(
        ImmutableArray<byte> fragment,
        ImmutableArray<Nativeˉserviceˉcode> services,
        Nativeˉpublicationˉplan plan)
    {
        Validateˉinput(fragment, services, plan);
        var Result = ImmutableArray.CreateBuilder<ImmutableArray<byte>>();
        for (var Offset = 0; Offset < plan.Imageˉbytes; Offset += MAXIMUM_SEGMENT_BYTES)
        {
            Result.Add(Buildˉrequest(fragment, services, plan, Offset));
        }
        return Result.ToImmutable();
    }

    internal static ImmutableArray<byte> Buildˉrequest(
        ImmutableArray<byte> fragment,
        ImmutableArray<Nativeˉserviceˉcode> services,
        Nativeˉpublicationˉplan plan,
        int segmentˉoffset)
    {
        Validateˉinput(fragment, services, plan);
        if (segmentˉoffset < 0 ||
            segmentˉoffset >= plan.Imageˉbytes ||
            segmentˉoffset % MAXIMUM_SEGMENT_BYTES != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(segmentˉoffset));
        }
        var Segmentˉbytes = Math.Min(
            MAXIMUM_SEGMENT_BYTES,
            plan.Imageˉbytes - segmentˉoffset);
        var Segmentˉend = checked(segmentˉoffset + Segmentˉbytes);
        var Planˉrequest = X64ˉnativeˉpublicationˉlayout.Buildˉrequest(
            fragment.Length,
            services.Select(Service => new Nativeˉpublicationˉservice(
                Service.Service,
                Service.Code.Length)).ToImmutableArray());
        var Payloadˉbytes = Overlapˉlength(0, fragment.Length, segmentˉoffset, Segmentˉend);
        for (var Index = 0; Index < services.Length; Index++)
        {
            Payloadˉbytes = checked(Payloadˉbytes + Overlapˉlength(
                plan.Placements[Index].Offset,
                plan.Placements[Index].Size,
                segmentˉoffset,
                Segmentˉend));
        }

        var Totalˉbytes = checked(REQUEST_HEADER_BYTES + Planˉrequest.Length + Payloadˉbytes);
        if (Totalˉbytes > Bytecodeˉlimits.MAX_BYTE_DATA_BYTES)
        {
            throw new Nativeˉbackendˉexception(
                "WVN4015",
                "A segmented native service-bundle request exceeds the byte-value limit.");
        }

        var Result = new byte[Totalˉbytes];
        BinaryPrimitives.WriteUInt32LittleEndian(Result, REQUEST_MAGIC);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(4), FORMAT_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8), checked((uint)Totalˉbytes));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(12),
            checked((uint)Planˉrequest.Length));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(16),
            checked((uint)segmentˉoffset));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(20),
            checked((uint)Segmentˉbytes));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(24),
            checked((uint)Payloadˉbytes));
        Planˉrequest.CopyTo(Result, REQUEST_HEADER_BYTES);

        var Targetˉoffset = checked(REQUEST_HEADER_BYTES + Planˉrequest.Length);
        Copyˉoverlap(
            fragment.AsSpan(),
            0,
            segmentˉoffset,
            Segmentˉend,
            Result,
            ref Targetˉoffset);
        for (var Index = 0; Index < services.Length; Index++)
        {
            Copyˉoverlap(
                services[Index].Code.AsSpan(),
                plan.Placements[Index].Offset,
                segmentˉoffset,
                Segmentˉend,
                Result,
                ref Targetˉoffset);
        }
        if (Targetˉoffset != Result.Length)
        {
            throw Invalidˉresponse("The segmented service-bundle request payload is incomplete.");
        }
        return Result.ToImmutableArray();
    }

    internal static ImmutableArray<byte> Verifyˉresponse(
        ImmutableArray<byte> fragment,
        ImmutableArray<Nativeˉserviceˉcode> services,
        Nativeˉpublicationˉplan plan,
        int segmentˉoffset,
        int requestˉbytes,
        ImmutableArray<byte> response)
    {
        Validateˉinput(fragment, services, plan);
        if (response.IsDefault || response.Length < RESPONSE_HEADER_BYTES)
        {
            throw Invalidˉresponse("The Windvale service-bundle segment response is truncated.");
        }

        var Span = response.AsSpan();
        if (BinaryPrimitives.ReadUInt32LittleEndian(Span) != RESPONSE_MAGIC ||
            BinaryPrimitives.ReadUInt32LittleEndian(Span[4..]) != FORMAT_VERSION ||
            BinaryPrimitives.ReadUInt32LittleEndian(Span[8..]) != (uint)response.Length)
        {
            throw Invalidˉresponse("The Windvale service-bundle segment envelope is invalid.");
        }
        var Status = BinaryPrimitives.ReadUInt32LittleEndian(Span[12..]);
        var Failureˉoffset = BinaryPrimitives.ReadUInt32LittleEndian(Span[16..]);
        if (Status != 0)
        {
            throw new Nativeˉbackendˉexception(
                "WVN4015",
                $"Windvale rejected the service-bundle segment with status {Status} " +
                    $"at offset {Failureˉoffset}.");
        }

        var Expectedˉplanˉbytes = checked(
            X64ˉnativeˉpublicationˉlayout.REQUEST_HEADER_BYTES +
            services.Length * X64ˉnativeˉpublicationˉlayout.SERVICE_RECORD_BYTES);
        var Expectedˉsegmentˉbytes = Math.Min(
            MAXIMUM_SEGMENT_BYTES,
            plan.Imageˉbytes - segmentˉoffset);
        if (Failureˉoffset != (uint)requestˉbytes ||
            BinaryPrimitives.ReadUInt32LittleEndian(Span[20..]) != (uint)Expectedˉplanˉbytes ||
            BinaryPrimitives.ReadUInt32LittleEndian(Span[24..]) != (uint)plan.Imageˉbytes ||
            BinaryPrimitives.ReadUInt32LittleEndian(Span[28..]) != (uint)segmentˉoffset ||
            BinaryPrimitives.ReadUInt32LittleEndian(Span[32..]) != (uint)Expectedˉsegmentˉbytes ||
            BinaryPrimitives.ReadUInt32LittleEndian(Span[36..]) != (uint)services.Length ||
            response.Length != checked(RESPONSE_HEADER_BYTES + Expectedˉsegmentˉbytes))
        {
            throw Invalidˉresponse("The Windvale service-bundle segment shape is inconsistent.");
        }

        var Segment = Span.Slice(RESPONSE_HEADER_BYTES, Expectedˉsegmentˉbytes);
        var Segmentˉend = checked(segmentˉoffset + Expectedˉsegmentˉbytes);
        var Verifiedˉbytes = 0;
        Verifyˉdataˉregion(
            Segment,
            segmentˉoffset,
            Segmentˉend,
            0,
            fragment.AsSpan(),
            ref Verifiedˉbytes);
        if (services.IsEmpty)
        {
            Verifyˉfillˉregion(
                Segment,
                segmentˉoffset,
                Segmentˉend,
                fragment.Length,
                plan.Imageˉbytes - fragment.Length,
                0,
                ref Verifiedˉbytes);
        }
        else
        {
            Verifyˉfillˉregion(
                Segment,
                segmentˉoffset,
                Segmentˉend,
                fragment.Length,
                plan.Placements[0].Offset - fragment.Length,
                0,
                ref Verifiedˉbytes);
            for (var Index = 0; Index < services.Length; Index++)
            {
                var Placement = plan.Placements[Index];
                Verifyˉdataˉregion(
                    Segment,
                    segmentˉoffset,
                    Segmentˉend,
                    Placement.Offset,
                    services[Index].Code.AsSpan(),
                    ref Verifiedˉbytes);
                if (Index + 1 < services.Length)
                {
                    var End = checked(Placement.Offset + Placement.Size);
                    Verifyˉfillˉregion(
                        Segment,
                        segmentˉoffset,
                        Segmentˉend,
                        End,
                        plan.Placements[Index + 1].Offset - End,
                        0x90,
                        ref Verifiedˉbytes);
                }
            }
        }
        if (Verifiedˉbytes != Segment.Length)
        {
            throw Invalidˉresponse("The Windvale service-bundle segment is incomplete.");
        }
        return Segment.ToArray().ToImmutableArray();
    }

    private static void Validateˉinput(
        ImmutableArray<byte> fragment,
        ImmutableArray<Nativeˉserviceˉcode> services,
        Nativeˉpublicationˉplan plan)
    {
        if (fragment.IsDefault ||
            services.IsDefault ||
            plan is null ||
            fragment.Length is < 1 or > Nativeˉcontract.MAXIMUM_CODE_BYTES ||
            services.Length > X64ˉnativeˉpublicationˉlayout.MAXIMUM_SERVICES ||
            plan.Fragmentˉbytes != fragment.Length ||
            plan.Placements.Length != services.Length ||
            services.Any(Service => Service is null || Service.Code.IsDefault))
        {
            throw new Nativeˉbackendˉexception(
                "WVN4015",
                "The segmented native service-bundle materialization input is inconsistent.");
        }
        var Cursor = Alignˉtoˉsixteen(fragment.Length);
        for (var Index = 0; Index < services.Length; Index++)
        {
            if (plan.Placements[Index].Service != services[Index].Service ||
                plan.Placements[Index].Offset != Cursor ||
                plan.Placements[Index].Size != services[Index].Code.Length ||
                services[Index].Code.IsEmpty)
            {
                throw new Nativeˉbackendˉexception(
                    "WVN4015",
                    "A segmented native service-bundle placement is inconsistent.");
            }
            Cursor = checked(plan.Placements[Index].Offset + plan.Placements[Index].Size);
            if (Index + 1 < services.Length)
            {
                Cursor = Alignˉtoˉsixteen(Cursor);
            }
        }
        if (plan.Imageˉbytes != Cursor ||
            plan.Imageˉbytes > X64ˉnativeˉpublicationˉlayout.MAXIMUM_IMAGE_BYTES)
        {
            throw new Nativeˉbackendˉexception(
                "WVN4015",
                "The segmented native service-bundle image extent is inconsistent.");
        }
    }

    private static int Alignˉtoˉsixteen(int value) => checked((value + 15) & ~15);

    private static int Overlapˉlength(
        int regionˉstart,
        int regionˉbytes,
        int segmentˉstart,
        int segmentˉend)
    {
        var Start = Math.Max(regionˉstart, segmentˉstart);
        var End = Math.Min(checked(regionˉstart + regionˉbytes), segmentˉend);
        return Math.Max(0, End - Start);
    }

    private static void Copyˉoverlap(
        ReadOnlySpan<byte> source,
        int imageˉstart,
        int segmentˉstart,
        int segmentˉend,
        Span<byte> target,
        ref int targetˉoffset)
    {
        var Start = Math.Max(imageˉstart, segmentˉstart);
        var End = Math.Min(checked(imageˉstart + source.Length), segmentˉend);
        if (Start >= End)
        {
            return;
        }
        var Count = End - Start;
        source.Slice(Start - imageˉstart, Count).CopyTo(target[targetˉoffset..]);
        targetˉoffset = checked(targetˉoffset + Count);
    }

    private static void Verifyˉdataˉregion(
        ReadOnlySpan<byte> segment,
        int segmentˉstart,
        int segmentˉend,
        int imageˉstart,
        ReadOnlySpan<byte> expected,
        ref int verifiedˉbytes)
    {
        var Start = Math.Max(imageˉstart, segmentˉstart);
        var End = Math.Min(checked(imageˉstart + expected.Length), segmentˉend);
        if (Start >= End)
        {
            return;
        }
        var Count = End - Start;
        if (Start != checked(segmentˉstart + verifiedˉbytes) ||
            !segment.Slice(verifiedˉbytes, Count).SequenceEqual(
                expected.Slice(Start - imageˉstart, Count)))
        {
            throw Invalidˉresponse("A Windvale service-bundle source region changed.");
        }
        verifiedˉbytes = checked(verifiedˉbytes + Count);
    }

    private static void Verifyˉfillˉregion(
        ReadOnlySpan<byte> segment,
        int segmentˉstart,
        int segmentˉend,
        int imageˉstart,
        int imageˉbytes,
        byte value,
        ref int verifiedˉbytes)
    {
        var Start = Math.Max(imageˉstart, segmentˉstart);
        var End = Math.Min(checked(imageˉstart + imageˉbytes), segmentˉend);
        if (Start >= End)
        {
            return;
        }
        var Count = End - Start;
        if (Start != checked(segmentˉstart + verifiedˉbytes))
        {
            throw Invalidˉresponse("A Windvale service-bundle fill region is misplaced.");
        }
        for (var Index = verifiedˉbytes; Index < verifiedˉbytes + Count; Index++)
        {
            if (segment[Index] != value)
            {
                throw Invalidˉresponse("A Windvale service-bundle fill byte is invalid.");
            }
        }
        verifiedˉbytes = checked(verifiedˉbytes + Count);
    }

    private static Nativeˉbackendˉexception Invalidˉresponse(string message) =>
        new("WVN4016", message);
}
