# Decision 0071: Native text arena and core text services

- Date: 2026-08-01
- Status: Qualified on Windows and Debian x64
- Refines: [Decision 0069](0069-Dynamic-Native-Text-And-Complete-Wvdump.md)'s execution-owned text arena
- Extends: [Decision 0070](0070-First-Runtime-Native-Utf8-Service.md)'s exact runtime-native service pattern
- Advances: Native ABI 11, execution-context version 3, kernel native bridge 6, and firmware probe 13
- Advanced by: [Decision 0356](0356-Windvale-Owned-Native-Integer-Format-Construction.md)

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

## Qualification evidence

The focused native dynamic-text test covers exact service identities, real W^X execution, linked WVO/AOT execution, shared managed/native allocation, `WVR3012`, and `WVR3018`. The complete Windvale-written `wvdump` and existing native UTF-8 service tests continue to pass. The pre-commit Windows Standard suite passes all 56 tests in 235.402 seconds.

Exact commit `88889513e6c6d9ef673f7dcdca761628a430e31f`, tree `875c8626e3e605c5080ce86c1adbb2ec00e960cc`, was archived as 2,885,974 bytes with SHA-256 `98b775a5cae3b0f48a83b9465931707226a96b8dc67bb5302008e58a41318ef3`. The digest and size match after transfer to the isolated Debian GNU/Linux 12 x64 QA host. Windows and Debian with .NET SDK `10.0.302` build Release with zero warnings, pass all 56 tests and exact compiler reproduction, and complete the CLI verifier. The Windows suite takes 233.375 seconds and the Debian suite 248.582 seconds.

The 15,563-byte Windows conformance report has SHA-256 `c34a2199e548631323b2186dda0dcf8ffcb0a3a3c6eb7d53d9a405c314837a4b`; its 11,918-byte timing report has SHA-256 `c08679ef53e9f5b722989feb276ec28e4882f97dadffb9c8614b782477a39ec8`. The 15,473-byte Debian report has SHA-256 `0a8116b03185d7344dd47fb0996c1cc9402c3b9583522574a2a77b0e2fa1f5cf`; its 11,524-byte timing report has SHA-256 `b613d16c834cc6a35de69b8b7eb620dd057ff345d822514f53aef13b6e28fd19`. Their normalized contracts match exactly.

All 61 portable artifacts, totaling 7,752,647 bytes, match byte for byte and retain the canonical manifest SHA-256 `11ac1d4a57fce3648004d7a6002e6124d6e2fbeefc108b31bfe305523b2de0de`. The retrieved 2,297,411-byte Debian evidence bundle has SHA-256 `7e723eeb634e145e7b7a8dcd609ec7f0e7a78e04fe402bf8ea0a54e012997b6a`. Both hosts pass all 15 OS tests. The portable kernel WVB remains 929 bytes with SHA-256 `0653613d868abbba99b5e31230fb2a1f92581c4989318577cb77a6d6e60f8339`; its service-free WVO remains 8,010 bytes with SHA-256 `f3d0d2aec5b7fb81d02e4188fb6ba48b6a21dc91c89bdf7f00daaf7b0a981038`. The 118-byte bridge code produces a 330-byte WVO with SHA-256 `8b28ed85af29baa65810e0ed0ce8e2893e9696cebd666ccf72a1a53f68cde2b9`. Firmware probe 13 is 15,872 bytes with SHA-256 `ceffc3e33bf007e47b109f3b6a71db2fdceac3c0e908d1471f056909ee42532d`; pinned QEMU 11.0/Q35/TCG emits the complete success marker and returns guest-controlled host exit code 1. The Debian QA host does not provide QEMU.

GitHub [Verify run 30694649557](https://github.com/eworker-inc/Windvale/actions/runs/30694649557) passes for the exact candidate. After evidence retrieval and comparison, the resolved QA directory, transferred archive, remote evidence bundle, and temporary QA inputs were removed and confirmed absent.

## Consequences

Four of the six deterministic pure runtime-service operations now execute without a managed callback or platform calling-convention adapter: UTF-8 validation from Decision 0070 plus concatenation and both integer formatters here. The text arena has one explicit shared owner rather than coincident managed and native cursors.

ABI 11 changes only the internal experimental native seam. Canonical WVB remains the portable identity, service-table slots remain stable, and the complete `wvdump` source and semantics do not change. The service-free OS image changes because its exact context constructor changes.

Native enum-name lookup needs a bounded representation of verified nominal metadata. Native quoting needs a byte-oriented implementation of the specified UTF-16-code-unit escape semantics or a deliberately revised text representation. Hosted services need narrow native Windows/Linux adapters. Those remain separate slices.

## Reconsider when

- Concurrent execution requires an atomic, thread-local, segmented, or reclaiming allocator instead of one single-threaded monotonic arena.
- Native exceptions or a richer trap record replace the small service-failure detail field.
- A standalone PE, ELF, or Windvale-native container must construct and verify context ownership without the Stage 0 executor.
- A Windvale-written allocator/service object can replace the Stage 0 byte builder while preserving exact behavior and recovery provenance.
