# Decision 0509: Native WVB-runner source reconstruction and step reporting

- Status: Current-Windows focused evidence complete; independent Linux and grouped qualification pending
- Date: 2026-08-10
- Scope: source-built native WVB runner and exact overall instruction reporting

## Context

[Decision 0507](0507-Native-Wvb-Runner-Reconstruction.md) established exact
native lower/link and paired profile-5 construction from a retained runner WVB.
The Project 1 manifest omitted imported modules, however, so that evidence did
not reconstruct the product from current source. The broad Seed verification
scripts also retained one managed `--report-steps` invocation after Decisions
0505, 0506, and 0508 transferred the representative native build, inspection,
AOT, and plain-execution cases.

Project 1 resolves paths relative to each manifest. Root-level manifests are
not required for every component; component-local manifests remain the normal
organization direction, with any future workspace/index design kept separate
from the serialized Project 1 contract.

## Decision

Windvale now:

1. records the complete WVB-runner source closure in canonical module order;
2. builds the runner WVB through the digest-bound native Project front door;
3. lowers and links that exact WVB once and constructs both profile-5 target
   applications through the retained native hosted-verifier toolsets;
4. accepts one exact optional `--report-steps` runner argument while preserving
   the default result report;
5. makes the paired Seed front-door helper own exact overall instruction
   reporting for the Sum fixture; and
6. removes the corresponding managed reporting invocation from each broad Seed
   verification script.

The exact current products are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| WVB | 121,593 | `5042a57e3281621ee126a64cadef70834800524de60ed0521cedba043bd271f1` |
| WVO | 1,078,577 | `118cdd634026d7d616f3b7c7dc951176985e725f5852b4d3b045aab4cf5e5ca5` |
| linked fragment | 1,077,675 | `cb9b08b1d88cc67fa26f210832cbdc542df51d2eb8816ab5ef2a7fc296f426ec` |
| Windows application | 1,094,656 | `ab0c2384ecdfd07bc7351562732ae4b1f97e07dcbd2c92e96dc8cb3dee4d3ff7` |
| Linux application | 1,093,632 | `ffc0ad10e0e1dcffc8344bb040885535f5ab67a50cbebb1980c980888c1b5322` |

The Sum fixture reports result `29` and exactly `203` instructions. The native
front-door helper grows from eight to nine cases. This removes one additional
managed invocation per host script, fifteen cumulatively with Decisions 0505,
0506, and 0508. The machine-readable retirement inventory remains three normal
and nine recovery direct managed files, and the fixed native plan remains 43
suites and 3,204 cases.

## Evidence

- The exact paired constructor completed from source in 51.9 seconds.
- `Test-Wvb-Runner-Reconstruction.cmd` passed 3/3 in 50.1 seconds: inventory,
  source-built paired reconstruction, and current-host reporting/rejection.
- `Verify-Seed-Native-Front-Door.ps1` passed all nine cases in 3.6 seconds.
- The focused Stage 0/native differential test passed 1/1; its test body took
  33.535 seconds and the focused command completed in 57.1 seconds.

The feature-frozen Stage 0 compiler emits a distinct 126,271-byte recovery WVB
with SHA-256
`00b87804c047b626b00c167bf99ea9834bc77ab8e88e454d39a738b2787e2bcf`.
The current native semantic verifier rejects that recovery product while
accepting the 121,593-byte compiler-aligned native product. The C# path remains
valuable independent recovery/differential evidence; it is not used to define
the current source product.

Linux scripts were reviewed but not executed because Bash/WSL was unavailable
on the evidence host. The complete broad Seed command was not rerun because the
narrow helper, reconstruction owner, and focused differential test directly
own the changed behavior.

## Consequences

- E1 now has a current source-to-WVB-to-paired-application reconstruction path.
- T2 removes fifteen managed invocations per broad host script cumulatively,
  but remains `managed-normal`.
- Per-function reporting, capability-bearing execution, the broad managed
  harness, later qualification phases, and repository automation remain open.
- This does not prove independent Linux execution, a clean or previous-release
  bootstrap, grouped qualification, promotion, or Stage 0 recovery deletion.

## Reconsideration

Reconsider this decision if the source closure, WVB/WVO/fragment identities,
profile-5 service contract, reporting text, or instruction accounting changes;
if Stage 0 is intentionally advanced to the current compiler semantics; or
when paired Linux and grouped retirement qualification can promote this path.
