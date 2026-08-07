# Decision 0338: Fixed native console-container mutations

- Date: 2026-08-06
- Status: Implemented current-host evidence; Linux execution pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md), [Decision 0307](0307-Native-Console-Application-Publication.md), [Decision 0330](0330-Manifest-Driven-Native-Retirement-Test-Suite.md), and [Decision 0334](0334-Fixed-Native-Console-Container-Hostile-Input-Corpus.md)
- Contract: [Native console-container mutation tests](../../Specifications/Windvale-Native-Console-Container-Mutation-Tests.md)

## Context

The managed version-1 Windows and Linux console tests retain exact structural
rejections around canonical `Sumˉdata` PE and ELF images. Decision 0334 moved
the two arbitrary-byte containment loops to native orchestration, but explicitly
left these valid-shaped cases for a separate focused owner.

Copying only the detailed Stage 0 codes into documentation would not prove the
Windvale verifier still rejects the exact malformed containers. Conversely,
generating mutations during each run would keep mutable host logic between the
reviewed oracle and the native boundary.

## Decision

- Derive 10 PE and 9 ELF inputs once from the exact canonical images and the
  existing truncation, XOR-one, and trailing-zero operations.
- Freeze every complete input, operation/offset, Stage 0 detailed code, length,
  and digest in one compact archive and manifest.
- Run a one-time focused managed oracle to confirm all 19 detailed codes, then
  remove that program and its build output from the repository.
- Run every permanent case through the public native console publisher. Require
  exact rejection, empty standard output, unchanged input and destination, and
  zero publication scratch.
- Add a distinct `console-container-mutations` retirement-suite lane. Do not
  merge curated structure into the arbitrary hostile-input family.
- Keep the two `MAX_APPLICATION_BYTES + 1` cases pending. They require the
  portable verifier's segmented input boundary, whereas the public publisher
  deliberately snapshots at most one 4-MiB value.

## Evidence and consequences

- The canonical bases remain 5,120 PE bytes at SHA-256
  `5947c00a81f4cf94651d42d619f3173a622448d042f4fa20e3042940d4a56c77`
  and 8,304 ELF bytes at SHA-256
  `8af8b46c290965cfc4475d882ac2d5fbdb0ffe4c493a19883a19c2683a319ec4`.
- The 4,432-byte archive at SHA-256
  `63b7d5187aa0f5407aa5a68be851c03fb0b64991c418f8c2407548f0ad6c89c9`
  contains 125,936 input bytes and a 2,626-byte manifest at SHA-256
  `35794ce75d80a06b099f705a8c0fce91295a5d627cee2a76803617f372e13669`.
- The reviewed one-time Stage 0 command passes all 19 exact detailed codes in
  14.110 seconds. Independent static review then reconstructs every archived
  input byte-for-byte from its base and recorded operation.
- The reviewed direct Windows native command passes all 19 cases in 6.235
  seconds. Every case produces the exact publisher rejection and preserves both
  files with no scratch.
- The retirement plan is now 1,461 LF-only bytes at SHA-256
  `f5ea90968f7e53bc885e6baa49635cbf430ec5f4abd329d2b92ca8ac4f6792f2`;
  it fixes 18 suites and 3,005 declared cases.

The already-passing child is not rerun through the changed coordinator. Linux
execution, segmented maximum-size rejection, hosted version-2 mutations,
hostile-size WVO, promotion, and the grouped end-of-goal gate remain deferred.
This slice changes no product implementation, container format, candidate
artifact, managed reference, or WebAssembly implementation.

## Reconsideration triggers

Revise the corpus and identities if either canonical version-1 image, structural
mutation set, detailed recovery code, portable admission behavior, publisher
report, or transaction contract changes. Add segmented and hosted-container
families as separate focused owners rather than weakening their distinct limits.
