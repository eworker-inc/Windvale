# Decision 0103: Second exact WVB and broader scalar control flow

- Status: Qualified
- Date: 2026-08-02
- Implements: the next bounded Phase 12 extension after the first same-module proof
- Extends: [Decision 0101](0101-First-Exact-Wvb-Across-Three-Environments.md)
- Contracts: WVB admission 4, interpreter profile 6, kernel memory 9, kernel paging 4, protected process 11, native ABI 17/context 7/service table 5, and firmware Probe 32

## Context

Probe 31 proves that the exact canonical `Sum-Data.wv` WVB executes on Windows, Linux, and in two protected Windvale OS client generations. Its deliberately narrow two-function profile cannot yet tell us whether the interpreter model scales coherently to another ordinary compiler output. The next useful pressure source is the existing cross-compiler differential fixture [`Tests/Fixtures/Source-Wvb/Function-Only.wv`](../../Tests/Fixtures/Source-Wvb/Function-Only.wv), not a new OS-specific demonstration.

That fixture adds four functions, `bool`, `u8`, and `u32` values, forward branches, equality and comparison, multiple call sites, and local mutation while remaining capability-free and deterministic. Stage 0 and the Windvale-written compiler already produce the same canonical bytes for it. Using those bytes in the OS tests a broader real compiler output without changing language semantics or inventing a second module identity.

## Decision

- Replace Probe 31's admitted `Sum-Data.wv` resource with the exact 815-byte canonical WVB compiled from `Function-Only.wv`, SHA-256 `9ccfed0509e84bfc63979c6dc13170c14762efbdaa448b4c5894325f31aa7761`.
- Admit portable module `Sourceˉwvbˉfixture`, no capabilities, nominal types, or data, with functions `Add`, `Main`, `Probe`, and `Select`, and exactly exported `Main() -> i32`. It executes 199 guest instructions and returns `6`.
- Advance the interpreter to profile 6. In addition to the retained integer/local/call/control-flow operations, accept `bool.const`, `bool.not`, `u8.const`, `u8.equal`, `u32.const`, `u32.add`, `u32.less`, and `i32.greater_equal`.
- Replace profile 5's fixed branch-target list with bounded instruction decoding that records valid opcode boundaries and rejects any branch target that is outside a function or does not begin an instruction. Exact function indices and typed operands remain checked before execution.
- Keep execution specialized to the admitted module shape. The interpreter has one bounded executor per admitted function and does not claim arbitrary function counts, signatures, recursion, or dynamic dispatch.
- Compute the client native stack bound from the verified WVO call graph before image construction. Sum the generated frame for every reachable call edge, return addresses, and the entry shim's saved `r15`. Reject recursion in this bounded OS profile. The exact maximum is 58,800 bytes, so the smallest whole-page envelope is 15 pages (61,440 bytes). A 13-page Probe-31 stack faults in QEMU, and 14 pages cannot cover the computed bound.
- Keep ABI 17's 2,048-cell frame limit unchanged. The largest generated function remains below it at 1,900 cells. This slice therefore needs no compiler contract or implementation change.
- Expand the execution-scoped record arena inside the existing client data page from 256 to 1,024 bytes. The successful path creates eleven 48-byte immutable result records and uses exactly 528 bytes. A measured 512-byte arena reaches 480 bytes and fails on the next 48-byte allocation; 1,024 is the next simple bounded capacity and adds no page.
- Advance protected processes to `WVPROC11`. Each generation owns 141 RX code pages, 15 RW/NX stack pages, one RW/NX data/context page, and four private page-table pages: 161 pages total. The two resource aliases remain later virtual mappings backed by init-owned pages. Native instruction/call budgets become `189,114/5`; the guest budget becomes exactly `199`.
- Advance memory to `WVKMEM09` with 182 pages and 177 initially free allocator pages. Retain the four-page kernel stack, exact LIFO tail release, generation-stamped same-root reconstruction, and full-width page-count comparisons. Paging remains version 4 because the 768 KiB supervisor executable window and its permission topology remain sufficient.
- Advance admission to version 4 and firmware to Probe 32. Preserve the typed two-resource grant, cleanup, root reuse, IPC, contained user fault, terminal kernel faults, retained native WVB probe, and compiler-generated system-profile Main.
- Keep C# Stage 0 responsible for raw memory/page-table construction, x86-64 emission, linking, and UEFI packaging. Windvale owns exact admission, process policy, init selection, and bytecode interpretation; WVA owns syscall and privileged leaves.

## Required evidence

- Stage 0 and Windvale-written compiler byte equality for the canonical fixture.
- Exact admission plus rejection of changed magic, section shape, truncation, and a semantically valid result-changing code mutation.
- Interpreter success at 199 guest instructions, exhaustion at 198, malformed function/opcode/branch rejection, missing or malformed resource rejection, and deterministic artifacts.
- Independent native call-graph stack analysis, recursion rejection, the exact 58,800-byte maximum, and proof that 15 pages are the minimal whole-page envelope.
- Exact 1,024-byte arena initialization and 528-byte final use in both generations; init remains without an arena.
- All 25 focused OS tests and all four pinned-QEMU scenarios on Windows.
- Complete Windows and digest-pinned Debian qualification for the exact committed candidate before this decision becomes Qualified.

## Qualification evidence

The local changed-files gate passes all 67 Seed tests with a zero-warning Release build, and the focused Windows OS suite passes all 25 tests. All four local Windows pinned-QEMU scenarios pass with complete Probe-32 serial evidence:

| Scenario | EFI bytes | SHA-256 | Host code |
| --- | ---: | --- | ---: |
| Normal | 714,752 | `a2c69181b55178b0e23c9b1012239a8fb1c8a53e2ffb089c8c59f45fa9dd0a6a` | 0 |
| Invalid opcode | 714,752 | `04d801d25c0b5876fccb796d14b0c1ba14123f468200d5fb28728033867a5df5` | 3 |
| General protection | 714,752 | `b5abc555c11d3585b73e0d3e5391bbf33ca864933d8f0514265fce355273f799` | 3 |
| Contained user fault | 715,264 | `0233c10f323aaa3ee30dbe710b11c05f4d85a08fd88d6a52705c02f4b271347b` | 0 |

Current deterministic implementation identities are:

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Canonical `Function-Only.wv` WVB | 815 | `9ccfed0509e84bfc63979c6dc13170c14762efbdaa448b4c5894325f31aa7761` |
| Admission WVB | 4,068 | `f8f92352abed3c042c6ca6e5cbfd65b650a87837dd252802014b3a787cdb75cf` |
| Interpreter WVB | 56,165 | `3669e94d712bd5a78f0061e29d8054ed3b54b687efc9508114f79bb78aa8832f` |
| Interpreter WVO | 577,140 | `b55f9525cccab5fc2efbf5b4c488b2498a7689d4905d7e5e3d0950a791b00a85` |
| Linked normal client | 576,541 | `afec9522862a6a69656c1a4a93f62d3e7b1b5b0f0d7c8759180410beb3429260` |
| Normal process-machine WVO | 608,198 | `c2e393fc5fa5c348be34aa7aaa239646ea8278616b8459b24fd3677f9f928d13` |
| Fault process-machine WVO | 608,278 | `4054d3884eb8d45a1c7cda56132ae71c70e125a51183c5199c39de77cf1687a6` |

Exact implementation commit `da938979ae9fe59e5f752bdb81359ded58a0e6ac` passes GitHub [Verify run 30758910402](https://github.com/eworker-inc/Windvale/actions/runs/30758910402). Windows and digest-pinned Debian 12 each pass all 67 Seed tests, all 25 OS tests, and the complete native CLI qualification gate. Seed elapsed time is 251.909 seconds on Windows and 200.120 seconds on Debian. QEMU execution remains Windows-only evidence.

## Consequences

The OS now interprets a second existing compiler fixture rather than a hand-shaped demonstration. The profile covers the common scalar and control-flow shapes needed by its four functions, and the verifier derives branch boundaries instead of carrying a fixture-specific target table.

The slice also replaces guessed stack sizing with a build-time proof from verified native structure. The large 577,140-byte AOT interpreter and 58,800-byte call-graph stack are useful pressure signals, not desirable steady-state costs. They strengthen the case for a more compact baseline execution engine later without requiring a premature JIT or compiler ABI increase now.

## Deliberate non-claims

This decision does not add arbitrary WVB loading, a general semantic verifier, arbitrary function signatures or counts, recursion, complete opcode/type coverage, dynamic allocation, text or floating-point execution, a public namespace, independent resource lifetimes, scheduling, executable publication, JIT, general physical-memory management, SMP, Hyper-V, physical-hardware evidence, or .NET retirement.

## Reconsideration triggers

Reconsider this boundary when:

- a third real program requires a genuinely new semantic family rather than another exact specialization;
- repeated specialized function executors become harder to verify than a bounded general dispatch model;
- native size or stack cost justifies a compact baseline interpreter or JIT publication experiment;
- an independently lived resource requires non-tail reclamation; or
- a third runnable creates measurable scheduling pressure.
