# Decision 0814: Connect generic nominal materialization to main WVB

- Status: Accepted with current-Windows development evidence; serialized Types
  insertion order superseded by Decision 0843; independent Linux qualification
  pending
- Date: 2026-08-21
- Advances: [generic type evidence](../../Specifications/Compiler-Source-Generic-Types.md), [source WVB](../../Specifications/Compiler-Source-Wvb.md), and [Decision 0812](0812-Thread-Generic-Nominal-Evidence-Through-Main-Analysis.md)

Decision 0843 retains the main materialization connection and private-identity
mapping but replaces the declared-prefix/catalog-suffix ordering in item 2 with
canonical WVB semantic-category/name order.

## Context

Main analysis already retained concrete generic nominal identities in WVGT and
carried private shapes through paired WVLB/WVIR evidence. The accepted focused
serializer could produce ordinary WVB record and variant entries, but the main
emitter did not invoke it. Generic templates also occupied the source nominal
directory even though Language 1.0 gives a template no runtime type identity.
Using that source directory directly as the WVB Types index space would either
emit templates or leave holes.

A second ambiguity appeared when templates were removed: a remapped concrete
generic output index can numerically equal an ordinary source nominal index.
Converting private WVGT references too early therefore loses the evidence needed
to distinguish the two identities.

The complete emission compiler initially grew from 121 to 133 nominal types.
The native compiler profile retains an explicit 128-type bound, so the
connection also needed to remove compiler-only representation duplication rather
than widen the backend limit.

## Decision

1. Main Source WVB extracts the exact WVGT catalog from WVLB, reconstructs the
   bounded materialization plan, and invokes the accepted generic nominal
   serializer before publishing any WVB bytes.
2. Generic record and variant declarations are templates, not WVB Types entries.
   The declared prefix contains concrete records, all enums, and concrete
   variants only. Materialized WVGT instances follow in catalog order, then the
   existing concrete Foundation specialization suffix.
3. Build explicit source-nominal-to-WVB target maps. A template receives the
   sentinel target and cannot be referenced as a concrete value. Function
   metadata, fields, temporaries, record/enum/variant operations, and public
   reachability analysis all consume the same immutable maps.
4. Preserve an earlier materialized field's private WVGT identity in compiler
   evidence until final WVB shape planning. The final serializer resolves it to
   the assigned ordinary Types index and validates the recorded record/variant
   kind. No private shape enters WVB.
5. Preserve ordinary source output bytes when WVGT is empty and no template
   affects nominal ordering. Do not add a legacy template entry, placeholder
   type, runtime generic registry, or alternate compiler path.
6. Keep the existing 1,024 language Types limit, 256-instance WVGT limit, 256
   Foundation-specialization limit, and 4 MiB generic Types payload bound.
   Invalid or oversized evidence publishes no partial WVB.
7. Keep the active Windvale compiler evolvable. This implementation becomes
   part of the self-hosting Language 1.0 compiler; the immutable Stage 0 recovery
   release remains only the separate bootstrap and recovery provenance.
8. Restore native compiler capacity by removing five transient compiler-only
   record representations: the digit quotient/remainder wrapper, duplicate
   capability encoding wrapper, nominal-plan wrapper, and materialized type/case
   accessor records. The packed evidence instead exposes bounded word accessors
   whose invalid result is the existing `u32` sentinel.

## Evidence

The final 22-module emission-driver analysis publishes 1,848,314 source bytes,
283,268 binding bytes, and 3,739,652 WIR bytes. The optimized compiler contains
529 functions and 798,745 code bytes in a 964,539-byte WVB at SHA-256
`9c11b7eb3b9e250817a0a763adf1fea8d7406bf6e2869247f4a7f84146307347`.
Its Types count is exactly 128. The existing profile-7 native boundary accepts
it without a limit change; the six-fragment Windows package is 21,254,144 bytes
at SHA-256
`57c36ac13745b103fccbd677d4f54c3dbc112c739b520690b424b40bae491278`.

`Generic-Nominal-Main-Pipeline.wv` deliberately declares the template `Box<T>`
before the concrete record `Point`, then admits `Box<Point>`. This gives ordinary
source target 1 and generic output target 1—the collision that requires retained
private identity. Main analysis publishes exact 272-byte WVSS, 104-byte WVCA,
208-byte WVLB 1.3, and 368-byte WVIR 1.3 artifacts. The new emitter publishes a
252-byte WVB 1.11 at SHA-256
`8871f2876c9135e8f4f8740f7643d1ff5a5eb0e771da0dddd3357e1bed9d29aa`.
Its Types section contains `Point { X: i32 }` at index 0 and
`__WvY0000 { Value: Point }` at index 1. It contains no `Box` template. The
independent compiler-aligned verifier accepts the WVB and the native runner
returns `42`.

The focused generic nominal materialization owner passes all 30 cases and
returns `42`. Its updated compiler-aligned WVB is 734,722 bytes at SHA-256
`080990672a4f2912877ddae201c9fe0b35c858c40d51dc072567a3191e6e7757`.

These are current-Windows development results, not paired-host conformance or
release qualification.

## Consequences

The main compiler can now publish ordinary WVB metadata for retained generic
record and variant instances without teaching the verifier, interpreter,
runtime, or native backend about generics. Templates consume no runtime Types
capacity. All type identity changes are explicit at one planning boundary.

This checkpoint proves analysis-through-WVB materialization, deterministic
metadata, verification, and executable publication. Runtime construction and
field access for general generic nominal values, generic-function-context type
uses, and migration of the remaining Foundation special planning are later
connected checkpoints. The fixture's `Main` returns an ordinary constant, so its
successful execution must not be misread as proof of those body operations.

The emission compiler is at, not below, the current 128-type native ceiling.
Future compiler features should prefer cohesive extraction and packed immutable
evidence before considering any bound change.

## Reconsideration triggers

Revisit this decision if templates acquire an explicitly accepted runtime
identity, if WVB gains reified generics, if reachable-type pruning changes
catalog order, if public WVB naming rules admit the private synthetic namespace,
or if representative compiler work cannot remain within the documented native
representation bound after measured refactoring.
