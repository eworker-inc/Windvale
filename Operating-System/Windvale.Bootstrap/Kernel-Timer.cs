using System.Buffers.Binary;
using System.Collections.Immutable;

namespace Windvale.Bootstrap;

public static class Kernelˉtimerˉcontract
{
    public const string IRQ_ENTRY_SYMBOL = "Windvale_kernel_x64_timer_irq0_entry";
    public const string INTERRUPT_SYMBOL = "Windvale_kernel_x64_timer_interrupt";
    public const string RESUME_SYMBOL = "Windvale_kernel_x64_timer_resume";
    public const string READ_CLOCK_SYMBOL = "Windvale_kernel_x64_timer_read_clock";
    public const string REARM_SYMBOL = "Windvale_kernel_x64_timer_rearm";
    public const string ARM_SYMBOL = "Windvale_kernel_x64_timer_arm";
    public const string STOP_SYMBOL = "Windvale_kernel_x64_timer_stop";

    public const uint IRQ_VECTOR = 32;
    public const uint QUANTUM_MICROSECONDS = 5_000;
    public const uint HPET_PERIOD_FEMTOSECONDS = 10_000_000;
    public const uint HPET_CALIBRATION_TICKS = 500_000;
    public const uint MAXIMUM_TICKS = 4;
    public const uint EXPECTED_SWITCHES = 3;
    public const uint EXPECTED_DIRECTORY_RESUMES = 1;
    public const uint INIT_SENTINEL_BASE = 0x1101;
    public const uint CLIENT_SENTINEL_BASE = 0x2201;
    public const uint DIRECTORY_SENTINEL_BASE = 0x3301;
    public const uint PREEMPTION_PROBE_SETUP_BYTES = 83;
    public const uint PREEMPTION_PROBE_BYTES = 88;

    public const uint NORMALIZED_FRAME_BYTES = 176;
    public const uint CONTEXT_RECORD_BYTES = 224;
    public const ulong CONTEXT_MAGIC = 0x3130_3052_4854_5657;
    public const uint CONTEXT_VERSION = 1;
    public const uint CONTEXT_PROCESS_REFERENCE_OFFSET = 16;
    public const uint CONTEXT_THREAD_ID_OFFSET = 20;
    public const uint CONTEXT_STATE_OFFSET = 24;
    public const uint CONTEXT_FRAME_BYTES_OFFSET = 28;
    public const uint CONTEXT_TICK_COUNT_OFFSET = 32;
    public const uint CONTEXT_DISPATCH_COUNT_OFFSET = 36;
    public const uint CONTEXT_RESUME_COUNT_OFFSET = 40;
    public const uint CONTEXT_PREEMPTION_COUNT_OFFSET = 44;
    public const uint CONTEXT_FRAME_OFFSET = 48;
    public const uint CONTEXT_STATE_READY = 1;
    public const uint CONTEXT_STATE_RUNNING = 2;
    public const uint CONTEXT_STATE_SAVED = 3;

    public const uint FRAME_VECTOR_OFFSET = 0;
    public const uint FRAME_ERROR_OFFSET = 8;
    public const uint FRAME_RAX_OFFSET = 16;
    public const uint FRAME_RBX_OFFSET = 24;
    public const uint FRAME_RCX_OFFSET = 32;
    public const uint FRAME_RDX_OFFSET = 40;
    public const uint FRAME_RSI_OFFSET = 48;
    public const uint FRAME_RDI_OFFSET = 56;
    public const uint FRAME_RBP_OFFSET = 64;
    public const uint FRAME_R8_OFFSET = 72;
    public const uint FRAME_R9_OFFSET = 80;
    public const uint FRAME_R10_OFFSET = 88;
    public const uint FRAME_R11_OFFSET = 96;
    public const uint FRAME_R12_OFFSET = 104;
    public const uint FRAME_R13_OFFSET = 112;
    public const uint FRAME_R14_OFFSET = 120;
    public const uint FRAME_R15_OFFSET = 128;
    public const uint FRAME_RIP_OFFSET = 136;
    public const uint FRAME_CS_OFFSET = 144;
    public const uint FRAME_RFLAGS_OFFSET = 152;
    public const uint FRAME_RSP_OFFSET = 160;
    public const uint FRAME_SS_OFFSET = 168;
    public const ulong USER_CODE_SELECTOR = 0x23;
    public const ulong USER_DATA_SELECTOR = 0x1B;
    public const ulong USER_RFLAGS = 0x202;
    public const uint USER_RFLAGS_CONTROL_MASK = 0x001A_7202;
    public const uint USER_RFLAGS_REQUIRED = 0x202;

    public const uint INIT_CONTEXT_RECORD_OFFSET = 0x8A0;
    public const uint CLIENT_CONTEXT_RECORD_OFFSET = INIT_CONTEXT_RECORD_OFFSET + CONTEXT_RECORD_BYTES;
    public const uint DIRECTORY_CONTEXT_RECORD_OFFSET = CLIENT_CONTEXT_RECORD_OFFSET + CONTEXT_RECORD_BYTES;
    public const uint TIMER_RECORD_OFFSET = DIRECTORY_CONTEXT_RECORD_OFFSET + CONTEXT_RECORD_BYTES;
    public const uint TIMER_RECORD_BYTES = 96;
    public const ulong TIMER_MAGIC = 0x3130_454D_4954_5657;
    public const uint TIMER_VERSION = 1;
    public const uint TIMER_CLOCKSOURCE_HPET = 2;
    public const uint TIMER_EVENT_LOCAL_APIC_ONE_SHOT = 1;
    public const uint TIMER_EVENT_FEATURE_CALIBRATED = 1;
    public const uint TIMER_SOURCE_OFFSET = 16;
    public const uint TIMER_EVENT_KIND_OFFSET = 20;
    public const uint TIMER_VECTOR_OFFSET = 24;
    public const uint TIMER_QUANTUM_OFFSET = 28;
    public const uint TIMER_MAXIMUM_TICKS_OFFSET = 32;
    public const uint TIMER_TICK_COUNT_OFFSET = 36;
    public const uint TIMER_SWITCH_COUNT_OFFSET = 40;
    public const uint TIMER_EOI_COUNT_OFFSET = 44;
    public const uint TIMER_CURSOR_OFFSET = 48;
    public const uint TIMER_ACTIVE_PROCESS_OFFSET = 52;
    public const uint TIMER_DIRECTORY_RESUME_COUNT_OFFSET = 56;
    public const uint TIMER_FLAGS_OFFSET = 60;
    public const uint TIMER_HPET_PERIOD_OFFSET = 64;
    public const uint TIMER_EVENT_INITIAL_COUNT_OFFSET = 68;
    public const uint TIMER_CLOCK_FEATURES_OFFSET = 72;
    public const uint TIMER_EVENT_FEATURES_OFFSET = 76;
    public const uint TIMER_LAST_CLOCK_OFFSET = 80;
    public const uint TIMER_CLOCK_READ_COUNT_OFFSET = 88;
    public const uint TIMER_CALIBRATION_TICKS_OFFSET = 92;
    public const uint TIMER_FLAG_ACTIVE = 1;
    public const uint TIMER_FLAG_COMPLETE = 2;
}

public sealed record Kernelˉinterruptˉframe(
    ulong Rax,
    ulong Rbx,
    ulong Rcx,
    ulong Rdx,
    ulong Rsi,
    ulong Rdi,
    ulong Rbp,
    ulong R8,
    ulong R9,
    ulong R10,
    ulong R11,
    ulong R12,
    ulong R13,
    ulong R14,
    ulong R15,
    ulong Rip,
    ulong Rsp);

public sealed record Kernelˉthreadˉcontext(
    uint Processˉreference,
    uint Threadˉid,
    uint State,
    uint Tickˉcount,
    uint Dispatchˉcount,
    uint Resumeˉcount,
    uint Preemptionˉcount,
    Kernelˉinterruptˉframe Frame);

public static class Kernelˉthreadˉcontextˉcodec
{
    public static ImmutableArray<byte> Write(Kernelˉthreadˉcontext value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Validateˉidentity(value.Processˉreference, value.Threadˉid);
        if (value.State is not (Kernelˉtimerˉcontract.CONTEXT_STATE_READY or
                Kernelˉtimerˉcontract.CONTEXT_STATE_RUNNING or
                Kernelˉtimerˉcontract.CONTEXT_STATE_SAVED))
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }

        var Result = new byte[Kernelˉtimerˉcontract.CONTEXT_RECORD_BYTES];
        BinaryPrimitives.WriteUInt64LittleEndian(Result, Kernelˉtimerˉcontract.CONTEXT_MAGIC);
        Writeˉu32(Result, 8, Kernelˉtimerˉcontract.CONTEXT_VERSION);
        Writeˉu32(Result, 12, Kernelˉtimerˉcontract.CONTEXT_RECORD_BYTES);
        Writeˉu32(Result, Kernelˉtimerˉcontract.CONTEXT_PROCESS_REFERENCE_OFFSET,
            value.Processˉreference);
        Writeˉu32(Result, Kernelˉtimerˉcontract.CONTEXT_THREAD_ID_OFFSET, value.Threadˉid);
        Writeˉu32(Result, Kernelˉtimerˉcontract.CONTEXT_STATE_OFFSET, value.State);
        Writeˉu32(Result, Kernelˉtimerˉcontract.CONTEXT_FRAME_BYTES_OFFSET,
            Kernelˉtimerˉcontract.NORMALIZED_FRAME_BYTES);
        Writeˉu32(Result, Kernelˉtimerˉcontract.CONTEXT_TICK_COUNT_OFFSET, value.Tickˉcount);
        Writeˉu32(Result, Kernelˉtimerˉcontract.CONTEXT_DISPATCH_COUNT_OFFSET,
            value.Dispatchˉcount);
        Writeˉu32(Result, Kernelˉtimerˉcontract.CONTEXT_RESUME_COUNT_OFFSET, value.Resumeˉcount);
        Writeˉu32(Result, Kernelˉtimerˉcontract.CONTEXT_PREEMPTION_COUNT_OFFSET,
            value.Preemptionˉcount);
        Writeˉframe(Result.AsSpan((int)Kernelˉtimerˉcontract.CONTEXT_FRAME_OFFSET), value.Frame);
        return Result.ToImmutableArray();
    }

    public static Kernelˉthreadˉcontext Read(ReadOnlySpan<byte> source)
    {
        if (source.Length != Kernelˉtimerˉcontract.CONTEXT_RECORD_BYTES ||
            BinaryPrimitives.ReadUInt64LittleEndian(source) != Kernelˉtimerˉcontract.CONTEXT_MAGIC ||
            Readˉu32(source, 8) != Kernelˉtimerˉcontract.CONTEXT_VERSION ||
            Readˉu32(source, 12) != Kernelˉtimerˉcontract.CONTEXT_RECORD_BYTES ||
            Readˉu32(source, Kernelˉtimerˉcontract.CONTEXT_FRAME_BYTES_OFFSET) !=
                Kernelˉtimerˉcontract.NORMALIZED_FRAME_BYTES)
        {
            throw new InvalidDataException("The kernel thread-context record is malformed.");
        }
        var Processˉreference = Readˉu32(
            source, Kernelˉtimerˉcontract.CONTEXT_PROCESS_REFERENCE_OFFSET);
        var Threadˉid = Readˉu32(source, Kernelˉtimerˉcontract.CONTEXT_THREAD_ID_OFFSET);
        Validateˉidentity(Processˉreference, Threadˉid);
        var State = Readˉu32(source, Kernelˉtimerˉcontract.CONTEXT_STATE_OFFSET);
        var Tickˉcount = Readˉu32(source, Kernelˉtimerˉcontract.CONTEXT_TICK_COUNT_OFFSET);
        var Dispatchˉcount = Readˉu32(
            source, Kernelˉtimerˉcontract.CONTEXT_DISPATCH_COUNT_OFFSET);
        var Resumeˉcount = Readˉu32(source, Kernelˉtimerˉcontract.CONTEXT_RESUME_COUNT_OFFSET);
        var Preemptionˉcount = Readˉu32(
            source, Kernelˉtimerˉcontract.CONTEXT_PREEMPTION_COUNT_OFFSET);
        if (State is not (Kernelˉtimerˉcontract.CONTEXT_STATE_READY or
                Kernelˉtimerˉcontract.CONTEXT_STATE_RUNNING or
                Kernelˉtimerˉcontract.CONTEXT_STATE_SAVED) ||
            Tickˉcount > Kernelˉtimerˉcontract.MAXIMUM_TICKS ||
            Dispatchˉcount > 2 ||
            Resumeˉcount > Kernelˉtimerˉcontract.EXPECTED_DIRECTORY_RESUMES ||
            Preemptionˉcount > Kernelˉtimerˉcontract.MAXIMUM_TICKS)
        {
            throw new InvalidDataException("The kernel thread-context record has invalid state or reserved bytes.");
        }
        return new(
            Processˉreference,
            Threadˉid,
            State,
            Tickˉcount,
            Dispatchˉcount,
            Resumeˉcount,
            Preemptionˉcount,
            Readˉframe(source[(int)Kernelˉtimerˉcontract.CONTEXT_FRAME_OFFSET..]));
    }

    private static void Writeˉframe(Span<byte> destination, Kernelˉinterruptˉframe value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value.Rip == 0 || value.Rsp == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }
        Writeˉu64(destination, Kernelˉtimerˉcontract.FRAME_VECTOR_OFFSET,
            Kernelˉtimerˉcontract.IRQ_VECTOR);
        Writeˉu64(destination, Kernelˉtimerˉcontract.FRAME_ERROR_OFFSET, 0);
        var Registers = new[]
        {
            value.Rax, value.Rbx, value.Rcx, value.Rdx, value.Rsi, value.Rdi, value.Rbp,
            value.R8, value.R9, value.R10, value.R11, value.R12, value.R13, value.R14, value.R15,
        };
        for (var Index = 0; Index < Registers.Length; Index++)
        {
            Writeˉu64(destination, Kernelˉtimerˉcontract.FRAME_RAX_OFFSET +
                checked((uint)(Index * sizeof(ulong))), Registers[Index]);
        }
        Writeˉu64(destination, Kernelˉtimerˉcontract.FRAME_RIP_OFFSET, value.Rip);
        Writeˉu64(destination, Kernelˉtimerˉcontract.FRAME_CS_OFFSET,
            Kernelˉtimerˉcontract.USER_CODE_SELECTOR);
        Writeˉu64(destination, Kernelˉtimerˉcontract.FRAME_RFLAGS_OFFSET,
            Kernelˉtimerˉcontract.USER_RFLAGS);
        Writeˉu64(destination, Kernelˉtimerˉcontract.FRAME_RSP_OFFSET, value.Rsp);
        Writeˉu64(destination, Kernelˉtimerˉcontract.FRAME_SS_OFFSET,
            Kernelˉtimerˉcontract.USER_DATA_SELECTOR);
    }

    private static Kernelˉinterruptˉframe Readˉframe(ReadOnlySpan<byte> source)
    {
        if (source.Length != Kernelˉtimerˉcontract.NORMALIZED_FRAME_BYTES ||
            Readˉu64(source, Kernelˉtimerˉcontract.FRAME_VECTOR_OFFSET) !=
                Kernelˉtimerˉcontract.IRQ_VECTOR ||
            Readˉu64(source, Kernelˉtimerˉcontract.FRAME_ERROR_OFFSET) != 0 ||
            Readˉu64(source, Kernelˉtimerˉcontract.FRAME_CS_OFFSET) !=
                Kernelˉtimerˉcontract.USER_CODE_SELECTOR ||
            Readˉu64(source, Kernelˉtimerˉcontract.FRAME_RFLAGS_OFFSET) !=
                Kernelˉtimerˉcontract.USER_RFLAGS ||
            Readˉu64(source, Kernelˉtimerˉcontract.FRAME_SS_OFFSET) !=
                Kernelˉtimerˉcontract.USER_DATA_SELECTOR ||
            Readˉu64(source, Kernelˉtimerˉcontract.FRAME_RIP_OFFSET) == 0 ||
            Readˉu64(source, Kernelˉtimerˉcontract.FRAME_RSP_OFFSET) == 0)
        {
            throw new InvalidDataException("The normalized timer interrupt frame is malformed.");
        }
        return new(
            Readˉu64(source, Kernelˉtimerˉcontract.FRAME_RAX_OFFSET),
            Readˉu64(source, Kernelˉtimerˉcontract.FRAME_RBX_OFFSET),
            Readˉu64(source, Kernelˉtimerˉcontract.FRAME_RCX_OFFSET),
            Readˉu64(source, Kernelˉtimerˉcontract.FRAME_RDX_OFFSET),
            Readˉu64(source, Kernelˉtimerˉcontract.FRAME_RSI_OFFSET),
            Readˉu64(source, Kernelˉtimerˉcontract.FRAME_RDI_OFFSET),
            Readˉu64(source, Kernelˉtimerˉcontract.FRAME_RBP_OFFSET),
            Readˉu64(source, Kernelˉtimerˉcontract.FRAME_R8_OFFSET),
            Readˉu64(source, Kernelˉtimerˉcontract.FRAME_R9_OFFSET),
            Readˉu64(source, Kernelˉtimerˉcontract.FRAME_R10_OFFSET),
            Readˉu64(source, Kernelˉtimerˉcontract.FRAME_R11_OFFSET),
            Readˉu64(source, Kernelˉtimerˉcontract.FRAME_R12_OFFSET),
            Readˉu64(source, Kernelˉtimerˉcontract.FRAME_R13_OFFSET),
            Readˉu64(source, Kernelˉtimerˉcontract.FRAME_R14_OFFSET),
            Readˉu64(source, Kernelˉtimerˉcontract.FRAME_R15_OFFSET),
            Readˉu64(source, Kernelˉtimerˉcontract.FRAME_RIP_OFFSET),
            Readˉu64(source, Kernelˉtimerˉcontract.FRAME_RSP_OFFSET));
    }

    private static void Validateˉidentity(uint processˉreference, uint threadˉid)
    {
        var Valid = (processˉreference, threadˉid) switch
        {
            (Kernelˉprocessˉcontract.INIT_PROCESS_REFERENCE,
                Kernelˉprocessˉcontract.INIT_THREAD_ID) => true,
            (Kernelˉprocessˉcontract.FIRST_CLIENT_PROCESS_REFERENCE,
                Kernelˉprocessˉcontract.CLIENT_THREAD_ID) => true,
            (Kernelˉprocessˉcontract.DIRECTORY_PROCESS_REFERENCE,
                Kernelˉprocessˉcontract.DIRECTORY_THREAD_ID) => true,
            _ => false,
        };
        if (!Valid)
        {
            throw new InvalidDataException("The kernel thread-context identity is invalid.");
        }
    }

    private static uint Readˉu32(ReadOnlySpan<byte> source, uint offset) =>
        BinaryPrimitives.ReadUInt32LittleEndian(source[(int)offset..]);

    private static ulong Readˉu64(ReadOnlySpan<byte> source, uint offset) =>
        BinaryPrimitives.ReadUInt64LittleEndian(source[(int)offset..]);

    private static void Writeˉu32(Span<byte> destination, uint offset, uint value) =>
        BinaryPrimitives.WriteUInt32LittleEndian(destination[(int)offset..], value);

    private static void Writeˉu64(Span<byte> destination, uint offset, ulong value) =>
        BinaryPrimitives.WriteUInt64LittleEndian(destination[(int)offset..], value);
}

public sealed record Kernelˉtimerˉhardware(
    uint Clocksource,
    uint Eventˉkind,
    uint Eventˉinitialˉcount,
    uint Clockˉfeatures,
    uint Eventˉfeatures,
    ulong Lastˉclock,
    uint Clockˉreadˉcount);

public sealed record Kernelˉtimerˉstate(
    Kernelˉtimerˉhardware Hardware,
    uint Tickˉcount,
    uint Switchˉcount,
    uint Eoiˉcount,
    uint Cursor,
    uint Activeˉprocessˉreference,
    uint Directoryˉresumeˉcount,
    uint Flags);

public sealed record Kernelˉpreemptionˉstep(
    Kernelˉtimerˉstate State,
    uint? Nextˉprocessˉreference);

public static class Kernelˉtimerˉstateˉcodec
{
    public static ImmutableArray<byte> Write(Kernelˉtimerˉstate value)
    {
        Validate(value);
        var Result = new byte[Kernelˉtimerˉcontract.TIMER_RECORD_BYTES];
        BinaryPrimitives.WriteUInt64LittleEndian(Result, Kernelˉtimerˉcontract.TIMER_MAGIC);
        Writeˉu32(Result, 8, Kernelˉtimerˉcontract.TIMER_VERSION);
        Writeˉu32(Result, 12, Kernelˉtimerˉcontract.TIMER_RECORD_BYTES);
        Writeˉu32(Result, Kernelˉtimerˉcontract.TIMER_SOURCE_OFFSET, value.Hardware.Clocksource);
        Writeˉu32(Result, Kernelˉtimerˉcontract.TIMER_EVENT_KIND_OFFSET,
            value.Hardware.Eventˉkind);
        Writeˉu32(Result, Kernelˉtimerˉcontract.TIMER_VECTOR_OFFSET,
            Kernelˉtimerˉcontract.IRQ_VECTOR);
        Writeˉu32(Result, Kernelˉtimerˉcontract.TIMER_QUANTUM_OFFSET,
            Kernelˉtimerˉcontract.QUANTUM_MICROSECONDS);
        Writeˉu32(Result, Kernelˉtimerˉcontract.TIMER_MAXIMUM_TICKS_OFFSET,
            Kernelˉtimerˉcontract.MAXIMUM_TICKS);
        Writeˉu32(Result, Kernelˉtimerˉcontract.TIMER_TICK_COUNT_OFFSET, value.Tickˉcount);
        Writeˉu32(Result, Kernelˉtimerˉcontract.TIMER_SWITCH_COUNT_OFFSET, value.Switchˉcount);
        Writeˉu32(Result, Kernelˉtimerˉcontract.TIMER_EOI_COUNT_OFFSET, value.Eoiˉcount);
        Writeˉu32(Result, Kernelˉtimerˉcontract.TIMER_CURSOR_OFFSET, value.Cursor);
        Writeˉu32(Result, Kernelˉtimerˉcontract.TIMER_ACTIVE_PROCESS_OFFSET,
            value.Activeˉprocessˉreference);
        Writeˉu32(Result, Kernelˉtimerˉcontract.TIMER_DIRECTORY_RESUME_COUNT_OFFSET,
            value.Directoryˉresumeˉcount);
        Writeˉu32(Result, Kernelˉtimerˉcontract.TIMER_FLAGS_OFFSET, value.Flags);
        Writeˉu32(Result, Kernelˉtimerˉcontract.TIMER_HPET_PERIOD_OFFSET,
            Kernelˉtimerˉcontract.HPET_PERIOD_FEMTOSECONDS);
        Writeˉu32(Result, Kernelˉtimerˉcontract.TIMER_EVENT_INITIAL_COUNT_OFFSET,
            value.Hardware.Eventˉinitialˉcount);
        Writeˉu32(Result, Kernelˉtimerˉcontract.TIMER_CLOCK_FEATURES_OFFSET,
            value.Hardware.Clockˉfeatures);
        Writeˉu32(Result, Kernelˉtimerˉcontract.TIMER_EVENT_FEATURES_OFFSET,
            value.Hardware.Eventˉfeatures);
        Writeˉu64(Result, Kernelˉtimerˉcontract.TIMER_LAST_CLOCK_OFFSET,
            value.Hardware.Lastˉclock);
        Writeˉu32(Result, Kernelˉtimerˉcontract.TIMER_CLOCK_READ_COUNT_OFFSET,
            value.Hardware.Clockˉreadˉcount);
        Writeˉu32(Result, Kernelˉtimerˉcontract.TIMER_CALIBRATION_TICKS_OFFSET,
            Kernelˉtimerˉcontract.HPET_CALIBRATION_TICKS);
        return Result.ToImmutableArray();
    }

    public static Kernelˉtimerˉstate Read(ReadOnlySpan<byte> source)
    {
        if (source.Length != Kernelˉtimerˉcontract.TIMER_RECORD_BYTES ||
            BinaryPrimitives.ReadUInt64LittleEndian(source) != Kernelˉtimerˉcontract.TIMER_MAGIC ||
            Readˉu32(source, 8) != Kernelˉtimerˉcontract.TIMER_VERSION ||
            Readˉu32(source, 12) != Kernelˉtimerˉcontract.TIMER_RECORD_BYTES ||
            Readˉu32(source, Kernelˉtimerˉcontract.TIMER_EVENT_KIND_OFFSET) !=
                Kernelˉtimerˉcontract.TIMER_EVENT_LOCAL_APIC_ONE_SHOT ||
            Readˉu32(source, Kernelˉtimerˉcontract.TIMER_VECTOR_OFFSET) !=
                Kernelˉtimerˉcontract.IRQ_VECTOR ||
            Readˉu32(source, Kernelˉtimerˉcontract.TIMER_QUANTUM_OFFSET) !=
                Kernelˉtimerˉcontract.QUANTUM_MICROSECONDS ||
            Readˉu32(source, Kernelˉtimerˉcontract.TIMER_MAXIMUM_TICKS_OFFSET) !=
                Kernelˉtimerˉcontract.MAXIMUM_TICKS ||
            Readˉu32(source, Kernelˉtimerˉcontract.TIMER_HPET_PERIOD_OFFSET) !=
                Kernelˉtimerˉcontract.HPET_PERIOD_FEMTOSECONDS ||
            Readˉu32(source, Kernelˉtimerˉcontract.TIMER_CALIBRATION_TICKS_OFFSET) !=
                Kernelˉtimerˉcontract.HPET_CALIBRATION_TICKS)
        {
            throw new InvalidDataException("The kernel timer record is malformed.");
        }
        var Hardware = new Kernelˉtimerˉhardware(
            Readˉu32(source, Kernelˉtimerˉcontract.TIMER_SOURCE_OFFSET),
            Readˉu32(source, Kernelˉtimerˉcontract.TIMER_EVENT_KIND_OFFSET),
            Readˉu32(source, Kernelˉtimerˉcontract.TIMER_EVENT_INITIAL_COUNT_OFFSET),
            Readˉu32(source, Kernelˉtimerˉcontract.TIMER_CLOCK_FEATURES_OFFSET),
            Readˉu32(source, Kernelˉtimerˉcontract.TIMER_EVENT_FEATURES_OFFSET),
            Readˉu64(source, Kernelˉtimerˉcontract.TIMER_LAST_CLOCK_OFFSET),
            Readˉu32(source, Kernelˉtimerˉcontract.TIMER_CLOCK_READ_COUNT_OFFSET));
        var Result = new Kernelˉtimerˉstate(
            Hardware,
            Readˉu32(source, Kernelˉtimerˉcontract.TIMER_TICK_COUNT_OFFSET),
            Readˉu32(source, Kernelˉtimerˉcontract.TIMER_SWITCH_COUNT_OFFSET),
            Readˉu32(source, Kernelˉtimerˉcontract.TIMER_EOI_COUNT_OFFSET),
            Readˉu32(source, Kernelˉtimerˉcontract.TIMER_CURSOR_OFFSET),
            Readˉu32(source, Kernelˉtimerˉcontract.TIMER_ACTIVE_PROCESS_OFFSET),
            Readˉu32(source, Kernelˉtimerˉcontract.TIMER_DIRECTORY_RESUME_COUNT_OFFSET),
            Readˉu32(source, Kernelˉtimerˉcontract.TIMER_FLAGS_OFFSET));
        Validate(Result);
        return Result;
    }

    public static Kernelˉtimerˉstate Initial() => new(
        new(
            Kernelˉtimerˉcontract.TIMER_CLOCKSOURCE_HPET,
            Kernelˉtimerˉcontract.TIMER_EVENT_LOCAL_APIC_ONE_SHOT,
            1,
            0,
            Kernelˉtimerˉcontract.TIMER_EVENT_FEATURE_CALIBRATED,
            1,
            0),
        0,
        0,
        0,
        0,
        Kernelˉprocessˉcontract.DIRECTORY_PROCESS_REFERENCE,
        0,
        Kernelˉtimerˉcontract.TIMER_FLAG_ACTIVE);

    private static void Validate(Kernelˉtimerˉstate value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var Hardware = value.Hardware;
        if (Hardware.Clocksource != Kernelˉtimerˉcontract.TIMER_CLOCKSOURCE_HPET ||
            Hardware.Clockˉfeatures != 0 ||
            Hardware.Eventˉkind != Kernelˉtimerˉcontract.TIMER_EVENT_LOCAL_APIC_ONE_SHOT ||
            Hardware.Eventˉinitialˉcount == 0 ||
            Hardware.Eventˉfeatures != Kernelˉtimerˉcontract.TIMER_EVENT_FEATURE_CALIBRATED ||
            Hardware.Lastˉclock == 0 ||
            Hardware.Clockˉreadˉcount != value.Tickˉcount ||
            value.Tickˉcount > Kernelˉtimerˉcontract.MAXIMUM_TICKS ||
            value.Switchˉcount > Kernelˉtimerˉcontract.EXPECTED_SWITCHES ||
            value.Eoiˉcount != value.Tickˉcount ||
            value.Cursor >= Kernelˉreadyˉdispatcher.RECORD_COUNT ||
            value.Directoryˉresumeˉcount > Kernelˉtimerˉcontract.EXPECTED_DIRECTORY_RESUMES ||
            value.Flags is not (Kernelˉtimerˉcontract.TIMER_FLAG_ACTIVE or
                Kernelˉtimerˉcontract.TIMER_FLAG_COMPLETE) ||
            value.Activeˉprocessˉreference is not
                (Kernelˉprocessˉcontract.INIT_PROCESS_REFERENCE or
                    Kernelˉprocessˉcontract.FIRST_CLIENT_PROCESS_REFERENCE or
                    Kernelˉprocessˉcontract.DIRECTORY_PROCESS_REFERENCE))
        {
            throw new InvalidDataException("The kernel timer state is outside its bounded contract.");
        }
    }

    private static uint Readˉu32(ReadOnlySpan<byte> source, uint offset) =>
        BinaryPrimitives.ReadUInt32LittleEndian(source[(int)offset..]);

    private static ulong Readˉu64(ReadOnlySpan<byte> source, uint offset) =>
        BinaryPrimitives.ReadUInt64LittleEndian(source[(int)offset..]);

    private static void Writeˉu32(Span<byte> destination, uint offset, uint value) =>
        BinaryPrimitives.WriteUInt32LittleEndian(destination[(int)offset..], value);

    private static void Writeˉu64(Span<byte> destination, uint offset, ulong value) =>
        BinaryPrimitives.WriteUInt64LittleEndian(destination[(int)offset..], value);
}

public static class Kernelˉpreemptionˉoracle
{
    public static Kernelˉpreemptionˉstep Advance(Kernelˉtimerˉstate current)
    {
        _ = Kernelˉtimerˉstateˉcodec.Read(Kernelˉtimerˉstateˉcodec.Write(current).AsSpan());
        return current.Tickˉcount switch
        {
            0 when current == Kernelˉtimerˉstateˉcodec.Initial() =>
                Step(current, Kernelˉprocessˉcontract.INIT_PROCESS_REFERENCE, 1, 1, 1, 0),
            1 when current.Activeˉprocessˉreference == Kernelˉprocessˉcontract.INIT_PROCESS_REFERENCE &&
                current.Switchˉcount == 1 && current.Cursor == 1 =>
                Step(current, Kernelˉprocessˉcontract.FIRST_CLIENT_PROCESS_REFERENCE, 2, 2, 2, 0),
            2 when current.Activeˉprocessˉreference == Kernelˉprocessˉcontract.FIRST_CLIENT_PROCESS_REFERENCE &&
                current.Switchˉcount == 2 && current.Cursor == 2 =>
                Step(current, Kernelˉprocessˉcontract.DIRECTORY_PROCESS_REFERENCE, 3, 3, 0, 1),
            3 when current.Activeˉprocessˉreference == Kernelˉprocessˉcontract.DIRECTORY_PROCESS_REFERENCE &&
                current.Switchˉcount == Kernelˉtimerˉcontract.EXPECTED_SWITCHES &&
                current.Cursor == 0 &&
                current.Directoryˉresumeˉcount == Kernelˉtimerˉcontract.EXPECTED_DIRECTORY_RESUMES =>
                new(new(
                    Advanceˉhardware(current.Hardware),
                    Kernelˉtimerˉcontract.MAXIMUM_TICKS,
                    Kernelˉtimerˉcontract.EXPECTED_SWITCHES,
                    Kernelˉtimerˉcontract.MAXIMUM_TICKS,
                    0,
                    Kernelˉprocessˉcontract.DIRECTORY_PROCESS_REFERENCE,
                    Kernelˉtimerˉcontract.EXPECTED_DIRECTORY_RESUMES,
                    Kernelˉtimerˉcontract.TIMER_FLAG_COMPLETE), null),
            _ => throw new InvalidDataException(
                "The kernel preemption oracle received an unexpected timer progression."),
        };
    }

    private static Kernelˉpreemptionˉstep Step(
        Kernelˉtimerˉstate current,
        uint nextˉprocessˉreference,
        uint tick,
        uint switches,
        uint cursor,
        uint directoryˉresumes) =>
        new(new(
            Advanceˉhardware(current.Hardware),
            tick,
            switches,
            checked(current.Eoiˉcount + 1),
            cursor,
            nextˉprocessˉreference,
            directoryˉresumes,
            Kernelˉtimerˉcontract.TIMER_FLAG_ACTIVE), nextˉprocessˉreference);

    private static Kernelˉtimerˉhardware Advanceˉhardware(Kernelˉtimerˉhardware current) =>
        current with
        {
            Lastˉclock = checked(current.Lastˉclock + 1),
            Clockˉreadˉcount = checked(current.Clockˉreadˉcount + 1),
        };
}
