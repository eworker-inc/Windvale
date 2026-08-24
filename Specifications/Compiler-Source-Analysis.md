# Windvale source-analysis phase artifact

## Status and purpose

`Compilerˉsourceˉanalysis` separates reusable source and type analysis from WVB
emission. It consumes one canonical admitted WVSS source set and publishes three
individually bounded values:

- one fixed 104-byte `WVCA 1.0` manifest;
- one canonical `WVLB 1.1`, function-specialized `WVLB 1.2`, or combined
  specialization-evidence `WVLB 1.3` binding directory; and
- one canonical `WVIR 1.9` through `WVIR 1.14` typed source directory, with the
  exact ordinary/specialized and memory/append feature pairing defined by WVIR.

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

The hosted Language 1.0 admission front door accepts an ordered source closure,
the exact source-input lock and expected hash, and the selected composite
source profile:

```text
wvadmit --source-input-lock <lock.wvlock> <sha256>
    --source-profile <profile.wvsp>
    <root.wv> [dependency.wv ...] <output.wvss>
```

It constructs bounded WVSS 1 input, applies the same
`Compilerˉadmitˉsourceˉprofileˉinputs` contract used by one-shot compilation,
and publishes descriptor-free WVSS 2 only after every module agrees on edition
and profile. Rejection publishes no admitted source set.

The hosted analyzer front door accepts either an ordered Project 2 source
closure or one already admitted source set:

```text
wvanalyze <root.wv> [dependency.wv ...]
    <output.wvss> <output.wvca> <output.wvlb> <output.wvir>

wvanalyze --admitted-source-set <input.wvss>
    <output.wvss> <output.wvca> <output.wvlb> <output.wvir>
```

The hosted emitter accepts exactly that persisted set and publishes unoptimized
portable WVB:

```text
wvemit <input.wvss> <input.wvca> <input.wvlb> <input.wvir> <output.wvb>
```

These are three bounded front-door products over one semantic compiler. The
admitter owns Language 1.0 descriptor/profile admission, the analyzer owns
source scanning, symbols, binding, and typed WIR construction, and the emitter
revalidates all persisted values and calls the same prepared backend as the
retained one-shot compiler. Descriptorless Project 2 input starts at the
analyzer; descriptor-bearing Language 1.0 input starts at the admitter.
Optimization is not an implicit command-line mode; a later optimized product
must have a distinct target and producer identity.

`Build-Cached-Split-Project-Wvb` is a development-only Project 2 coordinator.
Its analysis key binds the complete project source closure and exact analyzer
identity. Its emission key additionally binds the exact emitter identity and
the checkpoint binds the selected analysis key. Both identities name target
`portable-wvb-v1` and the current host family. A cache hit hashes and validates
the small phase values and resulting WVB, but neither reads nor launches the
large compiler executables. A miss hashes the selected executable immediately
before and after execution against its packaging-time identity. Consequently a
stale or corrupt producer cannot publish under another producer's key, while a
valid warm analysis result can be reused without repeating source analysis or
large executable hashes.

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
