# Decision 0148: First WVA native descriptor allocator leaf

- Date: 2026-08-03
- Status: Implemented with local Windows evidence; cross-host qualification pending
- Retains: Native ABI 21, execution-context version 7, service-table version 5, target `x86-64-wvb-baseline-v21`, WVB 1.6, WVO 1.0, the existing 16-byte descriptor, the 16 MiB dynamic-value arena, and every selected ABI-21 machine byte
- Refines: [Decision 0143](0143-Bounded-First-Fit-Dynamic-Arena-Replay.md) and [Decision 0147](0147-Native-Descriptor-Ownership-Plan.md)

## Context

Decision 0143 proves that a 16-byte-header, 16-byte-aligned first-fit allocator can execute the complete exact-compiler allocation trace inside the retained 16 MiB arena. Decision 0147 then publishes and independently reconstructs every descriptor ownership action that a successor native ABI must consume. Neither decision supplies executable allocator mechanics.

Changing the shared selector immediately would combine allocator construction, descriptor layout, execution-context revision, call lowering, independent instruction verification, host-container rebuilds, and a Windvale OS rebuild in one transition. The smaller machine-foundation boundary is an exact WVA-owned allocator leaf that can be assembled, verified, executed, and compared with an independent model before any consumer adopts it.

## Decision

- Add the exported WVA 1 function `Windvale_descriptor_allocator` as one platform-neutral x86-64 allocator candidate. `R8` points to a 32-byte state and `R9` points to a 40-byte version-1 request. The function returns its status in `EAX` and clobbers only `RAX`, `RCX`, `RDX`, `R8`, `R9`, `R10`, and `R11`, which are volatile on both Windows x64 and System V.
- Keep the leaf outside ABI-21 instruction selection. The current descriptor reserved word, execution context, service table, target name, selected fragments, host containers, and OS consumers remain unchanged.
- Represent allocator state as arena pointer, aligned arena length, state magic, address-ordered free-list head token, allocated-block count, charged-byte count, and a zero reserved word. Lazily initialize only an otherwise-zero state over one nonzero 16-byte-aligned arena of 16 through 16,777,216 bytes.
- Represent each request with format, record size, operation, payload size, owner token, status, data pointer, charged size, and a zero reserved word. The request output fields must be zero on entry and are published deterministically.
- Use owner token zero for borrowed storage. A nonzero owner is the allocated block's arena-relative header offset plus one, preserving zero as the borrowed sentinel without publishing a host pointer as ownership identity.
- Give every block a 16-byte in-band header containing charged size, reference count, next-free owner token, and allocated/free magic. Charge `align16(payload + 16)` bytes, accept payloads through the existing 4 MiB byte-value limit, and reject overflow or an inconsistent header before dereferencing the derived block extent.
- Implement operations `acquire`, `retain`, and `release`. Acquisition uses address-ordered first fit, splits only a valid remainder, and publishes one initial reference. Retain increments a checked reference count. Final release inserts by address and immediately coalesces the predecessor, successor, or both; non-final release decrements the reference count.
- Return closed status values for success, invalid request, exhaustion, corrupt state, invalid owner, and reference overflow. Borrowed retain/release require no reference-count change and succeed over a valid allocator state. Stale or forged nonzero tokens cannot release a free block.
- Bound the state counters to the arena capacity and at most 1,048,576 minimum-size blocks. Validate every traversed free-list node, extent, magic, reference state, and strictly increasing link before using it. No allocation or release loop is unbounded independently of the fixed arena.
- Check in the canonical WVO produced from the WVA source and embed it in the native compiler assembly. The loader accepts exactly one aligned `.text` section, one matching exported function, and no relocations, then verifies the complete object and code digests before publication.
- Keep a separately implemented C# reference/recovery model. The conformance test executes the WVA bytes from writable-then-executable memory and compares the complete request, state, and arena byte for byte with the model after every operation.
- Project the verified Decision 0147 ownership plan into allocator-leaf operations before selection. `acquire`, `retain`, and `release` require leaf calls; static/host borrowing, call borrowing, caller result acceptance, and callee return transfer remain ownership movement. This projection is cardinality evidence, not emitted ABI-22 code.

## Local evidence

The canonical object is 3,087 bytes with SHA-256:

```text
75d82e97fcc6652b0153ebed1b849569248ca4371c3c365605f32092a17f4cfb
```

Its single relocation-free `.text` section is 2,989 bytes with SHA-256:

```text
67a8b6648389589b59ca1dd6b6b87e80fafaa31696d496e14be9fcf4711ccf70
```

Stage 0 reassembles the embedded source to that exact object. The bounded loader accepts the canonical object and rejects a changed code byte. Live W^X execution and the independent reference agree byte for byte through four 32-byte allocations, exhaustion, retain/non-final release, address-ordered insertion, predecessor/successor/two-sided coalescing, complete-arena reuse, stale-owner rejection, borrowed no-ops, reference overflow, and a corrupt free-list link. Completion restores one full 128-byte free span.

The exact 328-function compiler's 186,557-action ownership plan projects to 435 acquisitions, 34,772 retains, and 144,983 releases: 180,190 allocator leaf calls. The remaining 6,367 actions move or borrow ownership without changing a reference count. The established action-map digest remains `8681cfd9d8c96e3d5dc70c2b97f62795c2e29b632fb66065f2dea8ca102b0511`.

The allocator, focused ownership, and exact-compiler boundary tests pass locally after zero-warning Release builds. Change-aware Windows verification then completes another zero-warning build and passes all 84 selected Seed tests in 343.962 suite seconds; the golden compiler contract takes 215.835 seconds and the allocator case takes eight milliseconds. This is proportional Windows development evidence rather than cross-host qualification. Because no ABI-21 selected fragment, host container, OS input, or guest artifact changes, QEMU is not rerun.

## Consequences

The next selector transition starts from executable, Windvale-owned allocator mechanics rather than a new C# runtime behavior. The WVA source is the transferred implementation; the C# model remains an independent Stage 0 oracle and recovery seam.

This decision does not yet make native execution reclaim dynamic descriptors. A successor ABI must define the descriptor owner-token field, allocator state placement, operation lowering, success/failure propagation, independent selected-instruction proof, and coordinated host/OS consumer rebuild. That transition should begin with a small descriptor-bearing fixture before selecting the complete compiler.

This is not a garbage collector, general heap, public allocator API, concurrency contract, unwind mechanism, native self-hosting result, or .NET-retirement qualification. WVA assembly and WVO embedding still use the Stage 0 toolchain in the normal build.

## Reconsider when

- A successor ABI cannot call or inline the leaf while preserving the independently reconstructed ownership order and measured Decision 0143 capacity.
- A contained recoverable trap requires cross-frame cleanup rather than terminal arena teardown.
- Concurrency, cycles, mutable shared ownership, or long-lived external resources require an ownership mechanism beyond checked reference counts.
- Cross-host calling-convention evidence reveals a platform-common volatile-register assumption that this leaf does not satisfy.
