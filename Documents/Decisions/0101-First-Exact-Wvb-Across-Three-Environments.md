# Decision 0101: First exact WVB across three environments

- Status: Qualified
- Date: 2026-08-02
- Implements: Phase 12 of [Decision 0084](0084-Minimal-Capability-Oriented-Windvale-Os-Architecture.md)
- Extends: [Decision 0093](0093-First-User-Space-Windvale-Bytecode-Interpreter.md), [Decision 0098](0098-First-Typed-Two-Resource-Lookup.md), and [Decision 0100](0100-First-Reclaimed-And-Reused-Process-Root.md)
- Contracts: WVB admission 3, interpreter profile 5, kernel memory 8, kernel paging 4, protected process 10, native ABI 17/context 7/service table 5, and firmware probe 31

## Context

Probe 30 interprets one exact four-opcode module in a protected Windvale OS process, but that program is deliberately too small to exercise ordinary compiled structure. It has no data section, internal calls, loop, or control-flow branch. The host runtime and native backend already execute the canonical `Examples/Seed/Sum-Data.wv`, so using that same compiler-produced WVB inside the OS is the smallest honest proof that one portable program identity survives three environments:

1. the reference/runtime path on Windows;
2. the reference/runtime path on Linux; and
3. the Windvale-written interpreter running at CPL3 under Windvale OS.

The slice must not turn an evidence program into an implicit general loader or general WVB interpreter. It must also preserve the existing compiler's bounded ABI-17 frame contract unless measured compiler pressure proves that contract wrong.

## Decision

- Replace the private 174-byte embedded fixture with the exact canonical WVB compiled from `Examples/Seed/Sum-Data.wv`. The resource identity is 493 bytes with SHA-256 `6f3a272d37dd8893995c7f85c236414ed2864bf59de2f3775c08afd426013f8c`.
- Admit module `Sumˉdata`, portable profile, immutable `Values = [3, 5, 8, 13]`, functions `Add` and `Main`, one exported `Main() -> i32`, no capabilities or nominal types, and the exact seven canonical sections. The program executes 203 guest instructions and returns `29`.
- Advance the Windvale interpreter to runtime profile 5. It validates the complete bounded envelope and the exact module shape, then interprets `i32.const`, `local.load`, `local.store`, `data.length`, `data.load.i32`, `i32.add`, `i32.less`, `jump`, `branch.false`, `call`, and `return`.
- Keep the two typed runtime resources and atomic grant unchanged in shape. Change the supplied little-endian guest budget from `4` to exact value `203`, with maximum accepted value `256`. Charge once immediately before every guest opcode.
- Split guest execution into bounded Windvale helpers for `Add` and `Main`. The largest generated native frame is 1,883 slots, below ABI 17's existing 2,048-slot limit. The compiler contract therefore remains unchanged.
- Advance protected processes to `WVPROC10`. Each client generation receives 98 RX code pages, 13 RW/NX stack pages, one RW/NX context/data page, and two later RO/NX aliases. Its owned extent is 116 pages, native instruction budget is exactly 93,181, and maximum dynamic native call depth is 4.
- Give each interpreter generation a 256-byte execution-scoped record arena at context/data offset `0x200`. The arena occupies otherwise unused bytes in the existing RW/NX data page, begins empty, and must consume exactly 240 bytes for the five immutable execution-result records created by this program. Init receives no record arena. This changes no compiler ABI, page count, capability, or syscall.
- Advance kernel memory to `WVKMEM08` with a 137-page arena and 132 initially free pages. Retain the four-page kernel stack, exact tail release, same-root generation reuse, and all prior generation checks. Use full-width comparisons where the new page counts no longer fit signed imm8 encodings.
- Advance paging to `WVKPAG04` and expand the fixed supervisor RX window from 256 KiB to 768 KiB. Permissions, null guard, NX, `CR0.WP`, two code-table topology, and the absence of a public mapping API remain unchanged.
- Advance firmware to Probe 31. Retain all process, typed-resource, cleanup, process-reuse, IPC, fault-containment, native-WVB, and compiler-generated Main evidence from Probe 30.
- Keep the canonical program's AOT WVO as deterministic differential evidence, but do not link that native derivative into the user process. The OS execution path receives and interprets the same WVB bytes.
- Keep C# responsible for the current Stage 0 compiler invocation, independent admission/process/paging/memory oracles, raw x86-64 construction, linking, and UEFI packaging. Windvale owns admission, process policy, init selection, and bytecode interpretation; WVA owns syscall and privileged leaves.

## Required evidence

- Byte-identical canonical WVB reconstruction and exact AOT artifact identities.
- Reference execution of the canonical WVB to `29`, and a structurally valid one-byte data mutation to `28`.
- Interpreter success at exactly 203 guest instructions, exhaustion at 202, invalid-budget bounds, missing/malformed resource rejection, malformed-section/opcode coverage, and deterministic artifact repetition.
- Exact enforcement of the 1,883-slot interpreter frame without changing the compiler's 2,048-slot ABI-17 limit.
- Exact client record-arena pointer/capacity initialization and 240-byte post-execution use, with the init fields remaining zero.
- All 25 focused OS tests and all four pinned-QEMU scenarios on Windows.
- The complete Windows/Debian qualification gate for the exact committed candidate.

The focused Windows OS suite passes all 25 tests. Current deterministic qualified identities include:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Canonical `Sum-Data.wv` WVB | 493 | `6f3a272d37dd8893995c7f85c236414ed2864bf59de2f3775c08afd426013f8c` |
| Admission WVB | 3,424 | `fde22db922a283c11c56b6802587398172e0b03e7580d99e91fcf95e189f8629` |
| Interpreter WVB | 38,567 | `84c89011535f1d08febd6f41c6af1e2a0b933f6b20f41fbdd8a7a267f568d8a1` |
| Interpreter WVO | 398,000 | `9e6df332ded8ab1483811493ae2997c27a02a76452a3d0151cc17064b4f1dfcc` |
| Linked normal client | 397,741 | `f01dca52f965afc679bef80988a7fc62c1f413d26127c47e437dc81a5cc05f6f` |
| Normal process-machine WVO | 425,652 | `f65d889036c12415d7f1e9a9aa29f0e0cba371f51e7494f0e8c49fa86df5e28a` |
| Fault process-machine WVO | 425,732 | `1e59821fdb167b79035b54323c95edd9c0fe0865e5a6b16e84126876e1cf73d7` |

All four local Windows pinned-QEMU scenarios pass with exact serial evidence and expected host codes:

| Scenario | EFI bytes | SHA-256 | Host code |
| --- | ---: | --- | ---: |
| Normal | 531,456 | `30acb028e44b6d12bc4d0e4d34232d86a43b83b40f070d3a48b7c56e505bc0bc` | 0 |
| Invalid opcode | 531,456 | `dec5a39be132a3e6f140425547097b450a38a7b62e6ac3fa3d20f7d3c457587b` | 3 |
| General protection | 531,456 | `f0e9daacfa479945afec952692f69e3911b285e478f9ae8d12a3e14f0c091960` | 3 |
| Contained user fault | 531,968 | `795cb85aa599d2ead4e228bd0eb3da5ad28ecd8970955b294ab09f72c3f7ade7` | 0 |

Exact implementation commit `f3eca7c8dab290e3916fbf33dcabc41d685a91bb` passes GitHub [Verify run 30753663882](https://github.com/eworker-inc/Windvale/actions/runs/30753663882). Windows and digest-pinned Debian 12 each pass all 67 Seed tests, all 25 OS tests, and the complete native CLI qualification gate. The Seed suites take 210.128 seconds on Windows and 210.185 seconds on Debian; both logs emit the same 56 SHA-256 artifact identities in the same order, beginning with the canonical `Sum-Data.wv` WVB. QEMU execution remains Windows-only evidence.

## Consequences

Windvale now has one nontrivial portable WVB identity that can be compared directly across both permanent hosts and the OS. Its loop, immutable data, internal call, local state, branch, and return are enough to expose real section decoding and execution pressure without claiming arbitrary modules.

The code-size and stack growth are explicit. The interpreter's 398,000-byte native object is not a desirable steady-state implementation; it is evidence for future baseline-interpreter and native-backend work. The existing compiler frame cap proved sufficient once the interpreter was decomposed along semantic boundaries.

## Deliberate non-claims

This decision does not add a general WVB verifier, arbitrary modules/functions/data, recursion, dynamic allocation, floating point, text execution, complete opcode coverage, a loader namespace, executable publication, JIT, caching, a scheduler, a general allocator, SMP, Hyper-V evidence, physical-hardware evidence, or .NET retirement. Windows and Linux still execute through the Stage 0 host stack; the OS still boots and packages through Stage 0.

## Reconsideration triggers

Reconsider this boundary when:

- a second real program requires another opcode, type, data family, function shape, or dynamic section count;
- the interpreter's generated size or stack pressure blocks another bounded program;
- a third runnable creates measured scheduling pressure;
- an independently lived resource requires non-tail reclamation; or
- executable publication becomes necessary for a baseline JIT.
