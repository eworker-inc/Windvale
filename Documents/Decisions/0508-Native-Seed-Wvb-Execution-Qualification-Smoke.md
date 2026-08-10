# Decision 0508: Native Seed WVB execution qualification smoke

- Status: Current-Windows focused evidence complete; independent Linux and broad qualification execution pending
- Date: 2026-08-10
- Scope: exact capability-free WVB execution inside the broad Seed qualification commands
- Extends: Decisions 0213, 0457, 0458, 0505, 0506, and 0507

## Context

Decisions 0505 and 0506 moved representative project construction, WVB
verification and inspection, and one complete capability-free console AOT path
inside both broad Seed qualification commands to native front doors. Each script
still executed three already-built capability-free WVBs through the managed
interpreter solely to require their scalar results.

Decision 0507 reconstructed the exact current WVB-runner candidate and made the
digest-bound native launcher consume it. The three plain managed runs therefore
duplicated a bounded execution contract now owned by that native product.

## Decision

The paired `Verify-Seed-Native-Front-Door.ps1` and `.sh` helpers grow from five
to eight fixed cases. After constructing their existing exact products, they
execute these three modules through `Run-Wvb`:

| Product | Bytes | SHA-256 | Exact result |
| --- | ---: | --- | ---: |
| `Sum-Data.wvb` | 494 | `76b4fa3c4c0cc37e6f1350e8191ccd78c6272224f146ef9816b5f987114c15df` | 29 |
| `Read-Wvb-Header.wvb` | 1,701 | `c13efd14485afa1bf7fa418b54cea2fdd234fe34fdc824ae52346ce062be7793` | 1 |
| `Module-Composition-Demo-Project.wvb` | 660 | `030ce3f627e7bdeb8ff8a3432f01e94920c93551fd58d982bdafe9f9a5d24607` | 42 |

Each execution must emit exactly one `Result: <value>` line and leave the input
module byte-for-byte unchanged. The broad Seed scripts require the new
eight-case completion report and remove only their three equivalent plain
managed `run` calls. Managed step and per-function reporting, capability
authorization, hosted behavior, the test harness, and later qualification
phases remain in place.

This removes three more managed invocations from each host script, fourteen
cumulatively with Decisions 0505 and 0506. It changes invocation ownership,
not the direct-entry inventory: both broad scripts remain managed-normal.

## Evidence boundary

The current Windows helper passed all eight cases in about four seconds. It
reproduced the four existing exact products, executed the three modules to
results 29, 1, and 42, preserved their bytes, and retained the malformed-project
rejection owned by Decision 0505.

The Linux helper is structurally paired but was not executed because a Bash
host is unavailable in this environment. The broad Qualification command was
not rerun because it still contains managed work outside this transferred
boundary and would not be narrower evidence.

## Consequences

- E1 now owns the three representative plain-execution checks through the
  current native WVB-runner candidate.
- T2 remains `managed-normal`; the direct inventory remains three normal plus
  nine recovery files.
- The 43-suite/3,204-case fixed native retirement plan does not change.
- Native profiling/reporting, capability-bearing execution, broader harness
  transfer, independent Linux execution, GitHub orchestration cutover, grouped
  qualification, promotion, and recovery retirement remain open.

## Reconsider when

Reconsider this decision if any of the three source/project closures, exact WVB
identities, scalar results, runner output contract, input-preservation rule, or
qualification orchestration changes.
