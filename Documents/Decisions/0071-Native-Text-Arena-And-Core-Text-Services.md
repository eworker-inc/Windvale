# Decision 0071: Native text arena and core text services

- Date: 2026-08-01
- Status: Implemented; cross-host qualification pending
- Refines: [Decision 0069](0069-Dynamic-Native-Text-And-Complete-Wvdump.md)'s execution-owned text arena
- Extends: [Decision 0070](0070-First-Runtime-Native-Utf8-Service.md)'s exact runtime-native service pattern
- Advances: Native ABI 11, execution-context version 3, kernel native bridge 6, and firmware probe 13

## Context

ABI 10 cross-host qualifies one fixed 16 MiB text arena and the complete Windvale-written `wvdump`, but the arena's allocation cursor exists only as private C# executor state. Decision 0070 proves that an allocation-free service can execute as one identical x86-64 leaf on Windows and Linux. Allocation-bearing leaves cannot safely follow without an explicit state owner visible to both native leaves and the managed services that remain during migration.

Putting a process address or mutable cursor into WVB, WVO, exact service bytes, or an undocumented service-table tail would make the portable or serialized contracts depend on one execution. A parallel native-only allocator would allow native and managed results to overlap. The existing execution-context pointer in preserved register `R15` is the explicit per-run boundary already shared by generated code and services.

## Decision

- Advance the experimental target to `x86-64-wvb-baseline-v11` and native ABI 11. ABI-10 fragments remain historical evidence and are not admitted through a compatibility branch.
- Advance the execution context from version 2 / 48 bytes to version 3 / 72 bytes. Preserve every existing field and append text-arena pointer, length, and used fields, a service-failure detail, and a required-zero reserved field.
- Retain service-table version 4, its 96-byte layout, all service numbers, WVB 1.6, and WVO 1.0. The service interface did not change; only the per-execution state available behind it changed.
- Give one executor ownership of one fixed 16 MiB monotonic text arena. Context `text-arena-used` starts at zero. Native and managed allocation-bearing services read and advance that same checked field. Each result retains the 1 MiB WVB UTF-8 value limit, and no descriptor may outlive the execution.
- Define service-failure detail `0` as none, `1` as text-value limit, and `2` as text-arena exhaustion. A native leaf clears the field on entry, publishes an exact detail before returning nonzero, and never throws across machine code. Packed service status 5 remains unchanged; the executor maps details to `WVR3012` and `WVR3018`, with unknown detail retaining `WVR3013`.
- Replace the managed/platform-adapter implementations of `Textˉconcat`, `I32ˉformat`, and `U32ˉformat` with identical platform-neutral x86-64 leaves. Preserve `R10`, `R11`, and `R15` and use only the existing verified service arguments and descriptor result cell.
- Require exact service identity before W^X publication:
  - `Textˉconcat`: 249 bytes, SHA-256 `75c5588117e1f5f58a593a23aae6156a3a68a6302df5f50153b977bccbaaa3a0`;
  - `I32ˉformat`: 225 bytes, SHA-256 `c33758106e8d7cd31bbed8ef1e789a8e355c52736c119c75493154a4184fa41e`; and
  - `U32ˉformat`: 191 bytes, SHA-256 `b98f2d55e30bb7369e233f94e4ade5f3e8917a7730114446f1ebc81f353e1e43`.
- Keep `Enumˉname` and `Textˉquote` managed for this slice. Enum metadata ownership and UTF-8-native deterministic quoting are separate bounded migrations. Hosted console, diagnostic, argument, and file services also remain managed platform adapters.
- Preserve the managed reference runtime as the oracle. Differential coverage includes signed minimum, unsigned maximum, both zero forms, mixed managed/native arena allocation, exact value-limit and aggregate-exhaustion failures, deterministic service reconstruction, and corrupt service identity.
- Advance the service-free OS bridge to version 6 and firmware probe to version 13 because their AOT consumer is rebuilt through ABI 11/context 3. The bridge constructs the full 72-byte context and supplies zero service-table, record-arena, text-arena, failure-detail, and reserved fields; the guest still performs no dynamic allocation.
- Do not describe the result as a Windvale-written runtime. C# still constructs and publishes the leaves, owns executable memory and arenas, supplies the remaining services, and remains the reference/recovery implementation.

## Candidate evidence

The pre-commit Windows Release build completes with zero warnings. The focused native dynamic-text test passes in 1.011 seconds, including exact service identities, real W^X execution, linked WVO/AOT execution, shared managed/native allocation, `WVR3012`, and `WVR3018`. The complete Windvale-written `wvdump` test passes in 0.843 seconds, and the existing native UTF-8 service test passes in 0.328 seconds. Windows Standard passes all 56 tests in 235.402 seconds. All 15 deterministic OS tests pass.

The portable kernel WVB remains 929 bytes with SHA-256 `0653613d868abbba99b5e31230fb2a1f92581c4989318577cb77a6d6e60f8339`; its service-free WVO remains 8,010 bytes with SHA-256 `f3d0d2aec5b7fb81d02e4188fb6ba48b6a21dc91c89bdf7f00daaf7b0a981038`. The candidate 118-byte bridge code produces a 330-byte WVO with SHA-256 `8b28ed85af29baa65810e0ed0ce8e2893e9696cebd666ccf72a1a53f68cde2b9`. Candidate firmware probe 13 is 15,872 bytes with SHA-256 `ceffc3e33bf007e47b109f3b6a71db2fdceac3c0e908d1471f056909ee42532d`. Pinned QEMU 11.0/Q35/TCG emits the complete probe-13 success marker and returns guest-controlled host exit code 1.

These are development results, not cross-host qualification. Exact-commit Windows/Debian Qualification, artifact comparison, both exact-archive OS suites, and GitHub verification remain required before this decision becomes qualified.

## Consequences

Four of the six deterministic pure runtime-service operations now execute without a managed callback or platform calling-convention adapter: UTF-8 validation from Decision 0070 plus concatenation and both integer formatters here. The text arena has one explicit shared owner rather than coincident managed and native cursors.

ABI 11 changes only the internal experimental native seam. Canonical WVB remains the portable identity, service-table slots remain stable, and the complete `wvdump` source and semantics do not change. The service-free OS image changes because its exact context constructor changes.

Native enum-name lookup needs a bounded representation of verified nominal metadata. Native quoting needs a byte-oriented implementation of the specified UTF-16-code-unit escape semantics or a deliberately revised text representation. Hosted services need narrow native Windows/Linux adapters. Those remain separate slices.

## Reconsider when

- Concurrent execution requires an atomic, thread-local, segmented, or reclaiming allocator instead of one single-threaded monotonic arena.
- Native exceptions or a richer trap record replace the small service-failure detail field.
- A standalone PE, ELF, or Windvale-native container must construct and verify context ownership without the Stage 0 executor.
- A Windvale-written allocator/service object can replace the Stage 0 byte builder while preserving exact behavior and recovery provenance.
