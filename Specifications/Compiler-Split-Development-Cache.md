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

The analysis cache uses namespace `project-analysis-wvca-v3` and binds the
workspace marker, analyzer-identity bytes, project identity and bytes, and the
ordered root/source closure through the shared length-framed Project 2 key. Its
checkpoint records exact size and SHA-256 evidence for WVSS, the fixed 104-byte
WVCA, WVLB, and WVIR.

The coordinator validates the strict Project 2 directives independently of the
key builder, places the declared root first, reads every bounded source snapshot,
and sorts dependencies by the ordinal UTF-8 bytes of their declared module
identities before invoking the analyzer. A filename such as `*-Main.wv` is not
used as a proxy for that identity. Therefore Project 2's semantically irrelevant
directive order never changes source-module meaning or successful WVB bytes. The
version-3 family prevents a pre-fix path-ordered checkpoint from being reused.

On a miss, the coordinator passes those root-first canonical source paths to
the retained descriptorless Project 2 Analyzer route. The Analyzer owns its
bounded reads and publishes WVSS 1 beside WVCA, WVLB, and WVIR. The coordinator
does not construct an "admitted" WVSS, invoke the removed public
`--admitted-source-set` option, or enter the private authenticated Language 1.0
sequence. Project 2 remains development-only and its Analyzer precheck rejects a
`System` profile plus every platform or foreign declaration before analysis.

The optional private `--symbol-checkpoint` route divides that same miss into
two analyzer invocations. The first publishes the canonical WVSS plus a bounded
`WVSY 1.0` symbol checkpoint into the separate
`project-symbols-wvsy-v1` cache family. Its manifest binds the complete analysis
request key and exact size and SHA-256 evidence for both values. The second
copies those validated values into its private candidate, independently admits
the checkpoint against the unchanged WVSS, revalidates its directory, lookup,
visibility, and all aggregate counts, and publishes WVCA, WVLB, and WVIR. The
coordinator compares both copied values before and after consumption and removes
the private copies before final atomic cache publication. A retry can therefore
reuse completed symbol work after a later-phase interruption without admitting
stale source, producer, project, or dependency state. Final analysis cache hits
still consume only the same validated WVSS/WVCA/WVLB/WVIR products; `WVSY` is
internal resumable evidence, not a distributable compiler format or additional
compiler.

The emission cache uses namespace `project-split-wvb-optimized-v3`, binds both
producer identities and the same closure, and records the exact analysis key
beside the WVB size and hash. The fixed optimized target therefore participates
in both the producer identity and cache-family name. Both cache families are
additionally separated by the current host family.

## Hit, miss, and publication behavior

A finished-product hit validates the WVB record, its exact analysis request key,
and its bounded product bytes before consulting intermediate analysis or symbol
checkpoints. It does not read, validate, or reconstruct those intermediate
products; they may have been evicted independently. It still derives both keys
from the current complete source closure and producer identities. Invalid final
records or product bytes fail closed without falling back to reconstruction.
Only a final-product miss acquires analysis, ordering the bounded source modules
and validating or constructing its phase products as needed. A hit does not
read or execute either large compiler product.
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

## Qualified two-file bootstrap

The Language 1.0 front door retains the qualified current 1,552,090-byte
analyzer and 1,556,434-byte target-aware emitter under
`Artifacts/Language-1.0-Target-Aware-Emission-Bootstrap/`. Their manifest binds
the Decision 0896 promotion, current Project 2 roots, source identities, sizes,
and digests. The active gate packages that pair with normal role-specific
version-2 identities and uses it directly to reconstruct the current analyzer
and emitter.

The checkpoint is not another compiler source tree, native executable, release
artifact, source-admission claim, or fallback compiler. Both promoted products
consume the current compact WVIR format. The former Decision 0813 pair and
Decision 0846 bridge remain historical provenance only; keeping them in the
active inventory would preserve a transition that the current pair no longer
needs. The gate also omits the former monolithic compiler-source-set build
because its result was never consumed; compiler-scale evidence comes from the
current split analyzer/emitter reconstruction and fixed point.

`Build-Current-Split-Project-Wvb.mjs` accepts one through eight ordered
`<project.wvproj> <output.wvb>` pairs. It packages and identifies the pinned
pair, reconstructs and identifies the current analyzer/emitter pair once, then
builds every requested target through that same immutable current identity.
The current Analyzer WVB is packaged once under Profile 7 for ordinary target
analysis and once under Profile 8 for the larger artifact-reader emitter
closure. Both packages contain the same WVB and have separate executable
identities. Emitter and requested target analysis use the internal symbol
checkpoint route; their final WVSS/WVCA/WVLB/WVIR cache contract is unchanged.
Output paths must be distinct bounded `.wvb` targets with existing canonical
parents. A single pair retains the original result line; a multi-project run
reports each product's size and digest plus one aggregate completion line. This
bounded batching is a verification-time optimization only: it changes neither
the per-project split-cache key nor the resulting bytes.

The focused development owner validates the adapter's fixed optimized route,
requires the exact 308-byte reachable pruning oracle and its exact 395-byte
complete counterpart, and executes a twenty-eight-case cache sentinel. The sentinel
proves module ordering, identity publication, failure cleanup, replacement and
quarantine race safety, primary-plus-cleanup diagnostics, the root-first raw
Project 2 argument and WVSS/WVCA/WVLB/WVIR output order, resumable WVSY reuse
after a later analysis failure, fail-closed WVSY corruption handling, finished
product reuse after intermediate eviction, final product and analysis-key
corruption rejection before construction, producer-change invalidation, two-branch construction ordering, peer completion
after either branch fails, aggregate errors, and rejection before publication. It
deliberately does not rebuild three large compiler products already covered by
the Language 1.0 front door.
Compiler analysis/emission core changes select that broader semantic gate once;
split adapter, identity, and cache changes select this focused owner. Full
storage, OS, complete native, and paired-host qualification remain final
integration gates unless this boundary changes them directly.

## Reusable current compiler pair

`Build-Current-Split-Project-Wvb.mjs` acquires the current analyzer and emitter
as one development construction product before building requested projects.
A validated pair skips pinned compiler packaging and all intermediate compiler
construction. A miss uses twelve preparation steps and publishes
only the completed pair; existing intermediate caches remain independently
reusable. This does not make a genuinely cold compiler preparation fit the local
development budget or turn cached construction into qualification evidence.

The construction core first completes the pinned pair, current Analyzer WVB,
and its Profile 8 package and identity. It then runs exactly two independent
branches: Profile 7 Analyzer packaging/identity, and Emitter build/package/identity.
The Profile 8 package publishes the shared native image before either branch
starts, so parallel execution does not duplicate native staging. Profile-specific
memory, instruction, file-count, and output limits remain unchanged. Both branches
settle before final-product copying or temporary cleanup; failures prevent pair
publication and retain each branch error. The coordinator uses the existing
bounded process-tree command runner, with a ten-minute per-command timeout and
a 1 MiB combined diagnostic bound. Owner-level total budgets remain separate.
Each progress line retains its assigned step number during overlapping work.

The `current-split-compiler-v1` family is separated by `win32-x64` or
`linux-x64`. Its key binds the workspace marker, both compiler project manifests
and their complete declared source closures, pinned analyzer/emitter WVB bytes,
the coordinator, command lifecycle and split-cache implementations, source ordering, producer
identity writing, all segmented staging/linking/transport/admission producers,
the hosted packager's complete producer context, and the Node version and
executable identity. Requested test projects are separate downstream products.
Every input is remeasured before accepting a hit and before publishing a miss.

Each checkpoint contains exactly `Analyzer.exe`/`Emitter.exe` on Windows or
`Analyzer.elf`/`Emitter.elf` on Linux, their two `.identity` files, and
`Checkpoint.json`. Each executable is nonempty and at most 64 MiB; each identity
is at most 1 KiB and must exactly describe its corresponding executable and
role. The record is at most 4 KiB of canonical UTF-8 JSON with a final LF:
`format`, `key`, `host`, and four ordered `products` containing `name`, `bytes`,
and `sha256`. The format is `windvale-current-split-compiler-checkpoint-1`.
The implementation compares canonical record bytes rather than accepting extra
fields or alternate serialization.

Paths must be canonical ordinary files/directories without links; Linux
executables must retain an execute permission bit. Directory
inventory is bounded before reading product bytes. Missing, additional,
corrupt, oversized, wrong-role, or wrong-host products fail before use; a corrupt
hit never silently starts reconstruction. Hashing uses 1 MiB stream buffers.
Production uses a private sibling directory and atomic directory publication.
Concurrent writers must produce identical complete records. Failure cleanup
removes only its own private candidate and retains completed checkpoints owned
by other stages. Forced process termination may leave an unpublished candidate;
lookup never treats that private directory as a completed checkpoint.

The existing split-cache sentinel also covers current-pair reuse, corruption,
identity mismatch, size and link rejection, changed inputs and keys,
interruption cleanup, concurrent publication, and excess inventory. The ordinary
coordinator route keeps that owner and the foreign-binding integration owner;
it no longer rebuilds the WVB runner merely to replay compiler preparation.

## Compiler-scale development sentinel

The immutable native Seed is recovery provenance, not the semantic definition
or ordinary compiler-scale development front door. It may reconstruct a later
compiler stage, but a current source closure is not required to remain within
the historical Seed's capacity. The active Language 1.0 owner reconstructs one
current analyzer/emitter pair and uses that pair for compiler-scale projects.
This is one compiler divided at its explicit phase artifact, not a parallel
compiler implementation.

The Generic-WIR sentinel compiles its canonical Project 2 closure twice through
the split coordinator, requires byte-identical WVB output with a pinned size and
digest, and admits the first output through the independent compiler-aligned
WVB verifier. It does not widen the native staging output envelope or the
general runner's call-depth bound merely to execute this compiler-scale product.
The Language 1.0 gate separately executes its language fixtures. Failure of the
historical Seed on this closure does not waive the sentinel and does not
authorize replacing the recovery artifact.
