# Decision 0982: Complete direct budget helper authority in WVB 1.45

## Status

Accepted implementation direction within the maintainer's selected coherent
Language 1.0 ownership-to-storage work, 4 October 2026. This is a candidate
execution contract under implementation. Compiler reconstruction, Windows and
Debian execution, qualification and release remain separate evidence gates.

## Problem and result

The maintained native compiler serializer needs an exclusive borrowed budget
through ordinary helpers. The existing bytecode distinguishes an owned budget
from an immutable budget view, but cannot express that helper's mutation
authority. An owned budget helper return also needs an explicit ownership move.
Complete both paths through the existing compiler, independent verifier and
native backend without changing the frozen source allocation or release rules.

## Exact encoding and scope

Candidate WVB 1.45 extends the shared-value candidate selected by
[Decision 0980](0980-Bind-Native-Shared-Values-To-Budgets-And-Tool-Entries.md).
It retains canonical section layout, little-endian integers, opcodes, ABI 25 and
execution context 11. It adds no allocator, native cell layout or capability.

| Encoding | Authority |
| --- | --- |
| Shape 25 | An affine owned canonical Memory budget, including an owned helper return. |
| Shape 36 | An immutable borrowed budget; Split remains refused. |
| Shape 41 followed by 25, exactly two bytes | An exclusive borrowed budget parameter or compiler-generated call view, admitted only in minor 45. |
| Shape 41 followed by 39 | The earlier exclusive byte-builder view; minor 44 retains this exact meaning. |

Reject a truncated wrapper, wrong payload, private verifier-kind forgery or the
new budget wrapper under an older edition. The complete verifier derives its
private mutable and immutable budget kinds from admitted wire shapes; those
private kinds are not new serialized tags. Owned return shape 25 is admitted
in this direct-helper extension with the existing consuming Take instruction.
The source emitter selects minor 45 when a reachable function has an exclusive
budget formal or an owned budget return. Budget-bearing indirect and callable
descriptor signatures remain explicitly refused in this candidate.

## Ownership and independent admission

Canonical WVIR budget identity and parameter modes remain unchanged. Every
consuming parameter-origin use in the direct helper and reserved-byte
construction proof requires the exact source VALUE mode. A borrowed parameter
cannot become an owned local through a store, satisfy a consuming helper or
constructor, or become an owning return. Owning returns consume one live
same-block temporary and its live local origin, when present, before the
existing temporary-owner sweep. Existing CFG joins and bounds remain in force.
The same exact VALUE-mode check also guards the legacy named-slot Vector,
task and unsafe constructors in source ownership validation. Their source
signatures and opcodes are unchanged; this source proof repair does not add
their execution to the shared-value native subset or qualify those paths.

The bytecode verifier independently proves direct call authority and exact
budget roots. Same-root overlapping mutable/mutable or mutable/immutable call
arguments reject. Mutation invalidates sibling views; an incoming exclusive
parent may create a fresh view and forward it after a completed call. A
borrowed budget cannot be taken, consumed, released or returned as an owner.
Immutable shape 36 cannot authorize Split. Source admission alone is
insufficient evidence for any of these bytecode rules.

Native borrowed formals retain a non-owning marker. Passing or clearing that
alias neither releases the caller's budget nor refunds it. Owned helper
returns use the existing ownership move. Split refusal preserves both domains
and the parent token. Reserved-byte construction consumes its supplied budget;
failure after validating that budget releases it and credits its parent.
Child and final lease release also credit the original parent. These existing
accounting rules are unchanged. Terminal cleanup still closes the enclosing
resource domain.

## Bounds and verification

Retain the shared candidate's 4 MiB byte-value bound and native frame, call,
function and liveness bounds. Do not admit minor 45 through the older 1 MiB
owned-collection profile. Budget call classification uses at most 4,096
temporary-mode bytes and 4,096 operations per function, with at most 64 call
operands. At most 4,096 immutable table updates copy at most 33,558,528 bytes;
classification does not rescan the whole module for each temporary. The
independent loan proof retains its 16 MiB work bound and existing 64-owner and
64-control-block limits. Metadata normalization preserves the declared minor
and all later sections, including borrow-root evidence.

Extend the existing Foundation borrow-stack fixture for valid forwarding and
owned returns, plus malformed wrappers, authority upgrades, overlapping calls,
stale views and borrowed-owner consumption. Extend the existing shared-source
helper case through an additional forwarding helper. Require actual source
publication, complete verification and native accounting/cleanup on Windows
and real Debian, including the maintained Plan serializer consumer. These
checks belong to one causal final plan after the matched implementation is
coherent; passing private draft controls cannot replace them.

The [completion plan](../Project/Compiler-Tools-And-Libraries-Completion-Plan.md)
retains broader memory, essential-library, self-hosting and clean-install
requirements. This candidate does not establish complete Language/Libraries
1.0 delivery or migrate hosted interpreter working storage.
