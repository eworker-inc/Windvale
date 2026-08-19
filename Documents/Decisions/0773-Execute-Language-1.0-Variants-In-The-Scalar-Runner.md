# Decision 0773: Execute Language 1.0 variants in the scalar runner

- Status: Accepted
- Date: 2026-08-19

## Context

Decision 0772 carries zero-through-64-field variant cases through source,
typed WIR, canonical WVB 1.16, and the independent compiler-aligned verifier.
The source-built scalar runner still stopped at WVB 1.15, so the accepted
language feature could not yet execute through the portable reference path.

Variants need bounded immutable storage, exact nominal and case identity,
default values, collection under repeated construction, and deterministic
failure for a malformed case extraction. Adding a second variant heap or a
parallel interpreter would duplicate the record collector and make aggregate
ownership harder to audit.

## Decision

1. The source-built native WVB runner admits WVB 1.11 through 1.16. WVB 1.16
   markers and opcodes remain rejected under every earlier minor version.
2. A runtime value remains one eight-byte scalar cell. Its low `u32` is the
   first field slot, or `0xffffffff` when the selected case has no allocated
   fields. Its high `u32` is the exact owner token
   `0x80000000 + (type + 1) * 256 + case`.
3. Variant fields share the existing fixed 768-cell immutable aggregate arena
   with records. The discriminator is encoded in the owner token rather than
   consuming another arena cell, so a case allocates exactly its declared
   field count.
4. `variant.create` consumes fields in declaration order and copies them into
   one contiguous span. `variant.is_case` compares the exact owner token and
   allocates nothing. `variant.payload` and WVB 1.16 `variant.field` select the
   verifier-defined field shape and copy the corresponding cell.
5. A default variant selects case zero. Its cell uses the no-allocation sentinel
   until a field is read; that read recursively produces the exact default for
   scalar, record, enum, or admitted variant shapes.
6. Stack aggregate flags, active locals, and saved call-frame locals are roots.
   When no contiguous span is available, the bounded collector marks reachable
   record and variant allocations through their selected field directories,
   sweeps unreachable spans, releases descriptor fields, and retries once.
7. A payload or field instruction whose encoded case differs from the runtime
   value fails with `WVR3017`. Malformed directories, owners, bounds, reference
   counts, stack effects, or allocation metadata continue to fail closed.
8. Collector scans and descriptor retain/release operations live in focused
   helpers. This keeps the dispatch loop reviewable without increasing source,
   WIR, WVB, stack, arena, or instruction limits.

## Evidence

The source-built Windows runner executes the exact 918-byte WVB 1.16
multi-field fixture from Decision 0772 with result `42` in 76 guest
instructions. It executes the exact 428-byte named single-field fixture with
result `42` in 26 instructions and retains the WVB 1.11 return-42 baseline in
four instructions.

The malformed oracle changes a valid construction and its branch test to a
different in-range case while leaving field extraction on the original case.
The independent verifier rejects the module, and direct scalar execution fails
with `WVR3017` after 32 instructions. The 512-byte pressure fixture has SHA-256
`d93516d0d6679f6aa276933dc82489674fed765154f28bd1935f48c5a99c333a`;
900 one-field variant replacements force collection beyond the 768-cell arena
and still return `42` after 26,134 instructions.

The focused owner builds the runner once, reuses it for floating, unit, never,
and variant execution, and adds only the two valid variant runs, mismatch trap,
and bounded pressure run. No heavy storage workload or complete Qualification
gate is part of this checkpoint.

An isolated build confirmed that the newer reconstruction-candidate compiler
already reports `Bytecodeˉlimit` for the unchanged pre-checkpoint runner at
commit `46e5642013ec57d380de3383e8f1e853eb29f222`. The established native
front-door compiler builds and packages the refactored runner successfully.
That pre-existing candidate-capacity mismatch remains a separate compiler
checkpoint and is not hidden by widening a limit here.

## Non-decision

This does not define a public native ABI or exposed memory layout for variants,
advance the direct native selector, browser package, or Windvale OS interpreter
to WVB 1.16, add value-producing `if` or `match`, permit recursive aggregate
cycles, or complete Language 1.0.

## Consequences

Named variants now cross one complete source-to-runtime reference path without
a second heap or interpreter. Existing record allocation and reclamation remain
the single bounded aggregate mechanism. Other WVB consumers keep explicit
narrower version claims until their own focused checkpoints advance.

The next Language 1.0 Slice 2 item is value-producing control flow over the
same compiler, verifier, and scalar execution architecture.

## Reconsideration triggers

Reconsider the compact owner token only if the accepted limits exceed 8,388,607
variant types or 256 cases per type. Reconsider the shared arena only if measured
representative workloads show that record and variant pressure require distinct
resource policies without weakening deterministic bounds.
