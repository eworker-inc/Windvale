# Windvale native compiler reconstruction

## Status and scope

This contract owns the unqualified current-source compiler candidate separately
from the promoted semantic-freeze seed. It reconstructs the canonical compiler
WVB and paired format-3 applications without invoking .NET. It does not promote
the candidate, execute either reconstructed compiler over the complete source
set, or replace the retained Stage 0 recovery archive.

## Exact candidate

`Projects/Examples/Windvale-Compiler.wvproj` is 649 LF-only bytes at SHA-256
`a180b171446a6b047b737913ead74fb77a2ecb8d5eedcef833e881dc93ec9b05`.
The native seed bootstrap compiles that exact inventory to a transitional
Stage 1 compiler, packages it privately, and promotes its byte-stable Stage 2
output as the 923,818-byte candidate at SHA-256
`49b5cbf040de4bcb22c071a5da9a4fbad47f4f0658ef910957a67b52c07607c2`.
Its 414 functions contain 761,807 WVB code bytes.

The current native backend produces 27,647,511 linked image bytes at SHA-256
`8f808c868da20fe8ffbe01a40cd8473f0cb570e825283bb938a2e37668470c8e`.
The retained independent recovery oracle remains evidence for the previous
recovery checkpoint; this current native-only refresh does not claim a new
managed differential measurement.
The segmented native path transports the image as seven canonical chunks and
binds `Main` at entry offset 51,356.

The retained candidate artifacts are:

| Target | Bytes | SHA-256 |
| --- | ---: | --- |
| Canonical WVB 1.11 | 923,818 | `49b5cbf040de4bcb22c071a5da9a4fbad47f4f0658ef910957a67b52c07607c2` |
| Windows x64 format 3 | 27,678,720 | `6f266759e2d2524ad9ce2045cb21243538efc7bce35ab1f94a7da4009865eac8` |
| Linux x64 format 3 | 27,680,768 | `7a81bc84a433bec0b2dcebd1ec3be82de120b11427687b9926ec13592231dc37` |

`Artifacts/Native-Compiler-Reconstruction-Candidate/Manifest.json` binds those
three files. This inventory is intentionally distinct from
`Artifacts/Native-Compiler-Seed`, which remains the last qualified recovery
seed and must not be silently repinned to an unqualified source state.

## Native construction

`Construct-Compiler-Reconstruction.cmd` and `.sh` accept one existing output
directory. They:

1. invoke the digest-bound native seed bootstrap promotion over the exact
   project inventory;
2. require the canonical current WVB identity;
3. stage the large WVO through the admitted segmented native lowerer;
4. link and transport one canonical 27,647,511-byte image;
5. require entry offset 51,356 and exactly seven transported chunks;
6. package the same WVB and image for both permanent hosts through the current
   hosted-container toolset; and
7. reread and require all three final identities.

Each underlying publisher retains its own exact file-publication contract. The
wrapper does not claim an atomic transaction across the three independent
artifacts. A failure returns nonzero and never treats a partially advanced
directory as one accepted candidate inventory.

## Focused evidence

`compiler-reconstruction` owns three fixed cases: checked-in inventory, usage
rejection, and paired native reconstruction. The paired case is intentionally
not an inner-loop compiler execution test; it reconstructs the WVB and both
containers but does not start them. The long Stage 1/Stage 2 convergence remains
reserved for the final grouped qualification under the compiler-seed bootstrap
contract.

Decision 0494 measured the managed recovery oracle once without either Stage-2
execution. The current candidate is now reproduced through the Windows native
path without refreshing that managed oracle. Independent Linux reconstruction
and execution, the full current Stage-2 arena measurement, dual-host
qualification, promotion, and the final recovery release remain open.
