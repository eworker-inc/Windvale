# Decision 0812: Thread generic nominal evidence through main analysis

- Status: Accepted
- Date: 2026-08-21

## Context

WVGT already gives each concrete generic record or variant one bounded canonical
identity, WVLB 1.3 can retain that catalog beside local bindings, and the focused
materializer can assign ordinary WVB type indices. The main source analyzer did
not connect those pieces. A signature such as `Box<i32>` therefore stopped in
Source Symbols before typed WIR could admit the instance, while the focused
generic modules succeeded only when called directly.

The analyzer also has a fixed native packaging envelope. Pulling diagnostic
validators into its successful product path or duplicating generic parsing in
Source Symbols would spend that capacity without adding semantics.

## Decision

1. Source Symbols continues to own declarations, names, visibility, and ordinary
   nominal types. When a declaration's already-validated type production reaches
   a known unresolved `<...>` application, Source Symbols defers that one generic
   semantic decision instead of inventing a concrete shape. Exact family, arity,
   argument kind, constant width, depth, and catalog identity remain WVGT binder
   responsibilities.
2. Main Source WIR scans reachable non-generic function signatures in canonical
   module and declaration order before lowering bodies. It scans explicit local
   annotations as they are encountered. Successful binding returns a replacement
   immutable WVGT catalog; inner instances therefore precede their parents.
3. Compiler-private build state carries WVGC and WVGT together in a bounded
   `WVGI 1.0` envelope. This envelope is not a serialized public artifact. An
   empty WVGT preserves the prior raw WVGC bytes exactly so ordinary and
   function-generic source remain byte-compatible.
4. Main binding publication uses WVLB 1.3 whenever WVGT is nonempty. Parameter
   and local entries may retain only catalog-bounded private shapes
   `0x80000000..0x800000ff`. Source Analysis validates the selected WVLB version
   through the combined generic carrier.
5. WVIR uses the current ordinary version 1.3 when there are no function specializations. Function
   return shapes, parameter/local operations, and temporary evidence may retain
   private WVGT shapes only while paired with the validated WVLB catalog. Function
   specialization still selects the specialized WVIR envelope through the existing WVGC contract.
6. Successful production uses dedicated product publication functions. Direct
   validation entry points still run the independent WVLB and WVIR validators,
   while a failed product build retains compact exact status/module evidence and
   invokes diagnostic replay only when a caller requests that boundary.
7. This checkpoint does not emit a generic nominal WVB type and does not claim
   application execution. The next connection consumes WVGT from WVLB, runs the
   accepted materialization plan, remaps private shapes, and inserts the focused
   serializer's ordinary WVB Types entries.

## Evidence

`Generic-Nominal-Main-Pipeline.wv` declares `Box<T>` and gives one ordinary
function a `Box<i32>` parameter and return. The main analyzer publishes a
238-byte WVSS, 104-byte WVCA, 192-byte WVLB 1.3, and 320-byte WVIR 1.3. WVLB
retains one 68-byte WVGT catalog and one parameter binding with shape
`0x80000000`; the function return and parameter-load WIR operation retain that
same private shape. The ordinary `Main` function remains shape `i32` and emits
the exact constant `42` operation.

The 12-case artifact inspector checks the source/manifest lengths, WVLB 1.3
envelope, empty WVGC length, WVGT identity and `i32` argument, private binding,
WVIR 1.3 function signatures, and both operations. The existing generic-function
fixture produces byte-identical WVSS, WVCA, WVLB, and WVIR artifacts before and
after this connection, proving the empty-WVGT compatibility path.

After Decision 0813's current-format compaction, the Windows analyzer contains
477 functions, 811,632 code bytes, and 992,412 WVB bytes, SHA-256
`26ea9bccfe8c2763fb887a5a14c2f0a086a27265523c3df84187b361616f9120`.
Its eight-fragment profile-7 package is 31,740,416 bytes, SHA-256
`52c6cccdcaed1e99ea87759751d232e0f39bd1ed923d0555e4da5f4b236b442f`.
This is current-host development evidence, not a paired-host conformance claim.

## Consequences

General generic nominal identities now reach the exact boundary consumed by the
WVB backend without creating a runtime generic model or a parallel compiler.
The compiler pays for semantic generic binding once, retains immutable evidence,
and leaves ordinary/function-generic artifacts unchanged when WVGT is empty.

Source Symbols alone is no longer the final semantic oracle for a deferred
generic application. Any product accepting that application must complete the
WVGT-aware WIR path, and independent analysis validation must receive the paired
WVLB catalog.

## Reconsideration triggers

Revisit this decision if WVIR becomes independently distributable without its
paired binding evidence, if generic templates acquire runtime identity, if
reachable-type pruning changes catalog order, or if the private-shape range or
1,024-type WVB limit changes.
