# Windvale progress

> Status snapshot: 2 August 2026

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
| 8. Self-hosted compiler | ✅ Qualified | The committed 12-module inventory produces byte-identical 599,868-byte Stage 1 and Stage 2 compilers on Windows and Debian; its exact native form compiles a real fixture to byte-identical WVB on both hosts. | Require native Stage 1/Stage 2 equality over the complete compiler inventory. |
| 9. Shared native backend | 🎯 Current transfer | Cross-host-qualified Decision 0133 consumes the record maps in ABI 21, independently decodes frame-owned direct records, and runs the exact compiler fixture with zero record-arena use. | Measure the new `WVR3018` dynamic-value boundary, then prove native Stage 1/Stage 2 equality. |
| 10. Native host tools and .NET retirement | 🚧 In progress | The paired `windows-x64-console-v1` and `linux-x64-console-v1` targets are cross-host qualified over the same ABI-21 scalar fragment. Windvale owns startup templates, layout, every final PE/ELF byte, and completed-container structural verification, while bounded Stage 0 adapters retain publication and recovery oracles. | Transfer the next measured hosted/runtime ownership seam, then satisfy the larger .NET-retirement gate. |
| 11. Boot path and kernel | 🚧 In progress | Cross-host-qualified ABI-21 Probe 32 retains `WVPROC11` behavior while `WVKMEM11` bounds a 141-page arena, 120-page client root, exact 24,240-byte stack path, and zero record-arena use. `WVRS 1` plus one-page `WVRQ 1` / `WVRY 1` prove strict dynamic lookup through a format-blind capacity-one lifecycle and a live hosted Windvale service without changing the guest ABI. | Version checked one-page user-buffer IPC and an immutable store capability into the guest; prove service/client death cleanup and one configuration lookup in QEMU. |
| 12. Runtime on Windvale OS | ✅ Qualified | Exact `Sum-Data.wv` and `Function-Only.wv` compiler outputs run across both hosts and Windvale OS; the second covers four functions and four scalar families. | Use a third real program or measured native-size pressure to choose further generalization. |
| 13. Public foundation | 🚧 In progress | The public GitHub repository and its licensing, contribution, security, governance, support, and authorship policies are live. | Record the initial publication baseline and establish ongoing public project operations. |
| WebAssembly interoperability | 🚧 In progress | Cross-host-qualified profile 10 over execution ABI 3 composes checked unsigned scalars, compiler-produced nested control, bounded byte values, and a Windvale-written WVB 1.6 envelope verifier as deterministic import-free Wasm. Profile 8's editable .NET-free page remains intact. | Extend envelope verification into complete WVB payload and semantic verification, then add measured text/record/enum needs and obtain cross-browser evidence. |

## Working end to end

- ✅ Windvale source → canonical WVB → verification → execution on Windows or Linux
- ✅ Windvale assembly → verified WVO → deterministic linked x86-64 image
- ✅ Portable WVB → shared WVO/AOT backend → linked UEFI image → kernel-owned execution
- ✅ Hosted `Wv-Dump-Core.wv` → W^X/WVO execution → deterministic report for a real WVB
- ✅ Windvale-produced native bytes → Windvale validation and patching → live host-service consumption
- ✅ Verified native fragment → Windvale image layout → narrow host W^X publication adapter
- ✅ Windvale lifetime graph → internal state owner → allocate/copy/seal/invoke/release
- ✅ Qualified compiler WVB → native compiler execution → byte-identical WVB file publication
- 🚧 Portable scalar `.wv` → verified WVB/native fragment → deterministic import-free Windows `.exe` or Linux `.elf` → process result
- ✅ ABI-21 direct records → deterministic frame backing → caller-owned returns → zero record-arena use in both the exact compiler and rebuilt Probe 32
- ✅ Canonical constant WVB → Windvale-authored selector → deterministic Wasm → result `42`
- ✅ Checked-add WVB → Windvale-authored selector → execution ABI 1 → result or `WVR3007` plus exact instruction count
- ✅ Bounded straight-line `i32` WVB → Windvale-authored lowering → checked arithmetic → deterministic Wasm under Node.js
- ✅ Sequential loops and `if`/`if/else` → ABI-2 instruction metering → exact success and `WVR3011` exhaustion
- ✅ Bounded acyclic calls → real Wasm functions → one shared ABI-2 instruction budget across callees
- ✅ Pinned generated Wasm → static JavaScript host → disposable browser worker without loading .NET
- ✅ WVA trap entries + Q35 adapter → normalized terminal faults + clean VM poweroff
- ✅ Typed byte/word WVA + exact C# differential oracle → WVA-owned exception terminal and one bounded COM1 byte loop; Windows and Debian pass 77/77 Seed and 31/31 OS tests, and all four Windows pinned-QEMU scenarios pass
- ✅ WVA paging mechanics → kernel-owned low-1-GiB W^X identity root
- ✅ One embedded WVB → in-guest Windvale admission → its AOT form
- ✅ Fixed admission → separate CPL3 root → capability-checked send/receive/exit
- ✅ Deliberate CPL3 privileged fault → recorded process fault → kernel continuation
- ✅ Windvale init service → blocked receive → send-only client → cross-process wake
- ✅ Exact admitted WVB → Windvale interpreter at CPL3 → result 29 → init service
- ✅ Probe 24 → Windows and pinned-Debian Seed plus OS-test qualification
- ✅ Probe 25 → section-derived interpreter → Windows and pinned-Debian Seed plus OS-test qualification
- ✅ Probe 26 → separate RO/NX WVB boot resource → Windows/pinned-Debian Seed and OS qualification plus four Windows pinned-QEMU scenarios
- ✅ Probe 27 → Windvale init selection → one-shot immutable grant → Windows/pinned-Debian Seed and OS qualification plus four Windows pinned-QEMU scenarios
- ✅ Probe 28 → terminal borrower → cleared alias and private publication → 67/67 Seed and 25/25 OS tests on Windows and Debian plus four Windows pinned-QEMU scenarios
- ✅ Probe 29 → atomic typed WVB/budget set → exact WVA lookup and Windvale opcode charging → 67/67 Seed and 25/25 OS tests on Windows and Debian plus four Windows pinned-QEMU scenarios
- ✅ Probe 30 → exact tail release/zero → generation-safe same-root rebuild → 67/67 Seed and 25/25 OS tests on Windows and Debian plus four Windows pinned-QEMU scenarios
- ✅ Probe 31 → exact canonical `Sum-Data.wv` WVB → 203 charged guest opcodes → result `29` in both rebuilt clients → 67/67 Seed and 25/25 OS tests on Windows and Debian plus four Windows pinned-QEMU scenarios
- ✅ Probe 32 → exact cross-compiler `Function-Only.wv` WVB → four functions and `bool`/`u8`/`u32`/`i32` control flow → 199 guest opcodes → result `6` in both rebuilt clients → 67/67 Seed and 25/25 OS tests on Windows and Debian plus four Windows pinned-QEMU scenarios
- 🚧 Three typed resources → canonical `WVRS 1` image → one-page `WVRQ 1` / `WVRY 1` exchange → Windvale-owned live hosted lookup; guest ABI/lifetime integration and cross-host qualification remain

## Reading the evidence

- [README overview](../../README.md#what-works-today) summarizes user-visible working paths and routes current detail here.
- [Development roadmap](Roadmap.md) defines the phase gates and detailed execution plan.
- [Qualification evidence](Seed-Verification-Evidence.md) records the exact cross-host reports, artifacts, and digests.
- [Changelog](../../CHANGELOG.md) summarizes the newest accepted slices.

The SVG is a dated visual aid. It should be refreshed only when the phase picture becomes materially misleading; ordinary wording or milestone changes belong in the Markdown sources first.
