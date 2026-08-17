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
959,320-byte Stage 1 compiler, packages it privately, and promotes its byte-stable
Stage 2 output as the 935,163-byte candidate at SHA-256
`a7d47b2de29faee089c7a22ef23eac4657f719331dc02044eb2d818457dac5b6`.
Its 418 functions contain 770,988 WVB code bytes.

The current native backend produces 28,141,686 linked image bytes at SHA-256
`492c79a9e1ef17fb4dd610de62491b9d1b2d181bf69267c7847121fad827fa57`.
The retained independent recovery oracle remains evidence for the previous
recovery checkpoint; this current native-only refresh does not claim a new
managed differential measurement.
The segmented native path transports the image as seven canonical chunks and
binds `Main` at entry offset 51,356.

The retained candidate artifacts are:

| Target | Bytes | SHA-256 |
| --- | ---: | --- |
| Canonical WVB 1.11 | 935,163 | `a7d47b2de29faee089c7a22ef23eac4657f719331dc02044eb2d818457dac5b6` |
| Windows x64 format 3 | 28,172,800 | `a5db938a814471fdacda75efcf57d28934ae52b3b2290732627c14ba173fd70d` |
| Linux x64 format 3 | 28,172,288 | `da11ab3b70b428087cbcb9de5614a2dbdccd31afc6861cc15881fd65c12ff19b` |
| Build-driver WVB 1.11 | 1,142,818 | `125d2b4080889615877d843a36b2f9f6b50d049d011cc06fa8ab426ab83c0574` |
| Windows x64 build driver | 30,071,296 | `f556f0e2c794d9424cbcd9f5e3f8e5aee54f49373c7c18ea1d4829facea7dc6f` |
| Linux x64 build driver | 30,072,832 | `628fd60ea702c4a3b3ffb01d32cba7ba9708477acccf190cc6506a56f159d7a9` |

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
4. link and transport one canonical 28,141,686-byte image;
5. require entry offset 51,356 and exactly seven transported chunks;
6. package the same WVB and image for both permanent hosts through the current
   hosted-container toolset;
7. package the reconstructed compiler for the current host and use its explicit
   source-set launcher to compile the current build-driver closure, then
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
