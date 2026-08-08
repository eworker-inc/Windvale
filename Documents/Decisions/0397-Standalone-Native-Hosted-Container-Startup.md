# Decision 0397: Standalone native hosted-container startup

- Status: Implemented candidate; advanced by [Decision 0398](0398-Standalone-Native-Hosted-Container-Runtime.md)
- Date: 2026-08-08
- Advances: [Decision 0396](0396-Standalone-Native-Hosted-Container-Platform-Bytes.md), [Decision 0384](0384-Windvale-Owned-Hosted-Startup-Instantiation.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Native hosted-container startup producer](../../Specifications/Windvale-Native-Hosted-Container-Startup.md)

## Context

Windvale already owned the hosted-startup WVO validator and relative-i32
instantiator. Decision 0385 also moved the exact target list into the
Windvale-owned container plan. Normal construction nevertheless still used C#
to select the canonical WVO, project the plan into `WVSI 1`, dispatch the
retained fragment, and verify `WVSD 1`.

A process that accepts only a prebuilt `WVSI` request would preserve that
managed projection seam. Embedding another startup template would recreate the
code-twice problem this retirement work is removing.

## Decision

Add `Native-Hosted-Container-Startup-Tool.wv` as a focused hosted shell over
the existing portable instantiation core. It consumes the successful `WVCD 1`
plan and canonical target-specific startup WVO, verifies the WVO's exact size
and SHA-256, constructs the complete `WVSI 1` request from the plan target
table, admits the `WVSD 1` response, and writes it without loading .NET.

Expose exact Windows/Linux targets through `windvale compile` and
`windvale aot`. Reuse the deletion-bound hosted-tool package builder and the
existing compiler-authority host envelope. Keep the new process source focused:
the WVA/WVO owns machine code, the existing portable core owns object
validation and relocation, and the shell owns only resource identity, request
projection, response admission, and reporting.

## Exact local evidence

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Startup-producer WVB | 42,508 | `ae1401613548724f35f40699249963cf7e0d04cbffbb8b4a0459a7e0d493003e` |
| Windows startup producer | 373,248 | `d4ba697dc124d79ed25dbb60ba17bdf84cb7a2c6650296901b99b1cb67d02929` |
| Linux startup producer | 372,736 | `43eb89b3d30cd0760a492ea6431dc736807ef322158beda20501d7f1180adc48` |

The reviewed startup-producer test passes 1/1 in 7.085 test seconds after an
8.49-second zero-warning build. It pins both packages, exercises the public CLI
target, matches a real retained-fragment response exactly, observes no CLR
load, rejects a changed canonical WVO while preserving the output, rejects an
alias, and rebuilds the WVB through the native front door. No broader verifier
was run.

## Consequences

- The native plan now feeds exact startup construction without managed target
  projection or a second startup template.
- Canonical WVO identity is checked inside the native process before the
  existing structural and relocation validator runs.
- The new source remains 179 lines and reuses focused SHA-256 and startup-core
  modules rather than growing one large source file.
- Decisions 0398 through 0402 now supply the raw runtime header, metadata,
  service-bundle segments, and metadata request. Ordered resource requests,
  segment-request orchestration, complete pipeline composition, Linux
  execution, promotion, and the grouped gate remain.

## Reconsideration triggers

Version the command if `WVCD`, `WVSI`, `WVSD`, the canonical startup WVOs,
symbol/relocation counts, or target numbering changes. Keep assembly and object
construction separate unless one authoritative source can still be preserved.
