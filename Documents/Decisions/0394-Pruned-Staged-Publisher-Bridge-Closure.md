# Decision 0394: Pruned staged-publisher bridge closure

- Status: Implemented candidate; advanced by [Decision 0395](0395-Standalone-Native-Hosted-Container-Planner.md)
- Date: 2026-08-08
- Advances: [Decision 0393](0393-Paired-Native-Hosted-Container-Publishers.md), [Decision 0346](0346-Bounded-Native-Publisher-Self-Lowering.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Native x64 lowering](../../Specifications/Windvale-Native-X64-Lowering.md)

## Context

Decision 0393 moved the durable WVO and hosted-container transactions to one
shared WVA publication-state object. The staged-WVO admission source still
imported the portable publication transaction and its native bridge solely to
define two private functions that no package called. The project therefore
compiled two unnecessary source modules and retained dormant duplicate bridge
code in every staged-publisher module and native image.

## Decision

Remove the two private functions and the unused native-bridge import from
`Native-X64-Lowering-Staging-Admission-Tool.wv`. Remove the transaction and
bridge modules from its project and focused test source closures. The resulting
26-module project owns admission only; publication-state execution remains in
the shared 433-byte WVA object, while the portable transaction module remains
the independently tested semantic and recovery oracle.

Do not delete the portable publication source: other publisher families still
consume it and Decision 0057 requires a final recovery archive before retired
source deletion.

## Exact local evidence

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Staged-publisher admission WVB | 431,568 | `9ca9c1225eb5b9b9e95021b7ef897faf97e14121c5a94d72d9489b95b4d0e4c2` |
| Stage 0 publisher WVO | 6,355,569 | `727e7da06f11340dcee4552f119de3422dee17968c49438906242bbf1166e7e5` |
| Windows staged-WVO publisher | 6,364,672 | `5d9d2d8e899732b2821b6a07b98dde99532dce40d34f2e10eeb53104f3081635` |
| Linux staged-WVO publisher | 6,361,965 | `f2166008e744b856f9df18949230b47e0fceba3cdec65dfb6784be38edd5577b` |

The native Project 1 front door reconstructs the WVB with 447 functions and
369,695 code bytes. The reviewed focused staged-publisher test passes 1/1 in
6.771 test seconds after a 9.17-second zero-warning build, including exact
package construction and current-host native atomic publication. The extended
self-lowering test was used only to derive the changed WVO identity and was not
allowed to continue into its longer native production/publication phase; that
phase remains intentionally deferred with the grouped retirement work.

## Consequences

- The staged admission module no longer carries publication-state product code.
- Its WVB shrinks by 9,426 bytes, its WVO by 94,320 bytes, and its paired
  packages by roughly 95 KiB.
- Package runtime publication still uses the shared WVA state owner proved by
  Decision 0393.
- The remaining hosted-container retirement seam is Stage 0 package
  construction and orchestration, not response concatenation or publication
  semantics.

## Reconsideration triggers

Add a source dependency back only when the admission command itself consumes a
versioned portable contract from it. Do not retain an otherwise unused module
to make private function offsets available to a package builder.
