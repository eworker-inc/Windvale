# Language 1.0 paper workload 10: System and FFI boundary

## Status

Draft reviewed after the project owner accepted all seven findings on 2026-08-17
under
[Decision 0764](../../../Decisions/0764-Resolve-Language-1.0-System-Ffi-Findings.md).
This is paper Language 1.0 source. Current Seed tools do not accept it, and it
does not implement System source, unsafe operations, foreign calls, the paper
ABI, or freeze edition 1.

## Result

Five modules express one deliberately narrow foreign-buffer adapter that:

1. targets exactly Linux x86-64 plus registered SysV AMD64 C ABI version 1;
2. allocates one zero-initialized 64-byte, 8-byte-aligned foreign scratch owner;
3. creates one exclusive checked write region inside an explicit unsafe block;
4. passes its non-null opaque pointer to one no-retain/no-unwind symbol;
5. returns only the foreign i64 status from the unsafe value block;
6. translates rejection, foreign failure, stale generation, invalid status, and
   impossible returned length before observing ordinary bytes;
7. validates a complete versioned 24-byte record in portable Core source;
8. copies four payload bytes into an independently owned immutable value; and
9. emits one exact 62-byte report with SHA-256
   `c0a915258a1d23e50599c51f208465768368683158b8d9a17af2b981999961cd`.

No pointer, raw address, scratch owner, region, ABI witness, unsafe handle, host
layout, or foreign status enters the published safe record.

## Source modules

| Module | Profile | Responsibility |
| --- | --- | --- |
| `Foreignˉrecordˉtypes` | Core | Safe limits, record, normalized failures, result. |
| `Foreignˉrecordˉdecode` | Core | Exact range/field validation and owned payload copy. |
| `Foreignˉrecordˉreport` | Core | Canonical text from the safe record only. |
| `Foreignˉrecordˉsystem` | System | ABI declaration, scratch/region/pointer, unsafe call, translation. |
| `Foreignˉrecordˉapplication` | System | Limits, budgets, orchestration, final safe publication. |

The three Core modules target Windows, Linux, and Windvale. The two System
modules target only `linux.x86_64.sysv_amd64_c_v1`. A future Windows binding is
a separate target-scoped System adapter over the same Core decoder, not
conditional source or a different language.

## Evidence index

- [foreign ABI contract](Foreign-Abi-Contract.md)
- [record format](Record-Format.md)
- [package and execution plan](Package-Plan.md)
- [semantic review](Semantic-Review.md)
- [rejected and boundary cases](Rejected-Cases.md)
- [expected outcomes](Expected-Outcomes.md)
- [implementation responsibilities](Implementation-Responsibilities.md)
- [review findings](Review-Findings.md)

## Acceptance answer

Language 1.0 keeps unsafe work small and reviewable: 23 source lines across the
foreign declaration and the only unsafe value block, followed by ordinary typed
translation and Core decoding. Exact ABI identity plus borrow-tied caller-owned
memory is sufficient; no pointer-sized integer, C-layout inheritance, general
exception, preprocessor, or capability bypass is needed.

## Nonclaims

This is not libc binding policy, a stable Windvale product ABI, a Windows ABI,
an arbitrary C header importer, callback model, variadic call, C++ interop,
kernel syscall ABI, DMA API, device driver, or proof that an already corrupted
process can recover safely.
