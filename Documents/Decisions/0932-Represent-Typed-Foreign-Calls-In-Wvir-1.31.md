# Decision 0932: represent typed Foreign calls in WVIR 1.31

## Status

Accepted and implemented locally on Windows on 2026-09-03. The source Analyzer
and independent WVIR validator implement this typed boundary. Authenticated WVFB
pairing, WVB encoding, runtime/provider containment, native ABI invocation,
Linux reproduction, and Slice 8 qualification remain pending.

## Context

[Decision 0925](0925-Publish-And-Retain-Authenticated-Foreign-Lowering-Carrier.md)
made the production coordinator retain an independently validated `WVFB 1.0`
carrier for authenticated Foreign declarations. The ordinary typed Analyzer
could resolve a Foreign callable, but it still encoded the invocation as an
ordinary call and had no versioned operation that a later authenticated lowering
phase could recognize safely.

The first typed operation must preserve the language-level parameter and return
contract without pretending that WVIR alone authenticates the native symbol,
ABI spelling, or no-retain/no-unwind promises.

## Decision

1. Assign WVIR operation `190` to `Callˉforeign`.
2. Emit it only for a resolved Foreign declaration invoked inside the existing
   lexical unsafe context. `Target` is the declaration's canonical kind-`9`
   WVSD entry and `Auxiliary` is zero.
3. In this checkpoint, require exactly three by-value operands in declaration
   order: canonical
   `Foundationˉunsafe.Foreignˉpointer<u8, Abi>`, exact `u64` capacity, and exact
   `u64` expected generation. Require an exact `i64` result.
4. Publish WVIR 1.31 when the operation has no specialization catalog and WVIR
   1.32 when one is retained. Preserve the function-type header even when its
   catalog is empty so the boundary remains unambiguous.
5. Independently reconstruct and validate the Foreign declaration signature,
   target kind and range, pointer nominal and generic arguments, shared ABI enum,
   scalar parameters, result, modes, arity, and reserved field.
6. Treat WVIR as typed but unauthenticated. Production lowering must pair each
   operation with the exact WVFB record retained for the same source module and
   declaration before resolving a native symbol or granting call authority.
7. Keep WVB closed in this checkpoint. After standalone WVIR validation, the
   source-WVB backend returns exact `Unsupportedˉoperation`; operation `190` has
   no bytecode encoding yet.
8. Teach source-symbol generic discovery to step over a Foreign declaration's
   ABI string before locating its `fn` token. This repairs the real declaration
   path exercised by the typed fixture without adding a compatibility branch.

## Verification

The existing `language-1-authenticated-foreign-binding` owner reconstructs the
current Analyzer and emitter once, builds four logical fixtures into two
packaged applications, and executes 26 isolated selectors. Selector 25 compiles
a real typed Foreign call and checks operation `190`, WVIR 1.31, its result,
three operands, canonical target, and zero auxiliary field. Selector 26 uses an
independently built source/type catalog and rejects altered result, arity,
target kind or range, reserved field, parameter index, pointer type, and return
type. The focused owner passed in 470.874 seconds on the Windows development
host.

## Consequences

Slice 8 now has a typed compiler operation that distinguishes Foreign calls from
ordinary calls and fails closed at WVB emission. The next checkpoint must pair
that operation with retained authenticated WVFB facts, then assign the WVB
encoding and verifier/runtime/native contracts. This decision does not execute a
Foreign function, form a host address, load a library, grant a capability, or
complete Language 1.0.

## Reconsideration triggers

Revisit the operation before WVB publication if the first real migrated boundary
requires a different bounded signature, if exact WVFB correlation cannot be
proved without changing WVIR identity, or if native ABI lowering needs typed
facts that this operation cannot retain without ambient state.
