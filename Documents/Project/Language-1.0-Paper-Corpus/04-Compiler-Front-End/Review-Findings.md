# Workload 4 review findings

## Status

All six findings are accepted by the project owner through
[Decision 0758](../../../Decisions/0758-Resolve-Language-1.0-Compiler-Front-End-Findings.md).
The bundle is draft reviewed. Foundation/grammar identities remain candidates
until complete corpus reconciliation and source freeze.

## Finding 1: empty generic owners need explicit arguments

Diagnostics, tokens, declarations, nodes, bindings, symbols, and operations all
begin empty. First-item construction would add state branches to every compiler
phase and cannot represent a valid zero-declaration symbol table cleanly. This is
the second complete workload meeting Decision 0754's reconsideration trigger.

Resolution: accept `Qualifiedˉfunction::<T, const...>(...)` for named generic
function declarations. Every parameter is supplied in declaration order; no
partial/default/result/protocol inference occurs. Ordinary calls retain
argument-derived inference. `::` makes the grammar unambiguous with `<` and `>`.

## Finding 2: a borrowed map result must name only one owner

Workload 3's provisional `Mapˉborrowˉexisting(Map borrow, Key borrow)` has two
borrowed parameters but no lifetime syntax to say the result belongs to Map.

Resolution: supersede it with owned `Mapˉfindˉrank(Map borrow, Key borrow) ->
Option<u64>` followed by `Mapˉborrowˉat(Map borrow, Index) -> borrow V`.
`Mapˉkeyˉat` and `Mapˉborrowˉat` share canonical rank. Arena borrow operations
take Copy handles by value, leaving the arena as their one borrowed owner. This
uses the exact Copy read-through rule for a borrowed handle, preserves two-step
recovery, and removes ambiguous lifetime provenance without adding pointer or
dereference syntax.

## Finding 3: parser publication needs an immutable arena

Destroying the mutable arena would invalidate AST handles, while passing it
mutably into binding would violate phase isolation.

Resolution: accept consuming `Arenaˉfreeze(Arena<T>) -> Immutableˉarena<T>`.
Freeze preserves arena identity, live slots, generations, capacity, and retained
charge; invalidates mutable borrows; admits validation/borrow/slot-order
observation only; and cannot compact in a way that changes handles.

## Finding 4: source text needs exact scalar-position primitives

Byte offsets alone misdiagnose Unicode, while host characters/UTF-16/display
columns are target dependent.

Resolution: accept strict reserved UTF-8 decode, rune-at, rune UTF-8 width, and
shared range operations. Compiler spans retain byte and rune offsets plus
one-based scalar line/column. No normalization or host newline conversion occurs.

## Finding 5: diagnostic cascades need a canonical saturation policy

A mere vector maximum makes the last failure depend on which phase discovers
capacity first and can lose evidence that output was intentionally truncated.

Resolution: reserve one last slot. Retain at most maximum-minus-one ordinary
diagnostics, append exactly one `Diagnosticˉlimit` at the next issue, then ignore
later issues without work/state growth. Any diagnostic suppresses artifact
publication.

## Finding 6: phase models and byte emission need exact publication APIs

General vector/builder prose was insufficient to write complete compiler source.

Resolution: accept reserved empty vector construction, append with ownership
return, consuming sequence freeze, empty map/arena construction, immutable arena
observations, and atomic `u8`/little-endian `u32`/`u64` builder appends. Mutable
owners never cross phase publication; the new calls lower through ordinary
collection/byte semantics rather than compiler-specific WIR operations.

## Conclusion

The front end is readable without classes, exceptions, GC, unsafe pointers,
hidden caches, or packed AST offsets. Explicit generic syntax appears only at
genuinely empty construction sites. The revised rank/borrow shape also corrects
a lifetime ambiguity discovered by cross-workload review rather than adding
general lifetime syntax prematurely.
