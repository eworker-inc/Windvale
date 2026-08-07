# Decision 0340: Windvale-native hosted-console admission

- Date: 2026-08-06
- Status: Implemented current-host evidence; Linux execution pending
- Advances: [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md), [Decision 0156](0156-First-Standalone-Hosted-Console-Capability.md), [Decision 0307](0307-Native-Console-Application-Publication.md), [Decision 0330](0330-Manifest-Driven-Native-Retirement-Test-Suite.md), and [Decision 0338](0338-Fixed-Native-Console-Container-Mutations.md)
- Contract: [Native hosted-console container mutation tests](../../Specifications/Windvale-Native-Hosted-Console-Container-Mutation-Tests.md)

## Context

The public native console publisher admitted only the version-1 portable recipe.
Format-2 hosted applications ran without .NET after construction, but ordinary
admission and the exact thirteen valid-shaped mutations still depended on the
C# `Hostedˉconsoleˉapplicationˉverifier`. Removing those tests alone would have
discarded evidence; freezing the mutations without transferring SHA-256,
startup, metadata, import, and segment verification would have produced a
permanent false rejection lane.

The version-1 verifier and packager are already qualified focused components.
Replacing them with one enlarged implementation would invalidate unrelated
artifacts and obscure the distinct format boundaries.

## Decision

- Add focused portable common, Windows, and Linux format-2 verifier modules.
  Use portable `Foundation/Sha256.wv` for output/native digests and normalized
  startup identities rather than adding a host hashing capability.
- Add one small admission dispatcher shared by the verification bridge and
  atomic publisher. Select exact format-2 markers, require an empty second
  chunk for the current one-snapshot path, and otherwise retain the existing
  version-1 verifier unchanged.
- Preserve the existing 36-byte `WVCV 1` evidence and native-image write
  contract so callers do not gain a parallel result format.
- Construct the successor bridge and publisher artifacts once through Stage 0,
  pin their exact identities, and continue to label construction
  `stage0-recovery`. This advances normal admission, not final bootstrap
  independence or artifact promotion.
- Freeze both valid `Helloˉhosted` applications and the exact thirteen managed
  mutation operations. Confirm them once against the managed verifier, then
  remove the temporary oracle program and its repository build output.
- Add one permanent `.NET`-free native lane that requires exact valid
  publication, exact rejection, input/destination preservation, and zero
  transaction scratch.

## Evidence and consequences

- The successor verification bridge is 101,811 bytes at SHA-256
  `0ee99abb83b71a0e60ed6c47852f5f99b57d3dc3f5737dd0f46c604be3181861`.
- The publisher WVB is 113,525 bytes at SHA-256
  `39965e723bec6904c605c74123d5e4ef1590d1cd9af5cd52d6a94494435c8da5`.
  The Windows publisher is 1,135,616 bytes at SHA-256
  `1ffab13c1b94ec57f31fbdfbced5465bf598dfb1a237552995fece1d43c2ba37`;
  the Linux publisher is 1,135,557 bytes at SHA-256
  `fdfe5876f1217b747ec637a3a8407948f1402505ec27c91aa6a44fd3e06fcfa2`.
- The one-time differential bridge command passes 15/15 with exact target,
  status, recovered 691-byte native image, entry, and write-count agreement.
- The reviewed 3,534-byte fixed archive at SHA-256
  `a8027a9d4238767ae9b7ab18e3d0114da4e4fdf3edcbbc044d4358f2ce1fd055`
  contains two valid bases and thirteen exact rejections. Static review
  reconstructs every mutated value byte-for-byte before execution.
- The direct Windows native command passes 15/15 in 5.9 seconds. Both valid
  candidates publish exactly; all rejected candidates preserve the sentinel;
  every input remains unchanged and no publisher scratch remains.
- The retirement plan becomes 1,644 LF-only bytes at SHA-256
  `833589bcc40dfcd5017a29d10b5c8a93d7d75ed3edde7f2af3c123bccb4434b6`;
  it fixes 20 suites and 3,024 declared cases.

The already-passing child is not rerun through the changed coordinator. The
managed hosted verifier remains frozen recovery and final independent evidence.
Linux execution, segmented console maximum-size rejection, large-native hosted
construction, broader unsafe/WVA evidence, candidate promotion, Development,
Standard, Qualification, and the grouped end-of-goal gate remain deferred.

## Reconsideration triggers

Revise this decision if hosted format 2, `WVHC 1`, the canonical startup or
output leaves, publisher snapshot limit, evidence envelope, or atomic
publication transaction changes. Add segmented admission as a focused boundary;
do not silently widen one Windvale byte value or weaken the exact maximum-size
case. Final removal of the managed oracle still requires the complete
digest-bound Stage 0 recovery release and Decision 0057 gate.
