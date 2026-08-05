# Decision 0226: Expanded native portable test plan

- Date: 2026-08-05
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md), [Decision 0217](0217-Windvale-Sha256-And-Native-Wvb-Runner-Profile.md), and [Decision 0218](0218-First-Native-Test-Orchestration.md)
- Contract: [Windvale native test plan](../../Specifications/Windvale-Native-Test-Plan.md)

## Context

The first `WVNT 1` candidate proves two scalar/control programs, but the pinned native runner already contains the complete bounded interpreter support needed by several existing portable fixtures. Leaving those fixture oracles only inside the C# harness would understate native test ownership and delay evidence that can be transferred without a new runtime or packaging mechanism.

The existing conformance suite independently fixes each selected WVB digest and expected result. Reusing that evidence avoids deriving expected values from the implementation being tested. A broad dynamic-plan parser, malformed-fixture bundle, or line-for-line port of the managed harness is not required for this slice.

## Decision

Extend the unqualified version-1 plan from two to five cases. Retain the existing scalar/control cases and append these single-source projects in deterministic order:

| Test | Added semantic surface | Expected WVB SHA-256 | Result |
| --- | --- | --- | ---: |
| `function-only` | Typed constants and a six-function control graph | `28d215b982a7b7185cfa80c4cc5346666bd0181582fe80bec8b7035d514da936` | 6 |
| `data-text` | Static scalar, text, and byte data plus immutable byte operations | `8ff9b57819fae8bd027a8a294f51797160821be57cb3f29c7a97ab9f2685b3cc` | 13 |
| `nominal-types` | Records, enums, nominal calls, text/byte fields, and enum naming | `b1c3543f8064732a0039d071f4e3a7da2bb901f8cfb890fb1de42193a228ff4b` | 11 |

The expected results come from the checked-in source semantics and are already asserted by the independent reference/WebAssembly tests. The exact WVB digests are their pre-existing compiler conformance identities. The native plan requires its own builder to reproduce those bytes before the native runner executes them.

The complete plan is 635 UTF-8/LF bytes with SHA-256 `79294d8e1a08325cd41042e6068b4a6bc9f3c15bd05372ad4bb7eda268a47b73`. The Windows and Linux launchers remain thin and unchanged except for that digest. No new large coordinator, parser, runtime feature, or managed implementation is added.

## Consequences

- Five portable source fixtures now build, verify, publish, and execute through the native front doors without invoking .NET.
- The native plan covers dynamic text/bytes and nominal record/enum values in addition to its scalar/control surface.
- The broad managed suite remains the independent oracle and qualification lane for malformed inputs, capabilities, resource failures, native backend modes, OS images, packaging, recovery, and untransferred fixtures.
- The candidate still awaits the grouped Windows/Linux gate; this decision does not promote the runner or declare native test retirement complete.

## Reconsideration triggers

Introduce a separately versioned bounded plan parser only when native orchestration needs malformed binary fixtures, multiple execution modes, runtime selection, structured failure expectations, or richer reports. Keep fixtures and their oracles focused even after that boundary exists.
