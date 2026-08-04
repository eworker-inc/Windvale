# Init-granted Windvale OS bytecode interpreter

## Status and purpose

Interpreter runtime profile 6 is the qualified Probe-32 contract owned by [Decision 0103](../Documents/Decisions/0103-Second-Exact-Wvb-And-Broader-Scalar-Control-Flow.md). It executes the exact canonical WVB compiled from [`Tests/Fixtures/Source-Wvb/Function-Only.wv`](../Tests/Fixtures/Source-Wvb/Function-Only.wv) inside protected process `2` and returns `6` after four functions exercise scalar types, comparisons, forward control flow, locals, and internal calls. Exact implementation commit `da938979ae9fe59e5f752bdb81359ded58a0e6ac` passes complete Windows/Debian qualification in GitHub [Verify run 30758910402](https://github.com/eworker-inc/Windvale/actions/runs/30758910402).

Profile 5 and the exact `Sum-Data.wv` Probe-31 contract remain qualified under [Decision 0101](../Documents/Decisions/0101-First-Exact-Wvb-Across-Three-Environments.md).

The interpreter is hosted Windvale compiled AOT and run at CPL3. The separately mapped program is interpreted; neither the complete WVB nor its AOT derivative is linked into the client executable. The kernel contains no source-language interpreter and no WVB semantic decoder.

## Typed runtime inputs

The interpreter declares only `file.read_bytes` and performs exactly these lookups:

1. `boot:main.wvb` — resource `1`, kind `wvb-module`, exact 816-byte admitted WVB;
2. `boot:main.budget` — resource `2`, kind `u32-execution-budget`, exactly four little-endian bytes containing `199`.

Process `2` starts with both resource PTEs and context pointers absent. Windvale init selects ordered set token `131073`; one checked syscall publishes two distinct user-readable RO/NX aliases, service-table version 5 with only `file.read_bytes`, the exact two-entry `WVBR002` directory, and both context pointers as one atomic transition.

The retained 347-byte WVA leaf accepts only those two names. It validates directory magic, version, size, count, identifier, kind, pointer, exact length, flags, and reserved bytes before returning an ABI-17 borrowed-bytes descriptor. Stage 0 independently reconstructs the leaf and requires byte equality with the WVA stencil.

## Admitted module profile

The sole accepted program has this exact semantic shape:

| Property | Required value |
| --- | --- |
| Module | `Sourceˉwvbˉfixture`, portable profile |
| WVB | version 1.11, absent module metadata, seven ordered canonical sections, 816 exact bytes |
| Capabilities/types/data | none |
| Functions | `Add`, `Main`, `Probe`, and `Select` |
| Scalar values | `bool`, `u8`, `u32`, and `i32` |
| Export | exactly `Main() -> i32` |
| Result | `6` |
| Guest instructions | exactly `199` |

Profile 6 validates the envelope, section order and lengths, portable module record, empty capability/type/data sections, four function records and code envelopes, export, operand widths, local/function indices, call targets, and branch targets before execution. A bounded decoder records every instruction boundary in each function; a branch target must name one of those boundaries rather than merely fall inside the code range.

The accepted opcode set is:

- `bool.const`, `bool.not`, `u8.const`, and `u8.equal`;
- `u32.const`, `u32.add`, and `u32.less`;
- `i32.const`, `i32.add`, and `i32.greater_equal`;
- `local.load` and `local.store`;
- `jump`, `branch.false`, `call`, and `return`.

`Executeˉadd`, `Executeˉmain`, `Executeˉprobe`, and `Executeˉselect` are separate bounded Windvale helpers. Calls are limited to the exact admitted graph. Recursion and arbitrary dynamic function dispatch are not admitted.

## Budget and execution contract

The guest budget must be exactly four bytes, decode to a nonzero unsigned value no greater than `256`, and have the exact typed directory entry. One unit is charged immediately before every guest opcode:

- canonical value `199` completes and returns `6`;
- value `198` exhausts before completion and returns interpreter status `-34`;
- zero or `257` is invalid and returns status `-17`;
- a three-byte resource returns status `-16`; and
- a missing budget name remains a contained runtime capability failure.

Malformed exact-profile coverage rejects an invalid function record, an invalid opcode, and a branch target that does not begin an instruction. A structurally valid byte mutation at program offset `396` executes to `9` on the ordinary runtime but fails exact admission.

The complete AOT interpreter path consumes exactly 189,137 native instructions with maximum dynamic call depth 5. The largest generated function uses 1,900 frame slots, below ABI 17's unchanged 2,048-slot ceiling. Guest budget, native instruction budget, native call depth, per-function frame size, and whole-call-graph stack are separate contracts.

Before image construction, the process builder derives the exact maximum native stack from verified WVO calls and generated frames. It includes each active frame, return address, and the client entry shim's saved `r15`; recursion is rejected. The exact path needs 58,800 bytes, making 15 pages (61,440 bytes) the minimal whole-page envelope.

## Process and artifact bounds

Each client generation owns 141 RX code pages, 15 RW/NX stack pages, and one RW/NX context/data page. Its 161-page physical extent also contains four private table pages. Two following virtual pages become resource aliases without client-owned placeholder pages. User-page count moves atomically from 157 to 159.

The context's record-arena pointer names 1,024 bytes at data-page offset `0x200`; used begins at zero. This is an execution-scoped monotonic arena inside the existing RW/NX data page, not a general allocator. The canonical execution creates eleven three-field immutable result records and must finish with exactly 528 bytes used. A 512-byte diagnostic configuration exhausted after 480 bytes before the next 48-byte allocation. Init's record-arena fields remain zero.

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Canonical program WVB | 816 | `28d215b982a7b7185cfa80c4cc5346666bd0181582fe80bec8b7035d514da936` |
| Interpreter WVB | 56,165 | `3669e94d712bd5a78f0061e29d8054ed3b54b687efc9508114f79bb78aa8832f` |
| Interpreter WVO | 577,140 | `b55f9525cccab5fc2efbf5b4c488b2498a7689d4905d7e5e3d0950a791b00a85` |
| Linked normal client | 576,541 | `afec9522862a6a69656c1a4a93f62d3e7b1b5b0f0d7c8759180410beb3429260` |
| Linked fault client | 576,541 | `49c9afe4ddb29967ea5a19e1fdadbe1f352a283e9dcb7738c67a09da9558466a` |

Windows and digest-pinned Debian 12 each pass all 67 Seed tests and all 25 OS tests. All four pinned-QEMU scenarios pass on Windows; no Debian QEMU execution is claimed.

## Trust boundary and deliberate limits

AOT admission checks exact identity before process construction. Grant revalidates source pages and digests; WVA validates the private typed directory; Windvale profile 6 validates and interprets its semantic subset; terminal cleanup revalidates and clears both aliases and the complete publication.

Profile 6 is not a general resource namespace, filesystem, package loader, complete WVB verifier, general multi-function interpreter, JIT, cache, dynamic runtime selector, or stable runtime ABI. It accepts one measured module shape. Generalization waits for another real program or capability requirement.
