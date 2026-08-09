# Decision 0420: Multi-fragment current-lowerer reconstruction

- Status: Implemented Windows reconstruction; Linux execution, paired promotion, and grouped qualification pending
- Date: 2026-08-08
- Advances: [Decision 0418](0418-Segmented-Compiler-Hosted-Package-Composition.md), [Decision 0419](0419-Descriptor-Returning-Native-Main.md), and [Decision 0304](0304-Digest-Bound-Native-Wvb-To-Wvo-Candidate.md)
- Inventory: [.NET retirement inventory](../Project/Dotnet-Retirement-Inventory.md)

## Context

The segmented compiler pipeline accepted the current 409-function native
lowerer through WVB staging, image linking, and canonical 4 MiB transport, but
hosted packaging rejected its two-fragment image. Fixed-service acquisition
correctly placed services relative to the fragment count. The shared package
scripts still wrote the separately generated enum service to fixed
`.chunk-7`, which is correct only for a one-fragment image. With two fragments
that path overwrote service 6 and source-geometry admission failed.

## Decision

Derive the enum-service resource index as `fragment count + 6` in both host
scripts. This is the zero-based resource position immediately after fragments
and services 1 through 6, and it agrees with the existing fixed-service and
source-geometry contracts for every admitted count from one through eight.
Keep the index calculation in script control flow; scripts still do not decode
or construct a Windvale binary format.

Extend the segmented compiler packaging smoke beyond the one-fragment staging
application. It must build the exact current lowerer WVB through the native
source front door, compose its two-fragment profile-6 host application, lower
the descriptor-entry fixture, and reproduce the retained baseline-JIT bridge
WVO byte for byte. Retain paired Windows and Linux assertions even though this
decision records only current-host Windows execution.

## Evidence and consequences

The exact 399,691-byte lowerer WVB at SHA-256
`92655af0632b4dd3525c2b2de98353b095fa1df94b524a94aa47f16014f1e508`
stages into 5,790,127 WVO bytes across 53 chunks plus a 660-byte `WVOP`
manifest. Linking produces a 5,774,315-byte image at entry zero across 49
semantic chunks; canonical transport produces fragments of 4,194,304 and
1,580,011 bytes plus a 52-byte `WVLI` manifest.

Windows hosted composition produces the exact independently pinned
5,792,768-byte application at SHA-256
`e096dc7fec20e3318364da1f3b5289f772b53c16cc370f29622dfac35780e2bf`.
That native application emits the exact 793-byte descriptor-entry WVO at
SHA-256
`9936663f45c194441bfc5e8464286e57f83cd3a18948597a8011af608a4faa51`
and the exact retained 56,226-byte baseline-JIT bridge WVO at SHA-256
`bcc02cdc6134da2388265ad308d3dc739a7e10c1911effa918d5f2577c86ae8c`.
The focused Windows smoke passes 2/2 in 89.7 seconds. No C# compiler or runtime
implementation changed.

This closes current-lowerer native host-container reconstruction on Windows.
The supporting segmented process containers and the paired current lowerer
remain candidates: genuine Linux reconstruction/execution, artifact promotion,
ordinary-launcher cutover, and the grouped Decision 0057 gate remain required.

## Reconsideration triggers

Version the composition when hosted service order, canonical fragment geometry,
fragment limits, or source-resource naming changes. Do not return the enum slot
to a fixed filename or let host scripts infer it by reading binary manifests.
