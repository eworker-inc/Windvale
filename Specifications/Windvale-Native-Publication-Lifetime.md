# Windvale native publication lifetime

## Status and scope

`WVLQ 1` and `WVLT 1` are versioned internal contracts between the Stage 0 native executor and the retained Windvale publication-lifetime planner. They define the complete allowed state/action graph for one already-planned executable image. They do not carry machine bytes, native addresses, handles, operating-system error values, or arbitrary calls.

The contract is separate from [`WVPQ 1` and `WVPL 1`](Windvale-Native-Publication-Plan.md). `WVPL` owns image extent and service placement. `WVLT` owns the lifetime actions permitted after that exact image extent is accepted. Neither format is a public application, object, cache, or executable-container format.

All integers are unsigned 32-bit little-endian values. Every reserved field is zero. Unknown versions, trailing bytes, missing bytes, unknown status values, altered transitions, or image extents outside 1 through 34 MiB are rejected.

## Request envelope: `WVLQ 1`

The request is exactly 20 bytes.

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVLQ`, encoded as `0x514c5657` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | Exactly `20` |
| 12 | 4 | image bytes | Exact accepted `WVPL` extent, `1` through `35,651,584` |
| 16 | 4 | reserved | Zero |

## Response envelope: `WVLT 1`

A successful response is exactly 140 bytes: a 32-byte header followed by nine 12-byte transitions.

| Offset | Bytes | Field | Rule |
| ---: | ---: | --- | --- |
| 0 | 4 | magic | ASCII `WVLT`, encoded as `0x544c5657` |
| 4 | 4 | version | `1` |
| 8 | 4 | total bytes | Exactly `140` |
| 12 | 4 | status | Zero (`Valid`) |
| 16 | 4 | failure offset | Exact accepted request length, `20` |
| 20 | 4 | image bytes | Exact request value |
| 24 | 4 | transition count | Exactly `9` |
| 28 | 4 | reserved | Zero |

Each transition record contains `state`, `action`, and `next_state` as three 32-bit values. Version 1 defines these states:

| Value | State | Meaning |
| ---: | --- | --- |
| 0 | `Unallocated` | No executable-image allocation exists. |
| 1 | `Writable` | One writable, non-executable image allocation exists. |
| 2 | `Copied` | The complete exact image has been copied while still non-executable. |
| 3 | `Executable` | The image is read/execute and no longer writable. |
| 4 | `Invoked` | The verified entry returned to the host. |
| 5 | `Released` | The image allocation no longer exists. |

Version 1 defines these actions:

| Value | Action | Meaning |
| ---: | --- | --- |
| 1 | `Allocateˉwritable` | Allocate the exact bounded extent as writable and non-executable. |
| 2 | `Copyˉimage` | Copy the complete planned image exactly once. |
| 3 | `Sealˉexecutable` | Remove write permission, add execute permission, and perform the platform instruction-cache publication required for the range. |
| 4 | `Invoke` | Invoke one independently verified entry through the qualified native ABI. |
| 5 | `Release` | Release the complete image allocation. |
| 6 | `Complete` | Confirm the terminal released state without another host action. |

The exact ordered transition table is:

| Current state | Action | Next state | Purpose |
| --- | --- | --- | --- |
| `Unallocated` | `Allocateˉwritable` | `Writable` | Start normal publication. |
| `Writable` | `Copyˉimage` | `Copied` | Continue normal publication. |
| `Writable` | `Release` | `Released` | Clean up after a copy or pre-copy failure. |
| `Copied` | `Sealˉexecutable` | `Executable` | Continue normal publication. |
| `Copied` | `Release` | `Released` | Clean up after a seal failure. |
| `Executable` | `Invoke` | `Invoked` | Enter the verified native program. |
| `Executable` | `Release` | `Released` | Clean up after an invocation failure. |
| `Invoked` | `Release` | `Released` | Complete normal lifetime or clean up after post-call validation fails. |
| `Released` | `Complete` | `Released` | Prove the terminal state is idempotent. |

There is deliberately no executable-to-writable transition, second copy, second seal, second invocation, partial release, or operation after release.

## Failure response

A rejected request produces an exact 32-byte `WVLT 1` header with zero image extent and transition count. `failure_offset` identifies the first relevant request field or boundary.

| Value | Name | Meaning |
| ---: | --- | --- |
| 0 | `Valid` | The complete canonical transition table follows. |
| 1 | `Invalidˉsize` | Truncation, declared-size mismatch, trailing bytes, or a non-20-byte request. |
| 2 | `Invalidˉmagic` | Request magic differs. |
| 3 | `Invalidˉversion` | Request version differs. |
| 4 | `Invalidˉreserved` | The reserved field is nonzero. |
| 5 | `Invalidˉimage` | Image extent is zero or above 34 MiB. |

A planner rejection maps to `WVN4015`. A malformed successful response or forged in-process plan maps to `WVN4016`. An attempted host action outside the accepted current-state transition maps to `WVN4017` before that operation occurs.

## Windvale owner and host adapter

`Compiler/Windvale/Native-Publication-Lifetime-Core.wv` is portable and owns request validation, status selection, and canonical transition construction. [Decision 0365](../Documents/Decisions/0365-Native-Publication-Planner-Execution.md) makes `Compiler/Windvale/Native-Publication-Lifetime-Bridge.wv` a capability-free portable `Main(bytes) -> bytes` wrapper. The current core is 4,955 bytes with SHA-256 `a9e540c5c9ddaaeb4f45ab08a902a0a9019ce8155d544e319485c023b7d485d3`; the retained bridge is 4,442 bytes with SHA-256 `f966e7f7553def7f3d57be0d3bed67b1b010f0e2cd4907c4ef78760a140fd554`.

The retained bridge is digest-checked, verified, lowered, and run as a service-free native byte-input fragment. A narrow bootstrap supplies only the already accepted nine-transition lifetime needed to publish that planner without recursion. The host independently reconstructs every returned header field and transition, then passes only that verified immutable plan to one internal executable-image owner. The owner keeps the raw address private to the runtime assembly, tracks actual state, checks the accepted transition before each operation, and releases from every post-allocation partial state. The reference interpreter remains differential and recovery evidence.

Windows `VirtualAlloc`/`VirtualProtect`/`FlushInstructionCache`/`VirtualFree` and Linux `mmap`/`mprotect`/`munmap` remain narrow authority adapters inside that owner. Windvale chooses permitted lifecycle actions; it does not receive raw pointers or call a general FFI. Context, service-table, arena, result-cell, and hosted-resource lifetimes remain separate Stage 0 owners.

Any change to an envelope, limit, state, action, transition, status, or failure offset requires a new accepted contract version, regenerated retained WVB identity, hostile-input coverage, and exact Windows/Debian qualification.

[Decision 0515](../Documents/Decisions/0515-Native-Hosted-Construction-Build-And-Inspection-Transfer.md)
makes the paired native Project 1 helper the ordinary build and inspection
owner for the publication-lifetime core and bridge. The broad scripts retain
their exact retained-bridge WVB and WVNF comparisons, while capability-bearing
publication and the Stage 0 behavioral oracle remain separate boundaries.
