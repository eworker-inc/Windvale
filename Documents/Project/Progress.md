# Windvale progress

> Status snapshot: 4 August 2026

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
| 7. Foundation modules | 🔵 Ongoing | Machine contracts, byte ordering, decimal parsing, and byte construction are shared by real Windvale tools. Decisions 0145 and 0153 qualify explicit transitive platform-library approval plus the first versioned, rights-limited immutable directory read. The unqualified `WVDB 1` experiment adds a portable typed read-only page/B+tree consumer. Probe 35 exercises resource semantics through an isolated guest service, and Probe 36 contains one deliberate failure of that service. | Review the bounded database-reader API and select a real read-only consumer without treating its experimental bytes as a durable format; keep provider and platform metadata explicit. |
| 8. Self-hosted compiler | ✅ Qualified | The committed 12-module inventory produces byte-identical 599,868-byte Stage 1 and Stage 2 compilers on Windows and Debian. Cross-host-qualified Decisions 0168 and 0169 package the exact native compiler as independently verified, atomically published PE/ELF executables and directly reproduce Stage 2 on both hosts without loading .NET; the Stage 0 recovery runbook records clean-checkout provenance. | Advance the remaining native tools without weakening the retained recovery oracle. |
| 9. Shared native backend | 🎯 Current transfer | Cross-host-qualified Decision 0148 supplies the live WVA reclaiming leaf. Decision 0150 verifies generation-owned byte buffers and return checkpoints and completes cross-host native reproduction without implicitly calling that leaf; qualified Decision 0151 maps all 180,190 full-allocator invocations to physical owner locations and five verified phases. | Add caller-liveness evidence or consume the full-allocator schedule in one small successor fixture before broadening allocation integration. |
| 10. Native host tools and .NET retirement | 🚧 In progress | Cross-host-qualified Decisions 0160–0169 package and directly run the exact ABI-22 compiler. Decisions 0185–0187 add paired standalone verifier applications and a project-aware format-5 native build driver. Decisions 0190, 0194, 0195, and 0204 transfer the bounded metered ABI-22 selector path: Windvale verifies and exactly lowers `i32`/`bool` locals, checked arithmetic, comparisons, typed block-slot reuse, forward/backward jumps, conditionals, loops, and early returns, with current-host native-tool and malformed-input preservation evidence. Cross-host qualification is pending. | Qualify the verifier/driver/lowering evidence on both hosts, transfer bounded direct calls and multiple functions, and add a named atomic-replacement or external native publication step before moving native packaging and normal automation off Stage 0. |
| 11. Boot path and kernel | 🚧 In progress | Cross-host-qualified Probe 40 adds `WVKMEM17`, fixed generation-safe memory objects, and WVA-owned first-fit non-tail client release/zero/reuse while the later directory object remains live, without changing `WVPROC17`, paging 5, ABI 22, or Probe 39's timer proof. All 87 Seed and 39 OS tests pass on Windows/Debian; all five pinned Windows QEMU scenarios pass. | Add one flat resource domain without publishing the fixed timer or memory-object layouts as an ABI. |
| 12. Runtime on Windvale OS | ✅ Qualified | Exact `Sum-Data.wv` and `Function-Only.wv` compiler outputs run across both hosts and Windvale OS; the second covers four functions and four scalar families. | Use a third real program or measured native-size pressure to choose further generalization. |
| 13. Public foundation | 🚧 In progress | The public GitHub repository and its licensing, contribution, security, governance, support, and authorship policies are live. | Record the initial publication baseline and establish ongoing public project operations. |
| WebAssembly interoperability | 🚧 In progress | The portable `.wv` selector reaches local profile 16 over execution ABI 3. The complete compiler-aligned verifier and reclaiming WVB interpreter remain deterministic import-free Wasm artifacts. Node.js runs verify-then-interpret scalar/text/bytes/record/enum workloads plus `WVXI 2` byte-array entry. The current evolved hosted compiler and capability-free adapter use a four-phase compiler-capacity bundle, while reclaiming generated-Wasm values and conservatively traced guest records clear the former 1,511/1,512 and 37,085 boundaries and carry the compiler to an ordinary 100,000-instruction guest-budget result. Profile 8's editable .NET-free page remains intact; profile 10 is cross-host qualified. | Add ownership and reclamation to the separately retained 64 KiB guest text/bytes heap. Continue until the compiler publishes canonical WVB, then package the complete static-worker pipeline. |

The current unqualified language candidate advances Stage 0 and the Windvale-written compiler together through WVB 1.11: inference and trailing commas; constants; privacy, aliases, qualified identities, and metadata; named records and `else if`; exhaustive `match`; nominal payload variants and recoverable-result shapes; bounded sequences, affine builders, and `for`; loop control and short-circuit flow; compound assignment; checked division/remainder; bitwise operations and shifts; and exact text/bytes equality. The ordinary native compiler path, deterministic artifacts, editor grammar, and focused compiler/runtime/WebAssembly cases are synchronized. Resource-lifetime syntax remains at its explicit design gate until provider values, cleanup ordering/failures, and immutable manifest representation are decided.

The [Windvale Database reader experiment](../../Specifications/Windvale-Database-Reader.md) is the first concrete database-driven consumer of that evolved result surface. It validates a maximum 16,416-byte immutable snapshot containing at most 64 checksummed pages, performs an exact bounded B+tree lookup, returns typed found/missing/failure outcomes, and has independent malformed-input fixtures. It does not implement durable storage, transactions, caching, concurrency, or a service.

Proposed [Decision 0198](../Decisions/0198-Next-Integrated-Architecture-Defaults.md) is a documentation-only successor review set. It builds from qualified Probe 40 and recommends concrete defaults for resource domains and later memory generalization, clean launch/supervision, streams/terminal/shell, `LinkPort 1`/`virtio-net`, identity/time/entropy/trust, packages/releases/recovery, and language variants/collections/metadata. None of those proposed contracts changes the implemented or qualified indicators above.

## Working end to end

- ✅ Windvale source → canonical WVB → verification → execution on Windows or Linux
- 🚧 Evolved source surface → conditional WVB 1.6–1.11 → verified reference, Windvale-compiler, native, and bounded WebAssembly paths; independent dual-host qualification remains
- ✅ Windvale assembly → verified WVO → deterministic linked x86-64 image
- ✅ Portable WVB → shared WVO/AOT backend → linked UEFI image → kernel-owned execution
- ✅ Hosted `Wv-Dump-Core.wv` → W^X/WVO execution → deterministic report for a real WVB
- 🔵 Capability-bearing hosted library → explicit transitive application approval → canonical WVB requirement → separate runtime grant → live immutable `WVRS 1` lookup → versioned `WVDR 1` directory read → cross-host-qualified Probe-35 guest service → Probe-36 contained service peer loss → Probe-37 kernel-owned endpoint identity → Probe-38 split providers and two endpoints → Probe-39 bounded preemption → qualified Probe-40 non-tail memory objects; names and discovery remain
- 🚧 Immutable database bytes → complete `WVDB 1` page validation → bounded B+tree traversal → typed exact lookup; durable format, storage authority, mutation, transactions, and service placement remain
- ✅ Windvale-produced native bytes → Windvale validation and patching → live host-service consumption
- ✅ Verified native fragment → Windvale image layout → narrow host W^X publication adapter
- ✅ Windvale lifetime graph → internal state owner → allocate/copy/seal/invoke/release
- ✅ Qualified compiler WVB → native compiler execution → byte-identical WVB file publication
- ✅ Independently verified native PE/ELF compiler → complete 12-source inventory → byte-identical Stage 2 compiler on Windows and pinned Debian without loading .NET
- ✅ Portable scalar `.wv` → verified WVB/native fragment → cross-host-qualified deterministic import-free Windows `.exe` or Linux `.elf` → normalized process result
- ✅ Hosted `.wv` with `console.write_line` → verified service requirement → `WVHC 1` metadata and exact output leaf → cross-host-qualified standalone Windows/Linux console application
- ✅ Exact ABI-22 compiler → measured large-native WVO → bounded link/runtime/service profiles → cross-host-qualified direct PE/ELF Stage 2 reproduction → public atomic `compile`/`aot` recovery route
- 🔵 Explicit `.wv` inputs or bounded Project 1 → single source snapshots → Windvale-native compiler → shared portable compiler-aligned verifier → accepted WVB publication through format-5 Windows/Linux driver packages; current-host direct evidence passes, cross-host qualification and atomic source-visible replacement remain
- 🔵 Constant-return WVB 1.6 → Windvale-owned ABI-22 x86-64 selection → exact canonical WVO 1.0; two-immediate oracle agreement, native hosted-shell execution, and malformed-input output preservation pass on the current host, while broader operations and dual-host qualification remain
- ✅ ABI-21 direct records → deterministic frame backing → caller-owned returns → zero record-arena use in both the exact compiler and rebuilt Probe 32
- 🚧 Verified descriptor ownership → exact WVA allocator leaf → physical emission schedule → live W^X differential execution; full-allocator selection remains open
- ✅ ABI-22 dynamic values → generation-owned byte buffers → verified return checkpoints → complete native Stage 2 reproduction on Windows and Debian
- ✅ Canonical constant WVB → Windvale-authored selector → deterministic Wasm → result `42`
- ✅ Checked-add WVB → Windvale-authored selector → execution ABI 1 → result or `WVR3007` plus exact instruction count
- ✅ Bounded straight-line `i32` WVB → Windvale-authored lowering → checked arithmetic → deterministic Wasm under Node.js
- ✅ Sequential loops and `if`/`if/else` → ABI-2 instruction metering → exact success and `WVR3011` exhaustion
- ✅ Bounded acyclic calls → real Wasm functions → one shared ABI-2 instruction budget across callees
- ✅ Canonical WVB → complete Windvale verifier Wasm → bounded scalar/text/bytes interpreter Wasm → exact result, failure, and dual-budget evidence under Node.js
- ✅ Pinned generated Wasm → static JavaScript host → disposable browser worker without loading .NET
- ✅ WVA trap entries + Q35 adapter → normalized terminal faults + clean VM poweroff
- 🚧 Typed byte/word WVA + exact C# differential oracle → WVA-owned exception terminal and one bounded COM1 byte loop; the integrated Windows/Debian suites and four Windows pinned-QEMU scenarios pass, while Decision 0125's dedicated exact cross-host/pinned qualification claim remains open
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
- 🔵 Three typed resources → canonical `WVRS 1` lookup → separately attached `WVDS 1` snapshot → exact `WVDQ 1` / maximal `WVDR 1` exchange in both rebuilt clients → cross-host-qualified Probe 35 → Probe-36 malformed request → contained init fault → exact waiting-client wake and resource revocation; all five Windows QEMU scenarios pass

## Reading the evidence

- [README overview](../../README.md#what-works-today) summarizes user-visible working paths and routes current detail here.
- [Development roadmap](Roadmap.md) defines the phase gates and detailed execution plan.
- [Qualification evidence](Seed-Verification-Evidence.md) records the exact cross-host reports, artifacts, and digests.
- [Changelog](../../CHANGELOG.md) summarizes the newest accepted slices.

The SVG is a dated visual aid. It should be refreshed only when the phase picture becomes materially misleading; ordinary wording or milestone changes belong in the Markdown sources first.
