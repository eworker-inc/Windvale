# Decision 0130: Windvale-owned console-application construction

- Date: 2026-08-02
- Status: Implemented candidate; fresh dual-host qualification pending
- Targets: `windows-x64-console-v1` and `linux-x64-console-v1`
- Retains: Canonical WVB 1.6, native ABI 20/context 7, the 4 MiB byte-value limit, WVA 1, WVO 1.0, both executable format versions, and the .NET retirement gate

## Context

Decision 0127 made portable Windvale own every file and virtual placement but left C# writing all executable bytes. Returning a complete constructed application as one Windvale `bytes` value would fail at the accepted maximum: the opaque native image may already be 4 MiB, while canonical PE and ELF framing raises the finished files to 4,196,352 and 4,202,608 bytes.

Raising a portable value limit for one container adapter would broaden language/runtime semantics without measured general demand. Leaving maximum inputs to C# would make ownership partial. The construction boundary instead needs a representation whose size follows the nonzero container structure rather than the completed file.

## Decision

- Add a portable Windvale constructor that consumes the same versioned 32-byte request as the layout planner.
- Return a versioned sparse `WVCC 1` recipe containing exact literal segments, one opaque native-image copy segment, and implicit zero gaps.
- Fix the Windows recipe at 834 bytes and five segments: 512-byte headers, 98-byte startup, native copy, 112-byte context, and 12-byte relocation metadata.
- Fix the Linux recipe at 4,454 bytes and four segments: 4 KiB header page, 158-byte startup, native copy, and 112-byte context.
- Generate every PE/ELF header field, program/section record, startup displacement, context field, note, relocation record, and padding rule in portable Windvale.
- Preserve the existing 4 MiB native and byte-value limits. A maximum recipe describes the opaque 4 MiB native span without copying it into the recipe.
- Embed and digest-pin the exact hosted bridge WVB. Authorize only the one request read and bound evaluation to five million instructions.
- Treat the recipe as untrusted in Stage 0: validate the complete envelope and canonical segment table, materialize into a zeroed result with checked ranges, require exact payload consumption, and compare every final byte with a separately emitted C# recovery image.
- Return the Windvale-described materialization from both live writers. Keep the existing C# recovery writers and independent PE/ELF verifiers until later retirement gates.
- Extend the existing layout test rather than adding another application corpus. Compile both source sets, prove retained identities, evaluate both recipes, cover maximum descriptors, mutate envelope/descriptor/literal evidence, and compare complete materialized bytes. Keep direct executable behavior in the existing two platform tests.

The portable construction core compiles to 31,022 bytes with SHA-256 `80684e78839f0001950a7b65fbfce4ec79db81f3c089dc74df00fcde1707aa88`. The retained hosted bridge is 30,202 bytes with SHA-256 `43f1537c4c4038824512972173e0a5c8acc4e74710d315a5b7498a6dae668bb2`.

## Local evidence

The focused construction test passes with a zero-warning Release build. It proves exact portable and hosted source-set identities, one explicit read capability, deterministic Windows and Linux recipes, full materialization equality with the C# recovery writers, maximum native-span descriptors, mapped request failure, uninitialized/truncated/extended recipe rejection, every header and segment word mutation, and literal corruption rejection.

The two existing executable tests pass after both live writers switch to the Windvale recipe. Canonical output is unchanged: the PE remains 5,120 bytes with SHA-256 `5947c00a81f4cf94651d42d619f3173a622448d042f4fa20e3042940d4a56c77`, and the ELF remains 8,304 bytes with SHA-256 `8af8b46c290965cfc4475d882ac2d5fbdb0ffe4c493a19883a19c2683a319ec4`.

Windows Development passes a zero-warning Release build, all 76 regular Seed tests, and all 31 bounded OS tests on the merged candidate in 91.680 seconds wall time. Seed takes 73.201 seconds; the combined layout/construction and existing Windows/Linux container cases take 120, 1,164, and 34 milliseconds. The qualification-only golden contract and direct Linux execution are not part of Development, so fresh dual-host Qualification remains pending.

## Consequences

Portable Windvale now determines every byte of both supported console containers, including zero gaps, without changing a general runtime limit. C# performs a small generic sparse materialization and remains an independent byte-for-byte recovery oracle.

This is not yet a Windvale-owned untrusted-container verifier. The existing C# PE and ELF verifiers remain authoritative for accepting completed executable bytes, and Stage 0 still evaluates and materializes the retained WVB.

The next container-transfer slice should express structural PE/ELF verification and recovered-native evidence in portable Windvale while keeping the current C# verifiers differential and fail closed.

## Reconsider when

- A general bounded streaming byte builder can replace sparse materialization without increasing peak values or weakening determinism.
- A later container target needs repeated external inputs or nonzero segments that cannot fit the current descriptor model.
- Native Windvale execution can evaluate and materialize the recipe without the Stage 0 runtime.
- Recovery construction is no longer needed after the complete cross-host native-retirement gate.
