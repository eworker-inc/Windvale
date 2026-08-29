# Decision 0884: preserve the Slice 8 native data bound

## Status

Accepted implementation correction on 2026-08-29. Slice 8 remains in progress.

## Context

The first pushed Slice 8 front-door checkpoint passed its focused System/FFI
owner but failed the affected generic compiler-package job on both hosts. The
compiled package retained 265 static data records, while the qualified native
x64 lowerer accepts at most 256. The foreign parser had embedded separate text
literals for each accepted declaration layout, ABI identity, function name, and
symbol even though all of those values describe one frozen contract.

Raising the retained lowerer bound would rebuild and repin a wider native
artifact family without addressing that duplication. The existing bound is a
qualified resource contract, and this checkpoint does not provide evidence that
a larger general compiler catalog is required.

The exported evidence function also needed a stronger trust boundary. A caller
could pair a parser-produced declaration record with changed same-length source,
or construct a nominally valid record with different semantic fields, and still
request ABI and symbol spans.

## Decision

1. Keep the qualified native x64 static-data limit at 256.
2. Retain one 253-byte canonical foreign declaration literal. Match the compact,
   optional trailing-comma, and paper layouts through bounded spans and explicit
   punctuation instead of embedding a complete literal for every layout.
3. Preserve the first checkpoint's exact exported spelling. Only the seven-byte
   `export ` prefix may precede an admitted body; skipped comments, newlines, or
   additional trivia do not silently expand the frozen surface.
4. Authenticate both sides of foreign evidence. The complete admitted source
   layout must match, and the declaration record must retain valid lexical
   status, the exact name and declaration spans, an empty body, three items, the
   one `ffi.call` effect, matching continuation provenance, and the expected
   terminal identities.
   Continuation line and column values must be nonzero and internally
   consistent, but their absolute diagnostic coordinates remain owned by the
   parser; downstream semantic admission trusts authenticated byte offsets, not
   diagnostic coordinates.
5. Derive ABI and symbol spans directly from the authenticated scalar layout.
   Do not perform a redundant second lexer walk after every byte of the exact
   layout has already been proved.
6. Keep the owner at 12 isolated executions while adding rejection of a
   same-length `ffi.fail` source, rejection of a forged zero-effect record,
   rejection of export trivia, and exact exported trailing-comma evidence.

## Consequences

- The affected compiler package retains 254 static data records, eleven fewer
  than the failing 265-record artifact and two below the retained limit.
- The final 875,168-byte compiler WVB has SHA-256
  `6ffbe3622688d97410903448074503b35593ab8db2a04c5e945407c2ee606da8`.
- The focused System/FFI owner passes 12 cases with byte-identical rebuilds and
  12 isolated executions. The downstream generic/type-binding owner passes all
  59 cases, including package staging, linking, transport, and assembly.
- Invalid exported layouts reject directly after their exact body is recognized
  instead of spending thousands of fallback-parser instructions to reach the
  same `Invalidˉforeignˉsignature` result.
- The verification-owner registry remains at 115 owners and 5,630 cases; no
  routing or registry identity changes are required.
- These are local Windows results. The pushed correction must still pass the
  affected Windows and Linux workflow jobs before cross-host conformance is
  claimed.

## Reconsideration triggers

Raise the native static-data limit only when measured, non-duplicated compiler
or application requirements exceed 256 and the retained lowerer, publishers,
reconstruction artifacts, malformed-input cases, and paired-host qualification
are advanced together. Generalize foreign syntax or exported trivia only through
a named language decision with matching parser, evidence, editor, and migration
coverage.
