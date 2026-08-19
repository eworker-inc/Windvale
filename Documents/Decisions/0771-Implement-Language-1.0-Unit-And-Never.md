# Decision 0771: Implement Language 1.0 unit and never

- Status: Accepted
- Date: 2026-08-19

## Context

Decision 0767 freezes `unit` as an ordinary Copy type with exactly one value,
`()`, and freezes `never` as the uninhabited result of computation that cannot
return normally. The first migration checkpoint temporarily lowered only unit
returns through Seed's storage-free `void` representation. That shortcut could
not represent unit parameters, locals, fields, or calls as ordinary typed values
and could not prove never-returning control across compiler, verifier, and
runtime boundaries.

Equating `unit` with `void` would erase a source-level value. Giving `never` a
dummy runtime value would make an impossible continuation appear executable.
Both choices would contradict the frozen language semantics and weaken
independent bytecode verification.

## Decision

Implement the unit/never vertical checkpoint as follows.

1. Internal WVLB/WVIR shape `9` means `unit`; shape `10` means `never`.
   Unit is an ordinary value shape. Never is admitted only as a function result
   and never names a parameter, local, field, payload, collection element, or
   temporary.
2. WVIR operation `Unitˉconstant = 163` produces shape `9` with no operands.
   `()`, a bare return from a unit function, and implicit unit fallthrough all
   produce this operation and return its temporary.
3. A unit value may be passed, stored, assigned, returned, and placed in a
   record. Its logical identity is observable through typing, not through a
   payload; the current scalar runtime uses one canonical zero cell.
4. A call whose result type is `never` emits the ordinary call without a result
   temporary and closes the current WIR block with a self-loop. Logical shape
   `10` propagates through the enclosing expression, satisfies any expected
   result position without conversion, and terminates that source path.
5. A `never` function must prove that every reachable path is non-returning.
   Fallthrough and return instructions are invalid. `while true` is
   non-fallthrough unless an admitted `break` path reaches its after-block.
6. WVB 1.15 adds primitive tags `20` and `21` for unit and never and the
   one-byte `C3 unit.const` opcode. Any unit or never evidence selects minor
   version 15; unaffected modules retain their lowest earlier version and byte
   identity.
7. Calls returning unit push one value. Calls returning never push none. The
   verifier rejects never parameters, locals, fields, payloads, stack values,
   and return instructions before execution.
8. Seed `void` remains return-only shape zero. Descriptorless Seed does not gain
   the unit literal or either edition-1 type spelling.

## Evidence

`Tests/Fixtures/Language-1.0/Unit-Control.wv` covers explicit and implicit unit
returns, parameters, locals, assignment, calls, and record storage. It compiles
twice to the same 731-byte WVB 1.15 module with SHA-256
`f047706f0b4915e59120b54eef5746efe22eae9c2c658860082fe131fa85ad3c`
and executes with result `42`.

`Tests/Fixtures/Language-1.0/Never-Control.wv` covers a non-returning function,
never propagation into `i32`, Boolean, and loop-condition positions,
short-circuit continuation, unreachable continuation, and an unconditional loop
with a reachable break in another function. It compiles twice to the same
853-byte WVB 1.15 module with SHA-256
`955be78835ecec4bcd4be3b563932d5a933422c6ce1cbdd74ee928d4f9bf9a04`
and executes with result `42`.

Four negative source fixtures reject never fallthrough, return, parameter, and
unreachable-statement violations. Eleven byte-level mutations reject version,
shape, opcode, type, forbidden-position, and return corruptions. This is focused
Windows development evidence; paired-host CI owns the cross-host conformance
claim, and no unrelated storage or full qualification gate is claimed.

## Non-decision

This checkpoint does not add equality or conversion for unit, a literal or
value representation for never, general bottom-type subtyping, native ABI
lowering, value-producing `if` or `match`, aggregate destructuring, localized
token execution, or complete Language 1.0.

## Consequences

Unit and never are no longer aliases or front-end reservations. Their exact
semantics now cross one compiler architecture, versioned canonical bytecode,
the compiler-aligned verifier, and the scalar execution oracle. Consumers that
have not implemented WVB 1.15 retain an explicit narrower boundary.

The next Slice 2 checkpoint can build value-producing control and aggregate
destructuring on explicit normal-versus-nonreturning path evidence instead of a
void-shaped compatibility shortcut.

## Reconsideration triggers

Reconsider the physical unit cell only if a native ABI can erase it while
preserving identical type, call, return, aggregate, and verification semantics.
Reconsider never lowering if a future WIR gains an explicit unreachable
terminator that removes the self-loop without weakening canonical reachability
or bytecode validation.
