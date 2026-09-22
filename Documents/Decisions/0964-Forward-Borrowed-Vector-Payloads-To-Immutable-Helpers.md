# Decision 0964: forward borrowed Vector payloads to immutable helpers

## Status

Implemented candidate with focused Windows/Debian verification on 2026-09-22.
The [paired-host evidence](../Evidence/2026-09-22-Borrowed-Vector-Payloads.json)
records exact source-built identities and limits. Installed promotion and full
qualification remain separate.

## Context

Immutable Option/Result borrowing already preserves an owned aggregate while
helpers observe its Copy fields. Previously a direct Vector payload was rejected:
its borrowed local metadata was not admitted, and its call argument could not match
the existing immutable Vector parameter. Ordinary Vector forwarding now works,
but eliding a projected payload's temporary would lose the original variant
owner's provenance.

The maintained package parser already supplies the original goal's real
consumer. This extension closes an owned-payload coverage gap; it does not make
a second consumer or the full Libraries 1.0 suite a prerequisite for that
delivered parser milestone.

## Decision

1. Extend candidate WVB 1.40, without a new opcode or wire layout, to admit
   shape `37` wrapping shape `23` and an exact kind-5 Vector type index only in
   compiler-generated non-parameter local/temporary metadata. Minor 1.39 keeps
   rejecting that shape. Wrapped Vector parameters remain invalid.
2. Permit that borrowed Vector cell to satisfy only an exact immutable Vector
   parameter (shape `26`) at a synchronous direct call. Do not widen ordinary
   value matching, indirect calls, by-value transfer, or exclusive borrowing.
3. Preserve the projected view and its original-owner loan through local loads,
   stores, calls, and frame cleanup. The compiler's ordinary direct-Vector load
   elision must exclude Foundation-projected views before code-size analysis.
4. Keep existing `E2 vector.parameter_length` and parameter encodings unchanged.
   Either an `E2` read or an admitted borrowed Vector local establishes the
   candidate minor-40 feature; a version-only upgrade establishes neither.
5. Preserve rejection of wrong nominal types, borrowed return/capture/take,
   mutation, ownership-taking calls, and owner movement or replacement while a
   loan is live. Run complete verification before runtime admission.
6. Retain existing input, slot, operation, descriptor, loan, call-depth, and
   allocation limits. Reuse the existing verifier and scalar runtime rather
   than adding a second execution path.
7. Classify generic aggregate ownership from its validated declared field
   evidence, not only the operands of constructors present in the program.
   `Option<Vector>.Absent` and the opposite scalar arm of a Result still have
   an owned nominal type; moving those values must not emit a copying load.

## Required evidence

Extend the existing Foundation borrow and memory-budget execution owners with
all three payload projections, absence, empty/nonempty Vectors, repeated helper
calls, owner teardown and budget reuse. Verify exact nominal identity and old
minor rejection, forbidden consuming/exclusive calls, and a minor-40 module
without `E2`. Compare deterministic bytes and execute on Windows and Debian.
Keep 128 child-budget reuse trips separate from the denser helper-loop workload
and enforce a per-scenario instruction bound. The outer native host's bounded
dynamic-value arena is separate from the guest's collection budget; do not
raise either limit to make a stress fixture pass.

The focused gate passes on Windows and Debian: nine payload programs emit
identical bytes and instruction counts, nine malformed modules reject, and
three invalid ownership sources reject before publication. The same batch
passes 366 component groups, ten runtime-admission cases, and nine existing
owned-record scenarios per host. Fresh parser/lock builds match their prior
bytecode identities and pass current verification on both hosts; their earlier
native execution evidence is reused, not reported as a fresh execution run.

Native lowering, browser admission, installed identities, exclusive Option/Result
borrowing, Take, mapping, and arbitrary payload composition remain separate.
