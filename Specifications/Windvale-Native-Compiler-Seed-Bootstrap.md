# Windvale native compiler seed bootstrap

## Status and scope

This document defines the implemented candidate for rebuilding the accepted
Windvale compiler WVB from a documented native compiler seed without invoking
.NET. It advances the clean-bootstrap condition in Decision 0057; it does not
complete that condition until the seed is promoted by an independent Windows and
Linux qualification and a later accepted source state consumes that previous
release.

The version-1 bootstrap is intentionally narrower than a complete native toolchain
bootstrap. It rebuilds and independently publishes the canonical compiler WVB. It
does not yet rebuild the seed compiler PE/ELF, the complete native backend, every
developer tool, or the final Stage 0 recovery archive without .NET.

## Seed and source identities

`Artifacts/Native-Compiler-Seed/Manifest.json` binds one canonical WVB and paired
format-3 Windows/Linux compiler applications to byte lengths, SHA-256 identities,
source commit, source tree, target, and qualification provenance. `SHA256SUMS`
repeats the three artifact identities for external tools.

The seed compiler source is the qualified semantic-freeze tree at commit
`524e84afb6e5bab6bbd95ebc0b9eeaf886af834b`. The exact Stage 0 recovery state used
to package the candidate is commit
`3824f39d0997e3d7ab523f7cc1fe0f4bd8288e35`. The ordinary bootstrap input is the
explicit thirteen-module inventory in `Windvale-Compiler.wvproj` at that latter
commit. Its accepted output is exactly 921,900 bytes with SHA-256
`fd96bd567d08a18107a9b149560ce9f2e38b49454250e934a4375f465d132556`.

The bootstrap artifact root contains both `Native-Compiler-Seed` and the qualified
`Native-Front-Door` publisher inventory. This permits a released artifact bundle
to be copied outside the source checkout and admitted before use without
duplicating the publisher binaries in two checked-in inventories.

## Host launchers

Windows x64 uses `Tools/Native/Bootstrap-Compiler.cmd`; Linux x64 uses
`Tools/Native/Bootstrap-Compiler.sh`. PowerShell is not the native bootstrap host:
PowerShell 7 loads CoreCLR and therefore cannot provide no-.NET process evidence.
It remains suitable for the explicitly retained recovery lane.

Each launcher:

1. admits the exact seed WVB, current-host seed compiler, qualified native
   publisher, and compiler project manifest by length and SHA-256;
2. passes the fixed project inventory to the seed compiler in manifest order;
3. writes only a process-private candidate;
4. requires the exact accepted compiler WVB length and SHA-256; and
5. invokes the qualified native publisher, which repeats compiler-aligned
   verification and atomically replaces the requested output.

Any identity, compilation, verification, or publication failure preserves an
existing destination. The launchers remove their private candidate on success or
failure. They do not parse project syntax, discover sources, infer imports, lower
native applications, or install host dependencies.

## Recovery and promotion

`Tools/Recovery/Rebuild-Native-Compiler-Seed.ps1` and its Bash peer reconstruct the
three seed artifacts from exact Git archives. This is an explicit Stage 0 recovery
operation and deliberately invokes the pinned .NET SDK; it is not the native
bootstrap route. Reconstruction always targets a separate directory and must
reproduce the checked-in identities exactly.

Promotion requires the same copied-seed operation on Windows and pinned Linux,
exact compiler output agreement, no CLR/.NET mapping in the compiler and publisher
children, destination-preservation checks, malformed or altered seed rejection,
and an exact-commit qualification reference in the seed manifest. A later release
must then use the promoted previous seed before Decision 0057 condition 7 can be
called complete.

## Native self-convergence coordinator

The ordinary bootstrap verification entry points require more than a one-stage
seed rebuild. `Tools/Native/Verify-Compiler-Convergence.cmd` and `.sh` admit the
native verifier, build and publish Stage 1 through the seed launchers, package
that exact WVB through the segmented compiler-WVB profile, execute the newly
packaged Stage 1 compiler over the canonical project inventory, independently
verify Stage 2, and require complete Stage 1/Stage 2 byte equality.

`Tools/Native/Compile-Compiler-Source-Set.cmd` and `.sh` own the exact ordered
source invocation shared by the seed and convergence routes. They do not discover
files, parse the project, or weaken the project-manifest identity check.

The Windows candidate completes this chain with identical 921,900-byte outputs
at SHA-256
`fd96bd567d08a18107a9b149560ce9f2e38b49454250e934a4375f465d132556`.
The paired Linux coordinator has the same fail-closed contract; its independent
execution and promotion remain part of the grouped retirement qualification.
