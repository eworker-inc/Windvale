# Windvale native execution context

## Status and scope

Version 1 of this context is retained by the cross-host-qualified `x86-64-wvb-baseline-v7` implementation at exact candidate `8d375bf`. It carries per-run resource limits and a versioned runtime-service table without making generated code depend on the Windows x64 or System V x86-64 calling convention. [Decision 0065](../Documents/Decisions/0065-Versioned-Native-Execution-Context-And-Console-Service.md) qualifies the context and first service at ABI 6; [Decision 0066](../Documents/Decisions/0066-Borrowed-Bytes-And-Unsigned-Native-Values.md) qualifies the value and call representation at ABI 7 without changing the context bytes.

This is an experimental native ABI, not a stable public foreign-function interface. ABI 7 replaces ABI 6 in the current implementation. Qualified older artifacts remain historical evidence and are not accepted by the ABI-7 fragment verifier.

## Entry convention

The exported native `Main() -> i32` receives a pointer to one execution context in `RDX`. The Windows executor duplicates that pointer into its second and third bridge arguments so both Windows x64 and System V x86-64 place the same value in `RDX`. Generated `Main` preserves `R15`, copies the context pointer into `R15`, and loads the instruction and call-depth budgets into the shared `R11` and `R10` counters.

Internal functions accept as many as four parameters. `i32`, `bool`, `u8`, and `u32` values use `R8D`, `R9D`, `ECX`, and `EDX`. A borrowed `bytes` parameter uses the corresponding 64-bit register as a pointer to the caller's verified 16-byte descriptor; the callee copies the descriptor into its own frame before the call can return. Packed scalar value/status returns use `RAX`, and every internal call preserves the shared resource counters. Borrowed bytes and static text are not return types in this slice.

## Value-slot and borrowed-byte layout

Each native local and temporary owns one zero-initialized 16-byte frame slot. Scalars use the low four bytes. The wider slot is a universal representation boundary for values that require more than one machine word; it is not a heap object or a claim that every future Windvale value will use this exact shape.

An immutable borrowed `bytes` value occupies one slot:

| Offset | Bytes | Field | ABI-7 rule |
| ---: | ---: | --- | --- |
| 0 | 8 | data pointer | Points into verified immutable fragment read-only data |
| 8 | 4 | byte length | Exact remaining span length |
| 12 | 4 | reserved | Zero |

Static byte constants create this descriptor through a verified RIP-relative data reference. `Bytesˉslice` produces another borrowed descriptor only after unsigned offset/length bounds checks. `Bytesˉreadˉu8`, `Bytesˉreadˉu16ˉlittle`, `Bytesˉreadˉu32ˉlittle`, and `Bytesˉreadˉi32ˉlittle` check the complete fixed-width range before reading. A failed slice or read returns packed status 6 and becomes `WVR3008`; no host signal or unchecked pointer access is the language-level failure path.

ABI 7 also admits `u8` constants/equality, `u32` constants and unsigned comparisons, checked `u32` addition/subtraction/multiplication, and the lossless `U32ˉfromˉu8` conversion. Unsigned arithmetic overflow retains packed status 1 / `WVR3007`.

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

The baseline accepts a hosted module only when it declares the exact canonical `console.write_line(text) -> void` capability and no other capability. Its argument must resolve to one immutable module text object encoded as strict UTF-8. Static-text locals are admitted only when lowering and native-IR validation prove that every store and load retains one identical immutable data identity. Dynamic text, concatenation, text parameters, and ambiguous local provenance remain outside ABI 7.

Generated code calls the service-table entry with this Windvale-owned internal convention:

- `R8` is the address of verified immutable UTF-8 bytes inside the fragment;
- `R9D` is their bounded byte length; and
- `EAX` is zero on success and nonzero on service failure.

The runtime owns one exact 70-byte platform thunk. On Windows it maps `R8`/`R9D` to `RCX`/`EDX`; on Linux it maps them to `RDI`/`ESI`. Both variants preserve `R10`, `R11`, and `R15`, align the native stack, call a bounded managed adapter during Stage 0, restore the original stack, and return the adapter status. Generated fragment bytes remain identical across hosts.

Before allocating executable memory, the runtime requires explicit authorization for `console.write_line` (`WVR3010`) and an actual output implementation (`WVR3001`). The adapter revalidates that the complete byte range lies inside the already verified fragment, enforces the WVB UTF-8 byte limit, decodes strict UTF-8, writes the text followed by LF, and converts adapter exceptions to packed status 5 / `WVR3013`. No managed exception unwinds through generated machine code.

## Verification and publication

The native fragment verifier independently decodes the exact context prologue, `R15` restoration on every exit, typed 16-byte frame access, descriptor construction/copy, byte-source provenance, unsigned bounds branches, fixed-width reads, scalar operations, internal argument forms, service-table loads, service call, failure edges, immutable data targets, relocations, and packed statuses. Corrupt instruction bytes, descriptor fields, argument forms, displacement bytes, service metadata, or immutable data fail before WVO serialization or writable-to-executable publication.

The current `Nativeˉfragment` carries its required-service list beside code, symbols, and patches. WVO 1.0 does not serialize that list. A service-bearing linked image may therefore execute only while paired with its original verified fragment metadata; it is not yet a standalone native application. A future PE, ELF, or Windvale-native container must preserve and verify capability/service requirements before it can publish independently loadable hosted AOT modules.

## Windvale OS use

The version-2 native kernel bridge is byte-compatible with the unchanged context, while portable kernel probe version 3 supplies exact budgets `271` and `2` with a zero service-table pointer. The ordinary portable module loops over immutable i32 data, passes borrowed bytes through an internal function, slices and reads them, and checks `u8`/`u32` results. Firmware probe version 9 emits `native-context=pass` only after that ABI-7 path and the special-kernel path both succeed.

This does not give Windvale OS a runtime service table, WVB loader, verifier, JIT, or hosted capability implementation. It proves that the same ABI-7 generated code consumes one explicit versioned context and the same borrowed-byte representation in the host runtime and existing AOT OS path.
