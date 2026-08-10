# Windvale native compiler reconstruction

## Status and scope

This contract owns the unqualified current-source compiler candidate separately
from the promoted semantic-freeze seed. It reconstructs the canonical compiler
WVB and paired format-3 applications without invoking .NET. It does not promote
the candidate, execute either reconstructed compiler over the complete source
set, or replace the retained Stage 0 recovery archive.

## Exact candidate

`Windvale-Compiler.wvproj` is 649 LF-only bytes at SHA-256
`e097e9d007909a3cf17476ccfce41ace5fa89c566386d15ae24c7d91d9f91e7b`.
The native seed bootstrap compiles that exact inventory to a 921,640-byte WVB at
SHA-256
`18a657f8d4192f01a5822274a7348c02fc30b9bb3a4a9283e4ba302590c3f754`.
Its 427 functions contain 759,920 WVB code bytes.

The current native backend produces 27,635,298 linked image bytes at SHA-256
`80a3ebd54244487bdeafac7b6ebd6c11e1bd839c068405b6507aed83748ff3eb`.
The independent recovery oracle encodes the same closure as a 27,657,722-byte
WVO at SHA-256
`e0a334a805883fe443ed0c7a95b578a076104ea691e29c4e6ed87bf7af63108b`.
The segmented native path transports the image as seven canonical chunks and
binds `Main` at entry offset 43,146.

The retained candidate artifacts are:

| Target | Bytes | SHA-256 |
| --- | ---: | --- |
| Canonical WVB 1.11 | 921,640 | `18a657f8d4192f01a5822274a7348c02fc30b9bb3a4a9283e4ba302590c3f754` |
| Windows x64 format 3 | 27,666,432 | `c1be8bd7e2c9496fee0cd3e486348804469d72621bcf45e30d8b6e8a1814da9c` |
| Linux x64 format 3 | 27,668,480 | `25905e75e836ad8015a851aa6a52531bf5ab73c9dd97596628c2226740f37a34` |

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
4. link and transport one canonical 27,635,298-byte image;
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
execution, then reproduced the same WVB and both application identities through
the Windows native path. Independent Linux reconstruction/execution, the full
current Stage-2 arena measurement, dual-host qualification, promotion, and the
final recovery release remain open.
