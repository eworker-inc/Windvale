# Protected Windvale processes and typed resource sets

## Status and purpose

Protected-process contract version 11 is the qualified Probe-32 contract. It retains Probe 31's generation-safe reclaim/rebuild cycle while expanding the client for interpreter profile 6. [Decision 0103](../Documents/Decisions/0103-Second-Exact-Wvb-And-Broader-Scalar-Control-Flow.md) owns version 11; exact implementation commit `da938979ae9fe59e5f752bdb81359ded58a0e6ac` passes complete Windows/Debian qualification in GitHub [Verify run 30758910402](https://github.com/eworker-inc/Windvale/actions/runs/30758910402). [Decision 0101](../Documents/Decisions/0101-First-Exact-Wvb-Across-Three-Environments.md) retains the qualified version-10 history.

This is an internal experiment, not a stable syscall ABI, general process manager, dynamic namespace, transferable capability system, arbitrary WVB loader, complete verifier, or JIT.

## Ownership split

- `Process-Foundation.wv` binds init, interpreter, program, budget, roles, ordered grants, two generations, exact reuse, cleanup, and result policy.
- `Init-Resource-Service.wv` selects ordered identifiers `(1,2)`, grants and receives twice, then exits.
- `Bytecode-Interpreter.wv` reads both exact names, validates runtime profile 6, charges the guest budget, and interprets the program.
- `Boot-Resource-Service.wva` owns exact typed lookup for the two `WVBR002` entries.
- Stage 0 temporarily owns raw page-table writes, records, publication, dispatch, coordination, and firmware packaging, with independent checked planners.

## Fixed identities, roles, and budgets

| Identity | SHA-256 | Authority |
| --- | --- | --- |
| Init/resource-service WVB | `0554d80340440bf8895f0bf066d355da83337791f5404f2b72ca6da214664467` | receive plus fixed set grant |
| Bytecode-interpreter WVB | `3669e94d712bd5a78f0061e29d8054ed3b54b687efc9508114f79bb78aa8832f` | send; two reads after grant |
| `boot:main.wvb` | `9ccfed0509e84bfc63979c6dc13170c14762efbdaa448b4c5894325f31aa7761` | resource 1, WVB, immutable RO/NX |
| `boot:main.budget` | `add7f2a4843f8c512c0e2875546581db11b9ba227ee008b5f719dfacb125de76` | resource 2, four-byte LE value 199, immutable RO/NX |

Init is process/thread `1/1`, generation 1, process reference `65537`, runtime profile 1, instruction/call budgets `64/1`, five user pages, one handle, and five syscalls.

The client is process/thread `2/2`, generation 1 then 2, references `65538` then `131074`, runtime profile 6, native instruction/call budgets `189,114/5`, generated maximum frame 1,900 slots, exact call-graph stack use 58,800 bytes, 157 pre-grant and 159 post-grant user pages, one handle, and two syscalls per generation. The separate guest execution budget is `199` with maximum `256`.

Both retain result `6`, capability slot 0/generation 1, channel capacity 1, and ABI 17/context 7/service-table 5. Process policy must return token `97` before machine state is published.

## Address spaces

Init receives nine physical pages: four table pages, one RX image page, one RW/NX stack page, one RW/NX data/context page, and two owned RO/NX resource pages.

Each client generation receives this reclaimed 161-page physical extent plus two later aliases:

| Relative page | Purpose | Before grant | After grant |
| ---: | --- | --- | --- |
| `0..3` | private paging hierarchy | none | none |
| `4..144` | 141-page interpreter image | user RX | user RX |
| `145..159` | 15-page stack | user RW/NX | user RW/NX |
| `160` | ABI-17 context/data | user RW/NX | user RW/NX |
| `161` | module alias | absent | init WVB page, user RO/NX |
| `162` | budget alias | absent | init budget page, user RO/NX |

No placeholder backs an alias. Generation-1 cleanup clears both aliases and publications; the complete 161-page extent is zeroed and released. Generation 2 reconstructs every table, image, stack, data, context, and record byte at the same physical root with a different logical identity.

## `WVPROC11` records

The state page stores two 264-byte little-endian records at offsets `0x100` and `0x300`. Version 11 preserves version 10's field offsets and generation field while binding the new measured values:

- magic/version `WVPROC11` and `11`;
- user-page budgets init `5`, client `159`;
- client native instruction budget `189,114`, call depth `5`, and 141 RX code pages;
- runtime profiles init `1`, client `6`;
- process generation init/first client `1`, rebuilt client `2`;
- exact canonical program digest at offset `0xD8`.

Both context pages retain valid context-7 headers under ABI 17. Init's record-arena fields remain zero. Each rebuilt client begins with runtime service/resource pointers zero and a 1,024-byte record arena at data offset `0x200`, with used length zero. Grant publishes service table 5 at data offset `0x80`, `WVBR002` at `0x100`, and both resource pointers atomically while preserving the arena fields. The machine requires exactly 528 arena bytes used after successful interpretation; cleanup and generation-2 reconstruction restore used length zero.

## Native stack preflight

The builder decodes the verified interpreter WVO before process construction. Starting at the client entry export, it computes the maximum reachable stack use from generated frame sizes and exact call edges, adding return addresses and the entry shim's saved `r15`. A recursive edge is rejected for this bounded profile. The exact maximum is 58,800 bytes. Fifteen pages provide 61,440 bytes and are the minimal whole-page envelope; fourteen pages provide only 57,344 bytes.

## Typed resources and publication

The two retained 128-byte `WVRES004` records track fixed identifiers/kinds, generation-stamped owner/borrower references, source and target addresses, exact lengths, immutable RO/NX flags, SHA-256 identity, historical grant count, live mapping count, and exact target PTE. Resource 1 length is 815; resource 2 remains four bytes.

`WVBR002` remains exactly 80 bytes: a 16-byte header and two ordered 32-byte entries. Entry zero is `(1, wvb-module)` and entry one is `(2, u32-execution-budget)`. Unknown, duplicate, reversed, or partial tokens fail before mutation. The WVA leaf accepts only `boot:main.wvb` and `boot:main.budget` and validates the selected typed entry before returning a descriptor.

## Grant, execution, cleanup, and reuse

1. Init returns resource-set token `131073`; syscall 4 validates both owned records, pages, digests, absent PTEs, service leaf, and token.
2. The kernel installs both RO/NX aliases and publishes service table 5 plus `WVBR002` atomically.
3. Client generation 1 interprets the exact 815-byte program for 199 guest instructions, sends `6`, then exits or takes the contained fault.
4. Cleanup validates generation 1, clears aliases and publication, and preserves grant count 1.
5. Init's CR3 reload retires non-global translations. The exact 161-page tail is zeroed, released, immediately reallocated at the same root, and rebuilt as generation 2.
6. Init receives `6`, grants again, and blocks. The second grant records borrower `131074` and grant count 2.
7. Generation 2 independently repeats interpretation and sends `6`; generation-matched cleanup clears the second borrow.
8. Init receives the second result and exits. The allocator again ends exactly exhausted at cursor page 182.

The contained user-fault scenario sends `6` then executes privileged `CLI`; cleanup still completes. CPL0 invalid-opcode and general-protection scenarios remain terminal.

## Deterministic qualified artifacts

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Process-policy WVB | 6,553 | `10cb84b665c7cc40832ca2ba642babda433a091f2cbf3a4e3dc624baabad036d` |
| Process-policy WVO | 58,648 | `177618b72d0a1e9b4bba0b008f7c5a5f0954c6a37d88b7ce33c6ba613613ac86` |
| Init WVB | 525 | `0554d80340440bf8895f0bf066d355da83337791f5404f2b72ca6da214664467` |
| Interpreter WVB | 56,165 | `3669e94d712bd5a78f0061e29d8054ed3b54b687efc9508114f79bb78aa8832f` |
| Interpreter WVO | 577,140 | `b55f9525cccab5fc2efbf5b4c488b2498a7689d4905d7e5e3d0950a791b00a85` |
| Linked normal client | 576,541 | `afec9522862a6a69656c1a4a93f62d3e7b1b5b0f0d7c8759180410beb3429260` |
| Linked fault client | 576,541 | `49c9afe4ddb29967ea5a19e1fdadbe1f352a283e9dcb7738c67a09da9558466a` |
| Normal process-machine WVO | 608,198 | `c2e393fc5fa5c348be34aa7aaa239646ea8278616b8459b24fd3677f9f928d13` |
| Fault process-machine WVO | 608,278 | `4054d3884eb8d45a1c7cda56132ae71c70e125a51183c5199c39de77cf1687a6` |

Windows and digest-pinned Debian 12 each pass all 67 Seed tests and all 25 OS tests. All four Windows pinned-QEMU scenarios pass after the `WVPROC11` correction; no Debian QEMU execution is claimed.

## Deliberate limits

Version 11 retains two fixed names, one owner, one logical borrower, two generations, two ordered grants, and one exact LIFO reuse. It adds no enumeration, arbitrary resources, transfer/delegation, independent lifetimes, non-tail release, concurrent root reuse, SMP shootdown, general process creation, scheduling, arbitrary loading, executable publication, JIT, filesystems, packages, networking, Hyper-V, or physical-hardware evidence.
