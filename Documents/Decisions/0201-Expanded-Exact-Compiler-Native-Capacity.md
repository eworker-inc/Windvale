# Decision 0201: Expanded exact-compiler native capacity

- Date: 2026-08-04
- Status: Accepted and implemented locally; coherent-batch and cross-host qualification pending
- Supersedes the local capacity candidate in: [Decision 0184](0184-Language-Syntax-And-Operator-Evolution.md)
- Extends: [Decision 0160](0160-Bounded-Large-Native-Object-And-Link-Admission.md) and [Decision 0163](0163-Bounded-Hosted-Compiler-Runtime-Data.md)
- Retains: WVO 1.0, `flat-x86-64-large-v1`, native ABI 22, execution-context format 7, service-table format 5, hosted-container format 3, checked arithmetic, explicit admission profiles, and narrow version-1 container limits

## Context

The coherent language-evolution batch expands the exact Windvale compiler beyond the earlier local checkpoints. The current 859,555-byte compiler contains 397 functions and 707,044 aggregate code bytes. Native lowering selects 26,299,864 bytes: 26,298,864 text bytes and 1,000 read-only-data bytes, with 500 symbols and 199 relocations. Its canonical WVO is 26,320,498 bytes. These measured products no longer fit Decision 0160's 20 MiB large-native object and image bounds.

Exact native Stage 1 reproduction also reaches 104,885,093 dynamic-value bytes. It therefore exceeds Decision 0184's intermediate 80 MiB candidate even though descriptor-return checkpointing, frame-owned record storage, and allocator emission keep the largest projected native frame below the retained 2,048-cell limit. The current compiler workload also requires a higher instruction bound than the earlier 24,000,000,000 hosted contract.

The repository remains in active early development. Under Decision 0184's policy through at least September 3, 2026, these experimental admission limits may advance with the implementation without retaining readers, shims, or aliases for the superseded candidate bounds.

## Decision

- Keep standard WVO and flat-link admission unchanged at 4 MiB.
- Advance explicit `Largeˉnative` WVO encoded/object-memory admission, aggregate encoded link input, and linked-image admission from 20 MiB to 32 MiB. Keep the existing WVO 1.0 encoding and `flat-x86-64-large-v1` target identity because the serialized structure and layout semantics do not change.
- Advance the ordinary host executor and version-2/3 hosted-container dynamic text/byte arena from 80 MiB to 128 MiB. Keep narrow version-1 containers at 16 MiB.
- Advance the exact hosted compiler instruction ceiling to 48,000,000,000 in `WVHA 1`, its initial execution context, bootstrap scripts, and direct native reproduction tests. Keep call depth 1,024.
- Preserve the 32-bit arena capacity field, ABI 22 and its current service identities, context format 7, service-table format 5, the 2 MiB record arena, all individual text/byte/file limits, checked arena exhaustion, and the fixed 512 MiB hosted RW/NX virtual-extent ceiling.
- Pin the resulting WVB, WVO, native image, map, service bundles, metadata, runtime headers, PE, and ELF identities in focused deterministic evidence. Do not claim cross-host qualification until independent Windows and digest-pinned Debian jobs reproduce them.

## Local evidence

The exact compiler WVB is 859,555 bytes with SHA-256 `c08f76e998e0280b7c2e3e801a9752f000825c874abeb86e88420c31444d63f9`. Native selection produces 26,299,864 bytes with SHA-256 `8da929b94279189fcd7c58dc83bdaac24ce32af91c2450952d1c26a63d60e72c`; the 26,320,498-byte WVO has SHA-256 `76e24a25b4d18c8526dcf05916867512bec9f0b2f60ca0c9de3d30504ab30eb1`; and the canonical map has SHA-256 `008025a2bb4a40c034cd64dfdba24b1a55dc72fb4d89df5a291fa9cf577578de`.

Native Stage 1 returns zero, consumes no record-arena bytes, peaks at 104,885,093 of 134,217,728 dynamic-arena bytes, emits `source wvb status=Valid functions=397 code-bytes=707044 module-bytes=859555`, and reproduces the exact Stage 0 compiler bytes under the 48,000,000,000 ceiling. Record-storage projection reaches 1,904 frame cells; descriptor allocator emission reaches 1,907, both below the 2,048-cell ABI limit.

The Windows and Linux service bundles are 26,316,131 and 26,315,847 bytes. The initial runtime plans move only regions after the dynamic arena and end at 476,135,424 Windows bytes and 474,038,272 Linux bytes, below 512 MiB. The deterministic applications are 26,329,600-byte PE and 26,329,088-byte ELF containers. Focused WVB generation, native lowering, explicit WVO/link admission, hosted metadata/runtime verification, application construction, malformed-input mutation, and direct Windows Stage 2 reproduction pass locally. The final change-aware gate also passes all 92 affected Seed tests. Independent Linux execution and dual-host identity remain pending.

## Consequences

The complete evolved compiler remains one measured object and one shared native image. No size-only compiler partition, parallel linker, alternate object format, or compatibility reader is introduced. The larger bounds are explicit ceilings rather than expected steady-state committed memory.

The 32 MiB profile leaves 7,233,934 encoded-object bytes and 7,254,568 linked-image bytes of headroom. The 128 MiB dynamic arena leaves 29,332,635 bytes above the measured peak. These are bounded development margins, not stability guarantees.

Historical Decision 0160 and Decision 0163 evidence remains valid for the exact artifacts it qualified. It does not describe the current working-tree compiler, and the current local evidence does not replace that historical cross-host qualification claim.

## Reconsider when

- the exact compiler approaches 32 MiB encoded WVO or linked image;
- the measured dynamic arena approaches 128 MiB;
- a smaller segmented or reclaiming allocator can preserve observable semantics with independently verified bounds;
- independently useful compiler modules justify splitting for ownership or deployment rather than size alone;
- 48,000,000,000 instructions is insufficient or operationally unacceptable on a qualification host; or
- a named release policy introduces a compatibility obligation for admission-profile limits or container metadata.
