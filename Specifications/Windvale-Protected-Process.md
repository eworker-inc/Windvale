# Protected Windvale processes and typed resource sets

## Status and purpose

Protected-process contract version 10 is the implemented Probe-31 candidate. It retains Probe 30's generation-safe reclaim/rebuild cycle and expands the client only as required by the exact `Sum-Data.wv` interpreter profile. [Decision 0101](../Documents/Decisions/0101-First-Exact-Wvb-Across-Three-Environments.md) owns version 10; [Decision 0100](../Documents/Decisions/0100-First-Reclaimed-And-Reused-Process-Root.md) retains the qualified version-9 history.

This is an internal experiment, not a stable syscall ABI, general process manager, dynamic namespace, transferable capability system, arbitrary WVB loader, complete verifier, or JIT.

## Ownership split

- `Process-Foundation.wv` binds init, interpreter, program, budget, roles, ordered grants, two generations, exact reuse, cleanup, and result policy.
- `Init-Resource-Service.wv` selects ordered identifiers `(1,2)`, grants and receives twice, then exits.
- `Bytecode-Interpreter.wv` reads both exact names, validates runtime profile 5, charges the guest budget, and interprets the program.
- `Boot-Resource-Service.wva` owns exact typed lookup for the two `WVBR002` entries.
- Stage 0 temporarily owns raw page-table writes, records, publication, dispatch, coordination, and firmware packaging, with independent checked planners.

## Fixed identities, roles, and budgets

| Identity | SHA-256 | Authority |
| --- | --- | --- |
| Init/resource-service WVB | `0554d80340440bf8895f0bf066d355da83337791f5404f2b72ca6da214664467` | receive plus fixed set grant |
| Bytecode-interpreter WVB | `84c89011535f1d08febd6f41c6af1e2a0b933f6b20f41fbdd8a7a267f568d8a1` | send; two reads after grant |
| `boot:main.wvb` | `6f3a272d37dd8893995c7f85c236414ed2864bf59de2f3775c08afd426013f8c` | resource 1, WVB, immutable RO/NX |
| `boot:main.budget` | `3d0aa5ecdccdbdc20bc652773c47cfdba0a470ddee1e27fbdcb46a19cfe21897` | resource 2, four-byte LE value 203, immutable RO/NX |

Init is process/thread `1/1`, generation 1, process reference `65537`, runtime profile 1, instruction/call budgets `64/1`, five user pages, one handle, and five syscalls.

The client is process/thread `2/2`, generation 1 then 2, references `65538` then `131074`, runtime profile 5, native instruction/call budgets `93,181/4`, generated main-frame limit 1,883, 112 pre-grant and 114 post-grant user pages, one handle, and two syscalls per generation. The separate guest execution budget is `203` with maximum `256`.

Both retain result `29`, capability slot 0/generation 1, channel capacity 1, and ABI 17/context 7/service-table 5. Process policy must return token `97` before machine state is published.

## Address spaces

Init receives nine physical pages: four table pages, one RX image page, one RW/NX stack page, one RW/NX data/context page, and two owned RO/NX resource pages.

Each client generation receives this reclaimed 116-page physical extent plus two later aliases:

| Relative page | Purpose | Before grant | After grant |
| ---: | --- | --- | --- |
| `0..3` | private paging hierarchy | none | none |
| `4..101` | 98-page interpreter image | user RX | user RX |
| `102..114` | 13-page stack | user RW/NX | user RW/NX |
| `115` | ABI-17 context/data | user RW/NX | user RW/NX |
| `116` | module alias | absent | init WVB page, user RO/NX |
| `117` | budget alias | absent | init budget page, user RO/NX |

No placeholder backs an alias. Generation-1 cleanup clears both aliases/publications; the complete 116-page extent is zeroed and released. Generation 2 reconstructs every table, image, stack, data, context, and record byte at the same physical root with a different logical identity.

## `WVPROC10` records

The state page stores two 264-byte little-endian records at offsets `0x100` and `0x300`. Version 10 preserves version 9's field offsets and generation field while changing the bound values:

- magic/version `WVPROC10` and `10`;
- user-page budgets init `5`, client `114`;
- client native instruction budget `93,181`, call depth `4`, and 98 RX code pages;
- runtime profiles init `1`, client `5`;
- process generation init/first client `1`, rebuilt client `2`;
- exact canonical program digest at offset `0xD8`.

Both context pages retain valid context-7 headers under ABI 17. Init's record-arena fields remain zero. Each rebuilt client begins with runtime service/resource pointers zero and a 256-byte record arena at data offset `0x200`, with used length zero. Grant publishes service table 5 at data offset `0x80`, `WVBR002` at `0x100`, and both resource pointers atomically while preserving the arena fields. The machine requires exactly 240 arena bytes used after each successful interpretation; cleanup and generation-2 reconstruction restore used length zero.

## Typed resources and publication

The two retained 128-byte `WVRES004` records track fixed identifiers/kinds, generation-stamped owner/borrower references, source and target addresses, exact lengths, immutable RO/NX flags, SHA-256 identity, historical grant count, live mapping count, and exact target PTE. Resource 1 length is now 493; resource 2 remains four bytes.

`WVBR002` remains exactly 80 bytes: a 16-byte header and two ordered 32-byte entries. Entry zero is `(1, wvb-module)` and entry one is `(2, u32-execution-budget)`. Unknown, duplicate, reversed, or partial tokens fail before mutation. The WVA leaf accepts only `boot:main.wvb` and `boot:main.budget` and validates the selected typed entry before returning a descriptor.

## Grant, execution, cleanup, and reuse

1. Init returns resource-set token `131073`; syscall 4 validates both owned records, pages, digests, absent PTEs, service leaf, and token.
2. The kernel installs both RO/NX aliases and publishes service table 5 plus `WVBR002` atomically.
3. Client generation 1 interprets the exact 493-byte program for 203 guest instructions, sends `29`, then exits or takes the contained fault.
4. Cleanup validates generation 1, clears aliases and publication, and preserves grant count 1.
5. Init's CR3 reload retires non-global translations. The exact 116-page tail is zeroed, released, immediately reallocated at the same root, and rebuilt as generation 2.
6. Init receives `29`, grants again, and blocks. The second grant records borrower `131074` and grant count 2.
7. Generation 2 independently repeats interpretation and sends `29`; generation-matched cleanup clears the second borrow.
8. Init receives the second result and exits. The allocator again ends exactly exhausted at cursor page 137.

The contained user-fault scenario sends `29` then executes privileged `CLI`; cleanup still completes. CPL0 invalid-opcode and general-protection scenarios remain terminal.

## Deterministic candidate artifacts

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Process-policy WVB | 6,553 | `c811fb5c7ef7a194f238831fd91f6a306084e619cd7572300eab74e2107bdfa2` |
| Init WVB | 525 | `0554d80340440bf8895f0bf066d355da83337791f5404f2b72ca6da214664467` |
| Interpreter WVB | 38,567 | `84c89011535f1d08febd6f41c6af1e2a0b933f6b20f41fbdd8a7a267f568d8a1` |
| Interpreter WVO | 398,000 | `9e6df332ded8ab1483811493ae2997c27a02a76452a3d0151cc17064b4f1dfcc` |
| Linked normal client | 397,741 | `f01dca52f965afc679bef80988a7fc62c1f413d26127c47e437dc81a5cc05f6f` |
| Linked fault client | 397,741 | `9ea4bf727a73636a01b7f47584752475a27d8a6442cf669156645c0b3f2af0d5` |
| Normal process-machine WVO | 425,652 | `f65d889036c12415d7f1e9a9aa29f0e0cba371f51e7494f0e8c49fa86df5e28a` |
| Fault process-machine WVO | 425,732 | `1e59821fdb167b79035b54323c95edd9c0fe0865e5a6b16e84126876e1cf73d7` |

Focused Windows evidence passes all 25 OS tests. All four local Windows pinned-QEMU scenarios pass with exact Probe-31 serial evidence; complete committed Windows/Debian qualification remains pending.

## Deliberate limits

Version 10 retains two fixed names, one owner, one logical borrower, two generations, two ordered grants, and one exact LIFO reuse. It adds no enumeration, arbitrary resources, transfer/delegation, independent lifetimes, non-tail release, concurrent root reuse, SMP shootdown, general process creation, scheduling, arbitrary loading, executable publication, JIT, filesystems, packages, networking, Hyper-V, or physical-hardware evidence.
