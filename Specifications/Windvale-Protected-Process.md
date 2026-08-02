# Protected Windvale processes and typed resource sets

## Status and purpose

Protected-process contract version 8 is the cross-host-qualified Probe 29 boundary for one atomic, typed, two-resource borrow. It extends the version-7 terminal cleanup from Probe 28 without changing the stable language or native ABI contracts.

[Decision 0098](../Documents/Decisions/0098-First-Typed-Two-Resource-Lookup.md) owns qualified version 8. Exact implementation commit `3fd9ef7535d7536ed084144e4f697cda548bf35c` passes Windows and Debian qualification in GitHub [Verify run 30745623111](https://github.com/eworker-inc/Windvale/actions/runs/30745623111).

This is an internal experiment, not a stable syscall ABI, general process manager, dynamic namespace, transferable capability system, arbitrary WVB loader, complete verifier, or JIT.

## Ownership split

- [`Process-Foundation.wv`](../Operating-System/Kernel/Process-Foundation.wv) binds the init, interpreter, program, and execution-budget identities; roles; budgets; ordered resource-set token; atomic grant; terminal cleanup; result channel; and policy token `97`.
- [`Init-Resource-Service.wv`](../Operating-System/Kernel/Init-Resource-Service.wv) selects ordered identifiers `(1,2)` and returns resource-set token `131073` (`0x0002_0001`). Its WVA shim passes that token unchanged to syscall `4`.
- [`Bytecode-Interpreter.wv`](../Operating-System/Runtime/Bytecode-Interpreter.wv) reads both exact resource names and enforces the supplied execution budget.
- [`Boot-Resource-Service.wva`](../Operating-System/Runtime/Boot-Resource-Service.wva) owns the exact typed lookup leaf for the two names and `WVBR002` entries.
- Stage 0 temporarily owns raw page-table writes, initial records, atomic publication, syscall dispatch, fixed coordination, and firmware packaging. It independently reconstructs and checks Windvale/WVA-owned decisions.

## Fixed resources, roles, and budgets

Version 8 binds four canonical SHA-256 identities:

| Identity | SHA-256 | Authority |
| --- | --- | --- |
| Init/resource-service WVB | `0554d80340440bf8895f0bf066d355da83337791f5404f2b72ca6da214664467` | receive plus fixed resource-set grant |
| Bytecode-interpreter WVB | `7fbb25fe08136c86c063c08395451f8db1219bd17e0adc0748b5fa2d9a3f8fee` | send; two local reads after grant |
| `boot:main.wvb` | `7f08efbb20c6cc69c100f07407f759625b38c02a3f05bb4e8dabcc7bdd10c4e2` | resource `1`, kind `1`, immutable RO/NX |
| `boot:main.budget` | `fb5e512425fc9449316ec95969ebe71e2d576dbab833d61e2a5b9330fd70ee02` | resource `2`, kind `2`, four-byte LE value `4`, immutable RO/NX |

Init is process/thread `1/1`, has five user pages, instruction budget `64`, call-depth budget `1`, runtime profile `1`, one handle, and three syscalls. The interpreter is process/thread `2/2`, has 38 pre-grant and 40 post-grant user pages, instruction budget `4,822`, call-depth budget `3`, runtime profile `4`, one handle, and two syscalls. Both retain result `29`, capability slot `0`, generation `1`, reference `65536`, and channel capacity `1`.

Policy WVB must return token `97` before channel, resource, paging, descriptor, or MSR state is published. Version 8's larger policy requires the kernel's measured four-page owned stack; three pages do not complete process construction under pinned QEMU.

## Separate address spaces

Init receives a nine-page physical extent:

| Relative page | Purpose | Process access |
| ---: | --- | --- |
| `0..3` | Private paging hierarchy | none |
| `4` | Linked init image | user RX |
| `5` | Stack | user RW/NX |
| `6` | ABI-16 context/data | user RW/NX |
| `7` | Owned `boot:main.wvb` page | user RO/NX |
| `8` | Owned `boot:main.budget` page | user RO/NX |

The client receives 42 physical pages and two later virtual aliases:

| Relative page | Purpose | Before grant | After grant |
| ---: | --- | --- | --- |
| `0..3` | Private paging hierarchy | none | none |
| `4..36` | 33-page linked interpreter image | user RX | user RX |
| `37..40` | Stack | user RW/NX | user RW/NX |
| `41` | ABI-16 context/data | user RW/NX | user RW/NX |
| `42` | Module alias | absent | init page `7`, user RO/NX |
| `43` | Budget alias | absent | init page `8`, user RO/NX |

No client-owned placeholder page backs either alias. Both target PTEs begin exactly zero. The atomic grant writes two distinct init physical addresses with present/user/NX and without writable. Kernel mappings remain supervisor-only and no present leaf is writable and executable.

## Process records

The state page stores 256-byte little-endian `WVPROC08` records at offsets `0x100` and `0x300`. Version 8 retains the version-7 field layout while changing these bound values:

- magic/version are `WVPROC08` and `8`;
- user-page budgets are init `5` and interpreter `40`;
- interpreter instruction budget is `4,822`;
- interpreter RX code-page count is `33`;
- runtime profiles remain init owner `1` and granted interpreter `4`;
- the program digest at `0xD8` remains the admitted WVB identity; the Windvale policy and the separate resource record bind the budget digest.

All other lifecycle, saved-register, capability, channel, result, fault, stack-count, and runtime-kind fields retain their version-7 offsets. Saving/restoring `RDX` remains required because ABI 16 uses it for the native-context pointer.

Both context pages begin with valid context-7 headers. Init leaves its service/resource pointers zero. The client starts with context offsets 24 and 96 zero; grant syscall `4` publishes service-table version 5 at data offset `0x80`, `WVBR002` at `0x100`, and both pointers as one transition.

## Typed resource records

Two 128-byte little-endian `WVRES003` records begin at state offsets `0x440` and `0x4C0`. Both use this layout:

| Offset | Bytes | Field |
| ---: | ---: | --- |
| `0x00` | 8 | Magic `WVRES003` |
| `0x08` | 4 | Version `3` |
| `0x0C` | 4 | Record bytes `128` |
| `0x10` | 4 | State: owned `1`, borrowed `2` |
| `0x14` | 4 | Resource identifier |
| `0x18` | 4 | Owner process `1` |
| `0x1C` | 4 | Borrower: zero or process `2` |
| `0x20` | 8 | Init source identity address |
| `0x28` | 4 | Exact byte length |
| `0x2C` | 4 | Typed attributes |
| `0x30` | 8 | Target root |
| `0x38` | 8 | Target context/data address |
| `0x40` | 8 | Target resource virtual address |
| `0x48` | 8 | Shared `file.read_bytes` service address |
| `0x50` | 32 | Resource SHA-256 |
| `0x70` | 4 | Historical grant count |
| `0x74` | 4 | Live mapping count |
| `0x78` | 8 | Exact target PTE address |

Typed attributes retain immutable/read-only/no-execute bits `0..2` and encode the kind in bits `8..15`. Resource `1` has kind `wvb-module` (`1`), length `174`, and attributes `0x0107`. Resource `2` has kind `u32-execution-budget` (`2`), length `4`, and attributes `0x0207`.

The source fields are identity addresses only because the bounded low-memory process extents remain identity mapped. This is not a general virtual-to-physical lookup contract.

## `WVBR002` publication

The client-private directory is exactly 80 bytes: a 16-byte header followed by two 32-byte entries. The header carries magic `WVBR`, version `2`, total size `80`, and entry count `2`. Each ordered entry carries identifier, kind, mapped pointer, exact length, immutable RO/NX flags `7`, and eight zero reserved bytes.

Entry zero is `(1, wvb-module)` and entry one is `(2, u32-execution-budget)`. Publication is atomic: an unknown, duplicate, reversed, or partial token is `WVOS6104`, and the client never observes one entry or alias without the other.

The WVA leaf accepts only `boot:main.wvb` and `boot:main.budget`, then validates the selected typed entry, pointer, length, flags, and reserved bytes before returning a borrowed descriptor. It does not enumerate names or implement a dynamic namespace.

## Grant, execution, and cleanup

1. Init enters CPL3 and returns ordered resource-set token `131073`.
2. Syscall `4` validates both owned records, both source pages/digests, both absent target PTEs, the shared service leaf, and the exact token before any mutation.
3. The kernel installs both RO/NX aliases, publishes service table 5 plus `WVBR002`, and changes both records to borrower `2`, grant count `1`, mapping count `1`.
4. Init blocks in receive after exactly two syscalls. The coordinator revalidates the complete atomic publication before entering process `2`.
5. The interpreter reads both names, validates the module and exact four-byte budget, charges one unit per opcode, and returns `29` after the canonical four opcodes.
6. The client sends `29`, then exits or takes the contained vector-13 fault.
7. Cleanup accepts only the exact two live PTEs plus each hardware accessed bit, revalidates both records and both directory entries, clears both aliases and the complete private publication, and returns both records to owned/no-borrower/mapping-zero while preserving one grant.
8. Reloading init's CR3 flushes the retired non-global translations. Init receives and exits `29`.

The user-fault scenario sends `29` and executes privileged `CLI`; the same two-resource cleanup still completes. CPL0 invalid-opcode and general-protection scenarios remain terminal.

## Planner diagnostics

| Code | Meaning |
| --- | --- |
| `WVOS6001`–`WVOS6008` | Invalid process extent, image, identity, role, capability, or role-specific runtime resources. |
| `WVOS6101` | Either owner page, bytes, digest, or typed identity is invalid. |
| `WVOS6102` | Either target alias is already mapped or the resources are not exclusively owned by init. |
| `WVOS6103` | The service leaf or private publication is outside fixed client bounds. |
| `WVOS6104` | The requested set is unknown, partial, duplicate, or out of order. |
| `WVOS6201` | Borrower process/thread state is not coherently terminal. |
| `WVOS6202` | The record set is not two exact live typed borrows. |
| `WVOS6203` | A live alias, digest, PTE address, service, or private directory differs from the admitted grant. |

## Deterministic qualified evidence

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Process-policy WVB | 6,955 | `5134108d706aefd18ac90c18cefe793d9ec166f19066484219e2618300e4cedb` |
| Process-policy WVO | 62,210 | `6e49e4d2a71513cc7f14442a2744c06b414da0081a60d150529ca0b54f394563` |
| Init WVB | 525 | `0554d80340440bf8895f0bf066d355da83337791f5404f2b72ca6da214664467` |
| Init WVO | 3,959 | `a0e7d0815c40993d1846a44d230428feef1bea6350ebf536db672ca507ca6656` |
| Linked init image | 3,903 | `a08cc4b84772ea9e855acc5b9c7f0cc4e1b7e1ab24ad317ad3e59af129f531d1` |
| Interpreter WVB | 12,851 | `7fbb25fe08136c86c063c08395451f8db1219bd17e0adc0748b5fa2d9a3f8fee` |
| Interpreter WVO | 134,166 | `3de222684b7fd38a9ace76a58c5ddaaf715f34e847e81af802cf1a3289428a4e` |
| WVA typed-lookup stencil WVO | 462 | `fde44aad9549731d53c5ccf3a57733b3619df94369b61ef27a693e1059784bc9` |
| Published typed-lookup WVO | 462 | `ecb940abb9de8086d50ae418853021cf1f7566a9415a5a3a3b4e5cc45ed5e78c` |
| Linked normal client image | 134,077 | `4cb7edd21a44183fbddc9105834ecc6a69e576ac3bf4b0fcdf1ee98f111c55b3` |
| Linked fault client image | 134,077 | `f70fc9b66ea493863439fe4f4ad5510b1e666fb1466cfce25e0088b8af883ef8` |
| Normal process-machine WVO | 149,483 | `b18ccf8f4c2eb065d017e2fafb2254fbf0299af1cf0eb130c3ca3405a34392e2` |
| Normal process-machine code | 10,071 | `e83edda8691142d8ad777269ee0f03bc1febc2785edf0dcd547f77f6bc3ae8bb` |
| Fault process-machine WVO | 149,531 | `a3bc89dbdd1539934417ae03f4375bd36af1ef6617d1b4f7651b5955f923ebd0` |
| Fault process-machine code | 10,119 | `0b9941bab32ad8d01daeba277fe9e118f82de0974b4b68fb86582e1f5e0b06c6` |

Windows and digest-pinned Debian 12 pass all 67 Seed tests, all 25 OS tests, and the complete non-Fast verifier for exact implementation commit `3fd9ef7535d7536ed084144e4f697cda548bf35c`. All four pinned-QEMU Probe 29 scenarios pass on Windows.

## Deliberate limits

Version 8 has exactly two fixed names, one owner, one borrower, one ordered atomic grant, and one shared terminal lifetime. It does not add enumeration, arbitrary resource counts, transfer/delegation, explicit revocation, independent lifetimes, page/root reuse, SMP shootdown, process creation, scheduling, arbitrary loading, executable publication, JIT, filesystems, packages, networking, Hyper-V, or physical-hardware evidence.
