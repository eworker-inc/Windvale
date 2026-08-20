# Windvale split-compiler development cache

## Status and scope

`Build-Cached-Split-Project-Wvb` is a development-only coordinator for the
independent analyzer and emitter products specified by
[the source-analysis phase artifact](Compiler-Source-Analysis.md). It admits
one canonical Project 2 manifest, one private `.wvb` output, one analyzer plus
identity, and one emitter plus identity. Project 3 profiles and optimization
options remain outside this fixed-mode route rather than being silently omitted
from its key. The analyzer target is `source-analysis-v1`; the emitter and
published product target are `portable-wvb-optimized-v1`.

This cache is not a release, qualification, or cross-host conformance boundary.
Split output must remain byte-identical to the one-shot compiler's normal
optimized output for the same accepted Project 2 input. The one-shot
`--complete` mode remains the explicit diagnostic and differential oracle; it
is not a hidden split-cache option.

## Producer identity

`Write-Split-Compiler-Producer-Identity.mjs` hashes a bounded ordinary analyzer
or emitter executable once at packaging and publishes this exact six-line ASCII
identity for the current `win32-x64` or `linux-x64` host:

```text
windvale-split-compiler-producer 2
role <analyzer-or-emitter>
target <source-analysis-v1-or-portable-wvb-optimized-v1>
host <host-family>
bytes <canonical-positive-decimal>
sha256 <64-lowercase-hex>
```

The analyzer identity requires `source-analysis-v1`; the emitter identity
requires `portable-wvb-optimized-v1`. An existing identity may be reused only
when every byte is equal. A different producer requires a new identity path or
explicit removal by its packaging owner; the writer does not overwrite an
identity in place.

## Keys and checkpoints

The analysis cache uses namespace `project-analysis-wvca-v2` and binds the
workspace marker, analyzer-identity bytes, project identity and bytes, and the
ordered root/source closure through the shared length-framed Project 2 key. Its
checkpoint records exact size and SHA-256 evidence for WVSS, the fixed 104-byte
WVCA, WVLB, and WVIR.

The coordinator validates the strict Project 2 directives independently of the
key builder, places the declared root first, and sorts dependency source paths
by their canonical manifest spelling before invoking the analyzer. Therefore
Project 2's semantically irrelevant directive order never changes source-module
meaning or successful WVB bytes.

The emission cache uses namespace `project-split-wvb-optimized-v2`, binds both
producer identities and the same closure, and records the exact analysis key
beside the WVB size and hash. The fixed optimized target therefore participates
in both the producer identity and cache-family name. Both cache families are
additionally separated by the current host family.

## Hit, miss, and publication behavior

A hit validates the complete checkpoint and hashes only its bounded phase
values and product; it does not read or execute either large compiler product.
On a miss, the coordinator hashes the selected executable against its identity
both immediately before and after execution, rechecks the complete key input
set, syncs the candidate files, and atomically publishes the directory.
Analyzer and emitter launches report `Started`, one bounded `Active` heartbeat
every 30 seconds, and `Complete`, so a long producer is distinguishable from a
stalled coordinator without changing machine-significant result lines.

A race loser accepts only a completely valid canonical checkpoint. A `finally`
boundary removes only the exact locally allocated `.new-<key>-*` directory
after a producer, measurement, manifest, or lost-race failure, after proving
that the candidate remains a direct child of the selected family. A successful
rename clears the temporary path and is preserved.

## Development bootstrap bridge

The Language 1.0 front door retains a portable 949,355-byte analyzer and
746,557-byte target-aware emitter under
`Artifacts/Language-1.0-Target-Aware-Emission-Bootstrap/`. Their manifest binds
the exact source base, one `Optimize = true` emitter overlay, both Project 2
closures, producer, sizes, and digests. The gate validates both WVBs, packages
them for the active host, and assigns normal role-specific version-2 producer
identities. It uses the pair only to construct the current target-aware emitter
from the oversized compiler closure. Ordinary Language 1.0 inputs use the
separately reconstructed current analyzer and emitter.

The bridge is not another compiler source tree, native executable, release
artifacts, or qualification claim. They exist because the prior compiler can
reconstruct the current analyzer, but that analyzer's hosted execution envelope
does not admit the 1.57 MiB current compiler closure.

The focused development owner validates the adapter's fixed optimized route,
requires the exact 308-byte reachable pruning oracle and its exact 395-byte
complete counterpart, then forces a producer-identity failure and proves that
no temporary checkpoint directory remains. It deliberately does not rebuild
three large compiler products already covered by the Language 1.0 front door.
Compiler analysis/emission core changes select that broader semantic gate once;
split adapter, identity, and cache changes select this focused owner. Full
storage, OS, complete native, and paired-host qualification remain final
integration gates unless this boundary changes them directly.
