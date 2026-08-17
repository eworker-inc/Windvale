# Decision 0746: Separate Compiler Development Smoke From Reconstruction

**Status:** Accepted
**Date:** 2026-08-17

## Context

The `compiler-reconstruction` verification owner served two different needs
through one command. Qualification needs a cold reconstruction of the paired
Windows and Linux compiler and build-driver candidates. Ordinary compiler edits
need rapid evidence that the admitted current-host compiler and build driver
still execute the source-to-WVB path correctly.

The complete owner took 1,351,200 milliseconds (22 minutes 31 seconds) during
the compiler-improvement integration. Its candidate-inventory and invalid-usage
cases were fast; almost all of the elapsed time belonged to rebuilding the
paired distribution evidence. Repeating that construction in the development
loop did not add proportionate evidence for every edit.

## Decision

Keep the no-argument `compiler-reconstruction` owner as the complete cold
qualification contract. Add an explicit `--development` mode, selected only by
development-scoped changed-file verification.

Both modes retain exact candidate inventory admission and invalid constructor
usage rejection. The development mode then runs one deterministic semantic
oracle through both current-host entry points:

- the admitted candidate compiler directly compiles `Function-Only.wv`;
- the admitted candidate build driver compiles the corresponding Project 2
  manifest;
- both results must be byte-identical, exactly 816 bytes, and have SHA-256
  `28d215b982a7b7185cfa80c4cc5346666bd0181582fe80bec8b7035d514da936`;
- the result must pass the independent WVB verifier; and
- every producer, artifact, and source consumed by this mode belongs to the
  development-owner dependency registry.

Qualification-scoped changed-file verification, exact coordinator filters, and
direct no-argument execution continue to use the full reconstruction. The
development mode is execution evidence only and must never be cited as paired
compiler reconstruction, cross-host construction, or qualification evidence.

## Consequences

Ordinary compiler changes receive a small deterministic execution check instead
of reconstructing four large native containers. Release and promotion evidence
is unchanged, and a developer can still request it deliberately through the
canonical owner command.

The development oracle is intentionally narrow. A change that affects another
compiler feature still receives its mapped focused owners, while a change to
the reconstruction machinery itself remains admitted by the dependency registry
and executes this smoke before qualification. Any future expansion must keep
the development path bounded rather than gradually recreating qualification.

## Verification

The Windows development path, invalid-argument behavior, dependency closure,
and changed-file dispatch are verified locally. The paired shell implementation
is syntax-checked locally; live Linux execution remains dual-host CI evidence.
The complete no-argument reconstruction is not repeated for this dispatcher-only
change because its construction inputs and cold qualification body are unchanged.

## Reconsideration triggers

Reconsider this decision if the semantic oracle no longer covers both admitted
entry points, if the development path becomes materially slow, if compiler
construction becomes incremental enough to fit the inner loop, or if release
qualification adopts a different reproducible reconstruction contract.
