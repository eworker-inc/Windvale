# Decision 0457: Normal-path .NET audit

- Status: Implemented audit; verification cutover pending
- Date: 2026-08-09
- Advances: [Decision 0456](0456-Native-Probe-40-Process-Object.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)

## Context

After the last frozen Probe object moved to a native producer, continuing to
port managed implementation item by item would risk spending effort on code
that is no longer in a product path. The repository needed one complete audit
of executable entry points and their call chains before the final retirement
gate.

The existing machine-readable inventory counted direct managed invocations but
did not distinguish normal from recovery use. That made nine intentionally
retained Stage 0 entry points look like ordinary blockers and did not expose
the smaller remaining cutover clearly.

## Decision

- Classify every direct managed entry point as `normal` or `recovery` and make
  the inventory verifier enforce that recovery entries own only the recovery
  lane while normal entries cannot claim it.
- Record four normal direct files: the main GitHub verification workflow, the
  paired Seed verifiers, and the legacy WebAssembly verifier.
- Record nine direct recovery files under `Tools/Recovery`. They remain required
  provenance and do not block a .NET-free normal path.
- Treat `Verify-Changed.ps1` as one indirect normal dependency because its
  qualification path calls the managed Windows Seed verifier.
- Do not port the large managed Seed harness line for line. Freeze it as an
  independent recovery oracle after native evidence covers the required
  contracts.
- Close only the remaining native verification gaps, cut local changed-file
  verification to native commands, then replace the two GitHub qualification
  jobs with one exact dual-host native gate. Remove `setup-dotnet` only in that
  final cutover.
- Reconcile the separately progressing WebAssembly work immediately before the
  broad gate rather than duplicating it here.

## Audit result

The following ordinary product boundaries contain no direct or indirect .NET
invocation in their public path: source build, WVB/WVO verification and
inspection, bounded execution, WVA assembly, WVB-to-WVO lowering, linking,
console/hosted/UEFI packaging, native publication, compiler convergence,
normal Probe 40 construction, supplied-image boot execution, website
verification, and homepage deployment.

The remaining normal dependency is verification orchestration:

| Entry | Role | Retirement action |
| --- | --- | --- |
| `Tools/Verify/Verify-Changed.ps1` | Indirect local development gate | Select native focused suites by changed boundary. |
| `Tools/Verify/Verify-Seed.ps1` and `.sh` | Direct managed broad gate | Retain as recovery oracle after native cutover. |
| `.github/workflows/verify.yml` | Direct dual-host qualification/release gate | Run the final native matrix and remove SDK installation. |
| `Tools/Verify/Verify-WebAssembly.ps1` | Direct legacy WebAssembly gate | Reconcile the separate native WebAssembly result before cutover. |

This is the shortcut: the remaining work is a bounded evidence and orchestration
cutover, not a second implementation of every managed test.

## Evidence and consequences

Repository-wide executable-entry searches, workflow/package inspection, and
call-chain review find 13 direct managed entry points: four normal and nine
recovery. The inventory verifier independently discovers the same set and now
fails on missing or inconsistent mode classification.

No product source, compiler semantic, byte format, native artifact, or test
expectation changes in this decision. The next implementation slice is the
native changed-file verification front door. Broad Seed, OS, WebAssembly,
QEMU, Standard, Qualification, and complete retirement gates remain deferred
to the final committed-source-state gate.

## Reconsideration triggers

Re-run this audit whenever a direct managed invocation appears outside the
recorded files, a recovery command enters a normal call chain, or the separate
WebAssembly work changes its ownership boundary. Do not reclassify recovery as
normal merely because it remains buildable.
