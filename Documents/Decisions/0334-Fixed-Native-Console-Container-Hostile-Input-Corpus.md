# Decision 0334: Fixed native console-container hostile-input corpus

- Date: 2026-08-06
- Status: Implemented current-host evidence; Linux execution pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md), [Decision 0307](0307-Native-Console-Application-Publication.md), [Decision 0330](0330-Manifest-Driven-Native-Retirement-Test-Suite.md), and [Decision 0332](0332-Fixed-Native-Linker-Hostile-Input-Corpus.md)
- Contract: [Native console-container hostile-input tests](../../Specifications/Windvale-Native-Console-Container-Hostile-Input-Tests.md)

## Context

The managed Windows and Linux console-application tests each create 128 bounded
arbitrary byte values through a seeded framework PRNG and require the platform
verifier to contain every value. That coverage still depends on the C# harness
even though the Windvale-native console publisher already owns portable PE/ELF
admission before publication.

Framework PRNG bytes are not a console-format semantic. A permanent native test
needs immutable inputs, exact provenance, both format routes, and observable
failure behavior without turning the managed sequence into a new dependency.

## Decision

- Replace both live framework-specific generators with two 128-case immutable
  xorshift32 families, seeded by `WVW` and `WVL`, with explicit zero and
  4,096-/9,000-byte boundaries.
- Retain all 802,246 input bytes and a 27,372-byte per-file manifest in one
  digest-bound archive instead of 256 source fragments.
- Run every `.exe` and `.elf` candidate through the public current-host native
  publisher. Require exact rejection, empty standard output, unchanged input,
  unchanged destination, and zero publication scratch.
- Add the focused `console-container-hostile` lane to the retirement manifest.
  Do not fold these containment inputs into the two-case publisher diagnostic
  lane or the three-case console-packager argument lane.
- Generate nothing and consult no managed oracle during the permanent run.

## Evidence and consequences

- Static review verifies exactly 128 inputs per target, complete unique names,
  manifest-owned sizes/digests, explicit boundaries, and 802,246 total bytes.
- The first Windows command attempt stopped before any case because batch
  `set /p` did not isolate the first line of the LF-only manifest; the reviewed
  wrapper now reads that header with `for /f`. The next pre-case attempt exposed
  LF line endings in the new Windows command itself, which repository policy
  requires to be CRLF. Neither wrapper correction changed a fixture or expected
  result.
- The final direct Windows command passes all 256 cases in 74.286 seconds. Every
  candidate is rejected by Windvale with exact input/destination preservation
  and no scratch.
- The retirement plan is now 971 LF-only bytes at SHA-256
  `436eb69af01cb74e244880ff2949d9d007cd9086b23449a68809f94197c36b94`;
  it fixes 12 suites and 530 cases.
- This removes both platform-container random-byte loops from the set of tests
  that require live managed orchestration. Curated valid-shaped mutations,
  WVO/WVA/source differential families, Linux execution, promotion, and the
  grouped end-of-goal gate remain.

The already-passing 256-case child was not rerun through the manifest wrapper;
the unchanged coordinator and reviewed exact plan reuse that result instead of
duplicating the 74-second native process loop.

## Reconsideration triggers

Revise the corpus version and identities if either raw-input bound, case count,
portable console-verifier contract, publisher rejection report, or publication
transaction changes. Add valid-shaped PE/ELF mutations in a separate focused
owner rather than weakening this raw containment contract.
