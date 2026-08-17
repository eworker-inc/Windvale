# Decision 0745: Compact WVIR ownership records

- Date: 2026-08-17
- Status: Implemented with local Windows compiler reconstruction evidence; independent Linux qualification pending
- Advances: [typed source IR](../../Specifications/Compiler-Source-Wir.md)
- Contracts: [compiler architecture](../Architecture/Language-Design.md) and [source-to-WVB backend](../../Specifications/Compiler-Source-Wvb.md)

## Context

WVIR 1.0 repeated module and function ownership in every block and operation
record even though each function already owns exact, canonical block and operation
ranges. The independent validator checked both copies against those ranges. That
made the repeated fields validation work rather than independent information and
inflated every compiler intermediate.

The representative typed-WVIR fixture has eight function entries, eleven blocks,
44 operations, 36 temporaries, and 29 operands. Its directory was 3,200 bytes;
440 bytes, or 13.75 percent, were removable ownership duplication.

## Decision

- Advance the WVIR minor version from 1.0 to 1.1.
- Reduce a block record from 36 to 28 bytes by removing its module and function
  fields. Derive both from the enclosing canonical function block range.
- Reduce an operation record from 48 to 40 bytes by removing its module and
  function fields. Derive both from the canonical function operation range and
  retain the function-local block ID in the operation.
- Keep the 48-byte function, four-byte temporary, and four-byte operand records
  unchanged.
- Reject WVIR 1.0 at this early-development boundary. No legacy decoder or
  parallel format path is retained.
- Preserve deterministic construction, exact range validation, source spans,
  operation numbering, and source-to-WVB behavior.

## Evidence

The representative fixture retains the same eight functions, eleven blocks,
44 operations, 36 temporaries, and 29 operands while its exact directory falls
from 3,200 to 2,760 bytes. Block storage falls 22.2 percent and operation storage
falls 16.7 percent. A reconstructed compiler reaches its fixed point with the
compact directory, and the corruption demo checks the new sizes and offsets.

These figures describe serialized compiler-intermediate size, not an asserted
end-to-end speedup. Qualification must still measure construction and validation
on both permanent hosts before making a cross-host performance claim.

## Consequences

Every WVIR producer, independent validator, backend reader, fixture, and exact
artifact identity moves together to version 1.1. The format has one ownership
source instead of three agreeing copies, reducing bytes and consistency checks
without weakening the canonical range proof.

## Reconsideration triggers

Reconsider explicit per-record ownership only if a future format permits
non-contiguous ownership or independently addressable record fragments. Such a
change requires a new version and evidence that the additional identity is not
derivable from a canonical enclosing range.
