# Decision 0132: Windvale-owned console-application verification

- Date: 2026-08-03
- Status: Implemented candidate; fresh dual-host qualification pending
- Targets: `windows-x64-console-v1` and `linux-x64-console-v1`
- Retains: Canonical WVB 1.6, native ABI 20/context 7, the 4 MiB byte-value limit, WVA 1, WVO 1.0, both executable format versions, and the .NET retirement gate

## Context

Decision 0130 made portable Windvale describe every final PE/ELF byte through a sparse recipe, but arbitrary completed applications were still accepted only by detailed C# verifiers. A maximum completed application is larger than the 4 MiB Windvale byte-value limit, so passing it to one ordinary `bytes` parameter would either reduce accepted native inputs or broaden a general runtime contract.

The next transfer needs to accept the existing maximums, recover the exact native input, retain independent PE/ELF oracles, and avoid cloning the established malformed application tests.

## Decision

- Add one portable verifier over the canonical Windvale construction recipe.
- Present a completed file as a first chunk of at most 4 MiB and a second chunk of at most 8,304 bytes. Require an exact 4 MiB first chunk whenever the second is nonempty.
- Infer only target, native length, and native entry from bounded early PE/ELF fields. Regenerate the accepted 32-byte plan request and sparse recipe, then compare every literal and implicit-zero byte against the candidate.
- Skip the one opaque native-copy segment during container comparison and recover it as one bounded byte value, including when it crosses the chunk boundary.
- Return fixed 36-byte `WVCV 1` evidence. On success, write the recovered native image once through a named hosted resource; on rejection, write nothing.
- Embed and digest-pin the exact hosted bridge WVB. Authorize only named byte reads and the one bounded byte write, and limit evaluation to ten million instructions.
- Validate all returned evidence and write behavior independently in C# before accepting the recovered result.
- Require both live Windows and Linux writers to obtain exact target/native/entry agreement from the portable verifier, then retain the existing detailed C# verifier as a second independently maintained oracle.
- Route every existing malformed PE/ELF case through both verifiers. Add only verifier-boundary tests for source/artifact identity, evidence shape, no-write rejection, noncanonical segmentation, determinism, and maximum-size cross-chunk recovery.

The portable verification core compiles to 46,074 bytes with SHA-256 `326e2ecfc1f4dd8bd24f71fff4a9db960de2519d9aae17afbcd6a005c2e7c94d`. The retained hosted bridge is 46,150 bytes with SHA-256 `74542907a1b7a90d6d13ee157e7a9e7a4e60e83c042a5486e2f0ab3113ad6013`.

## Local evidence

Focused zero-warning Release verification passes the combined layout/construction/verification test in 1.320 seconds on the merged candidate. It recompiles both new source sets, proves retained identities and exact capabilities, checks deterministic `WVCV 1` evidence, rejects malformed evidence, noncanonical chunks, and an 8,305-byte second chunk without a write, and recovers exact 4 MiB native images plus final-byte entries from both maximum-size containers.

The existing Windows and Linux console tests pass in 1.659 seconds and 608 milliseconds after their complete malformed-input corpora are routed through both verifier implementations. Canonical output remains unchanged: the PE is 5,120 bytes with SHA-256 `5947c00a81f4cf94651d42d619f3173a622448d042f4fa20e3042940d4a56c77`, and the ELF is 8,304 bytes with SHA-256 `8af8b46c290965cfc4475d882ac2d5fbdb0ffe4c493a19883a19c2683a319ec4`.

Windows Development completes a zero-warning Release build, all 76 regular Seed tests, and all 31 bounded OS tests in 90.6 seconds wall time on the merged candidate. Seed takes 74.315 seconds; the combined ownership test and existing Windows/Linux container cases take 484, 1,430, and 144 milliseconds. The qualification-only golden contract and direct Linux execution are outside Development, so fresh dual-host Qualification remains pending for this candidate.

## Consequences

Portable Windvale now owns both exact construction and untrusted completed-container verification for the paired console targets without changing a general byte limit. Maximum inputs remain supported, and accepted native bytes can cross the host-to-Windvale chunk boundary without being interpreted as container structure.

C# still materializes the sparse construction recipe, validates `WVCV` host evidence, supplies retained recovery provenance, and independently parses PE/ELF. This is an intentional Stage 0 differential boundary, not a second product compiler or a claim that .NET retirement is complete.

The next native milestone may consume the already proven record-storage offsets in a successor ABI. It must not remove the C# recovery path or claim Stage 1-to-Stage 2 reproduction until the paired host qualification and documented native-retirement gates are satisfied.

## Reconsider when

- A general bounded segmented/streaming byte abstraction replaces this two-chunk adapter without weakening exact limits.
- Another container target cannot be expressed by one opaque native span plus canonical construction literals.
- Native Windvale can run the verifier bridge and publish recovered evidence without the Stage 0 runtime.
- The complete cross-host native-retirement gate makes the independent C# container path recovery-only.
