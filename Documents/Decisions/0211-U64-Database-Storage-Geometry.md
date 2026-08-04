# Decision 0211: U64 database storage geometry

- Date: 2026-08-04
- Status: Implemented candidate with focused Windows evidence; independent Linux qualification pending
- Advances: [Windvale Database proposal](../Project/Windvale-Database-Proposal.md), [database storage geometry](../../Specifications/Database-Storage-Geometry.md), and [single current WVB 1.11](0209-Single-Current-Wvb-1-11-Format.md)
- Retains: experimental `WVDB 1` bytes and `u32` fields, no durable-format selection, no storage authority, and no writes

## Context

Durable database storage needs `u64` page identities and byte positions, but page sizes and many bounded in-memory counts remain naturally `u32`. Windvale deliberately has no implicit integer conversion. Without an explicit lossless widening operation, checked page calculations either cannot be expressed or would encourage duplicate byte-encoding workarounds.

The next storage boundary also needs one reusable definition of `Offset`, length, and exclusive end before Windows, Linux, Windvale OS, or WebAssembly providers are designed. That arithmetic should be portable and capability-free; opening files, granting authority, and defining flush or recovery behavior are different contracts.

## Decision

- Add pure source intrinsic `U64ˉfromˉu32(u32) -> u64` and canonical WVB 1.11 opcode `0xBF` (`u64.from_u32`). The operation preserves every `u32` value exactly and cannot trap.
- Append WVIR operation `128` and carry it through Stage 0, the Windvale-written source compiler, canonical bytecode verification and inspection, the portable compiler-aligned verifier, and the reference runtime.
- Add portable `Libraries/Database/Storage-Geometry.wv`. Its public operation accepts `u64` header size, `u32` page size, zero-based `u64` page identity, and `u64` storage length.
- Return a typed valid range containing page identity, offset, length, and exclusive end, or `Invalidˉpageˉsize`, `Arithmeticˉoverflow`, or `Outsideˉstorage`.
- Preflight every product and sum. Do not rely on runtime integer-overflow traps for ordinary malformed storage requests.
- Keep the baseline x64, bounded WebAssembly, and Windvale OS execution profiles explicit narrower WVB 1.11 subsets until they separately adopt wide scalar values.

## Evidence

The focused Windows Database verifier compiles the library deterministically, verifies its portable/capability-free typed surface, composes an executable adapter, and checks ordinary, above-4-GiB, maximum-`u32` page-identifier and page-size, zero-size, addition-overflow, multiplication-overflow, exclusive-end-overflow, and outside-storage cases. The existing `u64` codec case also executes the new widening opcode through the reference runtime.

## Consequences

Future storage providers can share exact page-range arithmetic without depending on host paths or handles. `u64` here removes the approximate 4 GiB position ceiling; it does not promise that a host, process, WebAssembly memory, or current runtime can allocate or map a value of that size.

This decision is a prerequisite, not the pre-opened storage-resource contract. Provider generation, lifetime, revocation, partial progress, indeterminate mutation completion, writer fencing, flush classes, publication ordering, and recovery remain to be specified before writable storage is implemented.

## Reconsider when

- a more general checked numeric-conversion family is accepted;
- the durable database format selects a page-size domain wider than `u32`;
- storage addressing requires a structured value wider than `u64`; or
- a named execution subset adopts the operation and needs target-specific lowering evidence.
