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
955,192-byte Stage 1 compiler, packages it privately, and promotes its byte-stable
Stage 2 output as the 931,035-byte candidate at SHA-256
`13558d9dbc0d185b161b770824aa29ff90b8873903b2b5d7184a23950a6fc1e4`.
Its 416 functions contain 767,871 WVB code bytes.

The current native backend produces 27,867,015 linked image bytes at SHA-256
`69c2250a4ebb19d63be0cb14b367a5aeb5fcbb64a0b825cd275f82595a851ac9`.
The retained independent recovery oracle remains evidence for the previous
recovery checkpoint; this current native-only refresh does not claim a new
managed differential measurement.
The segmented native path transports the image as seven canonical chunks and
binds `Main` at entry offset 51,356.

The retained candidate artifacts are:

| Target | Bytes | SHA-256 |
| --- | ---: | --- |
| Canonical WVB 1.11 | 931,035 | `13558d9dbc0d185b161b770824aa29ff90b8873903b2b5d7184a23950a6fc1e4` |
| Windows x64 format 3 | 27,898,368 | `4009e6747bbf9a6d2b0b2ec90e2368ca50fda863d445534f15ef96e22a657b34` |
| Linux x64 format 3 | 27,897,856 | `c266adf20fe2927a446483f68880ef323c480f011b0c26384716ea2f651bcd65` |
| Build-driver WVB 1.11 | 1,162,338 | `a214662da422443cd70c4be12c8f0bd06cbb5bce9fe3a56e2a52c46a37445a20` |
| Windows x64 build driver | 30,381,568 | `b0d58f4d8d6d32e09d45358035ce4521410ec1280f11fa72b66245e621ba49a3` |
| Linux x64 build driver | 30,380,032 | `b4fdc30a7242e03f7166491bf6b415aa3b5dce8ff0e16444f6ccb24c5bcb03a0` |

`Artifacts/Native-Compiler-Reconstruction-Candidate/Manifest.json` binds those
six files. This inventory is intentionally distinct from
`Artifacts/Native-Compiler-Seed`, which remains the last qualified recovery
seed and must not be silently repinned to an unqualified source state.

## Native construction

`Construct-Compiler-Reconstruction.cmd` and `.sh` accept one existing output
directory. They:

1. invoke the digest-bound native seed bootstrap promotion over the exact
   project inventory;
2. require the canonical current WVB identity;
3. stage the large WVO through the admitted segmented native lowerer;
4. link and transport one canonical 27,867,015-byte image;
5. require entry offset 51,356 and exactly seven transported chunks;
6. package the same WVB and image for both permanent hosts through the current
   hosted-container toolset;
7. use the frozen build driver to compile the current build-driver source, then
   independently stage it as eight chunks with entry offset 220,460;
8. package the current driver for both permanent hosts under profile 2; and
9. reread and require all six final identities.

Each underlying publisher retains its own exact file-publication contract. The
wrapper does not claim an atomic transaction across the six independent
artifacts. A failure returns nonzero and never treats a partially advanced
directory as one accepted candidate inventory.

## Focused evidence

`compiler-reconstruction` owns three fixed cases: checked-in six-artifact inventory, usage
rejection, and paired native reconstruction. The paired case is intentionally
not an inner-loop compiler execution test; it reconstructs the WVB and both
compiler containers plus the build-driver WVB and both of its containers, but
does not start them. The long Stage 1/Stage 2 convergence remains
reserved for the final grouped qualification under the compiler-seed bootstrap
contract.

Decision 0494 measured the managed recovery oracle once without either Stage-2
execution. The current candidate is now reproduced through the Windows native
path without refreshing that managed oracle. Independent Linux reconstruction
and execution, the full current Stage-2 arena measurement, dual-host
qualification, promotion, and the final recovery release remain open.
