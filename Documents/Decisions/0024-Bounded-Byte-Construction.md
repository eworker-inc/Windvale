# Decision 0024: Bounded immutable byte construction

- Date: 2026-07-30
- Status: Accepted and implemented; cross-host qualification pending

## Context

The qualified Windvale assembler constructed repeated test bytes one element at a time. The linker separately constructed zero-filled alignment/BSS ranges and owned two prefix/replacement/suffix implementations for relocation and adversarial-test patching. A Windvale-written bytecode encoder will need both efficient bounded filling and deterministic branch/field backpatching.

The runtime already provides immutable balanced byte values, slices, concatenation, and fixed-width encoders. Adding a mutable buffer, heap ownership model, general collection, or privileged host builder would introduce new semantics when a small portable source contract can express the required operations.

## Decision

Create portable module `Foundationˉbyteˉconstruction` with one nominal result and two exported functions:

```text
record Foundationˉbytesˉresult {
    Valid: bool;
    Value: bytes;
}

Foundationˉbytesˉrepeat(Value: u8, Count: u32) -> Foundationˉbytesˉresult

Foundationˉbytesˉreplace(
    Input: bytes,
    Offset: u32,
    Removedˉlength: u32,
    Replacement: bytes
) -> Foundationˉbytesˉresult
```

`repeat` accepts counts through the 4 MiB byte-value limit. It starts from one encoded byte and doubles the immutable value until the next doubling would exceed the requested count, then appends one shared prefix to reach the exact length. It therefore performs logarithmic concatenation steps rather than one step per output byte.

`replace` validates the removed span with subtraction-based range checks, measures the final length before construction, and rejects a result beyond 4 MiB. On success it returns `prefix + replacement + suffix`; insertion, deletion, fixed-width replacement, and an empty operation are all defined by the same contract.

Every invalid request returns `{ Valid: false, Value: empty }` before a slice or concatenation can trap. Neither function mutates its input, exposes the persistent-tree representation, invokes a capability, or changes the intrinsic byte-value limit.

The assembler uses `repeat` for its long-line boundary fixture. The linker uses `repeat` for alignment/BSS bytes and `replace` for relocation and independent-verifier patching. Consumers retain their domain measurements and treat an impossible invalid result after successful validation as an internal contract failure; the Foundation API remains total for arbitrary callers.

## Consequences

Repeated construction and immutable patching have one portable owner for two qualified tools and the future WVB encoder. The assembler removes a linear construction loop, while the linker no longer owns a page-filling algorithm or duplicate patch implementations.

This is not a mutable byte builder, general list, stream, cursor, allocator, or transaction. A future encoder may still justify a packed collection or measured builder contract; it should build on this evidence rather than assuming mutable storage.

## Verification gate

The standalone module and composed demo must cover empty, small, unsigned-byte, exact 4 MiB, and oversized repetition; middle replacement with a length change; insertion at the end; invalid offsets/ranges; and final-size overflow. Both tool consumers must preserve their canonical object, image, map, reports, deterministic no-write behavior, and fixed maximum-case instruction ceilings.

Qualification requires the exact committed archive to pass all 39 tests and the complete CLI verifier on Windows and Debian, equal normalized reports, and direct byte equality for the module, demo, assembler, linker, canonical object, linked image, and map.
