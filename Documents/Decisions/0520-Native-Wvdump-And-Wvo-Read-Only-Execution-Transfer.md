# Decision 0520: Native WvDump and WVO read-only execution transfer

## Status

Implemented current-Windows evidence. Independent Linux execution and grouped
qualification remain pending.

## Context

Decision 0519 transferred construction, independent WVB verification, and
inspection of WvDump, the WVO object core, the WVA assembler, and the Wv linker.
The next broad-script lines executed WvDump and the WVO read-only shell through
the managed reference runtime. Six of those calls already had complete,
digest-bound native owners:

- WvDump's no-argument self-test;
- WvDump's report for the canonical Sum WVB;
- WvDump's deterministic bad-magic report for a non-WVB input;
- construction of the canonical WVO inspection fixture through the qualified
  native WVA assembler;
- the native WVO verification report; and
- the native WVO inspection report.

Other adjacent calls do not yet have equivalent native contracts. Managed
capability refusal is a reference-runtime authorization test. Missing and empty
resource names produce different native host-boundary status from the retained
`WVR3021` and `WVR3022` reports. The pinned WVO application currently returns 1
from its no-argument self-test even though its verify and inspect forms succeed.
Those boundaries must remain visible rather than being counted as transferred.

Decision 0519 also exposed a separate test-ownership defect. Current Stage 0
application writers evolve with the native backend, service bundle, startup,
and container builders. Their tests nevertheless required today's recovery
writer to reproduce historical pinned application digests. The repository
already gives those immutable products distinct manifest/front-door owners, so
the duplicated fixed assertions conflated two contracts and failed whenever a
later backend correction changed current reconstruction bytes.

## Decision

### Transfer six ordinary executions

Extend the paired `Verify-Seed-Native-Front-Door` helpers with these exact
current-platform operations:

| Case | Exact owner and contract |
| --- | --- |
| WvDump self-test | verify the pinned platform application identity, execute with no arguments, require exit 0 and no output |
| WvDump valid module | inspect the exact 494-byte Sum WVB with SHA-256 `76b4fa3c4c0cc37e6f1350e8191ccd78c6272224f146ef9816b5f987114c15df`, require its module/data/call/export report, and prove the input unchanged |
| WvDump invalid file | pass `Examples/Seed/Sum-Data.wv`, require exit 2 and exact `Badˉmagic sections=0 offset=0`, and prove the source unchanged |
| canonical WVO fixture | invoke the digest-bound native WVA assembler over `Hello-Object.wva`, require the two-line success report and exact 218-byte WVO with SHA-256 `992c298a4f9b68dec27b7203a2770f2a37ef2016ea45e88d33ee21994060fe85` |
| WVO verification | invoke the digest-bound read-only launcher and require exact architecture and SHA-256 lines |
| WVO inspection | invoke the digest-bound read-only launcher and require the header, architecture, two sections, `Console_write` import, and relative relocation while proving the object unchanged |

The WvDump applications remain the qualified/pinned native-front-door products:

| Host | Bytes | SHA-256 |
| --- | ---: | --- |
| Windows | 795,136 | `61512dae2941607b93da7d29dd59f973c690f0fec3ba24f772f2101c87ed5381` |
| Linux | 794,624 | `d3215e8345bf5cd9f3265b8421cf57d456ae605c5493fcc215a3e11daab44627` |

The WVO launchers continue to bind the Decision 0500 candidate-2 applications:
606,208 bytes with Windows SHA-256
`bb39e58d51e7b6c3eab2690995ee52fc958557ab03cfcbcb9b5ef0f3070157d2`
and Linux SHA-256
`bf94145cee63a4d7014bd7a31a40832017f025b7d8086a4ae3875385ba8345c1`.

The native helper publishes `Sample.wvo` into the caller-owned Seed artifact
directory. The broad scripts retain the exact digest check and the independent
Stage 0 `object-verify` and `object-inspect` oracle calls, but no longer invoke
the managed assembler or managed WVB runtime for the six transferred cases.

Changed-file routing selects the native helper for `Hello-Object.wva` in
addition to the WvDump, WVO, and WVA product sources and manifests.

### Separate product identity from recovery-writer determinism

Pinned artifact identities remain immutable under their authoritative owners:

- `Native-Front-Door/Manifest.json`, `SHA256SUMS`, and the native-front-door
  test own the qualified WvDump and WVA applications;
- `Native-Wvo-Object-Candidate/Manifest.json`, its reconstruction owner, and
  the WVO front-door test own the candidate-2 WVO applications; and
- digest-bound launchers continue to reject any artifact that differs.

Current Stage 0 WVB-inspector, WVO-inspector, and WVA-assembler writer tests no
longer duplicate those historical digests. Each current writer must instead:

1. construct both target applications twice and produce byte-identical results;
2. pass the independent target application verifier with the exact profile,
   entry, capabilities, services, and bundle bytes;
3. equal the ordinary CLI AOT output for the current host;
4. execute the current-host application over accepted and rejected inputs; and
5. prove that no CLR host or .NET runtime module/mapping was loaded.

This resolves the reported WVO/WVA “drift” without repinning an artifact,
weakening a product identity, or treating a one-host current writer as dual-host
qualification evidence.

## Evidence

- One uninterrupted current-Windows invocation of
  `Verify-Seed-Native-Front-Door.ps1` passes all 174 cases over 102 exact
  artifacts in 1,197.8 seconds.
- The repaired WVB and WVO read-only front-door tests pass 2/2 in 9.483 test
  seconds after one Release build.
- The repaired WVA assembler application test passes 1/1 in 17.965 test
  seconds without rebuilding the unchanged solution.
- PowerShell parsing, Bash syntax, and the 27-general/54-native changed-file
  routing contract pass before the complete helper run.

This removes six managed invocations from each broad host script and brings the
cumulative removal across Decisions 0505, 0506, 0508 through 0520 to 180. It
removes no direct managed entry file. The inventory remains three normal direct
files plus nine recovery files, and T2 remains `managed-normal`.

## Consequences

The paired native helper grows from 101 to 102 exact artifacts and from 168 to
174 cases. Ordinary WvDump self-test/valid/invalid execution and canonical WVO
fixture construction plus successful verification/inspection no longer load
.NET in either permanent-host broad script.

The broad scripts deliberately retain WvDump and WVO capability-refusal,
missing-resource, and empty-name behavior; the WVO managed self-test; the two
independent Stage 0 object-report oracles; and every WVA/linker execution,
publication, rejection, preservation, and differential case. Current Windows
evidence is not independent Linux execution, grouped qualification, artifact
promotion, clean or previous-seed bootstrap, or recovery deletion.

## Reconsideration triggers

Repair and qualify the pinned WVO no-argument self-test and specify native
missing/empty-resource failure behavior before transferring those retained
calls. Continue separately with WVA assembler and Wv linker self-tests,
publication, rejection, preservation, and Stage 0 differential behavior.
