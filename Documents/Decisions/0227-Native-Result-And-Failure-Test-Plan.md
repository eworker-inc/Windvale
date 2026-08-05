# Decision 0227: Native result and failure test plan

- Date: 2026-08-05
- Status: Implemented candidate; grouped dual-host qualification pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md), [Decision 0218](0218-First-Native-Test-Orchestration.md), and [Decision 0226](0226-Expanded-Native-Portable-Test-Plan.md)
- Contract: [Windvale native test plan](../../Specifications/Windvale-Native-Test-Plan.md)

## Context

The five-case `WVNT 1` candidate proves successful portable execution only. Retiring the managed test lane also requires stable negative oracles: a native runtime must reject the same operation with the same Windvale status at the same defined guest boundary rather than merely returning a nonzero process code.

The existing scalar-interpreter conformance case already fixes suitable source, WVB identities, status codes, and executed guest-instruction counts. Three of those failures return a structured interpreter response and therefore exercise the runner's existing deterministic diagnostic path without adding a runtime feature.

## Decision

Supersede the unqualified version-1 plan with fixed `WVNT 2`. Every row now names an expectation kind and value:

- `result|<signed-i32>` requires exit `0`, one exact `Result: <value>` standard-output line, and empty standard error;
- `failure|<status>:<instructions>` requires exit `1`, empty standard output, and one exact `wvb run status=Failed code=<status> instructions=<instructions>` standard-error line.

Retain the five successful cases and append these existing independent oracles:

| Test | Expected WVB SHA-256 | Expected failure |
| --- | --- | --- |
| `invalid-utf8` | `6ed7d7dfe3e4443c2943c56c0a8afe6c61c5ffae275e808f1be08fac9266317c` | `3014:11` |
| `range-failure` | `e57e6183e121d3215beb854c8edb6aebd118480d0991917790fe7c569f8af794` | `3008:14` |
| `u16-failure` | `971e2296743b5dd1f3838a68c1401c18a869124f4b47c96b85ea3acbbb2e672e` | `3016:4` |

The complete 1,074-byte UTF-8/LF plan has SHA-256 `619b22496a6999b80ed25f601066c1fa07162dad52c6b6c79b9d836a1d46df62`. The thin Windows and Linux adapters implement the same closed two-kind policy. They compare complete output channels and reject an unexpected exit, extra line, cross-channel report, unknown expectation kind, or malformed failure value.

## Consequences

- Native test ownership now includes deterministic failure behavior, not only successful return values.
- The failure codes and guest-instruction counts come from existing independent reference/interpreter assertions; the native plan does not learn or rewrite them from its own output.
- The plan remains fixed and digest-bound. This slice adds no general parser, broad coordinator, C# test implementation, or new runtime operation.
- Invalid module envelopes, verifier diagnostics, capabilities, resource exhaustion, host I/O failures, backend/JIT/AOT modes, OS images, and recovery still remain in the managed suite until separate native fixtures and oracles exist.
- Version 1 had no qualification or distributed compatibility promise, so replacing it before the grouped gate does not create a retained compatibility path.

## Reconsideration triggers

Create a separately bounded Windvale plan parser when additional modes or external binary fixtures would otherwise grow duplicated host-script policy. Do not add arbitrary commands, shell fragments, regular expressions, or host-specific expected text to the plan.
