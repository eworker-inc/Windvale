# Decision 0459: Native WVB 1.11 verifier admission

- Status: Implemented current-host candidate; dual-host promotion and native hosted packaging pending
- Date: 2026-08-09
- Advances: [Decision 0213](0213-Stage0-Semantic-Freeze-And-Native-Front-Door.md), [Decision 0458](0458-Native-Changed-File-Verification.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [hosted verifier application](../../Specifications/Windvale-Hosted-Verifier-Application.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)

## Context

The qualified read-only verifier predates the complete WVB 1.11 nominal-variant
surface. Current Windvale-written database components compile to canonical WVB,
but their `Found`/`Missing`/`Failure` results require variant declarations and
the `variant.create`, `variant.is_case`, and `variant.payload` instructions.
The portable verifier must admit and type-check those modules before database
tests can leave the managed broad harness.

The executable verifier's central function also exceeded ABI 22's 2,048-cell
frame limit while the missing cases were added. Raising the limit would hide a
real native-lowering boundary and would enlarge every downstream consumer.

## Decision

- Admit nominal variant declarations as WVB 1.11 type kind 3 and nominal shape
  11. Validate declaration ordering, names, duplicate cases, payload shapes,
  nominal references, and the prohibition on nested variant payloads.
- Admit opcodes 151 through 153 structurally and type-check their exact nominal
  type and case operands, payload presence, stack inputs, and stack results.
- Complete the already-frozen arithmetic verifier surface for opcodes 159
  through 172: signed and unsigned division/remainder plus `u8` bitwise and
  shift operations.
- Extract variant and record instruction verification behind focused helpers
  that return the existing immutable operation-state evidence. Shared type
  walking is owned by `Hˉnextˉtype` and `Hˉshapeˉwidth`; variant case lookup is
  owned by `Hˉvariantˉcaseˉat`. This reduces `Hˉexecutable` from 2,211 to 1,683
  locals under the unchanged ABI 22 limit without numbered fragments.
- Put verifier dependencies in the canonical Project 1 source order required by
  the current pinned native driver. Apply the same order to the WVB publisher,
  which embeds the verifier.
- Update recovery golden identities, but do not replace the qualified ordinary
  verifier artifacts until the exact-commit Windows/Linux promotion gate passes.

## Evidence and consequences

The native Project 1 front door builds the 20-function verifier as an exact
148,351-byte WVB with SHA-256
`519c32fda8d95167d54c723a35860eb80663be5f90ff12e4555ffa6031d505e6`.
It also builds the 39-function WVB publisher as 159,328 bytes with SHA-256
`5da26ddb18cdb6511cb6c28b9603e79c7d318696a5371ca4410db47be7bcb219`.

Because the build driver embeds the verifier, the frozen recovery compiler also
reconstructs a new 1,100,882-byte driver WVB with SHA-256
`3829c63064a8c055940a2e4d606f17dfa6f692f6deb049d7a1406003bc86ea50`.
Its recovery-built Windows package is 29,112,320 bytes / SHA-256
`a6e7d312aeb06b5103aafa840556f2166842ec68728d505a7db02f6bf7a9f73a`;
the Linux package is 29,114,368 bytes / SHA-256
`8e4ab2ec3c5f8062f0c7dcccc431543dccd878b88daf9046f83051752fc000e9`.

The frozen Stage 0 recovery writer deterministically packages that verifier as
a 1,199,104-byte Windows application with SHA-256
`ef0e57d9c1e9d3c4da134611ac0154926c3489b5315bf0bd643fb2f468769460`
and a 1,200,128-byte Linux application with SHA-256
`8a3edd4ea8d56746e37dcd49bddfb55adcf0dd8f52967d3d435867b4d7b4f938`.
The temporary Windows application accepts both database probes compiled by the
native compiler. The geometry probe is SHA-256
`e2d395f5838341b5867b323b443b097f3349c629d30b6e2a261602c0e3d2398d`;
the reader probe is SHA-256
`7aa883f312628218b583e75e598238f50d2f5821f5e8eba9b775ef830481543c`.
For each probe, native and Stage 0 compilation produce byte-identical WVB.

This closes verifier admission, not database execution or artifact promotion.
The pinned runner still reports its explicit unsupported portable profile for
the variant-bearing probe. The accepted-subset native hosted packager reaches
its enum-request boundary and rejects this larger verifier, so native container
construction remains a named later slice. The pinned native build driver also
does not publish the enlarged driver candidate, while the frozen recovery
compiler does; native self-convergence is therefore another explicit next
boundary rather than an implied success. The qualified read-only and build
front doors retain their existing digest-bound applications.

No broad Seed, OS, Standard, Qualification, WebAssembly, QEMU, or complete
retirement gate ran. Those checks remain grouped at the end of the retirement
goal as required by the active verification rhythm.

## Reconsideration triggers

Revisit the helper boundaries only if another nominal family needs different
evidence or the shared type walker becomes independently reusable. Do not widen
ABI 22 or split the verifier into arbitrary numbered files to avoid measured
frame ownership.
