# Decision 0078: Multi-patch Windvale native stencil

- Date: 2026-08-01
- Status: Implemented Windows candidate; cross-host qualification pending
- Extends: [Decision 0077](0077-First-Windvale-Owned-Native-Stencil.md)
- Preserves: Native ABI 14, execution-context version 6, service-table version 4, WVB 1.6, WVO 1.0, kernel bridge 9, and firmware probe 16

## Context

Decision 0077 proves one live Windvale-assembled template with one typed patch, but that case cannot show how repeated and semantically distinct ABI values coexist without positional C# construction. The already-qualified 70-byte `process.argument` leaf is the smallest active second case: it has control flow, two repeated contract meanings, six distinct meanings, and eight byte locations while requiring no call, relocation, or architecture change.

## Decision

- Define the deliberately bounded ordered `WVSP 2` record in [the WVA native-stencil specification](../../Specifications/Wva-Native-Stencil.md). Its header fixes patch count and template size; each record carries a template-relative offset, width, and closed semantic kind.
- Author the complete zero-hole machine shell and all eight ordered records in `Compiler/Native/Stencils/Process-Argument.wva`.
- Retain the canonical 321-byte WVO produced by the Windvale-written WVA assembler as an embedded native-compiler resource. Its SHA-256 is `307e61dcb2a156eb0d4b77f7d93676d7b1ac24f9bb6fe1f31217837213352bad`.
- Name six distinct ABI meanings rather than accepting caller-chosen positional values. The service-detail and borrowed-text-length kinds each appear twice and must receive one checked value at both locations.
- Require strict object, metadata, ordered-record, fixed-shell, zero-hole, and instantiation validation. Missing values, unused kinds, duplicate or descending locations, changed widths, and unknown kinds fail closed.
- Route the live `X64ˉnativeˉargumentˉservices.Build(Processˉargument)` path through this artifact. The final leaf remains exactly 70 bytes with SHA-256 `2253e1435f141df5b68f9f7e9e9aa0de448410c42dcf33ad76dcf131afea65d1`.
- Retain `WVSP 1` unchanged for recovery and compatibility within the current source state. `WVSP 2` is a measured second exact contract, not a permissive generalization or a stable public format.
- Keep C# as the strict consumer, identity oracle, W^X publisher, executor, and Stage 0/recovery implementation for this slice.

## Evidence boundary

The focused Windows candidate compiles with zero warnings. Its combined conformance case compiles the Windvale-written assembler once, runs it twice over each accepted WVA source, requires deterministic WVO bytes and exact Stage 0 equality, verifies the two embedded artifact identities, instantiates one and eight patches, and preserves both final leaf digests. It rejects corrupt counts, sizes, offsets, widths, kinds, opcodes, holes, incomplete value sets, and duplicate locations. The live native hosted-input case executes the new `process.argument` construction successfully. Cold focused runs take 2.906 seconds for the stencil case and 0.632 seconds for live hosted input. The change-aware Windows gate selects all eight test areas and passes all 60 tests in 221.385 seconds with zero warnings; its warm stencil and live-input cases take 1.155 and 0.132 seconds. This gate includes the 166.719-second golden contract but remains development feedback rather than qualification evidence.

This is implementation evidence only. Qualification requires one exact committed source state to pass the complete Windows and isolated Debian gates, normalized-contract comparison, portable-artifact comparison, applicable OS tests, and independent GitHub verification before this decision may claim cross-host qualification.

## Consequences

Both process-input service leaves are now constructed from WVA-authored, Windvale-assembled objects. Production C# no longer contains either final argument-service byte array, although its strict loader intentionally retains exact shells and semantic mappings as the independent acceptance oracle.

This still does not constitute a general JIT. The contract has no branch-target patches, calls, relocations, data references, template selection, native Windvale loader, or executable-memory ownership. The next ownership transfer should move bounded object/stencil validation and patch application into a Windvale-written consumer before broadening executable shapes, unless measured implementation evidence shows a branch-bearing stencil is the smaller prerequisite.

## Reconsider when

- A measured compiler operation requires wider constants, branch targets, calls, data references, or multiple templates.
- A Windvale-written consumer can replace C# object loading and patch application while retaining the independent Stage 0 oracle and hostile-input boundary.
- Repeated canonical WVO loading materially affects startup, package size, or code-cache identity.
