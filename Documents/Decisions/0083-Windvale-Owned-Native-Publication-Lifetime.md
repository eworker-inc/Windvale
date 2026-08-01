# Decision 0083: Windvale-owned native publication lifetime

- Date: 2026-08-01
- Status: Accepted and implemented candidate; cross-host qualification pending
- Extends: [Decision 0082](0082-Windvale-Owned-Native-Publication-Layout.md)
- Preserves: Native ABI 14, execution-context version 6, service-table version 4, WVB 1.6, WVO 1.0, kernel bridge 9, both qualified firmware-probe-17 identities, and all final runtime-service leaf identities

## Context

Decision 0082 makes Windvale the authority for executable-image extent and service placement, but its executor still contains unstructured allocation, copying, protection, invocation, and release calls. A `finally` block guarantees release, yet the allowed lifecycle is implicit in C# control flow and the raw executable address is shared across that larger executor.

Moving operating-system calls into ordinary Windvale source would require raw pointers, native handles, platform imports, and a general lifetime-safe FFI before the runtime can safely publish that same source. The next transfer should instead separate policy from authority: Windvale chooses the closed lifecycle, while one internal host owner performs only the selected Windows or Linux operation.

## Decision

- Accept the bounded internal [`WVLQ 1` request and `WVLT 1` response contract](../../Specifications/Windvale-Native-Publication-Lifetime.md).
- Add portable `Compiler/Windvale/Native-Publication-Lifetime-Core.wv`. It validates one accepted image extent and emits the complete exact nine-transition graph for unallocated, writable, copied, executable, invoked, and released states.
- Add hosted `Compiler/Windvale/Native-Publication-Lifetime-Bridge.wv` and retain its exact WVB in `Runtime/Windvale.Native/Consumers`. It reads one immutable in-memory request through its sole `file.read_bytes` capability and returns the plan through Decision 0080's bounded byte-result path under the Stage 0 reference interpreter.
- Make every ordinary native execution evaluate and independently reconstruct the Windvale lifetime plan before allocating executable memory. Planner rejection maps to `WVN4015`; a malformed response or forged in-process plan maps to `WVN4016`.
- Move all executable-image operating-system calls and actual state into one internal `Nativeˉexecutableˉimage` owner. Its raw address is not public outside the runtime assembly. Before allocation, copy, seal, invocation, or release, the owner requires the exact accepted transition; an invalid attempt maps to `WVN4017` before the operation.
- Permit `Release` from writable, copied, executable, and invoked states so deterministic cleanup does not depend on completing the normal path. Preserve the existing policy that a failed platform release is surfaced rather than silently swallowed.
- Keep service/context construction, runtime arenas, result cells, hosted-resource tables, status mapping, and native fragment compilation/verification in their existing Stage 0 owners. This decision moves executable-image lifecycle policy and isolates platform authority; it does not claim complete execution-lifetime ownership.

## Why the ABI and OS images are retained

The fragment bytes, entry convention, context, service table, runtime-private tables, service leaves, invocation arguments, packed statuses, and observable results do not change. The lifecycle plan governs when the host may allocate, copy, protect, invoke, and release one already-verified image. The service-free OS probe consumes a linked AOT image and does not use this host JIT publication owner. Advancing ABI 14 or rebuilding probe 17 would describe no guest-visible contract change.

## Evidence contract

- Portable `Native-Publication-Lifetime-Core.wvb`: 4,954 bytes, SHA-256 `52b1cb6dd0d7fa9d17c1cba50b527912876e4acf1cd9663846ce915b4c56aed5`.
- Retained hosted `Native-Publication-Lifetime-Bridge.wvb`: 4,857 bytes, SHA-256 `74dfaf40bb6ea83f0fd72757c9c4cb85f5c8dd28a41f3993325871d348e88d32`.
- The bridge has exactly one capability, `file.read_bytes`, and exactly one `Main() -> bytes` export.
- Focused coverage must exercise minimum and maximum extents, deterministic output, every request status family, every response header and transition field, unknown/maximum values, forged plans, every normal transition, release from partial writable/copied/executable states, duplicate/out-of-order host actions, retained-source reproduction, and live native execution through the owner.
- Complete qualification must reproduce both WVBs and the retained bridge on Windows and Debian x64, compare all portable artifacts and normalized contracts, pass both Seed and OS suites, and retain both pinned-QEMU probe-17 identities unless an applicable contract changes.

## Consequences and limits

Windvale now owns the allowed executable-image state/action graph, and one small internal host owner contains the raw address plus every executable-memory platform call. The larger executor no longer imports or calls `VirtualAlloc`, `VirtualProtect`, `FlushInstructionCache`, `VirtualFree`, `mmap`, `mprotect`, or `munmap` directly.

The platform functions themselves remain C# P/Invoke authority. Windvale does not yet own native pointer types, platform imports, service/context/arena/result-cell lifetime, a native loader, code cache, concurrent publication, signal containment, or process isolation. The next transfer should be selected from measured owner boundaries rather than widening this contract into a general FFI.

## Reconsider when

- A Windvale-native platform adapter can receive opaque owned regions without arbitrary pointer escape.
- A code cache requires shared, concurrent, or reference-counted executable-image lifetime.
- An isolated compiler/runtime process changes failure containment or release guarantees.
- Native compiler execution makes service/context/arena lifetime a larger bottleneck than image publication.
