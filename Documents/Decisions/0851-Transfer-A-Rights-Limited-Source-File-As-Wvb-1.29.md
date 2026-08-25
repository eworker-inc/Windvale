# Decision 0851: Transfer a rights-limited source file as WVB 1.29

## Status

Accepted on 2026-08-24 with focused Windows development evidence. This
completes the Language 1.0 Slice 5 hosted-resource checkpoint. Independent
Linux reproduction remains part of the next paired-host integration gate; this
decision does not claim the broad repository Qualification gate or repin the
promoted runner candidate.

## Context

Language 1.0 already proved move-only Memory budgets, allocation leases,
Vectors, Vector-containing aggregates, and deterministic `using` cleanup. All
of those resources were compiler or runtime internals. Slice 5 still needed one
real hosted input whose authority comes from a launcher, cannot be forged by
source, and is released exactly once.

Passing a native path or handle would give the guest more authority than the
example needs and would make portable behavior inherit host filesystem rules.
Adding open, arbitrary read, asynchronous I/O, or general effect enforcement
at the same time would mix the ownership checkpoint with Slice 6 and later
library work. The smallest honest boundary is therefore an immutable admitted
snapshot with one observable property.

## Decision

1. The compiler supplies the representation-hidden `Platformˉfile.Sourceˉfile`
   identity only for the exact trusted module `Platformˉfile`. Its internal
   source shape is `805306369`; source cannot declare or construct a lookalike.
2. `Sourceˉfile` is move-only. It may not appear in record fields, variant
   payloads, fixed arrays, Vector elements, ordinary locals, results, or general
   signatures. The only exported entry is exactly
   `Main(Sourceˉfile) -> i32`, and the parameter is passed by value.
3. `Platformˉfile.Sourceˉlength(borrow File) -> u64` is the only operation in
   this checkpoint. It lowers to WVIR operation `176`, has no effect clause or
   hidden provider call, and may observe only a live immutable borrow.
4. WVB 1.29 serializes the source resource as shape `34`. Shape `34` is valid
   only in the admitted `Main` parameter and its move-owned local. Opcode `D2`
   (`210`) is `source.length <u32 local-index>` and is valid only for that
   non-parameter source local. A WVB 1.29 module must contain the exact entry
   and at least one `source.length` instruction.
5. The public launcher mode is
   `wvrun --source-file <module.wvb> <snapshot-file>`. The launcher reads one
   immutable snapshot before guest execution, rejects a length above 1 MiB,
   and never transfers its host path or handle into the guest.
6. Interpreter request major `5` carries exactly the admitted snapshot bytes,
   one read right, a nonzero provider generation, and an equal resource
   generation. The envelope rejects absent or extra rights, zero generations,
   stale generation pairs, malformed lengths, or oversized snapshots before
   executing a guest instruction.
7. The runtime source cell contains only the bounded snapshot length and
   generation. `source.length` checks shape, bound, and current generation
   before returning `u64`. Move, `using`, and teardown invalidate and release
   the owner exactly once; no path, handle, byte pointer, provider object, or
   ambient filesystem capability enters the value representation.
8. A hosted WVB 1.29 source module may have an empty capability-call directory.
   Metadata normalization admits that exception only for minor 29; semantic
   verification then requires the exact source-file entry contract. This is not
   a general hosted-without-authority escape.
9. Existing 64-owned-slot, 4,096-instruction, fixed-request, diagnostic, stack,
   and arena bounds remain unchanged. The source snapshot adds one explicit
   1 MiB pre-execution bound and no unbounded guest allocation.

## Consequences

- `Source-File-Snapshot-Executable.wv` moves its input into a semantic `using`
  scope, reads the length through a borrow, and returns `42` only for a 42-byte
  snapshot.
- Two independent compilations produce identical 373-byte WVB 1.29 at SHA-256
  `01065b752d7ea6d64e3bf36bdd4d8a0d2e5b7faf6794de173580003ed3935d05`.
- Six byte-level corruptions cover version downgrade, forgeable parameter and
  local shapes, unknown opcode, observation of the parameter before transfer,
  and copying instead of moving the parameter. Runtime cases cover 42-byte and
  41-byte snapshots plus bounded rejection at 1,048,577 bytes.
- The combined focused owner passes 113 cases: 14 valid modules, 65 malformed
  modules, 12 source-file cases, the retained owned-call, aggregate, Vector,
  budget, and `using` evidence, and result `42`.
- Native compiler and verifier helpers were factored to stay below existing
  frame/record limits. These are capacity-preserving implementation changes,
  not new language semantics.
- The registry remains 112 owners and advances to 5,442 cases; its 17,742
  LF-only bytes have SHA-256
  `9c966034fedace67e7b7ab32267badf9e8ecfbf814c77fcf1fa8049bea964b22`.
- General file opening, byte reads, mutable files, asynchronous operations,
  provider acquisition, capability/effect call enforcement, and resource-
  bearing collections remain later work.

## Reconsideration triggers

Add `Openˉsnapshot`, byte-range reads, or asynchronous file operations only
after Slice 6 gives their effects and calls exact enforceable semantics. Add
more source-resource entry shapes only with explicit rights, versioned request
framing, revocation/generation behavior, size and work bounds, and malformed-
input evidence. If 1 MiB becomes a measured product blocker, version the
launcher/resource contract or add bounded streaming rather than silently
turning the snapshot into ambient unbounded memory.
