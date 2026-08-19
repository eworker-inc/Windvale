# Windvale split-compiler development cache

## Status and scope

`Build-Cached-Split-Project-Wvb` is a development-only coordinator for the
independent analyzer and emitter products specified by
[the source-analysis phase artifact](Compiler-Source-Analysis.md). It admits
one canonical Project 2 manifest, one private `.wvb` output, one analyzer plus
identity, and one emitter plus identity. Project 3 profiles and optimization
remain outside version 1 rather than being silently omitted from its key. The
exact output target is `portable-wvb-v1`.

This cache is not a release, qualification, or cross-host conformance boundary.
One-shot and split output must remain byte-identical for the same accepted
unoptimized Project 2 input.

## Producer identity

`Write-Split-Compiler-Producer-Identity.mjs` hashes a bounded ordinary analyzer
or emitter executable once at packaging and publishes this exact six-line ASCII
identity for the current `win32-x64` or `linux-x64` host:

```text
windvale-split-compiler-producer 1
role <analyzer-or-emitter>
target portable-wvb-v1
host <host-family>
bytes <canonical-positive-decimal>
sha256 <64-lowercase-hex>
```

An existing identity may be reused only when every byte is equal. A different
producer requires a new identity path or explicit removal by its packaging
owner; the writer does not overwrite an identity in place.

## Keys and checkpoints

The analysis cache uses namespace `project-analysis-wvca-v1` and binds the
workspace marker, analyzer-identity bytes, project identity and bytes, and the
ordered root/source closure through the shared length-framed Project 2 key. Its
checkpoint records exact size and SHA-256 evidence for WVSS, the fixed 104-byte
WVCA, WVLB, and WVIR.

The coordinator validates the strict Project 2 directives independently of the
key builder, places the declared root first, and sorts dependency source paths
by their canonical manifest spelling before invoking the analyzer. Therefore
Project 2's semantically irrelevant directive order never changes source-module
meaning or successful WVB bytes.

The emission cache uses namespace `project-split-wvb-v1`, binds both producer
identities and the same closure, and records the exact analysis key beside the
WVB size and hash. Both cache families are additionally separated by the
current host family.

## Hit, miss, and publication behavior

A hit validates the complete checkpoint and hashes only its bounded phase
values and product; it does not read or execute either large compiler product.
On a miss, the coordinator hashes the selected executable against its identity
both immediately before and after execution, rechecks the complete key input
set, syncs the candidate files, and atomically publishes the directory.

A race loser accepts only a completely valid canonical checkpoint. A `finally`
boundary removes only the exact locally allocated `.new-<key>-*` directory
after a producer, measurement, manifest, or lost-race failure, after proving
that the candidate remains a direct child of the selected family. A successful
rename clears the temporary path and is preserved.

The focused development owner compiles the source-analysis corruption fixture,
analyzer, and emitter in reachable-product mode, then forces a producer-identity
failure and proves that no temporary checkpoint directory remains. Full
storage, OS, complete native, and paired-host qualification remain final
integration gates unless this boundary changes them directly.
