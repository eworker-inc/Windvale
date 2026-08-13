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
The native seed bootstrap compiles that exact inventory to a 927,274-byte WVB at
SHA-256
`d3dbadd987f10a98ebd90d1357973dca055094e2dbd3cc3e0e90afb3c3c17fae`.
Its 427 functions contain 764,800 WVB code bytes.

The current native backend produces 27,744,550 linked image bytes at SHA-256
`b1787162c3f265eaac2584e7dbf121fa01009ef18d7c092e5c495f1ac8aee5ec`.
The retained independent recovery oracle remains evidence for the previous
recovery checkpoint; this current native-only refresh does not claim a new
managed differential measurement.
The segmented native path transports the image as seven canonical chunks and
binds `Main` at entry offset 43,146.

The retained candidate artifacts are:

| Target | Bytes | SHA-256 |
| --- | ---: | --- |
| Canonical WVB 1.11 | 927,274 | `d3dbadd987f10a98ebd90d1357973dca055094e2dbd3cc3e0e90afb3c3c17fae` |
| Windows x64 format 3 | 27,776,000 | `0975f6181c78cd4b0007883d4b4ee9275b7cbb46bf904ce0cc79730d32308f7e` |
| Linux x64 format 3 | 27,774,976 | `93651adc36557aaa895627e8d8aa022b8765fc4f6cfaafbb5dc7c0a263287f67` |

`Artifacts/Native-Compiler-Reconstruction-Candidate/Manifest.json` binds those
three files. This inventory is intentionally distinct from
`Artifacts/Native-Compiler-Seed`, which remains the last qualified recovery
seed and must not be silently repinned to an unqualified source state.

## Native construction

`Construct-Compiler-Reconstruction.cmd` and `.sh` accept one existing output
directory. They:

1. invoke the digest-bound native seed compiler and publisher over the exact
   project inventory;
2. require the canonical current WVB identity;
3. stage the large WVO through the admitted segmented native lowerer;
4. link and transport one canonical 27,744,550-byte image;
5. require entry offset 43,146 and exactly seven transported chunks;
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
