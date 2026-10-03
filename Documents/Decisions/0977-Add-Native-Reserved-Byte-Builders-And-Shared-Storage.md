# Decision 0977: Add native reserved byte builders and shared storage

## Status

Accepted implementation direction under the maintainer's memory and essential
library priority, 3 October 2026. The runtime-private profile is a candidate;
source integration, cross-host qualification and release remain separate gates.

## Problem and result

The native budgeted allocator can own and release physical storage, but the
current immutable byte/text path still uses arena checkpoints and conservative
compaction. It cannot preserve a charge through arbitrary aliases or reclaim an
obsolete value below a function checkpoint. Scalar Vector support alone does
not supply the frozen byte-builder and immutable-sharing contracts.

Add one runtime-private byte-builder and shared-backing adapter over the existing
physical allocator and canonical budget accounting. A reserved builder commits
its entire capacity once. Appends either fit completely or leave it unchanged.
Freeze changes the ownership of that same backing; it does not compact or
allocate. Copies of immutable backing retain a reference, and final release
returns physical storage and credits its allocation lease.

This chooses reference counting as an implementation mechanism for this bounded
native profile. It does not amend the frozen source semantics, select a general
graph collector, or establish complete Language 1.0 memory management.

## Contract and boundaries

The exact private version-1 state, request, operations, limits and refusal rules
are owned by [native shared storage](../../Specifications/Windvale-Native-Shared-Storage.md).
The adapter owns 64 metadata slots and uses the existing generation-checked
physical handles and canonical budget/lease identities. Payload capacity is
bounded by the existing 4 MiB provider profile. These are implementation limits,
not universal language limits.

Extend the version-1 [budgeted adapter](../../Specifications/Windvale-Native-Budgeted-Storage.md)
with operation 12, read-only complete-domain validation. Existing operations,
state and request sizes retain their meanings. This additive private operation
avoids repeating full accounting validation once for every shared entry and
allows validation after the last allocation disappears. Previously pinned
artifacts retain their exact earlier accepted-operation set.

The leaf supports reserved byte construction, integer and decimal appends,
bounded mapped immutable spans, same-domain shared-byte appends, freeze, retain,
release, borrowed ranges and terminal teardown. Text encoding, rune indexing,
floating formatting, mutable byte-buffer access and broader collections require
their own implementation and coverage. No source-facing token or forgeable
record substitute is introduced.

## Integration and completion

Keep the implementation in the shared native runtime and its existing focused
storage verifier. Version any future descriptor ownership, native ABI/context,
WVIR or WVB changes before integrating source byte owners and cleanup. Context
10's reserved fields and existing byte/text descriptor meanings are unchanged
by this private leaf.

Compiler integration must balance references through copies, slices, aggregate
fields, calls, returns, replacement and every exit. A borrowed view creates no
semantic share and cannot outlive its owner. Trusted reference accounting does
not make a duplicated release of one semantic share safe while another remains.

Completion requires Windows and real Debian checks for alias survival, exact
charges, unchanged-on-refusal appends, constructor refusal cleanup, stale and
corrupt state, non-tail reuse and fixed-live-state repeated allocation/release.
Then migrate a maintained compiler serializer and working-state collection.
A passing assembly fixture alone does not close the language memory gate or
prove reduced interpreter process memory.
