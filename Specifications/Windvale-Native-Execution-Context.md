# Windvale native execution context

## Status and scope

Version 2 is implemented by the `x86-64-wvb-baseline-v9` candidate. It adds one execution-owned record arena while retaining per-run resource limits and a versioned runtime-service table without making generated code depend on the Windows x64 or System V x86-64 calling convention. [Decision 0065](../Documents/Decisions/0065-Versioned-Native-Execution-Context-And-Console-Service.md) qualifies the context and first service at ABI 6; [Decision 0066](../Documents/Decisions/0066-Borrowed-Bytes-And-Unsigned-Native-Values.md) qualifies the value and call representation at ABI 7; [Decision 0067](../Documents/Decisions/0067-Borrowed-Hosted-Input-And-First-Native-Wvb-Inspector.md) qualifies ABI 8's borrowed hosted-input boundary; [Decision 0068](../Documents/Decisions/0068-Bounded-Native-Nominal-Values-And-Wvdump-Structural-Core.md) defines the ABI-9 candidate.

This is an experimental native ABI, not a stable public foreign-function interface. ABI 9 replaces ABI 8 in the current implementation. Qualified older artifacts remain historical evidence and are not accepted by the ABI-9 fragment verifier.

## Entry convention

The exported native `Main() -> i32` receives a pointer to one execution context in `RDX`. The Windows executor duplicates that pointer into its second and third bridge arguments so both Windows x64 and System V x86-64 place the same value in `RDX`. Generated `Main` preserves `R15`, copies the context pointer into `R15`, and loads the instruction and call-depth budgets into the shared `R11` and `R10` counters.

Internal functions accept as many as four parameters. `i32`, `bool`, `u8`, `u32`, enums, and record-arena offsets use `R8D`, `R9D`, `ECX`, and `EDX`. A borrowed `text` or `bytes` parameter uses the corresponding 64-bit register as a pointer to the caller's verified 16-byte descriptor; the callee copies the descriptor into its own frame before the call can return. Packed scalar/enum/record value and status returns use `RAX`, and every internal call preserves the shared resource counters. Borrowed descriptors are not function return types in this slice.

## Value-slot and borrowed-descriptor layout

Each native local and temporary owns one zero-initialized 16-byte frame slot. Scalars use the low four bytes. The wider slot is a universal representation boundary for values that require more than one machine word; it is not a heap object or a claim that every future Windvale value will use this exact shape.

An immutable borrowed `text` or `bytes` value occupies one slot:

| Offset | Bytes | Field | ABI-9 rule |
| ---: | ---: | --- | --- |
| 0 | 8 | data pointer | Points into verified fragment data or one execution-owned immutable host buffer |
| 8 | 4 | byte length | Exact remaining span length |
| 12 | 4 | reserved | Zero |

Static text and byte constants create descriptors through verified RIP-relative data references. A native service may return a descriptor backed by its execution-owned immutable buffer; that borrow expires when the native call returns. `Bytesˉslice` produces another borrowed descriptor only after unsigned offset/length bounds checks. `Bytesˉreadˉu8`, `Bytesˉreadˉu16ˉlittle`, `Bytesˉreadˉu32ˉlittle`, and `Bytesˉreadˉi32ˉlittle` check the complete fixed-width range before reading. A failed slice or read returns packed status 6 and becomes `WVR3008`; no host signal or unchecked pointer access is the language-level failure path.

An enum occupies the low four bytes and retains its signed member value plus compile-time nominal identity. A record occupies the low four bytes as an offset into the current execution's record arena. Each immutable record field consumes one complete 16-byte arena cell. Record construction checks offset addition and arena capacity before copying typed cells; field access proves the complete selected cell is below the committed used boundary. Packed status 7 becomes `WVR3017` on arena exhaustion.

ABI 9 retains ABI 8's borrowed values and ABI 7's `u8`/`u32` operations and checked arithmetic. Unsigned arithmetic overflow retains packed status 1 / `WVR3007`.

## Execution-context memory layout

All integer fields are little-endian. The context is exactly 48 bytes:

| Offset | Bytes | Field | Version-2 rule |
| ---: | ---: | --- | --- |
| 0 | 4 | format version | `2` |
| 4 | 4 | structure bytes | `48` |
| 8 | 8 | instruction budget | Positive maximum charged with WVB instruction semantics |
| 16 | 8 | call-depth budget | Positive maximum active native call depth |
| 24 | 8 | service-table pointer | Zero when no runtime service is required; otherwise points to the exact table below |
| 32 | 8 | record-arena pointer | Execution-owned base; may be zero only when arena length is zero and the module performs no record construction |
| 40 | 4 | record-arena bytes | At most 1 MiB in the current host executor |
| 44 | 4 | record-arena used bytes | Starts at zero; generated checked construction advances it in 16-byte cells |

The platform executor or verified OS bridge owns this memory for the complete call. Ordinary generated code does not retain the pointer after return. The current adapters construct the exact version and size; the exact generated prologue and every use are independently decoded before publication.

## Runtime-service table

Service-table version 3 is exactly 48 bytes:

| Offset | Bytes | Field | Version-3 rule |
| ---: | ---: | --- | --- |
| 0 | 4 | format version | `3` |
| 4 | 4 | structure bytes | `48` |
| 8 | 8 | `console.write_line` entry | Pointer to the runtime-owned adapter thunk |
| 16 | 8 | `process.argument_count` entry | Pointer to the runtime-owned adapter thunk |
| 24 | 8 | `process.argument` entry | Pointer to the runtime-owned adapter thunk |
| 32 | 8 | `file.read_bytes` entry | Pointer to the runtime-owned adapter thunk |
| 40 | 8 | `Textˉutf8ˉisˉvalid` entry | Pointer to the runtime-owned pure validation thunk |

The table is deliberately closed. A fragment may require any distinct canonical-order subset of these five services; any unknown, duplicate, or noncanonical service list fails verification. The first four entries are capability services and retain explicit authorization. `Textˉutf8ˉisˉvalid` is deterministic runtime support, has no ambient authority, and requires no capability authorization. A later extension requires a new accepted contract, bounds/version handling, and cross-host evidence.

## `console.write_line` service

The baseline accepts `console.write_line(text) -> void` as one of four exact hosted capabilities. Its argument is a borrowed-text descriptor backed by strict UTF-8 fragment data or an execution-owned host buffer. Text parameters and initialized locals copy descriptors; dynamic concatenation and allocation remain outside ABI 9.

Generated code calls the service-table entry with this Windvale-owned internal convention:

- `R8` is the address of verified immutable UTF-8 bytes;
- `R9D` is their bounded byte length; and
- `EAX` is zero on success and nonzero on service failure.

The runtime owns one exact platform thunk per required service. For console, Windows maps `R8`/`R9D` to `RCX`/`EDX`; Linux maps them to `RDI`/`ESI`. Every variant preserves `R10`, `R11`, and `R15`, aligns the native stack, calls a bounded managed adapter during Stage 0, restores the original stack, and returns the adapter result. Generated fragment bytes remain identical across hosts.

Before allocating executable memory, the runtime requires explicit authorization for `console.write_line` (`WVR3010`) and an actual output implementation (`WVR3001`). The adapter revalidates that the complete byte range lies inside verified fragment data or an execution-owned buffer, enforces the WVB UTF-8 byte limit, decodes strict UTF-8, writes the text followed by LF, and converts adapter exceptions to packed status 5 / `WVR3013`. No managed exception unwinds through generated machine code.

## Hosted input services

`process.argument_count` has no generated-code arguments and returns the prevalidated `u32` count in `EAX`. `process.argument` receives the index in `R8D`, receives a verified output-descriptor address in `R9`, and returns status in `EAX`. `file.read_bytes` receives resource-name pointer/length in `R8`/`R9D`, receives a verified output-descriptor address in `RCX`, and returns status in `EAX`. Platform thunks translate these internal registers to the Windows or System V callback convention.

One `Nativeˉexecutionˉbuffers` owner exists for one native run. Argument strings are strict-UTF-8 encoded once per requested index. File reads use `Hostedˉresourceˉcontext.Readˉfileˉbytes`, so reference and native execution share the same bounded 64-name immutable snapshot cache and hosted adapter error mapping; each returned snapshot is copied once into execution-owned unmanaged storage. Repeated reads reuse the descriptor. All allocations are released after native return.

Service adapters write pointer/length/reserved descriptors only into independently verified frame slots. Resource-name and console input ranges must lie wholly inside fragment data or one registered execution allocation. Runtime resource errors retain their exact `WVR302x` code through packed status 5; unexpected adapter failures use `WVR3013`.

## Pure UTF-8 validation service

`Textˉutf8ˉisˉvalid(bytes) -> bool` passes one proven borrowed-byte pointer/length in `R8`/`R9D` and a verified bool output-cell address in `RCX`. The service writes normalized zero or one and returns status in `EAX`; adapter failure therefore follows the ordinary packed-status-5 path instead of allowing a host exception to cross generated code. Exact platform thunks adapt only those registers. The execution owner revalidates that the complete range belongs to fragment data or a registered immutable execution allocation and applies strict UTF-8 decoding. Invalid encoding writes false; it does not allocate text or gain a capability. This Stage 0 callback is replaceable by the same closed service in a future native runtime.

## Verification and publication

The native fragment verifier independently decodes the exact context prologue, `R15` restoration on every exit, typed 16-byte frame access, descriptor construction/copy, descriptor-source provenance, enum operations, record arena allocation/field copies, unsigned bounds branches, fixed-width reads, scalar operations, internal argument forms, all service-table loads, service calls, failure edges, immutable UTF-8/data targets, relocations, and packed statuses. Corrupt instruction bytes, descriptor fields, arena sizes/offsets, argument forms, displacement bytes, service metadata, or immutable data fail before WVO serialization or writable-to-executable publication.

The current `Nativeˉfragment` carries its required-service list beside code, symbols, and patches. WVO 1.0 does not serialize that list. A service-bearing linked image may therefore execute only while paired with its original verified fragment metadata; it is not yet a standalone native application. A future PE, ELF, or Windvale-native container must preserve and verify capability/service requirements before it can publish independently loadable hosted AOT modules.

## Windvale OS use

The version-4 native kernel bridge constructs context version 2, while the portable kernel probe supplies exact budgets `271` and `2`, a zero service-table pointer, and a zero-length record arena. The ordinary portable module loops over immutable i32 data, passes borrowed bytes through an internal function, slices and reads them, and checks `u8`/`u32` results. Firmware probe version 11 identifies the ABI-9 rebuild and emits `native-context=pass` only after that service-free path and the special-kernel path both succeed.

This does not give Windvale OS a runtime service table, record allocator, WVB loader, verifier, JIT, or hosted capability implementation. It proves that the ABI-9 compiler still supplies service-free generated code through one explicit versioned context and the borrowed-byte representation in the existing AOT OS path.
