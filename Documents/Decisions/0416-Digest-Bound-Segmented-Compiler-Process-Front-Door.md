# Decision 0416: Digest-bound segmented compiler process front door

- Status: Implemented candidate; canonical image rechunking, hosted-package composition, native container construction, Linux execution, and grouped qualification pending
- Date: 2026-08-08
- Advances: [Decision 0354](0354-Native-Compiler-Image-Staging-Reconstruction.md), [Decision 0415](0415-Managed-Hosted-Tool-Aot-Recovery-Lane.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Advanced by: [Decision 0417](0417-Canonical-Compiler-Image-Transport.md)
- Contract: [Windvale linking](../../Specifications/Windvale-Linking.md#hosted-immutable-snapshot-staging-boundary)

## Context

The segmented native WVO producer and compiler-image staging linker already
reconstructed a compiler-scale object and flat image without loading .NET, but
the evidence test rebuilt both PE/ELF containers through managed writers on
every run. There was no digest-bound process front door for ordinary native
orchestration, and the managed CLI still treated the staging producer as an
ordinary AOT target.

Hosted packaging cannot yet consume the image chunks unchanged. Its immutable
source geometry deliberately accepts one through eight fragment chunks and
requires every non-final fragment chunk to be exactly 4 MiB. The segmented
linker instead preserves semantically owned text, padding, and read-only-data
pieces. Weakening either contract would hide the missing transport boundary.

## Decision

- Check in the two exact native-built WVBs and their paired Windows/Linux
  applications under
  `Artifacts/Native-Segmented-Compiler-Toolset-Candidate` with one manifest.
- Add digest-bound `Stage-Compiler-Wvb` launchers for
  `WVB -> WVOP/chunks` and `Link-Staged-Compiler-Wvo` launchers for
  `WVOP/chunks -> WVLI/image chunks` on both hosts.
- Make the compiler-image staging process report its already-validated decimal
  image size, `Main` entry offset, chunk count, and manifest size. Scripts may
  parse this bounded process result; they do not decode `WVLI`.
- Move both staging application target pairs into the explicit
  `recovery-aot` set. Ordinary `compile` and `aot` reject them, while the
  checked-in candidates remain the normal executable evidence.
- Make the native reconstruction test load and digest-check the checked-in
  current-host candidates instead of constructing new applications through
  the managed writers. Retain Stage 0 only as the temporary differential
  WVO/image oracle.
- Keep canonical image rechunking as the next explicit layer. Do not relax the
  hosted source-geometry contract or make shell scripts decode binary formats.

## Candidate identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Segmented WVO producer WVB | 421,544 | `4d8fcda41a013768a10a2919d06658d9c37fc66d8acbf51bad839c5ef4d13fc6` |
| Windows segmented WVO producer | 6,170,624 | `c18253d135f15195cad32ccf6f7243711bfa959a44696b388475165406216adb` |
| Linux segmented WVO producer | 6,172,672 | `e38bc7b4128afc829de112098c5844d3c3fc159d11d09d6e97ef2f79d19845d7` |
| Compiler-image staging WVB | 75,503 | `e43e2cc868b5f7ac3ffbee322bef60ce748c736e666889aaeda7c06a90daa5bb` |
| Windows compiler-image staging linker | 851,968 | `967827e4592c23f30e2a70b9a60a43837c1dfec6112584596c09d382058e2752` |
| Linux compiler-image staging linker | 851,968 | `02b07d23b763fa4dd2d11bb9c9ca94be32bdbd698b1f9ce7b466af90b768eef8` |

## Evidence and consequences

After review, the three focused owners pass 1/1 each in 1.977, 3.610, and
7.154 test seconds after one zero-warning Release build; after adding explicit
ordinary-AOT rejection assertions, the reconstruction owner passes again in
7.356 seconds after a zero-warning incremental build. The Windows launchers
also compose directly over the 75,503-byte staging WVB: the producer emits an
834,822-byte WVO in 14 chunks, and the linker emits an 831,624-byte image at
entry offset 10,569 in 10 semantic chunks. This deliberately demonstrates why
canonical rechunking is the next boundary rather than an optional cleanup.
Broader Development, Standard, and Qualification verification remain deferred
to the grouped retirement gate.

The managed writers and backend still exist for recovery and differential
evidence. This slice removes their use from repeated native reconstruction and
from the ordinary CLI surface; it does not claim that the checked-in candidate
containers were constructed natively.

## Reconsideration triggers

Regenerate the candidate when either source closure, WVB identity, native ABI,
service bundle, startup, PE/ELF writer, `WVOP`, or `WVLI` changes. Preserve a
separate canonical image-transport contract if the hosted fragment geometry
changes; do not couple binary-format parsing to platform scripts.
