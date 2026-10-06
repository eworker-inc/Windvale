# Decision 0978: Represent reserved byte construction in WVIR 1.39 and 1.40

## Status

Accepted implementation direction under the maintainer's language, memory and
essential-library priority, 3 October 2026. The source/WVIR profile is a
candidate under implementation. Bytecode execution, native lifetime integration,
cross-host qualification and release remain separate gates.

## Problem and result

The reserved native byte-builder leaf has focused Windows and Debian development
evidence, but ordinary source cannot yet name or invoke the frozen builder
family. Existing immutable bytes still use bootstrap arena mechanics. Recognizing
a function spelling without its ownership, failure and lifetime evidence would
not connect the language to the new storage safely.

Extend the existing source compiler with the canonical `Foundationˉbytes`
identity, its representation-hidden owned `Bytesˉbuilder`, the complete frozen
reserved construction family, immutable byte observations and borrowed ranges.
Use the existing canonical Result, allocation failure and limit failure layouts.
No constructible record, integer token, ambient budget or alternate compiler is
introduced. The Language 1.0 source grammar and signature registry are unchanged.

## Representation and version boundary

The private source shape for `Bytesˉbuilder` is `805306370`. Builtin `bytes` and
`text` remain shared immutable shapes 6 and 5; they are not new nominal types.
Canonical `Foundationˉcollections.Slice<T>` uses generic catalog kind 13 and
the appended declaration kind `Borrowedˉslice`. Prior declaration and catalog
numbers retain their meanings. A Slice is borrowed, including when passed with
the ordinary Slice parameter spelling, and cannot be stored in escaping user
aggregates or captures.

Candidate WVIR 1.39 carries the byte/slice vocabulary without function generic
instances; 1.40 carries the same vocabulary with the existing generic catalog.
Both retain the existing function, block, operation, temporary and operand
record layouts. The paired header convention and function-type catalog tail
follow the earlier candidate 1.37/1.38 boundary.

The exact operations, canonical result checks, source-order evaluation and
borrow proof are owned by [Compiler source WVIR](../../Specifications/Compiler-Source-Wir.md).
Construction consumes a budget value temporary, so propagation during a later
argument must release already transferred temporary ownership. Named arguments
evaluate in their written order even when serialized into signature order.
Appending borrows the receiver exclusively from its evaluation until the call
completes. Freeze consumes its builder and yields shared immutable bytes.

## Delivery and verification

Keep this evidence in the existing source/WVIR checks and front-door development
owner. Exercise canonical identities and field widths, inferred results, every
append, source-order evaluation, moves, borrows, helper returns, scope cleanup,
malformed directories and unchanged older collection behavior.

This decision assigns the source/WVIR boundary. [Decision 0979](0979-Connect-Reserved-Byte-Construction-To-WVB-1.44.md)
selects WVB 1.44 and [Decision 0980](0980-Bind-Native-Shared-Values-To-Budgets-And-Tool-Entries.md)
selects the candidate native ABI and context. Executable admission must still
refuse the new shapes until their complete verifier and execution path is
connected. Accepted WVIR evidence alone is not an executable byte-builder
application or a complete memory-management claim.

The next gate connects that verified evidence to shared storage through copies,
slices, locals, calls, returns, aggregates, failure exits and terminal teardown.
Then migrate a maintained compiler serializer and working-state collection before
retiring the active arena paths they replace.
