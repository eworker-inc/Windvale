# Decision 0336: Fixed native WVA differential corpus

- Date: 2026-08-06
- Status: Implemented current-host evidence; Linux execution pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md), [Decision 0220](0220-First-Native-Wva-Assembler-Front-Door.md), [Decision 0321](0321-Fixed-Native-Wva-Assembler-Rejection-Families.md), and [Decision 0330](0330-Manifest-Driven-Native-Retirement-Test-Suite.md)
- Contract: [Native WVA differential tests](../../Specifications/Windvale-Native-Wva-Differential-Tests.md)

## Context

The extended managed WVA differential test applies one through four seeded
character assignments to the same canonical 432-character source in each of
200 cases. It compares Stage 0 and Windvale acceptance, exact object bytes on
success, and absence of output on rejection. The fixed eleven-case native
matrix pins every stable diagnostic family but does not preserve this broader
seeded sequence.

A permanent replacement should retain exact reference decisions and accepted
bytes without porting framework `Random`, running two assemblers forever, or
adding 200 loose source files to the repository.

## Decision

- Run the exact managed seed, source, alphabet, assignment count, position, and
  replacement sequence once at commit `d933dec`; freeze all 200 source values
  and Stage 0 results in one digest-bound compact archive.
- Retain Stage 0 diagnostic code, line, and column for rejected cases as
  provenance. Require the permanent native test to agree on the diagnostic code
  while Decision 0321 continues to own complete representative report text.
- Require the sole accepted case to reproduce the exact Stage 0 WVO bytes and
  stable native success report, then structurally verify that output through the
  existing native WVO verifier.
- Preserve every source and a pre-existing destination for all rejected cases.
- Remove the one-time managed generator and build output. Generate nothing and
  start no .NET process during the permanent run.

## Evidence and consequences

- The 27,485-byte oracle manifest covers 200 distinct 432-byte sources, 86,400
  total source bytes, exact one-through-four assignment distribution, one
  accepted no-op assignment, 199 rejections, and seven Stage 0 diagnostic codes.
  Every source was independently reconstructed from its recorded operations and
  rechecked against its size and SHA-256 before native execution.
- The compact 17,301-byte archive avoids both a large generated source file and
  200 loose fixtures while retaining every exact input.
- The reviewed direct Windows command passes all 200 cases in 61.325 seconds.
  Every rejection agrees on the Stage 0 diagnostic code and preserves input and
  destination; the accepted case produces and natively verifies the exact
  243-byte reference WVO.
- The retirement plan is now 1,127 LF-only bytes at SHA-256
  `d4482c944e608c2a2d39359345927a954c7423b6d152c32ccda2d3578b7d07b5`;
  it fixes 14 suites and 986 cases.
- The already-passing child is not rerun through the changed manifest wrapper.
  Linux execution, remaining WVA vectors, arbitrary-source containment,
  promotion, and the grouped end-of-goal gate remain.

This slice changes no WVA semantics, WVO format, assembler implementation,
candidate artifact, managed reference source, or WebAssembly implementation.

## Reconsideration triggers

Revise the corpus version and identities if the canonical source, mutation
count/rule/alphabet, Stage 0 acceptance or diagnostic code, accepted WVO bytes,
native success report, or ordinary assembler launcher changes. Keep complete
diagnostic text in its representative family matrix instead of copying it into
all 199 rejected rows.
