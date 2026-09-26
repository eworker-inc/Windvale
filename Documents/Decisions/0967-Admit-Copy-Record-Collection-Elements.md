# Decision 0967: admit Copy record collection elements

## Status

Implemented candidate with focused supplied-product checks passing on Windows
and Debian. Independent reconstruction and installed promotion remain open.
This extends the already accepted Vector and Sequence APIs for the typed
package-lock consumer. It does not complete the owned-element API or Libraries
1.0, and does not promote installed compiler or runtime identities.

## Context

Package-lock entries need typed records of offsets and lengths. Such records
are Copy values: reading one preserves its value without transferring a
resource. The collection backing already stores exact record handles and
traces their children. Source operations and complete bytecode verification
still need to agree on admission, and execution must retain record identity
when an element becomes an ordinary value.

## Decision

1. Add candidate WVB 1.42, retaining the seven-section envelope and all
   inherited instruction encodings. Canonical emission selects it for a
   reachable operation on a record-element Vector or Sequence. Complete
   verification requires record-element collection metadata; earlier minors
   retain their scalar operation contract.
2. Admit ordinary and materialized generic record elements only when bounded
   recursive classification proves Copy membership. Reuse the source value
   classifier. In complete bytecode verification, reuse the aggregate
   read-through traversal with Copy-only admission. Text, bytes, Sequences,
   owners, resource-bearing special records, and unproven callable fields
   refuse this candidate, including when nested.
3. Preserve exact record nominal identity through construction, append,
   growth, freezing, and indexing. A failed append returns the original record
   value and leaves the Vector unchanged. Growth copies live cells and keeps
   their original record handles reachable. Collect unreachable result wrappers
   before checking exclusive growth, with the active frame and all inherited
   root classes retained. Accept an empty-record sentinel only for its exact
   zero-field nominal type at the internal runtime boundary; source empty-record
   declarations retain their existing rejection.
4. Indexed Vector access still produces an immutable view under the existing
   owner-loan rules. Proven Copy read-through produces an ordinary record
   value. Sequence indexing likewise returns its Copy value. Runtime reads
   mark record results as aggregates and validate exact live handle identity
   before publication. Append validates before mutation or failure wrapping.
5. Preserve the existing collection, heap, aggregate-slot, descriptor, loan,
   and instruction limits. Recursive proof has at most 8,192 traversal steps,
   a 64-entry active type path, and bounded pending storage. Complete module
   verification precedes execution; runtime operations do not repeat the
   recursive type proof.
6. Extend the existing memory-budget split-execution owner. Keep focused
   supplied-product selection available so compiler reconstruction and earlier
   passing evidence need not be repeated after each fixture diagnosis.

## Remaining boundaries

Shared or owned record fields, top-level variant and array elements, mutable
indexing, replacement/removal, general borrowed returns, native collection
lowering, browser and OS consumers, installed promotion, and independent
Windows/Linux qualification remain separate gates. Reserved construction,
append, and growth retain Main-owned budget execution; freezing retains the
source validator's single-block restriction. The candidate's eventual
execution evidence must distinguish paired host execution from independent
compiler reconstruction.
