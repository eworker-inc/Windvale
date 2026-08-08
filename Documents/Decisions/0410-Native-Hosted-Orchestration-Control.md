# Decision 0410: Native hosted orchestration control

- Status: Implemented candidate; ordered host-process composition pending
- Date: 2026-08-08
- Advances: [Decision 0409](0409-Native-Fixed-Service-Acquisition.md), [Decision 0406](0406-Native-Hosted-Source-Geometry-Production.md), [Decision 0402](0402-Native-Hosted-Metadata-Request.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Native hosted orchestration control](../../Specifications/Windvale-Native-Hosted-Orchestration-Control.md)

## Context and decision

The native candidate now produces the fragment, variable enum service, nine
fixed service resources, source geometry, publication request, and metadata
request. A host orchestrator would still have needed to encode `WVMI 1` and
translate `WVSG 1` into the related `WVHS 1` hashing geometry.

Keep those formats out of PowerShell and Bash. Add one focused Windvale core
and paired hosted command with two single-output modes. Metadata mode owns the
fixed target/profile/entry record. Evidence mode admits canonical source
geometry, copies its chunk evidence exactly, and projects only the eleven
logical identity regions required by the existing streaming-hash boundary.
The later host adapter is therefore limited to process sequencing and private
resource lifecycle rather than binary construction.

## Evidence and consequences

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Orchestration-control WVB | 21,214 | `1d9f86cf636de119bde26a7b5fda5977e032db336d07c3937f0dd42df000e4bf` |
| Native WVO | 219,635 | `86ba4c10926dd95c4211859edef8604489d164f6b4a0e96e8ff8dafc9841036e` |
| Windows application | 236,032 | `eeec7c229b20ac006ed366849c91e2f03e035a9e3ee29da2e9aeb408c76b2709` |
| Linux application | 237,568 | `f7b40ac03478d54bdf8fed468fdfbe52a9449159a9fb45c05da6603935e24c67` |

The native front door and Stage 0 recovery compiler produce identical WVB
bytes. The native lowerer accepts the result without a limit change and
reproduces the exact Stage 0 WVO. After review, the single focused test passes
1/1 in 7.422 seconds after a zero-warning 9.04-second Release build. It checks
independent exact `WVMI` and `WVHS` oracles, public target routing, direct
current-host execution without CLR loading, source/WVO reconstruction, alias
and malformed rejection, and existing-output preservation. No broader local
verifier ran.

Managed orchestration no longer owns either control-file encoding in the
candidate path. Tool-package acquisition, ordered process execution, bounded
segment iteration, private cleanup, complete application comparison, Linux
execution, promotion, and the grouped gate remain.

## Reconsideration triggers

Version the command if `WVMI`, `WVSG`, `WVHS`, hosted profiles, native-code
limits, or the eleven-region bundle contract changes. Do not add child-process
authority or platform shell behavior to the portable core.
