# Decision 0417: Canonical compiler-image transport

- Status: Implemented Windows candidate; Linux execution, hosted-package composition, native container construction, and grouped qualification pending
- Date: 2026-08-08
- Advances: [Decision 0416](0416-Digest-Bound-Segmented-Compiler-Process-Front-Door.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Advanced by: [Decision 0418](0418-Segmented-Compiler-Hosted-Package-Composition.md)
- Contract: [Windvale linking](../../Specifications/Windvale-Linking.md#canonical-compiler-image-transport)

## Context

The segmented compiler-image linker emits validated semantic chunks owned by
text batches, alignment padding, and read-only data. Hosted source geometry
instead requires one through eight fragment chunks, with every non-final chunk
exactly 4 MiB. The first direct launcher composition made the mismatch
concrete: an 831,624-byte staging-tool image arrived as ten semantic chunks
even though its canonical hosted representation needs only one.

Shell scripts must not decode or rewrite `WVLI`, and relaxing hosted geometry
would discard useful canonicality and malformed-input checks. The missing
boundary therefore belongs in one small Windvale process.

## Decision

- Add a portable resource-plan module that validates source/output names, a
  strict `WVLI`, the native 62-source-snapshot limit, the one-through-eight
  canonical output bound, and every cross-set path collision before chunk
  acquisition or mutation.
- Add a hosted Windvale transport process that reads each admitted source
  chunk once, checks its exact manifest length, preserves byte order, and emits
  exact 4 MiB chunks plus one optional final remainder.
- Rebuild `WVLI` with the unchanged image size and `Main` entry offset, verify
  it through the existing portable builder, and write the manifest last.
- Pin the native-built WVB and paired recovery-built PE/ELF applications in
  the existing segmented compiler candidate. Add digest-bound Windows/Linux
  `Transport-Compiler-Image` launchers.
- Share one focused C# recovery application writer between the staging linker
  and transport tool so the nine-service profile, application verification,
  and identity checks are not duplicated. Both tool-specific targets remain
  `recovery-aot` only.
- Do not make the reference C# source compiler part of this native test. The
  qualified native front door owns the checked-in WVB, while the focused test
  verifies its identity, service profile, process behavior, and no-CLR
  evidence.

## Candidate identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Canonical image transport WVB | 23,836 | `dc5f460ce89bcce2678092030376c8ddc928e682b263af2a73ba2a57034b6d4d` |
| Windows x86-64 transport | 269,312 | `6c204b9b3ee90a4d73ecdaa1ae0f0c4d5f3056973f3ccd3a8489789c6b46ef6d` |
| Linux x86-64 transport | 270,336 | `4b7aa91e78880617c3abc8a1cbd59c098cfb274c020d2ecbe7dee214ed9576cd` |

## Evidence and consequences

The native source front door publishes the exact 23,836-byte WVB. After review,
the focused owner passes 1/1 in 1.038 test seconds after a zero-warning Release
build. It transports three irregular chunks totalling 4,200,023 bytes into one
exact 4,194,304-byte chunk and one 5,719-byte remainder, preserves every byte
and entry offset 1,234,567, writes a 52-byte `WVLI` last, rejects ordinary AOT,
and loads no CLR component. The digest-bound Windows launcher separately
canonicalizes the real 831,624-byte staging image to one chunk.
The two existing owners of the shared staging application writer continue to
pass 1/1 in 3.810 and 7.179 test seconds from the same built tree.

This closes the semantic-to-canonical image transport boundary. The next slice
can pass its output prefix, validated decimal entry, and one-through-eight
count into hosted source geometry. Stage 0 still constructs the transport
PE/ELF candidates for recovery; native container reconstruction and Linux
execution remain open.

No Development, Standard, Qualification, or complete 27.5 MiB compiler run was
performed. Those remain grouped at the end of the retirement goal.

## Reconsideration triggers

Version or regenerate this candidate when `WVLI`, the 4 MiB resource limit,
the 32 MiB compiler-image bound, snapshot-table capacity, source geometry,
native ABI, service bundle, or host-container construction changes. Do not
move binary parsing or chunk reconstruction into platform scripts.
