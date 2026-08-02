# Init-granted Windvale OS bytecode interpreter

## Status and purpose

Interpreter runtime profile 5 is the qualified Probe-31 contract owned by [Decision 0101](../Documents/Decisions/0101-First-Exact-Wvb-Across-Three-Environments.md). It executes the exact canonical WVB compiled from [`Examples/Seed/Sum-Data.wv`](../Examples/Seed/Sum-Data.wv) inside protected process `2` and returns `29` after data access, a loop, branches, locals, and internal calls.

Exact implementation commit `f3eca7c8dab290e3916fbf33dcabc41d685a91bb` passes all 67 Seed tests and all 25 OS tests on Windows and digest-pinned Debian 12 in GitHub [Verify run 30753663882](https://github.com/eworker-inc/Windvale/actions/runs/30753663882). Profile 4 and its four-opcode Probe-29 contract remain qualified history under [Decision 0098](../Documents/Decisions/0098-First-Typed-Two-Resource-Lookup.md).

The interpreter is hosted Windvale compiled AOT and run at CPL3. The separately mapped program is interpreted; neither the complete WVB nor its AOT derivative is linked into the client executable. The kernel contains no source-language interpreter and no WVB semantic decoder.

## Typed runtime inputs

The interpreter declares only `file.read_bytes` and performs exactly these lookups:

1. `boot:main.wvb` — resource `1`, kind `wvb-module`, exact 493-byte admitted WVB;
2. `boot:main.budget` — resource `2`, kind `u32-execution-budget`, exactly four little-endian bytes containing `203`.

Process `2` starts with both resource PTEs and context pointers absent. Windvale init selects ordered set token `131073`; one checked syscall publishes two distinct user-readable RO/NX aliases, service-table version 5 with only `file.read_bytes`, the exact two-entry `WVBR002` directory, and both context pointers as one atomic transition.

The retained 347-byte WVA leaf accepts only those two names. It validates directory magic, version, size, count, identifier, kind, pointer, exact length, flags, and reserved bytes before returning an ABI-17 borrowed-bytes descriptor. Stage 0 independently reconstructs the leaf and requires byte equality with the WVA stencil.

## Admitted module profile

The sole accepted program has this exact semantic shape:

| Property | Required value |
| --- | --- |
| Module | `Sumˉdata`, portable profile |
| WVB | version 1.6, seven ordered canonical sections, 493 exact bytes |
| Capabilities/types | none |
| Data | immutable `Values = [3, 5, 8, 13]` |
| Functions | `Add(i32, i32) -> i32` and `Main() -> i32` |
| Export | exactly `Main` |
| Result | `29` |
| Guest instructions | exactly `203` |

Profile 5 validates the envelope, section order/lengths, portable module record, empty capability/type sections, data declaration, both function records and code envelopes, export, operand widths, local/data/function indices, and allowed branch targets before execution.

The accepted opcode set is:

- `i32.const`, `local.load`, and `local.store`;
- `data.length` and `data.load.i32`;
- `i32.add` and `i32.less`;
- `jump` and `branch.false`;
- `call` and `return`.

`Executeˉadd` and `Executeˉmain` are separate bounded Windvale helpers. Calls are limited to the exact `Main -> Add` shape; recursion and arbitrary dynamic function dispatch are not admitted.

## Budget and execution contract

The guest budget must be exactly four bytes, decode to a nonzero unsigned value no greater than `256`, and have the exact typed directory entry. One unit is charged immediately before every guest opcode:

- canonical value `203` completes and returns `29`;
- value `202` exhausts before completion and returns interpreter status `-34`;
- zero or `257` is invalid and returns status `-17`;
- a three-byte resource returns status `-16`; and
- a missing budget name remains a contained runtime capability failure.

The complete AOT interpreter path consumes exactly 93,181 native instructions with maximum dynamic call depth 4. `Executeˉmain` uses 1,883 generated frame slots, below ABI 17's existing 2,048-slot ceiling. Guest budget, native instruction budget, native call depth, and generated frame size are separate contracts.

## Process and artifact bounds

Each client generation owns 98 RX code pages, 13 RW/NX stack pages, and one RW/NX context/data page. Its 116-page physical extent also contains four private table pages. Two following virtual pages become resource aliases without client-owned placeholder pages. User-page count moves atomically from 112 to 114.

The context's record-arena pointer names 256 bytes at data-page offset `0x200`; length is `256` and used begins at zero. This is an execution-scoped monotonic arena inside the existing RW/NX data page, not a general allocator. The canonical execution creates five three-field immutable result records and must finish with exactly 240 bytes used. The process machine rejects any other pointer, capacity, or final use. Init's record-arena fields remain zero.

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Canonical program WVB | 493 | `6f3a272d37dd8893995c7f85c236414ed2864bf59de2f3775c08afd426013f8c` |
| Interpreter WVB | 38,567 | `84c89011535f1d08febd6f41c6af1e2a0b933f6b20f41fbdd8a7a267f568d8a1` |
| Interpreter WVO | 398,000 | `9e6df332ded8ab1483811493ae2997c27a02a76452a3d0151cc17064b4f1dfcc` |
| WVA lookup stencil WVO | 462 | `fde44aad9549731d53c5ccf3a57733b3619df94369b61ef27a693e1059784bc9` |
| Published lookup WVO | 462 | `ecb940abb9de8086d50ae418853021cf1f7566a9415a5a3a3b4e5cc45ed5e78c` |
| Linked normal client | 397,741 | `f01dca52f965afc679bef80988a7fc62c1f413d26127c47e437dc81a5cc05f6f` |
| Linked fault client | 397,741 | `9ea4bf727a73636a01b7f47584752475a27d8a6442cf669156645c0b3f2af0d5` |

Focused tests cover exact success and exhaustion, budget bounds and shape, missing resources, changed data producing `28`, malformed envelopes/sections/opcodes, atomic grant, distinct aliases, typed-entry mutations, deterministic output, record-arena initialization/preservation, and equivalent two-generation exit/fault cleanup. Both permanent hosts pass the complete qualification gate; all four pinned-QEMU scenarios pass on Windows, with no Debian QEMU execution claimed.

## Trust boundary and deliberate limits

AOT admission checks exact identity before process construction. Grant revalidates source pages and digests; WVA validates the private typed directory; Windvale profile 5 validates and interprets its semantic subset; terminal cleanup revalidates and clears both aliases and the complete publication.

Profile 5 is not a general resource namespace, filesystem, package loader, complete WVB verifier, general multi-function interpreter, JIT, cache, dynamic runtime selector, or stable runtime ABI. It accepts one measured module shape. Generalization waits for another real program or capability requirement.
