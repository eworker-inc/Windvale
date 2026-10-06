# Decision 0979: Connect reserved byte construction to WVB 1.44

## Status

Accepted implementation direction under the maintainer's coherent compiler,
runtime and consumer memory change, 3 October 2026. This is a candidate execution
contract under implementation. No passing execution, qualification or release
claim follows from this decision.

## Problem and result

The source ownership checks and runtime-private storage leaves must meet in an
ordinary executable path. Completing source analysis as a separate milestone
would leave the maintained compiler on its historical storage and require another
construction cycle before the new API becomes usable.

Extend the existing WVB emitter, complete verifier and runtime with one versioned
byte-builder family. Integrate its shared backing and lifetimes in native
execution, then migrate an actual compiler serializer and its entry/callers.
Keep focused development checks during implementation and one causal Windows
and Debian final plan after that complete path is coherent. This does not close
the remaining memory, essential-library or installed-delivery gates.

## Serialized vocabulary

WVB 1.44 appends the following shape tags, preserving earlier meanings:

| Tag | Representation and ownership |
| --- | --- |
| 39 | Representation-hidden, uniquely owned canonical byte builder. |
| 40 | Borrowed canonical Slice, followed by its ordinary element shape encoding. The initial executable byte-range family requires u8. |
| 41 | Exclusive borrowed builder parameter, followed by tag 39. It carries mutation authority and owns no allocation. |

Tag 37 retains its immutable-borrow meaning. Tag 40 is inherently borrowed,
including an ordinary Slice parameter spelling. Tag 41 must not be used to grant
mutation to an ordinary builder value or an immutable parameter. Existing type
entries encode the canonical Result, AllocationFailure, AllocationReason and
LimitFailure identities; opaque and borrowed shapes do not become user records.

The operation family occupies the free opcodes 229 through 241 (E5 through F1).
Integer immediates are unsigned little-endian u32:

| Opcode | Source/WVIR operation | Immediate fields after the opcode |
| --- | --- | --- |
| 229 | 195, ConstructReserved | Result variant, AllocationFailure record, AllocationReason enum type indices. |
| 230–235 | 196–201, append u8/u32/u64/decimal/bytes/UTF-8 | Receiver local slot, Result variant, LimitFailure variant type indices. |
| 236 | 202, Freeze | None. |
| 237 | 203, byte length | None. |
| 238 | 204, byte indexed read | None. |
| 239 | 205, BorrowRange | Underlying byte-owner local slot. |
| 240 | 206, Slice indexed read | None. |
| 241 | 207, Slice length | None. |

Constructor and append instructions are exactly 13 bytes; BorrowRange is five;
the other instructions are one. Constructor consumes budget and maximum stack
values in signature order. Append consumes its one typed value; its receiver is
the proved local slot rather than a copied owner on the stack. Freeze consumes
the builder. Byte read operations consume their ordinary evaluated byte/index
values, and BorrowRange also consumes start and length. Slice reads consume
their evaluated view/index values. Source evaluation still follows written
argument order before this encoding arranges signature-order operands.

## Independent admission and lifetime obligations

The complete WVB verifier must independently establish exact types, nominal
failure layouts, operand counts, receiver authority, ownership consumption and
borrow roots. It must reject the new vocabulary under older minors, malformed
opaque/borrowed shapes, wrong result layouts, stale aliases and escapes. Source
validation is not a substitute for this admission boundary.

Byte copies and slices preserve live shared backing. Replacing a value retains
the incoming share before releasing the previous share; consuming stores and
returns transfer temporary ownership. Locals and evaluation temporaries both
participate in last-use and exit cleanup. Charged empty values retain a backing
identity even when their logical length is zero. Borrowed ranges add no share
and cannot outlive their proved owner. Scope, failure and terminal cleanup must
release the complete resource domain without resetting live aliases.

Native and interpreter value cells are separate implementation contracts. Their
identities must preserve generation, owning ranges and admitted maxima without
fallible metadata allocation during ordinary copying. Version their descriptor,
context and entry bridges explicitly before assigning an integrated native ABI.
Guest lease release alone does not prove native interpreter working-storage reuse.

## Completion checkpoint

The maintained consumer must actually call the reserved serializer through a
budget-bearing entry and reuse the same compiler/runtime. An unused export, a
duplicate test writer or an isolated source fixture does not establish adoption.
Exercise fixed-live iteration growth, aliases, owning and borrowed ranges,
charged empty values, refusal and failure cleanup, exact serializer output and
process/domain teardown on Windows and real Debian. Preserve unaffected earlier
evidence and keep unimplemented target/API gaps explicit.

The source evidence is represented by
[Decision 0978](0978-Represent-Reserved-Byte-Construction-In-WVIR-1.39-And-1.40.md).
The [completion plan](../Project/Compiler-Tools-And-Libraries-Completion-Plan.md)
retains the full memory and product exit requirements.
