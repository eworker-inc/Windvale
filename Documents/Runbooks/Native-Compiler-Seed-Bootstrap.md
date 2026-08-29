# Native compiler bootstrap and convergence

This runbook proves that the current split Windvale compiler rebuilds itself
exactly without invoking .NET. The normative identities and bounds are in the
[bootstrap specification](../../Specifications/Windvale-Native-Compiler-Seed-Bootstrap.md).

## Run the cold proof

On Windows x64:

```bat
Tools\Verify\Verify-Bootstrap.cmd
```

On Linux x64:

```sh
./Tools/Verify/Verify-Bootstrap.sh
```

The command accepts no arguments. It validates the checked-in target-aware
bootstrap WVBs, creates a private empty cache, constructs the current analyzer
and emitter, rebuilds both with that current pair, constructs the current WVB
verifier, independently verifies both products, and requires byte-for-byte
Stage 1/Stage 2 equality.

The coordinator reports 18 named phases and 30-second progress while a child is
active. Cold host-container construction is expected to dominate the runtime;
repeated phase progress is not a compiler retry. Success ends with:

```text
native compiler convergence status=Complete products=2 ... cache=Isolated
Native compiler bootstrap verification passed.
```

The exact products are:

- analyzer: 1,515,281 bytes, SHA-256
  `a8687f5ec9337d95ea105b5b2d5feea453a11686251802c14110d1f171a3983a`;
- emitter: 1,523,514 bytes, SHA-256
  `61ebad24f080a78059bfe3c2812cdb04978873eb6891d063ac2090876dc06403`;
  and
- current verifier WVB: 399,387 bytes, SHA-256
  `7da624b070b69c3a720a00df12b753ed28276b7909c48ec5e6c349bd15ed9800`.

A pass on one host is current-host evidence only. Run the same commit through
the paired Windows/Linux Qualification jobs before claiming cross-host
convergence or closing a release checkpoint.

## Ordinary development

Do not run the cold proof after every compiler edit. Use
`Tools/Verify/Verify-Changed.ps1` or the focused compiler owner. Ordinary builds
use `Build-Current-Wvb.cmd` or `.sh`, which reuse exact analyzer, emitter, and
content-addressed cache identities. A final Qualification run supersedes those
narrower checks for the unchanged commit.

## Retained compiler inventory

`Artifacts/Native-Compiler-Reconstruction-Candidate` is a historical WVB 1.11
compiler/build-driver inventory. It remains a small-source differential oracle
and a fixed WebAssembly stress input. It is not rebuilt from the current tree.
The former monolithic bootstrap and reconstruction launchers were retired by
Decision 0876.

## Reconstruct Seed through Stage 0

Seed reconstruction is recovery work, not ordinary bootstrap work. It requires
the SDK pinned by the exact recovery commit and must run in a separate workspace.

On Windows:

```powershell
pwsh -NoProfile -File Tools/Recovery/Rebuild-Native-Compiler-Seed.ps1 artifacts/Recovered-Native-Compiler-Seed
```

On Linux:

```sh
./Tools/Recovery/Rebuild-Native-Compiler-Seed.sh artifacts/Recovered-Native-Compiler-Seed
```

Both commands must reproduce `Artifacts/Native-Compiler-Seed/SHA256SUMS`
exactly. Recovery output does not become the current Seed without a separate
accepted promotion decision and paired-host evidence.
