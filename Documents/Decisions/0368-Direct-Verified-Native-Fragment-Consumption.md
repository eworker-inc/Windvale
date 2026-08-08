# Decision 0368: Direct verified native-fragment consumption

- Status: Accepted current-host normal-path WVB loader/lowerer removal; Linux execution and grouped qualification pending
- Date: 2026-08-07
- Advances: [Decision 0367](0367-Versioned-Verified-Native-Fragment-Artifact.md), [Decision 0362](0362-Windvale-Owned-Segmented-Native-Enum-Metadata.md), [Decision 0365](0365-Native-Publication-Planner-Execution.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale native fragment artifact](../../Specifications/Native-Fragment-Artifact.md)

## Context

The normal native runtime still embedded three portable WVB helpers after all
fixed-output generators were removed: segmented enum-metadata construction,
executable-image layout, and publication-lifetime planning. Each WVB was
digest-checked, decoded, semantically verified, lowered, independently decoded
as x86-64, and cached before its first variable-input execution.

Decision 0367 defines a strict artifact that preserves the complete output of
that process without discarding any native verifier evidence.

## Decision

- Generate one exact `WVNF 1.0` artifact from each retained WVB's verified
  Stage 0 lowering. Keep the source, project closure, and WVB in the repository
  for exact reconstruction, differential execution, and recovery provenance.
- Embed the WVNF files, not their WVBs, in `Runtime/Windvale.Native`. The
  runtime assembly now contains no embedded WVB helper or generator.
- On first use, require the selected WVNF's exact length and SHA-256, decode its
  complete target/ABI/code/symbol/patch/type/service state, run the existing
  native fragment verifier, and require the exact `Main(bytes) -> bytes` entry
  shape before caching it.
- Preserve the bounded native execution, planner bootstrap, segmented `WVEN`
  session, response verification, instruction limits, and final service and
  publication semantics. This removes repeated bootstrap work; it does not
  weaken the execution trust boundary.
- Keep the generated artifact comparison in the affected conformance tests.
  A source or backend change must intentionally refresh both the WVB and WVNF
  identity; a stale retained fragment fails byte comparison.

## Exact retained fragments

| Consumer | WVNF bytes | SHA-256 |
| --- | ---: | --- |
| Segmented enum metadata | 115,167 | `d2f53cd0fdd7812699a06234e19586f18492ffbca68ae0e5f507b09253c5a39b` |
| Executable-image layout | 61,583 | `9deeb8c4ab8f080cbc187036e0b015932379956930ec9cd1b7f51f7d1daa1f47` |
| Publication lifetime | 46,125 | `4d87911f2f442e6a2e4dd2364138f35a0037ddc0bff0775a16e37156768777a8` |

Their recovery WVBs remain, respectively, 13,920 bytes at
`a43a89cedd7fc58740132c2f666ea69866ceff6ebb87d090124207ff3e9154ce`,
6,758 bytes at
`111608af768b18adb9be8b531214aeb14c472efef482fad507224aaa1b18909c`,
and 4,442 bytes at
`f966e7f7553def7f3d57be0d3bed67b1b010f0e2cd4907c4ef78760a140fd554`.

## Evidence and consequences

The three affected tests were reviewed before execution. Each compiles its
Windvale source closure, compares the exact repository WVB, lowers it through
Stage 0, serializes WVNF, compares every artifact byte with the embedded file,
verifies that artifact again, proves the WVB resource is absent, and then runs
the real valid and malformed behavioral contract.

The focused Release build succeeds with zero warnings and errors in 8.68
seconds. Enum metadata passes 1/1 in 4.885 seconds, publication layout passes
1/1 in 0.855 seconds, and publication lifetime passes 1/1 in 0.563 seconds.
Both qualification scripts pin all three WVNF identities and pass PowerShell
and Bash syntax checks. No Development, Standard, Qualification, or grouped
cross-host gate was run.

Normal helper/planner startup no longer invokes managed WVB decoding, semantic
verification, or x86-64 lowering. Managed WVNF parsing and fragment
verification remain a temporary loader seam; application WVB admission and
lowering, service-bundle assembly, platform W^X ownership, contexts, arenas,
and invocation remain managed. Linux execution and the final grouped gate are
still open.

## Reconsideration triggers

Replace the managed WVNF decoder only with a native loader that passes the same
malformed corpus and exact fragment comparisons. Regenerate artifacts only
from their retained source/WVB path and record changed identities. Do not
remove the WVB recovery evidence before the final digest-bound Stage 0 archive.
