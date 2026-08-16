# Windvale native source-to-WVB front door

## Status and scope

`WVNF 1` is the implemented candidate for the first ordinary source-to-verified-WVB
workflow that does not invoke .NET. It composes the qualified format-5 native build
driver with the qualified exact native WVB publisher:

```text
project.wvproj
  -> native wvbuild -> caller-owned Candidate.wvb
  -> native wvpublish -> atomically replaced output.wvb
```

The publisher applications are cross-host qualified at exact commit
`9d36387867ebff80ee94c6f9f7996da4ef32a4a3` in GitHub
[Verify run 30971408639](https://github.com/eworker-inc/Windvale/actions/runs/30971408639).
The distributed front-door inventory, launchers, reconstruction route, and ordinary
workflow cutover are cross-host qualified at exact commit
`d2e71c1d6491153afb715674fc13ba2c6276326a` in GitHub
[Verify run 30974274271](https://github.com/eworker-inc/Windvale/actions/runs/30974274271).

This is a source-to-WVB cutover, not complete .NET retirement. The normal assembler,
linker, native application packaging, runtime, test runner, complete backend, and
final recovery archive still retain explicit Stage 0 responsibilities.

## Qualified and current-candidate inventories

[`Artifacts/Native-Compiler-Reconstruction-Candidate/Manifest.json`](../Artifacts/Native-Compiler-Reconstruction-Candidate/Manifest.json)
binds the current compiler and build-driver WVB modules and their paired native
applications. `Build-Current-Wvb` selects its host build driver for explicit
forward-language development. That launcher binds the exact candidate by SHA-256
and does not infer a newer local executable.

[`Artifacts/Native-Front-Door/Manifest.json`](../Artifacts/Native-Front-Door/Manifest.json)
and its `SHA256SUMS` retain the cross-host-qualified semantic-freeze toolset. The
ordinary `Build-Wvb` launcher takes both qualified tools from that inventory.
`Build-Current-Wvb` deliberately does not pass forward-language output through
that frozen publisher; it retains the current raw driver's self-verification and
non-atomic write contract. The frozen build driver remains available for recovery
and for reconstructing the current candidate, but it is not asked to accept
post-freeze language syntax.

The Windows launchers verify each executable they invoke through the inbox
`certutil` SHA-256 implementation. The Linux launchers perform the corresponding
exact SHA-256 checks through `sha256sum`. PE, ELF, WVB, and WVO files remain explicitly
binary in `.gitattributes`.

## Ordinary invocation

Windows x64 uses the inbox command processor rather than PowerShell:

```bat
Tools\Native\Build-Wvb.cmd project.wvproj output.wvb
```

Linux x64 uses Bash:

```sh
./Tools/Native/Build-Wvb.sh project.wvproj output.wvb
```

The output defaults to the project basename with a `.wvb` extension. The launcher
creates a process-unique private candidate directory, invokes `wvbuild --project`
over the explicit project, and calls `wvpublish` only after successful compiler and
compiler-aligned verifier admission. The publisher repeats admission over the exact
candidate snapshot and performs the Decision 0214 identity, sibling, durability,
replacement, and completion transaction. The launcher removes its caller-owned
candidate after success or failure.

Project syntax, the 63-module native project bound, compiler behavior, verifier
rules, diagnostics, and WVB bytes remain those of the qualified build driver. The
launcher does not discover source, infer imports, lower native code, or reinterpret
paths inside portable Windvale semantics.

## Recovery reconstruction

The checked-in, digest-bound native front door is part of the documented native
seed inventory. The final Stage 0 archive reconstructs the older native compiler
seed from the feature-freeze source, admits the archived front-door artifacts,
and passes both to the native compiler-convergence owner. That chain must compile
the current source graph and produce byte-identical Stage 1 and Stage 2 WVBs on
Windows and Linux.

The feature-frozen C# compiler does not directly reconstruct the current front
door. Post-freeze source semantics belong only to `Compiler/Windvale`; requiring
Stage 0 to accept them would violate the freeze and create a second forward
compiler. The removed direct front-door rebuild commands remain available in Git
history as provenance for their pre-freeze artifact generation.

## Required evidence

The focused test requires deterministic package reconstruction, exact manifest
lengths and hashes, successful replacement of an existing destination through the
ordinary host launcher, exact agreement with the retained project WVB, rejection
preservation, and caller-candidate cleanup. The underlying build-driver and
publisher tests retain malformed input, native identity, scratch cleanup, exact
transcript, no-CLR child execution, and platform package evidence.

Cross-host Qualification must run the same ordinary workflow on Windows and pinned
Debian before `WVNF 1` becomes qualified. Any artifact or source change requires an
explicit identity refresh; launchers never normalize a mismatched binary.
