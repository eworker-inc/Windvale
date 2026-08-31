# Windvale source-analysis phase artifact

## Status and purpose

`Compilerˉsourceˉanalysis` separates reusable source and type analysis from WVB
emission. It consumes one canonical WVSS source set and publishes three
individually bounded values. Production Language 1.0 reaches that input only
through the retained authenticated-snapshot relationship below; descriptorless
Project 2 remains a narrower development route.

- one fixed 104-byte `WVCA 1.0` manifest;
- one canonical `WVLB 1.1`, function-specialized `WVLB 1.2`, or combined
  specialization-evidence `WVLB 1.3` binding directory; and
- one canonical `WVIR 1.9` through `WVIR 1.22` typed source directory, with the
  exact ordinary/specialized, memory/append/growth, callable, closure, and
  structured-task feature pairing defined by WVIR.

The artifact set is an internal compiler-phase contract. It is not executable,
is not a package or distribution format, and does not create another source
language or semantic compiler. Canonical WVB remains the verified distribution
contract.

The source WVSS remains a required input to validation and emission. WVCA does
not duplicate source bytes or replace the source profile, package lock, or
source-set admission boundary.

## Public preparation and validation

```text
Compilerˉprepareˉsourceˉanalysis(Input: bytes)
    -> Compilerˉsourceˉanalysis

Compilerˉvalidateˉsourceˉanalysis(
    Input: bytes,
    Manifest: bytes,
    Bindingˉdirectory: bytes,
    Wirˉdirectory: bytes
) -> Compilerˉsourceˉanalysisˉvalidation
```

Preparation validates source symbols once, constructs binding and WIR evidence
through the fused typed-lowering pass, and publishes no artifact unless all
phases succeed. Repeated preparation of the same input produces byte-identical
manifest, WVLB, and WVIR values.

The implementation keeps phase ownership explicit. The analysis-contract
module owns only shared statuses. The analyzer links the analysis producer and
the WVIR producer; it does not retain the independent readers needed only after
serialization. The emitter links the analysis validator, the independent WVIR
validator, and the immutable WVIR consumer helpers; it does not retain source
WIR construction. The one-shot public source-to-WIR adapter composes the WVIR
producer and validator through the checked adapter. This is one semantic
pipeline with smaller executable closures, not separate compilers or a trusted
cache shortcut.

Validation does not trust the manifest as semantic evidence. It rescans WVSS,
reconstructs and validates the source-symbol model, compares every persisted
source count, validates the complete WVLB directory against that source model,
compares the WVIR header counts, and independently validates the complete WVIR
directory against WVSS, symbols, and WVLB.

The specialized pair is selected only when at least one generic function
instance is admitted. WVLB 1.2 or 1.3 retains the bounded WVGC catalog and
declaration mapping; the matching even WVIR minor retains appended concrete
function entries. The validator requires both artifacts to select the same
versioned specialization count and rejects a
missing, reordered, malformed, or mismatched catalog/body pair before emission.
WVCA remains version 1.0 because its existing byte lengths and WVIR function
count already bind either valid minor-version product without changing a field
meaning.

WVLB 1.3 additionally retains the bounded WVGT generic-nominal catalog after
the function ranges and optional WVGC evidence. The main fused analysis path
selects this carrier when an ordinary function signature or explicit local
admits at least one closed generic nominal type. Its parameter, return, local,
and operation shapes may then use catalog-bounded private shape identifiers in
the paired WVIR. Those identifiers are analysis-only evidence: prepared WVB
emission must materialize each admitted instance and replace every private
shape before publishing a distributable module. The independent carrier and
validation contract remains shared with the focused generic-type binding
extension.

The statuses distinguish an invalid manifest, rejected source symbols, source
WIR construction failure, invalid supplied WVLB, and invalid supplied WVIR.
Failure publishes no substitute evidence.

The validation result retains status, the reconstructed source-set scan, and
the reconstructed symbol summary. Keeping the larger WVLB and WVIR summaries
out of that result preserves the native backend's fixed 64-cell record bound.
After, and only after, a successful validation of the exact same four input
values, `Compilerˉsourceˉanalysisˉvalidatedˉbindings` and
`Compilerˉsourceˉanalysisˉvalidatedˉwir` materialize the already-proven
summaries for the prepared emitter. They are low-level phase plumbing with that
precondition, not alternate validators. The safe public emission adapter owns
their ordering.

## WVCA 1.0 manifest

All integers are unsigned little-endian. The manifest has no padding and its
length is exactly 104 bytes.

| Offset | Size | Field |
| ---: | ---: | --- |
| 0 | 4 | ASCII magic `WVCA` |
| 4 | 2 | Major version `1` |
| 6 | 2 | Minor version `0` |
| 8 | 4 | Manifest length `104` |
| 12 | 4 | WVSS byte length |
| 16 | 4 | WVLB byte length |
| 20 | 4 | WVIR byte length |
| 24 | 4 | Module count |
| 28 | 4 | Capability count |
| 32 | 4 | Data count |
| 36 | 4 | Record count |
| 40 | 4 | Enum count |
| 44 | 4 | Variant count |
| 48 | 4 | Function count |
| 52 | 4 | Record-field count |
| 56 | 4 | Enum-member count |
| 60 | 4 | Variant-case count |
| 64 | 4 | Source function-parameter count |
| 68 | 4 | WVLB entry count |
| 72 | 4 | WVLB parameter count |
| 76 | 4 | WVLB local count |
| 80 | 4 | WVIR function-entry count |
| 84 | 4 | WVIR block count |
| 88 | 4 | WVIR operation count |
| 92 | 4 | WVIR temporary count |
| 96 | 4 | WVIR operand count |
| 100 | 4 | Reserved, must be zero |

Every count is either recomputed from WVSS or compared with a validated WVLB or
WVIR header. The manifest intentionally carries no unverified diagnostic or
performance counters. A future incompatible field meaning requires a new WVCA
major version; a compatible append-only extension requires a new minor version
and exact admission rules.

Each supplied value remains subject to the existing 4 MiB Windvale bytes and
source-evidence bounds. The values are kept separate: an implementation must
not concatenate them into an unbounded aggregate merely to cross the phase
boundary.

## Prepared WVB emission

`Compilerˉsourceˉwvb` exposes the backend body as:

```text
Compilerˉemitˉpreparedˉsourceˉwvb(
    Input,
    Scan,
    Symbols,
    Bindings,
    Wirˉsummary,
    Optimize
) -> Compilerˉsourceˉwvbˉsummary
```

The ordinary one-shot compiler constructs those exact values and delegates to
this function. `Compilerˉsourceˉemission` is the safe phase adapter: it first
calls `Compilerˉvalidateˉsourceˉanalysis` and invokes prepared emission only for
a completely valid artifact set. Consequently the backend body, optimizer,
opcode selection, WVB version selection, and byte serialization are shared;
there is no parallel emitter.

An emitter process must treat WVSS, WVCA, WVLB, and WVIR as untrusted even when
they came from a local cache. A cache hit avoids source type analysis only after
the supplied evidence passes this validation boundary. Cache identity and
publication policy are tool concerns and must additionally bind exact compiler
and option identities; WVCA alone is not a cache key.

## Command-line products and development reuse

The public Language 1.0 host coordinator accepts an ordered source closure, the
exact source-input lock and expected hash, the selected composite source
profile, and one mandatory target descriptor:

```text
Run-Split-Compiler <wvadmit> <wvauth> <wvanalyze> <wvemit>
    [--foreign-binder <wvbind>]
    --source-input-lock <lock.wvlock> <sha256>
    --source-profile <profile.wvsp>
    --target-descriptor <input.wvtd>
    <root.wv> [dependency.wv ...] <output.wvb>
```

It snapshots every input once, constructs bounded private WVSS 1 without
reducing the 64-module limit, and retains the complete successor set in one
private directory. Its first child is the target-aware hosted admitter:

```text
wvadmit --source-input-lock <lock.wvlock> <sha256>
    --source-profile <profile.wvsp>
    --target-descriptor <input.wvtd>
    --source-set <input.wvss>
    <output.wvss> <output.wvtd> <output.wvfc> <output.wvae>
```

The admitter applies source-profile and exact-target admission, constructs the
canonical foreign catalog, and writes WVAE last only after one portable
in-memory success. The independent authenticator then consumes the same exact
six snapshots and publishes no transferable certificate or successor value:

```text
wvauth <input.wvae> <input.wvss> <input.wvtd> <input.wvfc>
    <input.wvlock> <input.wvsp>
```

Only its successful process result permits the coordinator to continue. Under
the proposed Decision 0895 checkpoint, an authenticated nonempty catalog first
causes the coordinator to recheck all six retained snapshots and then invokes
the private hosted compiler binder:

```text
wvbind <input.wvss> <input.wvtd> <input.wvfc>
```

The compiler-owned adapter in `wvbind` treats WVSS, WVTD, and WVFC as
untrusted. It validates their structure and source/catalog correspondence,
requires the exact supported target, constructs conditional WVSD 1.2/WVSI 1.3
symbols, completes direct-call binding, and checks every normalized Foreign
callable fact. A successful result is semantic fact evidence only: `wvbind`
accepts no WVAE, lock, or profile, publishes no authentication marker or
successor file, and has no admission authority.

Only after complete binding success does `wvbind` write one exact bounded
canonical line to standard output:

```text
foreign binding status=Published source-bytes=<decimal-u32> source-sha256=<64-lowercase-hex> target-bytes=<decimal-u32> target-sha256=<64-lowercase-hex> catalog-bytes=<decimal-u32> catalog-sha256=<64-lowercase-hex> foreign-count=<decimal-u32>\n
```

The lengths and digests describe the three byte values actually consumed by
`wvbind`; `foreign-count` is the validated WVFC record count. The coordinator
independently constructs the expected line from its retained WVSS, WVTD, and
WVFC bytes and requires byte-for-byte equality, including the one final newline
and the absence of prefix, suffix, alternate spelling, or additional output.
Under the current input and catalog bounds the exact line is at most 351 UTF-8
bytes. The coordinator then rechecks all six retained snapshots again, reports
exact `Foreignˉloweringˉpending`, and launches neither ordinary analysis
publication nor the emitter. Missing, malformed, mismatched, partial,
duplicated, extra, or oversized success output is failure, never transferable
success. Direct `wvbind` invocation produces only non-authoritative digest
evidence and cannot establish authentication or authorize later publication.

For an empty catalog, the coordinator instead rechecks the retained snapshot
bytes and invokes the Analyzer's explicitly non-authoritative source-set route:

```text
wvanalyze --internal-source-set <input.wvss> <output.wvss>
    <output.wvca> <output.wvlb> <output.wvir>
```

The Analyzer treats that WVSS as ordinary untrusted compiler input; neither the
option nor its path claims admission. This ordinary route explicitly rejects a
foreign-bearing WVSS with `Foreignˉrequiresˉauthenticatedˉbinding`. It scans
and analyzes an accepted foreign-free source set, then
re-publishes the exact consumed WVSS beside WVCA, WVLB, and WVIR. The
coordinator requires that WVSS to equal its retained authenticated snapshot
byte for byte before invoking the emitter with the retained original. The
emitter independently validates that original WVSS against all three analysis
artifacts. A direct Analyzer or emitter invocation therefore produces only an
untrusted intermediate or candidate; it cannot use the public coordinator to
publish a final WVB without the complete preceding `wvadmit` and `wvauth`
sequence.

The only descriptorless Analyzer form is the retained development-only Project
2 route:

```text
wvanalyze <root.wv> [dependency.wv ...]
    <output.wvss> <output.wvca> <output.wvlb> <output.wvir>
```

Before analysis, this route rejects a `System` profile and every platform or
foreign declaration. The former public `--admitted-source-set` route is
removed; possession of one serialized source set cannot bypass production
authentication.

The hosted emitter accepts exactly that persisted set and writes one
unoptimized portable WVB candidate:

```text
wvemit <input.wvss> <input.wvca> <input.wvlb> <input.wvir> <output.wvb>
```

In the authenticated route, `input.wvss` is the retained original rather than
the Analyzer's republished copy. The emitter writes only inside the private
candidate directory. The runner alone copies and syncs that completed WVB into
a unique destination-directory candidate and atomically creates the final path
without overwrite.

The production front door owns five bounded products over one semantic compiler:
`wvadmit`, `wvauth`, `wvbind`, the Analyzer, and the emitter. A nonempty catalog
launches the first three and then stops at `Foreignˉloweringˉpending`; an empty
catalog skips `wvbind` and launches the admitter, authenticator, Analyzer, and
emitter. The admitter owns Language 1.0 descriptor/profile admission and catalog
production; the authenticator independently proves the retained snapshot
relationship; `wvbind` proves only the compiler-owned foreign semantic facts;
the Analyzer owns ordinary source scanning, symbols, binding, and typed WIR
construction; and the emitter revalidates all persisted analysis values and
calls the same prepared backend as the retained one-shot compiler.
Descriptorless Project 2 input starts at the Analyzer. Descriptor-bearing
Language 1.0 input starts at the private host coordinator and admitter.
Optimization is not an implicit command-line mode; a later optimized product
must have a distinct target and producer identity.

The private `wvbind` product is an additional bounded compiler-owned stop inside
that same front door, not a public admitted-source interface. Its currently
supported target is exactly Linux x86-64, little-endian,
`sysv_amd64_c_v1`, 64 address bits, and no-unwind C scalar-pointer contract
major `1`. A structure-valid descriptor differing in any selected field is
unsupported. Foreign call lowering, WVIR/WVLB/WVCA publication for such a call,
WVB imports, native thunks, and execution containment remain later Slice 8
work.

The complete compiler-scale Analyzer remains inside Windvale 1.0's fixed 4 MiB
immutable-`bytes` ceiling. Under the pinned bootstrap transition, the
pre-ingress closure produces 4,181,228 WVIR bytes. Importing the foreign-catalog
and target contracts would add 79,212 bytes, and the first duplicated admission
adapter added another 48,860 bytes; even a geometry-only six-input adapter costs
9,748 bytes. The selected non-authoritative handoff plus Project 2 prechecks
produces 4,182,928 bytes, leaving 11,376 bounded bytes in that transition.

That narrow number is not the observed current-compiler margin. One packaged
current Analyzer successfully compiled its exact 2,132,771-byte source set to
3,815,704 WVIR bytes, leaving 378,600 bytes. The complete current-compiler
convergence check reproduced its stage-2 bytes exactly and pins the resulting
1,552,090-byte WVB at SHA-256
`5baba39b96932eca26d694b537d380f9ee6dcd4683afc81c09a99ab3c3cb9c77`.
Authentication
stays in dedicated `wvauth` because it is an independent boundary and because
duplicating the ingress closure did not fit the bootstrap transition, not
because the current compiler has only 11,376 bytes of measured headroom.

The first candidate that integrated foreign binding and its five foreign-only
modules into the Analyzer measured 4,193,688 WVIR bytes. That left only 616
bytes under the 4,194,304-byte immutable-value ceiling and triggered the
capacity reconsideration. The compiler-owned portable adapter is therefore
hosted by the separate `wvbind` product; the ordinary Analyzer retains shared
symbol and binding support plus its explicit foreign-bearing WVSS rejection,
but not the foreign-only hosted closure.

`Build-Cached-Split-Project-Wvb` is a development-only Project 2 coordinator.
Its analysis key binds the complete project source closure and exact analyzer
identity. Its emission key additionally binds the exact emitter identity and
the checkpoint binds the selected analysis key. The analyzer identity names
`source-analysis-v1`; the emitter identity and product name
`portable-wvb-optimized-v1`; both bind the current host family. A cache hit
hashes and validates the small phase values and resulting WVB, but neither
reads nor launches the large compiler executables. A miss hashes the selected
executable immediately before and after execution against its packaging-time
identity. Consequently a stale or corrupt producer cannot publish under
another producer's key, while a valid warm analysis result can be reused
without repeating source analysis or large executable hashes.

This cache is not qualification evidence. Release and cross-host conformance
continue to use canonical WVB and the named broad gates independently of local
development cache state.

## Verification boundary

The focused source-analysis fixture encodes a real WVSS input plus assertions
for deterministic artifact bytes, valid-set acceptance, a truncated manifest,
invalid magic, wrong source length, altered source-symbol count, nonzero
reserved field, corrupted WVLB, corrupted WVIR, and an inconsistent WVIR
count. The main-pipeline generic-nominal fixture additionally requires one
`Box<i32>` WVGT instance, the exact private binding shape, matching typed
parameter-load and return shapes in WVIR, and unchanged ordinary `Main`
lowering. This proves retained analysis evidence, not WVB materialization.

The current scalar WVB runner stops compiler-heavy fixtures at its documented
unsupported-operation boundary, and the general native and WebAssembly
lowerers reject this compiler closure at their current module/code admission
boundaries. Therefore the current checkpoint claims successful source
compilation of the fixture and availability of the independent validation path,
not execution of that fixture through those unsupported backends. The complete
compiler source set reconstructs successfully with the one-shot wrapper delegating to
the prepared emitter. Runtime equality evidence becomes required when a
qualified compiler-capable execution path admits the focused fixture.

WVLB and WVIR layouts and their detailed independent-validation rules remain
owned by [source bindings](Compiler-Source-Bindings.md) and
[typed source IR](Compiler-Source-Wir.md). WVB construction remains owned by
the [source-to-WVB backend](Compiler-Source-Wvb.md).
