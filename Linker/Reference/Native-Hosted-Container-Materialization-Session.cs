using System.Buffers.Binary;
using System.Collections.Immutable;
using Windvale.Bytecode;

namespace Windvale.Linker;

internal static class Nativeˉhostedˉcontainerˉmaterializationˉsession
{
    private readonly record struct Sourceˉregion(int Offset, ImmutableArray<byte> Bytes);

    private const uint REQUEST_MAGIC = 0x5448_5657;
    private const uint RESPONSE_MAGIC = 0x5548_5657;
    private const uint FORMAT_VERSION = 1;
    private const int REQUEST_HEADER_BYTES = 32;
    private const int PLAN_HEADER_BYTES = 128;
    private const int RESPONSE_HEADER_BYTES = 40;
    internal const int MAXIMUM_SEGMENT_BYTES =
        Bytecodeˉlimits.MAX_BYTE_DATA_BYTES - REQUEST_HEADER_BYTES - PLAN_HEADER_BYTES;

    internal static ImmutableArray<byte> Build(
        ImmutableArray<byte> plan,
        ImmutableArray<byte> header,
        ImmutableArray<byte> startup,
        ImmutableArray<byte> bundle,
        ImmutableArray<byte> imports,
        ImmutableArray<byte> runtime,
        ImmutableArray<byte> relocation)
    {
        var (Applicationˉbytes, Regions) = Prepare(
            plan, header, startup, bundle, imports, runtime, relocation);
        var Result = ImmutableArray.CreateBuilder<byte>(Applicationˉbytes);
        for (var Offset = 0; Offset < Applicationˉbytes; Offset += MAXIMUM_SEGMENT_BYTES)
        {
            var Request = Buildˉrequest(plan, Regions, Applicationˉbytes, Offset);
            var Response = Nativeˉhostedˉcontainerˉsegmentˉconstructor.Execute(Request);
            Result.AddRange(Verifyˉresponse(
                Regions,
                Applicationˉbytes,
                Offset,
                Request.Length,
                Response));
        }
        if (Result.Count != Applicationˉbytes)
        {
            throw Invalid("The segmented Windvale hosted container is incomplete.");
        }
        return Result.MoveToImmutable();
    }

    internal static ImmutableArray<ImmutableArray<byte>> Buildˉrequests(
        ImmutableArray<byte> plan,
        ImmutableArray<byte> header,
        ImmutableArray<byte> startup,
        ImmutableArray<byte> bundle,
        ImmutableArray<byte> imports,
        ImmutableArray<byte> runtime,
        ImmutableArray<byte> relocation)
    {
        var (Applicationˉbytes, Regions) = Prepare(
            plan, header, startup, bundle, imports, runtime, relocation);
        var Result = ImmutableArray.CreateBuilder<ImmutableArray<byte>>();
        for (var Offset = 0; Offset < Applicationˉbytes; Offset += MAXIMUM_SEGMENT_BYTES)
        {
            Result.Add(Buildˉrequest(plan, Regions, Applicationˉbytes, Offset));
        }
        return Result.ToImmutable();
    }

    private static ImmutableArray<byte> Buildˉrequest(
        ImmutableArray<byte> plan,
        ImmutableArray<Sourceˉregion> regions,
        int applicationˉbytes,
        int segmentˉoffset)
    {
        if (segmentˉoffset < 0 || segmentˉoffset >= applicationˉbytes ||
            segmentˉoffset % MAXIMUM_SEGMENT_BYTES != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(segmentˉoffset));
        }
        var Segmentˉbytes = Math.Min(
            MAXIMUM_SEGMENT_BYTES,
            applicationˉbytes - segmentˉoffset);
        var Segmentˉend = checked(segmentˉoffset + Segmentˉbytes);
        var Payloadˉbytes = 0;
        foreach (var Region in regions)
        {
            Payloadˉbytes = checked(Payloadˉbytes + Overlapˉlength(
                Region.Offset,
                Region.Bytes.Length,
                segmentˉoffset,
                Segmentˉend));
        }
        var Totalˉbytes = checked(
            REQUEST_HEADER_BYTES + PLAN_HEADER_BYTES + Payloadˉbytes);
        if (Totalˉbytes > Bytecodeˉlimits.MAX_BYTE_DATA_BYTES)
        {
            throw Invalid("A hosted-container segment request exceeds the byte-value limit.");
        }

        var Result = new byte[Totalˉbytes];
        BinaryPrimitives.WriteUInt32LittleEndian(Result, REQUEST_MAGIC);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(4), FORMAT_VERSION);
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(8), checked((uint)Totalˉbytes));
        BinaryPrimitives.WriteUInt32LittleEndian(Result.AsSpan(12), PLAN_HEADER_BYTES);
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(16), checked((uint)segmentˉoffset));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(20), checked((uint)Segmentˉbytes));
        BinaryPrimitives.WriteUInt32LittleEndian(
            Result.AsSpan(24), checked((uint)Payloadˉbytes));
        plan.AsSpan(0, PLAN_HEADER_BYTES).CopyTo(Result.AsSpan(REQUEST_HEADER_BYTES));
        var Targetˉoffset = REQUEST_HEADER_BYTES + PLAN_HEADER_BYTES;
        foreach (var Region in regions)
        {
            Copyˉoverlap(
                Region.Bytes.AsSpan(),
                Region.Offset,
                segmentˉoffset,
                Segmentˉend,
                Result,
                ref Targetˉoffset);
        }
        if (Targetˉoffset != Result.Length)
        {
            throw Invalid("A hosted-container segment request payload is incomplete.");
        }
        return Result.ToImmutableArray();
    }

    internal static ImmutableArray<byte> Verifyˉresponse(
        ImmutableArray<byte> plan,
        ImmutableArray<byte> header,
        ImmutableArray<byte> startup,
        ImmutableArray<byte> bundle,
        ImmutableArray<byte> imports,
        ImmutableArray<byte> runtime,
        ImmutableArray<byte> relocation,
        int segmentˉoffset,
        int requestˉbytes,
        ImmutableArray<byte> response)
    {
        var (Applicationˉbytes, Regions) = Prepare(
            plan, header, startup, bundle, imports, runtime, relocation);
        return Verifyˉresponse(
            Regions,
            Applicationˉbytes,
            segmentˉoffset,
            requestˉbytes,
            response);
    }

    private static ImmutableArray<byte> Verifyˉresponse(
        ImmutableArray<Sourceˉregion> regions,
        int applicationˉbytes,
        int segmentˉoffset,
        int requestˉbytes,
        ImmutableArray<byte> response)
    {
        if (response.IsDefault || response.Length < RESPONSE_HEADER_BYTES)
        {
            throw Invalid("The Windvale hosted-container segment response is truncated.");
        }
        var Span = response.AsSpan();
        uint Read(int offset) =>
            BinaryPrimitives.ReadUInt32LittleEndian(response.AsSpan()[offset..]);
        if (Read(0) != RESPONSE_MAGIC || Read(4) != FORMAT_VERSION ||
            Read(8) != response.Length)
        {
            throw Invalid("The Windvale hosted-container segment envelope is invalid.");
        }
        if (Read(12) != 0)
        {
            throw Invalid(
                $"Windvale rejected hosted-container segment status {Read(12)} " +
                $"at offset {Read(16)}.");
        }
        var Segmentˉbytes = Math.Min(
            MAXIMUM_SEGMENT_BYTES,
            applicationˉbytes - segmentˉoffset);
        if (Read(16) != requestˉbytes || Read(20) != PLAN_HEADER_BYTES ||
            Read(24) != applicationˉbytes || Read(28) != segmentˉoffset ||
            Read(32) != Segmentˉbytes || Read(36) != 6 ||
            response.Length != RESPONSE_HEADER_BYTES + Segmentˉbytes)
        {
            throw Invalid("The Windvale hosted-container segment shape is inconsistent.");
        }

        var Segment = Span.Slice(RESPONSE_HEADER_BYTES, Segmentˉbytes);
        var Segmentˉend = checked(segmentˉoffset + Segmentˉbytes);
        var Verifiedˉbytes = 0;
        var Imageˉcursor = 0;
        foreach (var Region in regions)
        {
            Verifyˉfill(
                Segment,
                segmentˉoffset,
                Segmentˉend,
                Imageˉcursor,
                Region.Offset - Imageˉcursor,
                ref Verifiedˉbytes);
            Verifyˉdata(
                Segment,
                segmentˉoffset,
                Segmentˉend,
                Region.Offset,
                Region.Bytes.AsSpan(),
                ref Verifiedˉbytes);
            Imageˉcursor = checked(Region.Offset + Region.Bytes.Length);
        }
        Verifyˉfill(
            Segment,
            segmentˉoffset,
            Segmentˉend,
            Imageˉcursor,
            applicationˉbytes - Imageˉcursor,
            ref Verifiedˉbytes);
        if (Verifiedˉbytes != Segment.Length)
        {
            throw Invalid("The Windvale hosted-container segment is incomplete.");
        }
        return Segment.ToArray().ToImmutableArray();
    }

    private static (int Applicationˉbytes, ImmutableArray<Sourceˉregion> Regions) Prepare(
        ImmutableArray<byte> plan,
        ImmutableArray<byte> header,
        ImmutableArray<byte> startup,
        ImmutableArray<byte> bundle,
        ImmutableArray<byte> imports,
        ImmutableArray<byte> runtime,
        ImmutableArray<byte> relocation)
    {
        if (plan.IsDefault || plan.Length < PLAN_HEADER_BYTES || header.IsDefault ||
            startup.IsDefault || bundle.IsDefaultOrEmpty || imports.IsDefault ||
            runtime.IsDefault || relocation.IsDefault)
        {
            throw Invalid("The hosted-container segmentation inputs are invalid.");
        }
        uint Read(int offset) =>
            BinaryPrimitives.ReadUInt32LittleEndian(plan.AsSpan()[offset..]);
        var Applicationˉbytes = checked((int)Read(28));
        if (Read(0) != 0x4443_5657 || Read(4) != 1 || Read(8) != plan.Length ||
            Read(12) != 0 || Applicationˉbytes < bundle.Length ||
            Applicationˉbytes > checked(bundle.Length + 16_384) ||
            Read(36) != header.Length || Read(44) != startup.Length ||
            Read(52) != bundle.Length || Read(60) != imports.Length ||
            Read(68) != runtime.Length || Read(76) != relocation.Length)
        {
            throw Invalid("The hosted-container segmentation plan is inconsistent.");
        }
        var Builder = ImmutableArray.CreateBuilder<Sourceˉregion>(6);
        Addˉregion(Builder, 0, header);
        Addˉregion(Builder, checked((int)Read(40)), startup);
        Addˉregion(Builder, checked((int)Read(48)), bundle);
        Addˉregion(Builder, checked((int)Read(56)), imports);
        Addˉregion(Builder, checked((int)Read(64)), runtime);
        Addˉregion(Builder, checked((int)Read(72)), relocation);
        var Regions = Builder.ToImmutable();
        var Cursor = 0;
        foreach (var Region in Regions)
        {
            if (Region.Offset < Cursor ||
                (long)Region.Offset + Region.Bytes.Length > Applicationˉbytes)
            {
                throw Invalid("A hosted-container segmentation region is inconsistent.");
            }
            Cursor = checked(Region.Offset + Region.Bytes.Length);
        }
        return (Applicationˉbytes, Regions);
    }

    private static void Addˉregion(
        ImmutableArray<Sourceˉregion>.Builder regions,
        int offset,
        ImmutableArray<byte> bytes)
    {
        if (!bytes.IsEmpty)
        {
            regions.Add(new(offset, bytes));
        }
    }

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
        if (Start >= End) { return; }
        var Count = End - Start;
        source.Slice(Start - imageˉstart, Count).CopyTo(target[targetˉoffset..]);
        targetˉoffset = checked(targetˉoffset + Count);
    }

    private static void Verifyˉdata(
        ReadOnlySpan<byte> segment,
        int segmentˉstart,
        int segmentˉend,
        int imageˉstart,
        ReadOnlySpan<byte> expected,
        ref int verifiedˉbytes)
    {
        var Start = Math.Max(imageˉstart, segmentˉstart);
        var End = Math.Min(checked(imageˉstart + expected.Length), segmentˉend);
        if (Start >= End) { return; }
        var Count = End - Start;
        if (Start != segmentˉstart + verifiedˉbytes ||
            !segment.Slice(verifiedˉbytes, Count).SequenceEqual(
                expected.Slice(Start - imageˉstart, Count)))
        {
            throw Invalid("A Windvale hosted-container source region changed.");
        }
        verifiedˉbytes = checked(verifiedˉbytes + Count);
    }

    private static void Verifyˉfill(
        ReadOnlySpan<byte> segment,
        int segmentˉstart,
        int segmentˉend,
        int imageˉstart,
        int imageˉbytes,
        ref int verifiedˉbytes)
    {
        var Start = Math.Max(imageˉstart, segmentˉstart);
        var End = Math.Min(checked(imageˉstart + imageˉbytes), segmentˉend);
        if (Start >= End) { return; }
        var Count = End - Start;
        if (Start != segmentˉstart + verifiedˉbytes)
        {
            throw Invalid("A Windvale hosted-container fill region is misplaced.");
        }
        for (var Index = verifiedˉbytes; Index < verifiedˉbytes + Count; Index++)
        {
            if (segment[Index] != 0)
            {
                throw Invalid("A Windvale hosted-container fill byte is invalid.");
            }
        }
        verifiedˉbytes = checked(verifiedˉbytes + Count);
    }

    private static InvalidOperationException Invalid(string message) => new(message);
}
