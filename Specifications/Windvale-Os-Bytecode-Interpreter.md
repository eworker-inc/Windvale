# Init-granted Windvale OS bytecode interpreter

## Status and purpose

Interpreter runtime profile 4 now has a cross-host-qualified Probe 29 revision that consumes an atomic typed pair: the admitted WVB and its execution budget. The profile number and ABI 16/context 7/service-table 5 remain unchanged, but the interpreter WVB and accepted runtime-input contract are no longer byte-identical to Probe 28.

[Decision 0098](../Documents/Decisions/0098-First-Typed-Two-Resource-Lookup.md) owns the qualified extension. Exact implementation commit `3fd9ef7535d7536ed084144e4f697cda548bf35c` passes Windows and Debian qualification in GitHub [Verify run 30745623111](https://github.com/eworker-inc/Windvale/actions/runs/30745623111).

The interpreter itself is an AOT boot component running at CPL3. The separately mapped program is interpreted; the kernel contains neither source-language interpretation nor WVB semantic decoding.

## Typed runtime inputs

The interpreter declares only `file.read_bytes` and performs exactly these lookups:

1. `boot:main.wvb` — resource `1`, kind `wvb-module`, exact admitted WVB;
2. `boot:main.budget` — resource `2`, kind `u32-execution-budget`, exactly four little-endian bytes.

Process `2` starts with context offsets 24 and 96 zero and both target PTEs absent. Windvale init selects ordered set token `131073`; one checked syscall publishes:

- two distinct user-readable RO/NX aliases of init-owned pages;
- native service-table version 5 with only `file.read_bytes` nonzero;
- one 80-byte `WVBR002` directory with a 16-byte header and two ordered 32-byte typed entries; and
- both context pointers as part of the same atomic transition.

The exact 347-byte WVA leaf matches only the two names. It validates directory magic/version/size/count, entry identifier/kind, pointer, exact length, flags, and zero reserved bytes before returning an ABI-16 borrowed-bytes descriptor. Wrong names fail with file-not-found detail `6`; a missing or malformed directory fails with unavailable detail `8`.

Stage 0 independently reconstructs the leaf with its x86-64 builder and requires byte equality with the checked-in WVA stencil. WVA owns the final service bytes; Stage 0 remains the explicit publication adapter.

## Module admission

The WVB resource retains the section-derived profile:

- total length from 12 through 4,096 bytes, magic `WVB1`, version `1.6`, and seven sections;
- seven ordered section kinds, zero flags/reserved fields, checked payload ranges, and exact consumption;
- portable profile, a nonempty module name no longer than 255 bytes, and empty capability/data/type sections;
- exactly one exported `Main() -> i32`, one `i32` local, maximum stack depth one, and code covering the complete code payload;
- opcode-specific operand bounds, local index `0`, initialization, and one final return at the code-envelope end.

The only accepted opcodes are `i32.const`, `local.store 0`, `local.load 0`, and `return`.

## Execution-budget contract

The budget resource must be exactly four bytes, decode to a nonzero unsigned value no greater than `64`, and have the exact typed directory entry. The interpreter charges one unit immediately before each WVB opcode:

- canonical value `4` executes all four opcodes and returns `29`;
- value `3` exhausts before `return` and returns interpreter status `-34`;
- zero or `65` is rejected as invalid budget with status `-17`;
- a three-byte resource is rejected with status `-16`; and
- a missing budget name remains a contained runtime failure (`WVR3022` in the reference harness).

The complete AOT interpreter path consumes the exact process instruction budget `4,822` with maximum dynamic call depth `3`. This host/native counter is separate from the four-opcode guest budget.

## Process and artifact bounds

The client has 33 RX pages, four RW/NX stack pages, and one RW/NX data page. The two aliases occupy following virtual pages 42 and 43 but consume no client-owned physical placeholder pages. User-page count moves atomically from `38` to `40`.

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Interpreter WVB | 12,851 | `7fbb25fe08136c86c063c08395451f8db1219bd17e0adc0748b5fa2d9a3f8fee` |
| Interpreter WVO | 134,166 | `3de222684b7fd38a9ace76a58c5ddaaf715f34e847e81af802cf1a3289428a4e` |
| Raw WVA leaf | 347 | `b43bc2457fd5b5622095bad6d59ad3cd2aa045bde1cc79576afbb419bac02fd7` |
| WVA stencil WVO | 462 | `fde44aad9549731d53c5ccf3a57733b3619df94369b61ef27a693e1059784bc9` |
| Published service WVO | 462 | `ecb940abb9de8086d50ae418853021cf1f7566a9415a5a3a3b4e5cc45ed5e78c` |
| Linked normal image | 134,077 | `4cb7edd21a44183fbddc9105834ecc6a69e576ac3bf4b0fcdf1ee98f111c55b3` |
| Linked fault image | 134,077 | `f70fc9b66ea493863439fe4f4ad5510b1e666fb1466cfce25e0088b8af883ef8` |

Focused tests cover exact budget success, exhaustion, invalid bounds, malformed length, missing name, shifted module sections, malformed WVB, atomic grant, distinct aliases, typed-entry mutations, and equivalent exit/fault cleanup. Windows and digest-pinned Debian 12 pass all 67 Seed tests and all 25 OS tests; all four pinned-QEMU Probe 29 scenarios pass on Windows.

## Trust boundary and deliberate limits

The AOT admission policy checks the canonical WVB before process construction. Grant revalidates both source pages and digests; the WVA leaf validates the typed private directory; the interpreter validates its semantic subset and supplied budget; terminal cleanup revalidates and clears both live aliases and the complete publication.

Profile 4 still is not a general resource namespace, filesystem, complete WVB verifier, multi-function interpreter, JIT, cache, dynamic runtime selector, or stable runtime ABI. Both names, kinds, owner, borrower, order, and lifetime are fixed. Generalization waits for a third resource, independent lifetime, package lookup, or executable-publication requirement.
