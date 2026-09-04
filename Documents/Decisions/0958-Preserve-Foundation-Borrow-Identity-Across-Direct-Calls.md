# Decision 0958: preserve Foundation borrow identity across direct calls

- Date: 2026-09-04
- Status: Implemented as a local Windows source-publication candidate; complete
  WVB verification, execution, Linux reproduction, and cross-host qualification
  remain pending
- Extends: [the candidate WVB 1.39 representation](0957-Represent-Immutable-Foundation-Payload-Borrows-In-Candidate-Wvb-1.39.md)
- Preserves: existing ownership, explicit consumer admission, earlier WVB bytes,
  and the qualified Language 1.0 compiler boundary

## Context

Candidate WVB 1.39 already distinguished an immutable Option/Result view and
its non-owning payload. A direct helper call nevertheless described the
callee's parameter as an ordinary value while its argument retained borrowed
metadata. This mismatch had to be resolved before complete verification or
execution could be admitted.

Global call-graph inference is unnecessary: source declarations already record
the exact parameter mode and bound type. The earlier global fixed-point attempt
also exceeded the bounded compiler text arena. Development checks must exercise
the real planner without reconstructing the whole compiler after every edit.

## Decision

1. In a module containing retained WVIR operation `191`, encode ordinary
   immutable-borrow parameters with shape `37` wrapping their exact planned
   value shape. Derive this identity from declarations, independently of
   caller order. Keep existing special-purpose borrow representations.
2. Give corresponding direct-call argument temporaries the same identity,
   whether they originate from a plain owner or a previously borrowed payload.
   Preserve that identity through direct borrowed forwarding. By-value
   parameters and function results remain ordinary shapes.
3. Retain the rule that only a reachable operation `191` selects minor 39.
   Do not introduce a second format, global inference pass, ownership transfer,
   native pointer, or runtime-admission shortcut.
4. Extract the bounded per-function provenance planner into one portable
   Windvale module. Its native self-test consumes that implementation directly.
   Keep the full source/WVIR validation and declaration resolution in their
   existing owners.
5. Apply the planner's 4,096-slot/operation/temporary bounds only to functions
   requiring borrow planning. Borrow-free functions retain ordinary temporary
   allocation and limits, including functions beside a retained borrow feature.
6. Keep the existing verification owner, with a small planner development
   selection and a separate publication selection. Reuse content-keyed compiler
   products, execute behavior afresh, and expose bounded child progress.

## Evidence

The [exact publication record](../Evidence/2026-09-04-Foundation-Borrow-Cross-Call-Publication.json)
identifies the source inputs, native compiler, candidate bytes, and commands.
The Windows publication selection passes 39 cases in 15,345 ms warm: sixteen
native planner cases, twenty structural publication cases, two large
borrow-free regressions, and one unchanged earlier-bytecode fixture.

The fixture covers all three Option/Result projections, a nominal record and
`u32`, four direct/forwarding helpers, eight exact direct-call argument
identities, and a by-value parameter. It is compiled twice and compared byte
for byte. Twelve mutations reject instruction, owner, type, parameter, and call
identity corruption.

A 1,100-assignment borrow-free function compiles alone to bytes identical to
the pinned emitter and also compiles beside the borrowing fixture. It retains
50,636 code bytes and 3,303 local slots. This guards against accidentally
applying borrow-specific limits to unrelated functions.

The preceding 37-case run took 1,740,955 ms while creating missing compiler
packages under an approved one-hour cap. The completed products were retained
and reused by the expanded run. These are local development observations,
not clean-machine performance thresholds.

## Consequences

Cross-call borrowed identity now has deterministic publication evidence without
global inference. The complete WVB verifier and every execution consumer still
reject minor 39. Typed-stack and lifetime verification, bounded scalar
execution, Linux reproduction, indirect-call combinations, and wider payload
classes require their own evidence before admission.

The larger Libraries 1.0 work remains active: exclusive borrow, take, mapping,
collection and byte operations, and migration of required real consumers are
not completed by this checkpoint.
