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
| 9. Shared native backend | 🎯 Current transfer | ABI 16/context 7, all 12 native service leaves, and verified internal calls through 64 parameters are qualified; exact compiler preflight passes the former eight-parameter blocker. | Admit the compiler's sole 1,049-local function under a measured frame contract. |
| 10. Native host tools and .NET retirement | 🚧 In progress | Windvale constructs the live process-input leaves and owns image layout plus lifecycle policy; one internal host owner contains raw W^X authority. | Qualify native compiler execution and standalone Windows/Linux tools, then satisfy the documented .NET-retirement gate. |
| 11. Boot path and kernel | 🚧 In progress | Probe 21 remains cross-host qualified; candidate probe 23 adds two CPL3 roots, a Windvale init/resource service, reduced endpoints, cross-process IPC, deterministic wait/wake, and client-fault containment. | Cross-host-qualify probe 23, then use a third runnable or loader case to measure the next contract. |
| 12. Runtime on Windvale OS | 🚧 In progress | One exact admitted client and one fixed Windvale service now execute in isolated processes; there is no general loader/runtime yet. | Generalize semantic admission and run one identical WVB through equivalent Windows, Linux, and Windvale OS contracts. |
| 13. Public foundation | 🚧 In progress | The public GitHub repository and its licensing, contribution, security, governance, support, and authorship policies are live. | Record the initial publication baseline and establish ongoing public project operations. |

## Working end to end

- ✅ Windvale source → canonical WVB → verification → execution on Windows or Linux
- ✅ Windvale assembly → verified WVO → deterministic linked x86-64 image
- ✅ Portable WVB → shared WVO/AOT backend → linked UEFI image → kernel-owned execution
- ✅ Hosted `Wv-Dump-Core.wv` → W^X/WVO execution → deterministic report for a real WVB
- ✅ Windvale-produced native bytes → Windvale validation and patching → live host-service consumption
- ✅ Verified native fragment → Windvale image layout → narrow host W^X publication adapter
- ✅ Windvale lifetime graph → internal state owner → allocate/copy/seal/invoke/release
- ✅ Qualified compiler WVB → native file publication
- 🎯 Qualified compiler WVB → qualified ABI-16 calls → bounded 1,049-local frame admission
- ✅ WVA trap entries + Q35 adapter → normalized terminal faults + clean VM poweroff
- ✅ WVA paging mechanics → kernel-owned low-1-GiB W^X identity root
- ✅ One embedded WVB → in-guest Windvale admission → its AOT form
- ✅ Fixed admission → separate CPL3 root → capability-checked send/receive/exit
- ✅ Deliberate CPL3 privileged fault → recorded process fault → kernel continuation
- ✅ Windvale init service → blocked receive → send-only client → cross-process wake
- 🚧 Candidate probe 23 → cross-host qualification → measured step-6 loader/runtime or third-runnable pressure

## Reading the evidence

- [README progress table](../../README.md#progress-at-a-glance) describes user-visible project areas and their next milestones.
- [Development roadmap](Roadmap.md) defines the phase gates and detailed execution plan.
- [Qualification evidence](Seed-Verification-Evidence.md) records the exact cross-host reports, artifacts, and digests.
- [Changelog](../../CHANGELOG.md) summarizes the newest accepted slices.

The SVG is a dated visual aid. It should be refreshed only when the phase picture becomes materially misleading; ordinary wording or milestone changes belong in the Markdown sources first.
