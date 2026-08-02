# Protected Windvale processes and typed resource sets

## Status and purpose

Protected-process contract version 9 is the implemented Probe 30 boundary for reclaiming a terminal client and rebuilding a second logical generation at the same physical root. It retains Probe 29's atomic typed pair and extends terminal cleanup with exact page release, generation-safe regrant, and a second real CPL3 execution.

[Decision 0100](../Documents/Decisions/0100-First-Reclaimed-And-Reused-Process-Root.md) owns version 9. [Decision 0098](../Documents/Decisions/0098-First-Typed-Two-Resource-Lookup.md) and exact implementation commit `3fd9ef7535d7536ed084144e4f697cda548bf35c` retain the qualified version-8 baseline.

This is an internal experiment, not a stable syscall ABI, general process manager, dynamic namespace, transferable capability system, arbitrary WVB loader, complete verifier, or JIT.

## Ownership split

- [`Process-Foundation.wv`](../Operating-System/Kernel/Process-Foundation.wv) binds the init, interpreter, program, and execution-budget identities; roles; budgets; ordered resource-set token; two generation-stamped grants; exact reuse; terminal cleanup; result channel; and policy token `97`.
- [`Init-Resource-Service.wv`](../Operating-System/Kernel/Init-Resource-Service.wv) selects ordered identifiers `(1,2)` and returns resource-set token `131073` (`0x0002_0001`). Its WVA shim performs grant/receive twice, then exits on syscall five.
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

Init is process/thread `1/1`, generation `1`, process reference `65537`, has five user pages, instruction budget `64`, call-depth budget `1`, runtime profile `1`, one handle, and five syscalls. The interpreter uses process/thread `2/2`, generation `1` then `2`, references `65538` then `131074`, 38 pre-grant and 40 post-grant user pages, instruction budget `4,822`, call-depth budget `3`, runtime profile `4`, one handle, and two syscalls per generation. Both retain result `29`, capability slot `0`, capability reference `65536`, and channel capacity `1`.

Policy WVB must return token `97` before channel, resource, paging, descriptor, or MSR state is published. The policy expresses each grant/execution/revocation generation as a bounded helper and requires one exact release/reuse transition between them. The inherited four-page owned kernel stack remains sufficient under pinned QEMU.

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

Each client generation receives the same reclaimed 42-page physical extent and two later virtual aliases:

| Relative page | Purpose | Before grant | After grant |
| ---: | --- | --- | --- |
| `0..3` | Private paging hierarchy | none | none |
| `4..36` | 33-page linked interpreter image | user RX | user RX |
| `37..40` | Stack | user RW/NX | user RW/NX |
| `41` | ABI-16 context/data | user RW/NX | user RW/NX |
| `42` | Module alias | absent | init page `7`, user RO/NX |
| `43` | Budget alias | absent | init page `8`, user RO/NX |

No client-owned placeholder page backs either alias. Both target PTEs begin exactly zero. The atomic grant writes two distinct init physical addresses with present/user/NX and without writable. After generation-1 cleanup, the complete 42-page extent is zeroed and released; generation 2 reconstructs every table, image, stack, data, context, and record byte at the same root. Kernel mappings remain supervisor-only and no present leaf is writable and executable.

## Process records

The state page stores 264-byte little-endian `WVPROC09` records at offsets `0x100` and `0x300`. Version 9 retains the first 256 bytes of version 8 and appends one field:

- magic/version are `WVPROC09` and `9`;
- user-page budgets are init `5` and interpreter `40`;
- interpreter instruction budget is `4,822`;
- interpreter RX code-page count is `33`;
- runtime profiles remain init owner `1` and granted interpreter `4`;
- the program digest at `0xD8` remains the admitted WVB identity; the Windvale policy and the separate resource record bind the budget digest.
- process generation at `0x100` is init `1`, first client `1`, or rebuilt client `2`.

All other lifecycle, saved-register, capability, channel, result, fault, stack-count, and runtime-kind fields retain their version-7 offsets. Saving/restoring `RDX` remains required because ABI 16 uses it for the native-context pointer.

Both context pages begin with valid context-7 headers under ABI 17. Init leaves its service/resource pointers zero. Each rebuilt client starts with context offsets 24 and 96 zero; grant syscall `4` publishes service-table version 5 at data offset `0x80`, `WVBR002` at `0x100`, and both pointers as one transition.

## Typed resource records

Two 128-byte little-endian `WVRES004` records begin at state offsets `0x450` and `0x4D0`. Both use this layout:

| Offset | Bytes | Field |
| ---: | ---: | --- |
| `0x00` | 8 | Magic `WVRES004` |
| `0x08` | 4 | Version `4` |
| `0x0C` | 4 | Record bytes `128` |
| `0x10` | 4 | State: owned `1`, borrowed `2` |
| `0x14` | 4 | Resource identifier |
| `0x18` | 4 | Owner process reference `65537` |
| `0x1C` | 4 | Borrower: zero, `65538`, or `131074` |
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
2. Syscall `4` validates both pristine owned records, both source pages/digests, both absent target PTEs, the shared service leaf, and the exact token before any mutation.
3. The kernel installs both RO/NX aliases, publishes service table 5 plus `WVBR002`, and changes both records to borrower `65538`, grant count `1`, mapping count `1`.
4. Init blocks in receive after exactly two syscalls. The coordinator revalidates the complete atomic publication before entering client generation 1.
5. The interpreter reads both names, validates the module and exact four-byte budget, charges one unit per opcode, and returns `29` after the canonical four opcodes.
6. Generation 1 sends `29`, then exits or takes the contained vector-13 fault. Cleanup validates generation `1`, clears both aliases and the complete private publication, and returns both records to owned/no-borrower/mapping-zero while preserving grant count `1`.
7. Reloading init's CR3 flushes the retired non-global translations. The coordinator validates the released records, zeroes and releases the exact 42-page allocator tail, and immediately reallocates the same root.
8. Every client page is reconstructed from immutable inputs. The new `WVPROC09` record carries generation `2`; stale generation-1 process and resource evidence is invalid.
9. Init receives the first `29`, returns token `131073` again on syscall three, and blocks on syscall four.
10. The second grant requires the exact released generation-1 record history, installs both aliases, records borrower `131074`, and advances both grant counts to `2`.
11. Client generation 2 independently interprets the same two resources and sends `29`, then exits or takes the contained vector-13 fault.
12. Cleanup validates generation `2`, clears both aliases/publications, and returns both records to owned/no-borrower/mapping-zero while preserving grant count `2`.
13. Init receives the second `29` and exits on syscall five. The allocator must again be exactly exhausted at cursor page `63`.

The user-fault scenario sends `29` and executes privileged `CLI`; the same two-resource cleanup still completes. CPL0 invalid-opcode and general-protection scenarios remain terminal.

## Planner diagnostics

| Code | Meaning |
| --- | --- |
| `WVOS6001`–`WVOS6008` | Invalid process extent, image, identity, role, capability, or role-specific runtime resources. |
| `WVOS6101` | Either owner page, bytes, digest, or typed identity is invalid. |
| `WVOS6102` | Either target alias is already mapped or the resources are not exclusively owned by init. |
| `WVOS6103` | The service leaf or private publication is outside fixed client bounds. |
| `WVOS6104` | The requested set is unknown, partial, duplicate, or out of order. |
| `WVOS6105` | The generation or prior released grant history is not the exact next lifecycle state. |
| `WVOS6201` | Borrower process/thread state is not coherently terminal. |
| `WVOS6202` | The record set is not two exact live typed borrows. |
| `WVOS6203` | A live alias, digest, PTE address, service, or private directory differs from the admitted grant. |
| `WVOS6204` | The terminal client generation is outside the supported first/second generation proof. |

## Deterministic Probe 30 evidence

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Process-policy WVB | 6,553 | `c811fb5c7ef7a194f238831fd91f6a306084e619cd7572300eab74e2107bdfa2` |
| Process-policy WVO | 58,648 | `ffd7d1c4a78e57f4bae3cff03314f632909359b6012f7fc8c747cabe710edaf9` |
| Init WVB | 525 | `0554d80340440bf8895f0bf066d355da83337791f5404f2b72ca6da214664467` |
| Init WVO | 3,959 | `a0e7d0815c40993d1846a44d230428feef1bea6350ebf536db672ca507ca6656` |
| WVA init-service shim WVO | 243 | `2e00ea9799cd8fc55e75611a9f2f5831c26162b0d3928841f003d5ab9802139a` |
| Linked init image | 3,935 | `328d075ce129aed204707b16fab7c22b9e8f624b917ce681959118b02d313814` |
| Interpreter WVB | 12,851 | `7fbb25fe08136c86c063c08395451f8db1219bd17e0adc0748b5fa2d9a3f8fee` |
| Interpreter WVO | 134,166 | `3de222684b7fd38a9ace76a58c5ddaaf715f34e847e81af802cf1a3289428a4e` |
| WVA typed-lookup stencil WVO | 462 | `fde44aad9549731d53c5ccf3a57733b3619df94369b61ef27a693e1059784bc9` |
| Published typed-lookup WVO | 462 | `ecb940abb9de8086d50ae418853021cf1f7566a9415a5a3a3b4e5cc45ed5e78c` |
| Linked normal client image | 134,077 | `4cb7edd21a44183fbddc9105834ecc6a69e576ac3bf4b0fcdf1ee98f111c55b3` |
| Linked fault client image | 134,077 | `f70fc9b66ea493863439fe4f4ad5510b1e666fb1466cfce25e0088b8af883ef8` |
| Normal process-machine WVO | 155,893 | `4f126af968669458c499e8e40b375cbbd614b2e3e8bb29f1ee46597fc19e21ea` |
| Normal process-machine code | 16,295 | `7db8a79b86e01f87de8e65881992cda321318f599b8c1d138acaaf89a464e42d` |
| Fault process-machine WVO | 155,973 | `821d4c1dba668566f9a42f839da14ea78814ac688cb16cbac649bd5bdfbeb6dc` |
| Fault process-machine code | 16,375 | `1c4b38b16e0840d5fcc1249e2af180e0a1c6f849671745b00b7318a92a28cc0f` |

Focused Windows evidence passes all 25 OS tests and all four pinned-QEMU Probe 30 scenarios. Cross-host qualification is pending.

## Deliberate limits

Version 9 has exactly two fixed names, one owner, one logical borrower at a time, two fixed generations, two ordered atomic grants, and one exact LIFO extent reuse. It does not add enumeration, arbitrary resource counts, transfer/delegation, explicit revocation, independent lifetimes, non-tail release, concurrent root reuse, SMP shootdown, general process creation, scheduling, arbitrary loading, executable publication, JIT, filesystems, packages, networking, Hyper-V, or physical-hardware evidence.
