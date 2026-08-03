using System.Buffers.Binary;
using Windvale.Bytecode;
using Windvale.Compiler.Native;

namespace Windvale.Runtime.Native;

public static class Nativeˉdescriptorˉallocatorˉreference
{
    public static Nativeˉdescriptorˉallocatorˉstatus Execute(
        Span<byte> state,
        Span<byte> request,
        Span<byte> arena,
        ulong arenaˉaddress)
    {
        if (state.Length != Nativeˉdescriptorˉallocatorˉcontract.STATE_BYTES ||
            request.Length != Nativeˉdescriptorˉallocatorˉcontract.REQUEST_BYTES ||
            arena.Length is < Nativeˉdescriptorˉallocatorˉcontract.BLOCK_HEADER_BYTES or
                > Nativeˉcontract.MAXIMUM_TEXT_ARENA_BYTES ||
            (arena.Length & (Nativeˉdescriptorˉallocatorˉcontract.BLOCK_HEADER_BYTES - 1)) != 0)
        {
            return Setˉstatus(request, Nativeˉdescriptorˉallocatorˉstatus.Invalidˉrequest);
        }

        if (Readˉu32(request, Nativeˉdescriptorˉallocatorˉcontract.REQUEST_FORMAT_VERSION_OFFSET) !=
                Nativeˉdescriptorˉallocatorˉcontract.FORMAT_VERSION ||
            Readˉu32(request, Nativeˉdescriptorˉallocatorˉcontract.REQUEST_SIZE_OFFSET) !=
                Nativeˉdescriptorˉallocatorˉcontract.REQUEST_BYTES ||
            Readˉu32(request, Nativeˉdescriptorˉallocatorˉcontract.REQUEST_RESERVED_OFFSET) != 0 ||
            Readˉu32(request, Nativeˉdescriptorˉallocatorˉcontract.REQUEST_STATUS_OFFSET) != 0 ||
            Readˉu64(request, Nativeˉdescriptorˉallocatorˉcontract.REQUEST_DATA_POINTER_OFFSET) != 0 ||
            Readˉu32(request, Nativeˉdescriptorˉallocatorˉcontract.REQUEST_CHARGED_BYTES_OFFSET) != 0)
        {
            return Setˉstatus(request, Nativeˉdescriptorˉallocatorˉstatus.Invalidˉrequest);
        }

        var Operationˉvalue = Readˉu32(
            request,
            Nativeˉdescriptorˉallocatorˉcontract.REQUEST_OPERATION_OFFSET);
        if (!Enum.IsDefined(typeof(Nativeˉdescriptorˉallocatorˉoperation), Operationˉvalue))
        {
            return Setˉstatus(request, Nativeˉdescriptorˉallocatorˉstatus.Invalidˉrequest);
        }
        var Operation = (Nativeˉdescriptorˉallocatorˉoperation)Operationˉvalue;
        var Payload = Readˉu32(
            request,
            Nativeˉdescriptorˉallocatorˉcontract.REQUEST_PAYLOAD_BYTES_OFFSET);
        var Owner = Readˉu32(
            request,
            Nativeˉdescriptorˉallocatorˉcontract.REQUEST_OWNER_TOKEN_OFFSET);
        if ((Operation == Nativeˉdescriptorˉallocatorˉoperation.Acquire && Owner != 0) ||
            (Operation != Nativeˉdescriptorˉallocatorˉoperation.Acquire && Payload != 0) ||
            (Operation == Nativeˉdescriptorˉallocatorˉoperation.Acquire &&
                Payload > Bytecodeˉlimits.MAX_BYTE_DATA_BYTES))
        {
            return Setˉstatus(request, Nativeˉdescriptorˉallocatorˉstatus.Invalidˉrequest);
        }

        Writeˉu32(request, Nativeˉdescriptorˉallocatorˉcontract.REQUEST_STATUS_OFFSET, 0);
        Writeˉu64(request, Nativeˉdescriptorˉallocatorˉcontract.REQUEST_DATA_POINTER_OFFSET, 0);
        Writeˉu32(request, Nativeˉdescriptorˉallocatorˉcontract.REQUEST_CHARGED_BYTES_OFFSET, 0);

        if (Readˉu64(state, Nativeˉdescriptorˉallocatorˉcontract.STATE_ARENA_POINTER_OFFSET) !=
                arenaˉaddress ||
            arenaˉaddress == 0 ||
            (arenaˉaddress & 15) != 0 ||
            Readˉu32(state, Nativeˉdescriptorˉallocatorˉcontract.STATE_ARENA_LENGTH_OFFSET) !=
                arena.Length ||
            Readˉu32(state, Nativeˉdescriptorˉallocatorˉcontract.STATE_RESERVED_OFFSET) != 0)
        {
            return Setˉstatus(request, Nativeˉdescriptorˉallocatorˉstatus.Corruptˉstate);
        }

        var Stateˉmagic = Readˉu32(
            state,
            Nativeˉdescriptorˉallocatorˉcontract.STATE_MAGIC_OFFSET);
        if (Stateˉmagic == 0)
        {
            if (Readˉu32(state, Nativeˉdescriptorˉallocatorˉcontract.STATE_FREE_HEAD_OFFSET) != 0 ||
                Readˉu32(
                    state,
                    Nativeˉdescriptorˉallocatorˉcontract.STATE_ALLOCATED_BLOCKS_OFFSET) != 0 ||
                Readˉu32(
                    state,
                    Nativeˉdescriptorˉallocatorˉcontract.STATE_CHARGED_BYTES_OFFSET) != 0)
            {
                return Setˉstatus(request, Nativeˉdescriptorˉallocatorˉstatus.Corruptˉstate);
            }
            Writeˉheader(
                arena,
                0,
                checked((uint)arena.Length),
                references: 0,
                next: 0,
                Nativeˉdescriptorˉallocatorˉcontract.FREE_BLOCK_MAGIC);
            Writeˉu32(
                state,
                Nativeˉdescriptorˉallocatorˉcontract.STATE_MAGIC_OFFSET,
                Nativeˉdescriptorˉallocatorˉcontract.STATE_MAGIC);
            Writeˉu32(state, Nativeˉdescriptorˉallocatorˉcontract.STATE_FREE_HEAD_OFFSET, 1);
        }
        else if (Stateˉmagic != Nativeˉdescriptorˉallocatorˉcontract.STATE_MAGIC ||
            Readˉu32(
                state,
                Nativeˉdescriptorˉallocatorˉcontract.STATE_ALLOCATED_BLOCKS_OFFSET) >
                    Nativeˉdescriptorˉallocatorˉcontract.MAXIMUM_BLOCKS ||
            Readˉu32(state, Nativeˉdescriptorˉallocatorˉcontract.STATE_CHARGED_BYTES_OFFSET) >
                arena.Length ||
            (Readˉu32(
                state,
                Nativeˉdescriptorˉallocatorˉcontract.STATE_CHARGED_BYTES_OFFSET) & 15) != 0)
        {
            return Setˉstatus(request, Nativeˉdescriptorˉallocatorˉstatus.Corruptˉstate);
        }

        return Operation switch
        {
            Nativeˉdescriptorˉallocatorˉoperation.Acquire =>
                Acquire(state, request, arena, arenaˉaddress, Payload),
            Nativeˉdescriptorˉallocatorˉoperation.Retain =>
                Retain(request, arena, Owner),
            Nativeˉdescriptorˉallocatorˉoperation.Release =>
                Release(state, request, arena, Owner),
            _ => Setˉstatus(request, Nativeˉdescriptorˉallocatorˉstatus.Invalidˉrequest),
        };
    }

    private static Nativeˉdescriptorˉallocatorˉstatus Acquire(
        Span<byte> state,
        Span<byte> request,
        Span<byte> arena,
        ulong arenaˉaddress,
        uint payload)
    {
        var Chargedˉwide = checked(
            (ulong)payload + Nativeˉdescriptorˉallocatorˉcontract.BLOCK_HEADER_BYTES + 15UL);
        var Charged = checked((uint)(Chargedˉwide & ~15UL));
        Writeˉu32(
            request,
            Nativeˉdescriptorˉallocatorˉcontract.REQUEST_CHARGED_BYTES_OFFSET,
            Charged);
        var Blocks = Readˉu32(
            state,
            Nativeˉdescriptorˉallocatorˉcontract.STATE_ALLOCATED_BLOCKS_OFFSET);
        var Used = Readˉu32(
            state,
            Nativeˉdescriptorˉallocatorˉcontract.STATE_CHARGED_BYTES_OFFSET);
        if (Blocks >= Nativeˉdescriptorˉallocatorˉcontract.MAXIMUM_BLOCKS ||
            (ulong)Used + Charged > (uint)arena.Length)
        {
            return Setˉstatus(request, Nativeˉdescriptorˉallocatorˉstatus.Exhausted);
        }

        uint Previous = 0;
        var Current = Readˉu32(
            state,
            Nativeˉdescriptorˉallocatorˉcontract.STATE_FREE_HEAD_OFFSET);
        while (Current != 0)
        {
            if (!Tryˉfreeˉnode(arena, Current, out var Offset, out var Size, out var Next))
            {
                return Setˉstatus(request, Nativeˉdescriptorˉallocatorˉstatus.Corruptˉstate);
            }
            if (Next != 0 && Next <= Current)
            {
                return Setˉstatus(request, Nativeˉdescriptorˉallocatorˉstatus.Corruptˉstate);
            }
            if (Size >= Charged)
            {
                var Remainder = Size - Charged;
                var Replacement = Next;
                if (Remainder != 0)
                {
                    if (Remainder < Nativeˉdescriptorˉallocatorˉcontract.BLOCK_HEADER_BYTES)
                    {
                        return Setˉstatus(
                            request,
                            Nativeˉdescriptorˉallocatorˉstatus.Corruptˉstate);
                    }
                    var Splitˉoffset = checked(Offset + Charged);
                    Replacement = checked(Splitˉoffset + 1);
                    Writeˉheader(
                        arena,
                        Splitˉoffset,
                        Remainder,
                        references: 0,
                        Next,
                        Nativeˉdescriptorˉallocatorˉcontract.FREE_BLOCK_MAGIC);
                }
                Writeˉfreeˉlink(state, arena, Previous, Replacement);
                Writeˉheader(
                    arena,
                    Offset,
                    Charged,
                    references: 1,
                    next: 0,
                    Nativeˉdescriptorˉallocatorˉcontract.ALLOCATED_BLOCK_MAGIC);
                Writeˉu32(
                    state,
                    Nativeˉdescriptorˉallocatorˉcontract.STATE_ALLOCATED_BLOCKS_OFFSET,
                    checked(Blocks + 1));
                Writeˉu32(
                    state,
                    Nativeˉdescriptorˉallocatorˉcontract.STATE_CHARGED_BYTES_OFFSET,
                    checked(Used + Charged));
                Writeˉu32(
                    request,
                    Nativeˉdescriptorˉallocatorˉcontract.REQUEST_OWNER_TOKEN_OFFSET,
                    Current);
                Writeˉu64(
                    request,
                    Nativeˉdescriptorˉallocatorˉcontract.REQUEST_DATA_POINTER_OFFSET,
                    checked(arenaˉaddress + Offset +
                        Nativeˉdescriptorˉallocatorˉcontract.BLOCK_HEADER_BYTES));
                return Setˉstatus(request, Nativeˉdescriptorˉallocatorˉstatus.Success);
            }
            Previous = Current;
            Current = Next;
        }
        return Setˉstatus(request, Nativeˉdescriptorˉallocatorˉstatus.Exhausted);
    }

    private static Nativeˉdescriptorˉallocatorˉstatus Retain(
        Span<byte> request,
        Span<byte> arena,
        uint owner)
    {
        if (owner == 0)
        {
            return Setˉstatus(request, Nativeˉdescriptorˉallocatorˉstatus.Success);
        }
        if (!Tryˉallocatedˉnode(arena, owner, out var Offset, out _, out var References))
        {
            return Setˉstatus(request, Nativeˉdescriptorˉallocatorˉstatus.Invalidˉowner);
        }
        if (References == uint.MaxValue)
        {
            return Setˉstatus(
                request,
                Nativeˉdescriptorˉallocatorˉstatus.Referenceˉoverflow);
        }
        Writeˉu32(
            arena,
            checked((int)Offset + Nativeˉdescriptorˉallocatorˉcontract.BLOCK_REFERENCE_COUNT_OFFSET),
            References + 1);
        return Setˉstatus(request, Nativeˉdescriptorˉallocatorˉstatus.Success);
    }

    private static Nativeˉdescriptorˉallocatorˉstatus Release(
        Span<byte> state,
        Span<byte> request,
        Span<byte> arena,
        uint owner)
    {
        if (owner == 0)
        {
            return Setˉstatus(request, Nativeˉdescriptorˉallocatorˉstatus.Success);
        }
        if (!Tryˉallocatedˉnode(arena, owner, out var Offset, out var Size, out var References))
        {
            return Setˉstatus(request, Nativeˉdescriptorˉallocatorˉstatus.Invalidˉowner);
        }
        Writeˉu32(
            request,
            Nativeˉdescriptorˉallocatorˉcontract.REQUEST_CHARGED_BYTES_OFFSET,
            Size);
        if (References > 1)
        {
            Writeˉu32(
                arena,
                checked((int)Offset +
                    Nativeˉdescriptorˉallocatorˉcontract.BLOCK_REFERENCE_COUNT_OFFSET),
                References - 1);
            return Setˉstatus(request, Nativeˉdescriptorˉallocatorˉstatus.Success);
        }

        var Blocks = Readˉu32(
            state,
            Nativeˉdescriptorˉallocatorˉcontract.STATE_ALLOCATED_BLOCKS_OFFSET);
        var Used = Readˉu32(
            state,
            Nativeˉdescriptorˉallocatorˉcontract.STATE_CHARGED_BYTES_OFFSET);
        if (Blocks == 0 || Used < Size)
        {
            return Setˉstatus(request, Nativeˉdescriptorˉallocatorˉstatus.Corruptˉstate);
        }

        uint Previous = 0;
        var Current = Readˉu32(
            state,
            Nativeˉdescriptorˉallocatorˉcontract.STATE_FREE_HEAD_OFFSET);
        while (Current != 0 && Current < owner)
        {
            if (!Tryˉfreeˉnode(arena, Current, out _, out _, out var Next) ||
                (Next != 0 && Next <= Current))
            {
                return Setˉstatus(request, Nativeˉdescriptorˉallocatorˉstatus.Corruptˉstate);
            }
            Previous = Current;
            Current = Next;
        }
        if (Current == owner)
        {
            return Setˉstatus(request, Nativeˉdescriptorˉallocatorˉstatus.Corruptˉstate);
        }
        if (Current != 0)
        {
            if (!Tryˉfreeˉnode(arena, Current, out _, out _, out var Currentˉnext) ||
                (Currentˉnext != 0 && Currentˉnext <= Current))
            {
                return Setˉstatus(request, Nativeˉdescriptorˉallocatorˉstatus.Corruptˉstate);
            }
        }

        var End = checked((ulong)Offset + Size);
        var Previousˉadjacent = false;
        if (Previous != 0)
        {
            if (!Tryˉfreeˉnode(
                    arena,
                    Previous,
                    out var Previousˉoffset,
                    out var Previousˉsize,
                    out _) ||
                (ulong)Previousˉoffset + Previousˉsize > Offset)
            {
                return Setˉstatus(request, Nativeˉdescriptorˉallocatorˉstatus.Corruptˉstate);
            }
            Previousˉadjacent = (ulong)Previousˉoffset + Previousˉsize == Offset;
        }
        var Successorˉadjacent = false;
        if (Current != 0)
        {
            var Currentˉoffset = checked(Current - 1);
            if (End > Currentˉoffset)
            {
                return Setˉstatus(request, Nativeˉdescriptorˉallocatorˉstatus.Corruptˉstate);
            }
            Successorˉadjacent = End == Currentˉoffset;
        }

        if (Previousˉadjacent)
        {
            _ = Tryˉfreeˉnode(
                arena,
                Previous,
                out var Previousˉoffset,
                out var Previousˉsize,
                out _);
            var Combined = checked(Previousˉsize + Size);
            var Next = Current;
            if (Successorˉadjacent)
            {
                _ = Tryˉfreeˉnode(
                    arena,
                    Current,
                    out _,
                    out var Currentˉsize,
                    out var Currentˉnext);
                Combined = checked(Combined + Currentˉsize);
                Next = Currentˉnext;
            }
            Writeˉu32(arena, checked((int)Previousˉoffset), Combined);
            Writeˉu32(
                arena,
                checked((int)Previousˉoffset +
                    Nativeˉdescriptorˉallocatorˉcontract.BLOCK_NEXT_FREE_OFFSET),
                Next);
            Writeˉheader(
                arena,
                Offset,
                Size,
                references: 0,
                next: 0,
                Nativeˉdescriptorˉallocatorˉcontract.FREE_BLOCK_MAGIC);
        }
        else
        {
            var Combined = Size;
            var Next = Current;
            if (Successorˉadjacent)
            {
                _ = Tryˉfreeˉnode(
                    arena,
                    Current,
                    out _,
                    out var Currentˉsize,
                    out var Currentˉnext);
                Combined = checked(Combined + Currentˉsize);
                Next = Currentˉnext;
            }
            Writeˉheader(
                arena,
                Offset,
                Combined,
                references: 0,
                Next,
                Nativeˉdescriptorˉallocatorˉcontract.FREE_BLOCK_MAGIC);
            Writeˉfreeˉlink(state, arena, Previous, owner);
        }

        Writeˉu32(
            state,
            Nativeˉdescriptorˉallocatorˉcontract.STATE_ALLOCATED_BLOCKS_OFFSET,
            Blocks - 1);
        Writeˉu32(
            state,
            Nativeˉdescriptorˉallocatorˉcontract.STATE_CHARGED_BYTES_OFFSET,
            Used - Size);
        return Setˉstatus(request, Nativeˉdescriptorˉallocatorˉstatus.Success);
    }

    private static bool Tryˉfreeˉnode(
        ReadOnlySpan<byte> arena,
        uint token,
        out uint offset,
        out uint size,
        out uint next)
    {
        offset = 0;
        size = 0;
        next = 0;
        if (!Tryˉheader(arena, token, out offset, out size) ||
            Readˉu32(
                arena,
                checked((int)offset +
                    Nativeˉdescriptorˉallocatorˉcontract.BLOCK_REFERENCE_COUNT_OFFSET)) != 0 ||
            Readˉu32(
                arena,
                checked((int)offset + Nativeˉdescriptorˉallocatorˉcontract.BLOCK_MAGIC_OFFSET)) !=
                    Nativeˉdescriptorˉallocatorˉcontract.FREE_BLOCK_MAGIC)
        {
            return false;
        }
        next = Readˉu32(
            arena,
            checked((int)offset + Nativeˉdescriptorˉallocatorˉcontract.BLOCK_NEXT_FREE_OFFSET));
        return true;
    }

    private static bool Tryˉallocatedˉnode(
        ReadOnlySpan<byte> arena,
        uint token,
        out uint offset,
        out uint size,
        out uint references)
    {
        references = 0;
        if (!Tryˉheader(arena, token, out offset, out size) ||
            Readˉu32(
                arena,
                checked((int)offset + Nativeˉdescriptorˉallocatorˉcontract.BLOCK_MAGIC_OFFSET)) !=
                    Nativeˉdescriptorˉallocatorˉcontract.ALLOCATED_BLOCK_MAGIC)
        {
            return false;
        }
        references = Readˉu32(
            arena,
            checked((int)offset +
                Nativeˉdescriptorˉallocatorˉcontract.BLOCK_REFERENCE_COUNT_OFFSET));
        return references != 0;
    }

    private static bool Tryˉheader(
        ReadOnlySpan<byte> arena,
        uint token,
        out uint offset,
        out uint size)
    {
        offset = 0;
        size = 0;
        if (token == 0)
        {
            return false;
        }
        offset = token - 1;
        if ((offset & 15) != 0 ||
            (ulong)offset + Nativeˉdescriptorˉallocatorˉcontract.BLOCK_HEADER_BYTES >
                (uint)arena.Length)
        {
            return false;
        }
        size = Readˉu32(arena, checked((int)offset));
        return size >= Nativeˉdescriptorˉallocatorˉcontract.BLOCK_HEADER_BYTES &&
            (size & 15) == 0 &&
            (ulong)offset + size <= (uint)arena.Length;
    }

    private static void Writeˉfreeˉlink(
        Span<byte> state,
        Span<byte> arena,
        uint previous,
        uint value)
    {
        if (previous == 0)
        {
            Writeˉu32(state, Nativeˉdescriptorˉallocatorˉcontract.STATE_FREE_HEAD_OFFSET, value);
            return;
        }
        var Offset = checked(previous - 1);
        Writeˉu32(
            arena,
            checked((int)Offset + Nativeˉdescriptorˉallocatorˉcontract.BLOCK_NEXT_FREE_OFFSET),
            value);
    }

    private static void Writeˉheader(
        Span<byte> arena,
        uint offset,
        uint size,
        uint references,
        uint next,
        uint magic)
    {
        var Start = checked((int)offset);
        Writeˉu32(arena, Start + Nativeˉdescriptorˉallocatorˉcontract.BLOCK_CHARGED_BYTES_OFFSET, size);
        Writeˉu32(
            arena,
            Start + Nativeˉdescriptorˉallocatorˉcontract.BLOCK_REFERENCE_COUNT_OFFSET,
            references);
        Writeˉu32(arena, Start + Nativeˉdescriptorˉallocatorˉcontract.BLOCK_NEXT_FREE_OFFSET, next);
        Writeˉu32(arena, Start + Nativeˉdescriptorˉallocatorˉcontract.BLOCK_MAGIC_OFFSET, magic);
    }

    private static Nativeˉdescriptorˉallocatorˉstatus Setˉstatus(
        Span<byte> request,
        Nativeˉdescriptorˉallocatorˉstatus status)
    {
        if (request.Length >=
            Nativeˉdescriptorˉallocatorˉcontract.REQUEST_STATUS_OFFSET + sizeof(uint))
        {
            Writeˉu32(
                request,
                Nativeˉdescriptorˉallocatorˉcontract.REQUEST_STATUS_OFFSET,
                (uint)status);
        }
        return status;
    }

    private static uint Readˉu32(ReadOnlySpan<byte> value, int offset) =>
        BinaryPrimitives.ReadUInt32LittleEndian(value[offset..]);
    private static ulong Readˉu64(ReadOnlySpan<byte> value, int offset) =>
        BinaryPrimitives.ReadUInt64LittleEndian(value[offset..]);
    private static void Writeˉu32(Span<byte> value, int offset, uint field) =>
        BinaryPrimitives.WriteUInt32LittleEndian(value[offset..], field);
    private static void Writeˉu64(Span<byte> value, int offset, ulong field) =>
        BinaryPrimitives.WriteUInt64LittleEndian(value[offset..], field);
}
