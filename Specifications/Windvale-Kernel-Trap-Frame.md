# Windvale x86-64 kernel trap frame

## Status and purpose

Kernel trap frame version 1 is cross-host qualified at exact commit `12e9e2e` through the pre-paging firmware probe-20 baseline and retained unchanged by qualified probe 21 at `860c69c`. It gives x86-64 exception entries with and without CPU error codes one common terminal-handler input. [Decision 0086](../Documents/Decisions/0086-First-Wva-Owned-Normalized-X64-Trap-Entries.md) owns the contract and its evidence boundary; [Decisions 0087](../Documents/Decisions/0087-Native-Windows-And-Linux-File-Output.md) and [0090](../Documents/Decisions/0090-First-In-Guest-Wvb-Admission.md) record the two complete compositions.

This is an internal machine-entry contract. Its qualified version-1 use is ring 0; probes 22 and 23 also apply the same normalized prefix to process-private privilege-transition frames. It is not a Windvale source value, WVB record, public syscall ABI, user-process signal frame, unwind record, or mapping of Windvale `WVR` runtime traps to CPU faults.

## Same-privilege frame

Version 1 admits exceptions delivered while executing at current privilege level 0 on the existing kernel-owned stack. On entry to the common terminal handler, `RSP` addresses this exact 40-byte little-endian record:

| Offset | Bytes | Field | Owner |
| ---: | ---: | --- | --- |
| `0` | 8 | Vector | WVA entry stub |
| `8` | 8 | Error code | CPU when defined; otherwise synthetic zero from WVA |
| `16` | 8 | Interrupted `RIP` | CPU |
| `24` | 8 | Interrupted `CS` | CPU |
| `32` | 8 | Interrupted `RFLAGS` | CPU |

All cells are 64 bits. The vector and synthetic error cells are produced by WVA `push_i32`, whose x86-64 encoding sign-extends the immediate to one 64-bit cell. Every admitted vector and synthetic error is nonnegative and therefore has a zero high half.

## Entry normalization

An entry stub must match the processor's error-code behavior exactly:

- for an exception without a CPU error code, push synthetic error code 0, then push the vector;
- for an exception with a CPU error code, leave that CPU cell in place and push only the vector; and
- tail-transfer to the common handler without a call, prologue, register save, or additional stack mutation.

Probe 19 implements two WVA-owned examples:

```text
# CPU frame starts RIP, CS, RFLAGS.
vector 6:  push_i32 0; push_i32 6; jump common

# CPU frame starts error, RIP, CS, RFLAGS.
vector 13: push_i32 13; jump common
```

After either sequence, the common frame has the same offsets. The stubs do not preserve general-purpose registers because the current handler is terminal. A later resumable or routed dispatcher must define saved registers, alignment, ownership, and restoration as a new frame version.

## Validation and terminal policy

The probe-21 common handler reads only vector and error code. It accepts `(6, 0)` as invalid opcode and `(13, 0)` as the deterministic general-protection scenario. Any other pair reaches a fixed malformed-frame panic. The handler does not decode or modify `RIP`, `CS`, or `RFLAGS`, and it never executes `IRETQ`.

The object and boot tests independently lock the WVA stub bytes, definition offsets and sizes, relocations to the common handler, IDT targets, frame offsets, exact scenario markers, and QEMU terminal exit. Real QEMU execution is required to prove both a CPU-no-error-code delivery and a CPU-error-code delivery; static object checks alone are insufficient.

## Process-private privilege-transition frame

When probe 23 takes vector 6, 13, or 14 from CPL3, the processor changes to the active process's TSS ring-0 stack and appends the interrupted user `RSP` and `SS`. After WVA normalization, `RSP` addresses this exact 56-byte record:

| Offset | Bytes | Field | Owner |
| ---: | ---: | --- | --- |
| `0` | 8 | Vector | WVA process-entry stub |
| `8` | 8 | Error code | CPU when defined; otherwise synthetic zero |
| `16` | 8 | Interrupted `RIP` | CPU |
| `24` | 8 | Interrupted `CS` | CPU; low two bits identify CPL3 |
| `32` | 8 | Interrupted `RFLAGS` | CPU |
| `40` | 8 | Interrupted user `RSP` | CPU |
| `48` | 8 | Interrupted user `SS` | CPU |

The process common entry consumes only vector, error, and `CS`. For CPL3 it records the fault and returns to the saved kernel continuation rather than using `IRETQ`; for CPL0 it preserves the existing terminal path. The frame is internal to protected-process version 2 and does not freeze a signal or debugger ABI.

## Limits

Version 1 does not define IST stack switches, page-fault `CR2`, double faults, NMI, interrupts, complete register preservation, SIMD state, nested faults, concurrency, recovery, resumption, unwinding, user signals, or a stable external ABI. Probe 23 supplies evidence for the 56-byte privilege-transition extension and explicit preservation of ABI-16 execution-context register `RDX` across init block/wake; all other cases remain open.
