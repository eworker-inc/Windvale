# Windvale native execution context

## Status and scope

Version 1 of this context is the internal contract for `x86-64-wvb-baseline-v6`. It carries per-run resource limits and a versioned runtime-service table without making generated code depend on the Windows x64 or System V x86-64 calling convention. [Decision 0065](../Documents/Decisions/0065-Versioned-Native-Execution-Context-And-Console-Service.md) defines the first implementation slice.

This is an experimental native ABI, not a stable public foreign-function interface. ABI 6 replaces ABI 5 in the current implementation; qualified ABI-5 artifacts remain historical evidence and are not accepted by the ABI-6 fragment verifier.

## Entry convention

The exported native `Main() -> i32` receives a pointer to one execution context in `RDX`. The Windows executor duplicates that pointer into its second and third bridge arguments so both Windows x64 and System V x86-64 place the same value in `RDX`. Generated `Main` preserves `R15`, copies the context pointer into `R15`, and loads the instruction and call-depth budgets into the shared `R11` and `R10` counters.

Internal functions retain ABI 5's qualified scalar convention: as many as four `i32`/`bool` parameters use `R8D`, `R9D`, `ECX`, and `EDX`; packed value/status returns use `RAX`; and every internal call preserves the shared resource counters. Static text is not a general parameter or return type in this slice.

## Execution-context memory layout

All integer fields are little-endian. The context is exactly 32 bytes:

| Offset | Bytes | Field | Version-1 rule |
| ---: | ---: | --- | --- |
| 0 | 4 | format version | `1` |
| 4 | 4 | structure bytes | `32` |
| 8 | 8 | instruction budget | Positive maximum charged with WVB instruction semantics |
| 16 | 8 | call-depth budget | Positive maximum active native call depth |
| 24 | 8 | service-table pointer | Zero when no runtime service is required; otherwise points to the exact table below |

The platform executor or verified OS bridge owns this memory for the complete call. Ordinary generated code does not retain the pointer after return. The current adapters construct the exact version and size; the exact generated prologue and every use are independently decoded before publication.

## Runtime-service table

Service-table version 1 is exactly 16 bytes:

| Offset | Bytes | Field | Version-1 rule |
| ---: | ---: | --- | --- |
| 0 | 4 | format version | `1` |
| 4 | 4 | structure bytes | `16` |
| 8 | 8 | `console.write_line` entry | Pointer to the runtime-owned adapter thunk |

The first table is deliberately closed. A fragment may require no service or exactly `console.write_line`; any other or noncanonical service list fails verification. A later table extension requires a new accepted contract, bounds/version handling, and cross-host evidence.

## `console.write_line` service

The baseline accepts a hosted module only when it declares the exact canonical `console.write_line(text) -> void` capability and no other capability. Its argument must resolve to one immutable module text object encoded as strict UTF-8. Static-text locals are admitted only when lowering and native-IR validation prove that every store and load retains one identical immutable data identity. Dynamic text, concatenation, text parameters, and ambiguous local provenance remain outside ABI 6.

Generated code calls the service-table entry with this Windvale-owned internal convention:

- `R8` is the address of verified immutable UTF-8 bytes inside the fragment;
- `R9D` is their bounded byte length; and
- `EAX` is zero on success and nonzero on service failure.

The runtime owns one exact 70-byte platform thunk. On Windows it maps `R8`/`R9D` to `RCX`/`EDX`; on Linux it maps them to `RDI`/`ESI`. Both variants preserve `R10`, `R11`, and `R15`, align the native stack, call a bounded managed adapter during Stage 0, restore the original stack, and return the adapter status. Generated fragment bytes remain identical across hosts.

Before allocating executable memory, the runtime requires explicit authorization for `console.write_line` (`WVR3010`) and an actual output implementation (`WVR3001`). The adapter revalidates that the complete byte range lies inside the already verified fragment, enforces the WVB UTF-8 byte limit, decodes strict UTF-8, writes the text followed by LF, and converts adapter exceptions to packed status 5 / `WVR3013`. No managed exception unwinds through generated machine code.

## Verification and publication

The native fragment verifier independently decodes the exact context prologue, `R15` restoration on every exit, service-table loads, service call, failure edge, UTF-8 data target, length, relocation, and packed status. Corrupt instruction bytes, displacement bytes, service metadata, or UTF-8 data fail before WVO serialization or writable-to-executable publication.

The current `Nativeˉfragment` carries its required-service list beside code, symbols, and patches. WVO 1.0 does not serialize that list. A service-bearing linked image may therefore execute only while paired with its original verified fragment metadata; it is not yet a standalone native application. A future PE, ELF, or Windvale-native container must preserve and verify capability/service requirements before it can publish independently loadable hosted AOT modules.

## Windvale OS use

The version-2 native kernel bridge constructs the same 32-byte context on the kernel-owned stack with exact budgets `203` and `2` and a zero service-table pointer. The OS probe module is portable and capability-free, so any required-service metadata is rejected during image construction. Firmware probe version 8 emits `native-context=pass` only after the aggregate native and special-kernel path succeeds.

This does not give Windvale OS a runtime service table, WVB loader, verifier, JIT, or hosted capability implementation. It proves that the same ABI-6 generated code consumes one explicit versioned context in the host runtime and the existing AOT OS path.
