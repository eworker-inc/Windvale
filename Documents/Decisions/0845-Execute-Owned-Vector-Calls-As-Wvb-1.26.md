# Decision 0845: Execute owned Vector calls as WVB 1.26

## Status

Accepted on 2026-08-24 with current-Windows development evidence. Paired Linux
execution remains pending before a cross-host conformance claim. This is a
bounded Slice 5 checkpoint, not completion of aggregate ownership, loop
ownership, semantic `using`, or the hosted resource migration.

## Context

[Decision 0844](0844-Prove-Owned-Vector-Calls-And-Forward-Joins-In-Wvir.md)
proved exact `Vector<T>` ownership through ordinary calls and forward joins in
typed WVIR, but WVB 1.25 could not distinguish a transferred parameter from an
immutable or mutable borrow. Publishing the call without that distinction
would leave caller invalidation and callee cleanup implicit.

The representation needs to remain small, deterministic, and independently
verifiable. A per-function trailer would duplicate the parameter directory and
make old readers skip a second mode table. Encoding a runtime pointer or source
slot would expose implementation details that are not part of source semantics.

## Decision

1. WVB 1.26 is the lowest minor that executes ordinary calls with exact
   `Vector<T>` parameters. The canonical writer selects it whenever any
   function has a Vector parameter; modules without such a parameter retain
   their previous lowest version and exact bytes.
2. A function parameter shape encodes its transfer mode directly: shape `23`
   is by-value Vector, shape `26` is immutable-borrowed Vector, and shape `27`
   is mutable-borrowed Vector. All three carry the same exact kind-5 nominal
   type index. Shapes `26` and `27` are invalid as results, non-parameter
   locals, fields, payloads, collection elements, or type descriptors. There
   is no mode trailer, source-slot identity, borrow handle, or owner bit.
3. A by-value call must receive unique Vector evidence produced by
   `local.take`; the caller's originating slot becomes unavailable. A borrowed
   call receives a retaining `local.load`; the caller's owner remains
   available. The emitter preserves a source owner when a load feeds a
   borrowed call and uses destructive transfer for its later by-value use.
4. The compiler-aligned verifier reconstructs the target parameter shapes at
   every call. It rejects a retained argument for a value parameter, unique
   evidence for a borrowed parameter, an unknown mode tag, a borrowed result
   or local, and every existing WVIR ownership violation.
5. The scalar runner derives one bounded internal mode byte per parameter while
   reading the function directory, then normalizes borrowed Vector parameters
   to the ordinary runtime Vector cell shape. A value parameter owns the
   transferred descriptor. A borrowed parameter receives a retained descriptor
   whose temporary retain is released by callee teardown, preserving the
   caller's owner. Normal function teardown releases owned cells in reverse
   slot order; failed execution tears down the bounded invocation domain.
6. The existing forward-control proof remains authoritative: at most 64 owned
   slots, 64 blocks, and 4,096 operations per affected function; Vector phis,
   backward control, aggregate-owned fields, and temporary escape remain
   closed.
7. WVB 1.26 contains at least one exact Vector parameter, so a writer cannot
   publish an unnecessarily high version. Every prior opcode and shape retains
   its existing encoding and minimum version.

## Consequences

- `Owned-Vector-Calls-And-Joins-Wir.wv` now emits a deterministic 1,733-byte
  WVB 1.26 module at SHA-256
  `ab79d05bb03afddbe6430adc127c8cdf084ea6499b16e3e25ebb3e477c408387`.
  The source-built verifier accepts it and the scalar runner returns `42`.
- Six byte corruptions reject version downgrade, an unknown borrowed shape,
  value/borrow mode substitution in both directions, a borrowed return, and a
  borrowed local. Three source fixtures continue to reject borrow-after-move,
  duplicate transfer, and asymmetric-join reuse before WVB publication.
- The combined focused owner passes 58 cases: seven valid modules, 37 malformed
  modules, four owned-call WVIR cases, and the executable result.
- The native verification registry remains 112 owners and advances to 5,387
  cases. Its 17,187 LF-only bytes have SHA-256
  `d482947c65e6c10dcb3b192c57d5f7bcb19fde0fe45cec71d5be92908ce3909b`.
- The compiler retains its existing 4 MiB aggregate WIR evidence ceiling. An
  obsolete unreferenced symbol-by-rank lookup was removed rather than raising
  that limit for the new lowering rule.

## Reconsideration triggers

Broaden this contract only with exact field-level provenance, loop fixed
points, and deterministic nested-resource destruction. Do not reinterpret
shapes `26` or `27` outside parameter directories, add a parallel mode trailer,
turn a borrow into a raw runtime pointer, or permit the verifier and runner to
derive different ownership from the same signature.
