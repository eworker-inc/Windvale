# Decision 0150: Bounded native dynamic-value lifetimes

- Date: 2026-08-03
- Status: Implemented and cross-host qualified
- Advances: Native ABI 22, target `x86-64-wvb-baseline-v22`, native host text/byte arena capacity to 64 MiB, and kernel memory `WVKMEM13`
- Retains: Execution-context version 7, service-table version 5, canonical WVB 1.6 compiler identity, WVO 1.0, the 2,048-cell frame ceiling, the 32 MiB fragment ceiling, the 34 MiB publication-image ceiling, `WVPROC13`, Probe 34, `WVCHAN03`, `WVRES005`, and console-application format 1
- Refines: [Decision 0071](0071-Native-Text-Arena-And-Core-Text-Services.md), [Decision 0115](0115-Exact-Compiler-Record-Lifetime-Pressure.md), [Decision 0133](0133-Frame-Owned-Direct-Native-Records.md), [Decision 0142](0142-Immutable-Guest-Resource-Store.md), [Decision 0147](0147-Native-Descriptor-Ownership-Plan.md), [Decision 0148](0148-First-Wva-Native-Descriptor-Allocator-Leaf.md), [Decision 0119](0119-First-Windows-Console-Application.md), and [Decision 0122](0122-First-Linux-Console-Application.md)

## Context

ABI 21 removed the full compiler's monotonic record pressure and exposed `WVR3018` at the 16 MiB dynamic text/byte arena. Raising that capacity without changing lifetime behavior would not explain whether the retained memory represented live values or obsolete intermediates. The exact native Stage 1 compiler must compile the canonical 12-source inventory and reproduce the qualified 599,868-byte Stage 2 module before Windvale can claim native compiler convergence.

The observed pressure has two distinct shapes. `Bytesˉconcat` repeatedly extends compiler output buffers and needs value-preserving growth rather than exact allocation on every append. Text and byte descriptors returned by helper functions frequently point wholly into storage allocated after function entry and can be moved to the entry cursor before the callee releases its transient suffix. Direct records containing descriptors are different: relocating one returned field without knowing which other caller-visible descriptors are live can invalidate an alias. That aggregate case needs caller-liveness evidence rather than an optimistic copy.

## Decision

- Advance the single shared x86-64 backend and independent fragment verifier to ABI 22. Older selected fragments remain historical evidence and are not admitted by the ABI-22 verifier.
- Keep the 16-byte descriptor layout. Pointer occupies the low machine word, length occupies the next dword, and the high dword becomes a generated-code ownership generation. Static, host-borrowed, sliced, and exported descriptors remain canonical with generation zero. Copies and calls propagate the complete high machine word.
- Give generated dynamic byte buffers of at least 64 bytes an eight-byte arena header containing capacity and a nonzero generation. Capacity doubles through 2 MiB and is capped at 4 MiB, the existing maximum byte-value size. Header, range, capacity, generation, arithmetic, and arena bounds are checked before reuse or publication.
- Allow `Bytesˉconcat` to reuse only the current valid owner at the arena tail. Growth advances the header generation so older descriptor aliases cannot later claim ownership. A stale, non-tail, non-owned, or insufficient buffer falls back to a checked owned or exact allocation. Empty and adjacent cases preserve immutable value semantics rather than relying on pointer adjacency as ownership.
- Save the entry text-arena cursor in every non-entry descriptor/record hidden-result cell. A direct descriptor return resets to the checkpoint when it returns pre-existing storage, compacts a wholly internal result to the checkpoint, and preserves the arena when the result spans the boundary. The returned public descriptor has generation zero.
- Reset the cursor after returning a direct record only when the record has no direct `text` or `bytes` field. Do not relocate descriptor-bearing aggregates until caller-visible descriptor liveness and aliasing are explicit in the ABI evidence.
- Require the independent fragment verifier to reconstruct the complete owned-concat stencil, generation/capacity/header checks, full descriptor copies, checkpoint prologue, descriptor classification/compaction return, scalar-record rollback, branch targets, and canonical exported result. Corrupt ownership thresholds and checkpoint bytes fail as `WVN3030`.
- Increase the ordinary Windows/Linux W^X executor's checked arena capacity from 16 MiB to 64 MiB. Keep the context field 32-bit and version 7; it already carries the exact per-run capacity. The narrow version-1 PE and ELF containers retain their separately bounded 16 MiB arena because they admit only capability-free programs within that container contract.
- Require the version-1 descriptor-ownership plan and its independent reconstruction to agree before ABI-22 selection. Retain Decision 0148's WVA first-fit/coalescing allocator leaf as an independently verified lower-level candidate rather than calling it implicitly from selected fragments. The generation/tail and checkpoint mechanics are a narrower emitted policy than the complete retain/release vocabulary; they do not claim to lower the first-fit replay, call the WVA leaf, or emit every planned action.
- Rebuild the current Probe-34 Windvale OS consumer through the same selector. The interpreter needs 110 RX pages. Advance `WVKMEM13` to a 144-page arena, a 121-page client root, and a 119-page client memory budget while retaining Probe 34's 11-page init/resource extent. Retain the exact six-page native stack proof, `WVPROC13`, guest WVB, resource semantics, and serial/firmware format.

## Evidence

The expanded byte fixture builds 2,313 bytes through 256 appends and retains older and forked aliases. The reference runtime, independently decoded W^X execution, and linked WVO/AOT all return `42`. A changed ownership threshold is rejected before publication.

The exact 328-function compiler retains the Decision 0118 record map and 1,489-cell maximum. ABI 22 selects 17,130,441 deterministic code bytes with SHA-256 `af8db63675a2441e57a763ca4caa411419a84879cf01a1eb62b4be7556487cab`. It passes independent decoding. Compiling the function-only fixture returns zero, uses zero record-arena bytes and 60,632 text-arena bytes, emits the exact success line with no diagnostic, and reproduces the canonical 815-byte WVB. A changed function-entry checkpoint is rejected as `WVN3030`.

The same native Stage 1 compiler compiles all 12 canonical sources under an eight-billion-instruction ceiling. It returns zero, uses zero record-arena bytes, peaks at 64,476,249 of 67,108,864 text-arena bytes, and emits:

```text
source wvb status=Valid functions=328 code-bytes=481356 module-bytes=599868
```

The produced 599,868-byte Stage 2 module is byte-identical to Stage 1 and retains SHA-256 `9673bf3331763181f443ec67b7a513bc66daa718969f7f6b0d197a4186071066`. This completes native in-memory Stage 1-to-Stage 2 compiler reproduction; it does not yet package that hosted compiler as a standalone PE or ELF tool.

The integrated Decision 0147/0148 compiler retains the exact fragment, compiler output, ownership-action map, and native high-water identities above. Focused ownership-plan, descriptor/alias, dynamic-return, single-source exact-compiler, and complete Stage 1-to-Stage 2 tests pass after a zero-warning build.

The shared Probe-34 rebuild retains the exact 24,240-byte deepest stack path, six-page client stack, zero record-arena use, 1,195-byte init-owned resource store, dynamic lookup, and terminal peer cleanup. Its interpreter WVO remains 447,652 bytes with SHA-256 `0748200721cab7d5c3c6a43916fc623dfa0ee35e304fea6ad899877c9601c8e2`. The normal client is 447,757 bytes / SHA-256 `aa3fdd6e836c71add4f24b6992fcfae090bf8d5aa056ec068a9c21dea516c919`; the fault client is 447,741 bytes / `3344865517e56066ec0b8fbd7e5f80695a715fa04adc9959a876a73e04bbbbe8`. All 31 in-process OS tests pass. Pinned QEMU 11.0/Q35/TCG passes all four integrated Probe-34 scenarios:

| Scenario | EFI bytes | SHA-256 | Host code |
| --- | ---: | --- | ---: |
| Normal | 568,320 | `584f1cbf06607722aa52cf3ec743a0f25fb595688349be0bc8c41260849a9490` | 0 |
| Invalid opcode | 568,320 | `2e373f6374691cdbfb84f02464b12f00da3a94e2baa9185c1807aab3c28c9efe` | 3 |
| General protection | 568,320 | `9b905c8202c95ce20a0c72a9b7b2cf6f6bce4b99d7c0c18a1f65b24164444b23` | 3 |
| Contained user fault | 568,832 | `e25741a6bd565af63498a199a7da9817a5fa8c60512a2c514a58c2e4311ed00d` | 0 |

Immediately before the additive WebAssembly Decision 0149 rebase, integrated Windows Qualification completes in 646.9 seconds with a zero-warning Release build, all 84 Seed tests in 352.724 seconds including the 212.138-second golden contract, all 31 OS tests, and the complete native CLI gate. Decision 0149 changes no native or OS input; its expanded WebAssembly case and the complete native Stage 1-to-Stage 2 case both pass after rebase.

Exact descendant commit `2591cd557f2b3055ae1e4ba96561bf3ca7864283` passes GitHub [Verify run 30797770080](https://github.com/eworker-inc/Windvale/actions/runs/30797770080). Windows and digest-pinned Debian 12 each complete a zero-warning Release build, all 84 Seed tests, the qualification-only golden compiler contract, all 31 OS tests, and the complete native CLI gate. Windows takes 331.420 Seed-suite seconds with a 191.546-second golden contract; its job completes in approximately 11 minutes 45 seconds. Debian takes 238.621 Seed-suite seconds with a 129.787-second golden contract; its job completes in approximately 7 minutes 46 seconds. Descendant Decision 0151 changes no selected ABI-22 bytes or OS inputs, so this run cross-host qualifies Decision 0150's exact compiler reproduction and `WVKMEM13`/Probe-34 composition while also exercising the additive allocator-emission-plan verifier.

## Consequences

The native compiler now crosses the complete self-reproduction workload without a garbage collector or unbounded arena. Its 64 MiB host capacity is an explicit measured admission bound with 2,632,615 bytes of headroom, not a claim that all future programs should use a fixed arena of that size.

Descriptor ownership is an internal generated-code optimization. It is not serialized in WVB or WVO, exposed as portable source semantics, accepted from host descriptors, or allowed to escape a native run. Generation wrap and all capacity arithmetic fail closed before mutation.

Native compiler convergence is only one condition of the .NET-retirement gate. The 17.1 MiB compiler still exceeds the 4 MiB WVO/flat-link limits; hosted file/argument/output requirements are not serialized into standalone container metadata; repository verification, packaging, recovery, and the remaining native tools still use Stage 0.

## Reconsider when

- Caller-liveness evidence can safely relocate descriptor-bearing record results.
- A real workload exceeds the measured 64 MiB host arena or needs escaping, asynchronous, shared, or long-lived dynamic values.
- Compact verified copy/growth code materially reduces the 17.1 MiB baseline fragment.
- Standalone compiler packaging revises the WVO/linker ceiling or introduces a different bounded native container.
- A tracing, region, reference-counted, or ownership-typed allocator can replace this execution-scoped bootstrap contract without changing Windvale semantics.
