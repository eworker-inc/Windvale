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
| 9. Shared native backend | 🎯 Current transfer | ABI 14/context 6 and publication ownership are qualified; ABI 15/context 7 implements the twelfth native leaf and advances exact compiler preflight beyond `file.write_bytes`. | Qualify native file output, then admit the compiler's bounded record-shaped function workload. |
| 10. Native host tools and .NET retirement | 🚧 In progress | Windvale constructs the live process-input leaves and owns image layout plus lifecycle policy; one internal host owner contains raw W^X authority. | Qualify native compiler execution and standalone Windows/Linux tools, then satisfy the documented .NET-retirement gate. |
| 11. Boot path and kernel | 🚧 In progress | Probe 17 is qualified; candidate probe 20 composes ABI 15/context 7, normalized vectors 6/13, clean Q35 poweroff, and a kernel-owned W^X identity root. | Cross-host qualify probe 20, then boot the AOT Windvale verifier in-guest. |
| 12. Runtime on Windvale OS | ○ Planned | The portability contract is defined; the guest does not yet load and verify WVB itself. | Run one identical verified WVB through equivalent Windows, Linux, and Windvale OS contracts. |
| 13. Public foundation | 🚧 In progress | The public GitHub repository and its licensing, contribution, security, governance, support, and authorship policies are live. | Record the initial publication baseline and establish ongoing public project operations. |

## Working end to end

- ✅ Windvale source → canonical WVB → verification → execution on Windows or Linux
- ✅ Windvale assembly → verified WVO → deterministic linked x86-64 image
- ✅ Portable WVB → shared WVO/AOT backend → linked UEFI image → kernel-owned execution
- ✅ Hosted `Wv-Dump-Core.wv` → W^X/WVO execution → deterministic report for a real WVB
- ✅ Windvale-produced native bytes → Windvale validation and patching → live host-service consumption
- ✅ Verified native fragment → Windvale image layout → narrow host W^X publication adapter
- ✅ Windvale lifetime graph → internal state owner → allocate/copy/seal/invoke/release
- 🎯 Qualified compiler WVB → native file publication → bounded record-shaped function admission
- 🚧 WVA trap/paging mechanics + Q35 adapter → normalized faults + owned W^X root + clean poweroff
- ○ One identical WVB → Windows + Linux + in-guest Windvale OS verification and execution

## Reading the evidence

- [README progress table](../../README.md#progress-at-a-glance) describes user-visible project areas and their next milestones.
- [Development roadmap](Roadmap.md) defines the phase gates and detailed execution plan.
- [Qualification evidence](Seed-Verification-Evidence.md) records the exact cross-host reports, artifacts, and digests.
- [Changelog](../../CHANGELOG.md) summarizes the newest accepted slices.

The SVG is a dated visual aid. It should be refreshed only when the phase picture becomes materially misleading; ordinary wording or milestone changes belong in the Markdown sources first.
