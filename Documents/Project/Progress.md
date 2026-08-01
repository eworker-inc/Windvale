# Windvale progress

> Status snapshot: 1 August 2026

<a href="Images/Windvale-Roadmap-August-2026.svg"><img src="Images/Windvale-Roadmap-August-2026.svg" alt="Windvale roadmap phase map showing qualified, ongoing, current, in-progress, and planned phases" width="100%"></a>

This is an editorial snapshot of the implemented and qualified project state, not a generated completion meter. Windvale phases overlap: later experiments can produce evidence while an earlier, deliberately open-ended foundation phase continues. The [development roadmap](Roadmap.md), accepted decisions, and [qualification evidence](Seed-Verification-Evidence.md) remain authoritative.

## Indicators

| Indicator | Meaning |
| :---: | --- |
| ✅ Qualified | The phase's defined gate has reproducible evidence. |
| 🔵 Ongoing | Useful qualified slices exist, and the phase continues as real tool pressure demands more. |
| 🎯 Current transfer | The immediate measured ownership boundary being moved into Windvale. |
| 🚧 In progress | Concrete implementation evidence exists, but the phase gate remains open. |
| ○ Planned | The phase has an accepted direction but not yet its completion evidence. |

These indicators describe evidence, not effort. Windvale does not publish percentage-complete estimates because the remaining work changes as compiler, runtime, native, and operating-system experiments reveal better boundaries.

## Roadmap gates

| Phase | Status | Evidence today | Next gate |
| --- | :---: | --- | --- |
| 0–6. Seed through assembler and linker | ✅ Qualified | The Stage 0 foundation, byte primitives, hosted resource boundary, `wvdump`, object model, assembler, and linker have Windows and Debian evidence. | Preserve these contracts as later native and OS work consumes them. |
| 7. Foundation modules | 🔵 Ongoing | Machine contracts, byte ordering, decimal parsing, and byte construction are shared by real Windvale tools. | Add facilities only when measured compiler, runtime, or tool pressure requires them. |
| 8. Self-hosted compiler | ✅ Qualified | The committed 12-module inventory produces byte-identical 599,868-byte Stage 1 and Stage 2 compilers on Windows and Debian. | Execute the qualified compiler through the shared native path. |
| 9. Shared native backend | 🎯 Current transfer | ABI 14/context 6, all 11 native service leaves, two WVA-owned stencils, their Windvale validator/patcher, bounded byte results, and Windvale-owned executable-image layout are cross-host qualified. | Define explicit publication-lifetime state and move the next measured W^X adapter boundary into Windvale. |
| 10. Native host tools and .NET retirement | 🚧 In progress | Windvale constructs the two live process-input leaves and plans every live executable-image extent and service placement. | Qualify native compiler execution and standalone Windows/Linux tools, then satisfy the documented .NET-retirement gate. |
| 11. Boot path and kernel | 🚧 In progress | Probe 17 runs the ABI-14 WVB path and owns a terminal vector-6 invalid-opcode boundary under pinned QEMU. | Add broader trap handling, in-guest loading and verification, clean shutdown, and Hyper-V evidence. |
| 12. Runtime on Windvale OS | ○ Planned | The portability contract is defined; the guest does not yet load and verify WVB itself. | Run one identical verified WVB through equivalent Windows, Linux, and Windvale OS contracts. |
| 13. Public foundation | 🚧 In progress | Licensing, contribution, security, governance, support, and authorship policies are prepared. | Complete GitHub settings, the publication baseline, and public project operations. |

## Working end to end

- ✅ Windvale source → canonical WVB → verification → execution on Windows or Linux
- ✅ Windvale assembly → verified WVO → deterministic linked x86-64 image
- ✅ Portable WVB → ABI-14 WVO/AOT → linked UEFI image → kernel-owned execution
- ✅ Hosted `Wv-Dump-Core.wv` → W^X/WVO execution → deterministic report for a real WVB
- ✅ Windvale-produced native bytes → Windvale validation and patching → live host-service consumption
- 🎯 Verified native fragment → Windvale image layout → narrow host W^X publication and lifetime adapter
- ○ One identical WVB → Windows + Linux + in-guest Windvale OS verification and execution

## Reading the evidence

- [README progress table](../../README.md#progress-at-a-glance) describes user-visible project areas and their next milestones.
- [Development roadmap](Roadmap.md) defines the phase gates and detailed execution plan.
- [Qualification evidence](Seed-Verification-Evidence.md) records the exact cross-host reports, artifacts, and digests.
- [Changelog](../../CHANGELOG.md) summarizes the newest accepted slices.

The SVG is a dated visual aid. It should be refreshed only when the phase picture becomes materially misleading; ordinary wording or milestone changes belong in the Markdown sources first.
