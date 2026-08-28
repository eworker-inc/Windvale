# Decision 0871: index callable lookup and retain base WIR

## Status

Accepted on 2026-08-28.

## Context

Compiler-scale source analysis remained bounded but could exhaust the native
tool carrier's fixed `2^37` instruction allowance. Inspection found three
avoidable costs on successful paths. Qualified-name resolution materialized an
entire owner or target module to compare a short alias or declaration name;
ordinary callable resolution still used a first-byte bucket; and discovery of
one generic instance discarded already completed base WIR before recompiling
the complete program.

These are implementation costs, not permission to weaken exact name identity,
generic order, diagnostics, evidence limits, or deterministic output.

## Decision

Private lookup evidence advances from WVSI 1.1 to WVSI 1.2. It appends 256
stable callable buckets keyed by resolved target module and exact UTF-8 name.
The hash is only a candidate selector. Resolution still requires exact target,
kind where applicable, byte length, and ordinal bytes, so collisions preserve
the original oracle.

The source-set module offset is exposed as a bounded internal query. Alias and
callable comparisons use absolute WVSS spans rather than constructing a module
slice. Qualified-name parsing stops after its first separator.

WIR and binding construction retains function-private payloads and publishes
at most 16 consecutive functions as one bounded group. The planning pass is the
canonical base WIR. Later rounds append only newly discovered WVGC
specializations. Closures are appended after the generic catalog stabilizes;
closure discovery may retain catalogs but never a partial closure payload. The
existing 32-round limit remains unchanged.

Foundation intrinsic resolution carries its already resolved canonical WIR
operation identity from the binding match instead of repeating the complete
name dispatch during lowering.

## Evidence

The focused generic identity, multiple-specialization, and nested-discovery
programs produce byte-identical source-set, manifest, binding, and current-WIR
artifacts before and after incremental specialization. The permanent nested
fixture specifically requires `Outer<T>` lowering to discover `Inner<T>`.

The qualified-name early exit is deliberately written as a simple loop with an
explicit terminal index. The qualified old build driver miscompiled the
equivalent compound `while A && B` spelling and its own verifier rejected the
result for control reachability, while the current analyzer and emitter
produced a valid module. The seed-compatible spelling preserves the early exit;
the focused Bindings build and all 20 generic WVLB-carrier cases pass.

The trusted bootstrap builds the 755-function Windows candidate from 2,094,437
source bytes with a 316,060-byte binding product and 4,100,604-byte WIR. Its
portable WVB is 1,511,092 bytes at SHA-256
`602ddd98461ec84da931a23e65a991714932220ee21400050cdd9aa9f0e7517f`.
The bounded special-intrinsic prefilter includes the two longest collection
operations, `Vectorˉgrowˉreserved` and `Vectorˉconstructˉreserved`; the
focused construct oracle passes one valid boundary and eight malformed WIR
cases with the matched candidate analyzer and emitter.

The candidate still did not complete whole-compiler self-analysis under the
current native carrier's fixed instruction allowance and published no output
artifacts. This decision therefore claims removal of specific redundant work
and exact-output preservation, not completed self-hosting or an overall
compiler speed threshold.

## Consequences

- Long Windvale identifiers do not require copying a large source module for
  each alias or callable comparison.
- Callable lookup is bounded by one deterministic collision bucket plus an
  exact oracle rather than all declarations sharing one first byte.
- Generic discovery no longer recompiles already accepted base functions or
  earlier specialization rounds.
- WVIR, WVLB, source semantics, diagnostics, and generic catalog order do not
  change.
- Whole-compiler self-analysis remains a named performance checkpoint. Raising
  the carrier budget alone is not evidence that the compiler became faster.

## Reconsideration triggers

Reconsider the private index when a source-set symbol table can retain canonical
absolute name spans directly. Reconsider grouped publication if Windvale gains
a bounded byte builder with demonstrably lower copying. Reconsider the generic
algorithm only with an exact-output oracle for nested specialization and
closure-driven discovery.
