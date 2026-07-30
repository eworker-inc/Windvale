# Decision 0013: Balanced persistent byte sequences

- Date: 2026-07-30
- Status: Accepted, implemented, and cross-host qualified at `89ce80b`

## Context

Windvale Linking 1 permits a 4 MiB image, 256 section contributions, and 65,536 four-byte relocations. Seed can express an immutable patch as two slices plus a replacement and concatenation, but the original reference runtime flattened every `Bytesˉconcat`. Building or repeatedly patching a large image would therefore copy the complete accumulated prefix on every operation. The result would preserve semantics while making a contract-valid Windvale linker impractical.

A mutable public byte buffer would solve the copying problem by changing the language's ownership and aliasing model before compiler or OS evidence justifies it. A linker-specific host callback would instead move portable image semantics into C#.

## Decision

- Preserve `bytes` as an immutable value with the existing 4 MiB bound and unchanged WVB instructions.
- Represent reference-runtime byte values internally as height-balanced persistent trees of immutable leaf spans.
- Make concatenation join balanced trees with structural sharing rather than flattening both inputs.
- Keep slicing zero-copy: whole subtrees are shared and only boundary leaves are narrowed.
- Make byte reads traverse the bounded tree and make operations that require contiguous native bytes—strict UTF-8 conversion, SHA-256, and hosted file output—materialize exactly once at that boundary.
- Do not expose tree shape, leaf count, identity, or materialization through Windvale semantics.
- Prove the implementation with a Windvale program that performs 65,536 ordered one-byte appends, reads both ends, structurally patches four middle bytes with slices and concatenation, reads across the new tree boundary, and confirms the original value remains unchanged.

## Consequences

- Repeated append and slice/replace patterns allocate logarithmic tree paths instead of repeatedly copying the whole accumulated value.
- Existing bytecode, compiler output, module hashes, source syntax, traps, length limits, byte order, hashing, UTF-8, and hosted file bytes remain unchanged.
- Materialization still costs linear time and one contiguous allocation, but only consumers that inherently require a complete native sequence pay it.
- Runtime instruction limits bound how many persistent operations a module can create; the byte-value limit continues to bound materialized content.
- This removes the immediate need for a mutable linker-only builder. A later general collection or ownership model must still be justified by compiler and Foundation use cases.

## Reconsider when

- Profiling representative linker or compiler workloads shows tree traversal or materialization dominates execution.
- A streaming capability can consume immutable segments without exposing host-dependent chunking.
- Native Windvale needs an ownership-checked mutable buffer for workloads that persistent sequences cannot serve efficiently.
