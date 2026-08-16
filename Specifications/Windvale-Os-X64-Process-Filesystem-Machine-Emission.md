# Windvale OS x86-64 filesystem-machine emission

## Status and scope

This contract source-owns the generation-three filesystem provider's record,
page-table, image-copy, and native-context construction bytes. The current boot
object links and exports all three constructors, but does not allocate,
publish, enter, or execute the provider.

The machine reuses released process/object slot 2 as process reference
`196610` and closed resource-endpoint slot 0 as endpoint reference `131072`.
The record fixes process and thread identity 2, generation 3, role 2, runtime
profile 5, a 262,144-instruction candidate budget, depth 16, primary capability
slot 0 at endpoint generation 2 with provider rights 46 and capacity one, the
endpoint-table address at state offset 1,168, and the 195,657-byte service digest
`57f79e283a33c7e874761a9c3713ae753736731d6cbf477c90fd7caac231c8d6`.
Its current configuration digest is the SHA-256 empty digest; boot integration
must replace that placeholder with the admitted FAT32 media/configuration
identity before claiming a real read.

## Memory construction

The allocation contains 85 physical pages. Four are kernel-only paging pages;
the resource domain charges the remaining 81 user pages:

| Allocation pages | User mapping | Rights |
| --- | --- | --- |
| 0–3 | private paging structures | kernel only |
| 4–51 | 48 service-image pages | read/execute, non-writable |
| 52–68 | context plus 65,600-byte transfer window | read/write, non-executable |
| 69–84 | 16-page native stack | read/write, non-executable |
| 85–86 | explicit guard entries | not present |

The service image starts at extent offset 16,384. Native context starts at
212,992. The exact 65,600-byte receive window starts at 214,016, which is
1,024 bytes into the first private page, and ends 1,088 bytes into page 68.
The separately measured 65,536-byte native stack occupies pages 69 through 84
and ends at extent offset 348,160. Transfer and stack ranges are disjoint. No
page is both writable and executable.

## Emitted bytes and relocation

The record constructor is 462 bytes and has independent polynomial hash pairs
`10324/43536` using multipliers 251/257. The paging constructor is 3,342 bytes
and hashes to `23484/25185` using multipliers 263/269. The image/context
constructor is 58 bytes and hashes to `16263/35901` using multipliers 271/277.

The image constructor has one external RIP-relative relocation at field 3. It
targets process-object symbol 7 with signed addend -4, copies exactly 195,657
bytes, and rejects zero or more than 196,608 bytes. The 956,230-byte process
object exports the image, paging, and record constructors at section addresses
780,192, 780,256, and 783,600 respectively.

## Deterministic evidence and remaining boundary

The focused owner builds and executes three independent test roots on Windows
and packages matching Linux images. The record, paging, and image WVB identities
are respectively:

- `4545513104718bee62af6eddf4d559922224ae08c99d831525889496560c0e92`;
- `1e626b1775f34af1356a10287c23b04523f39cd9971e57f60bf1105c3ec6aeae`;
- `c2630d4100b2e3a8447f850ac0be9ffb21160431de199908584ed4b08c49a743`.

Their Windows executable identities are
`a2d54fac0fe08bfd357a9111f57bdf072afe53464588bac7c585d08ae0962c11`,
`9fad8f8be8bfabaabd0d800ba3df4a8533ed7dc7df62a804d19d04a6a2e0db85`,
and `8b1d4296461f2e553ba1b4ed2f42bdfdcd4706cb4f88de74bfaa408b37b2d384`.
The packaged Linux identities are
`30703a9e858db1e57b86273ead431a4a4a7b5ce28c93a3f62c0f73d26d6faf57`,
`aebe8ae480c0e57ff1014030152d987091d463f047b8cdcc3c1a7ad83b887cd1`,
and `621ed9294cafae354b4b20099cb8bc0ef1f5fc1a5f1593b9efade98ce2aceee4`.
The native results are 50, 51, and 52.

The constructors are boot-linked in the deterministic 1,696,768-byte Probe 40
images, and pinned QEMU still completes the normal and two terminal fault paths
with paging version 7 active. Integration still must allocate the exact 85-page
extent, invoke the constructors, publish the generation-three record, advance
and bind endpoint `131072`, enter the service context, and execute one bounded
FAT32 request/reply lifecycle. Failure before publication must leave no visible
process or committed resource charge.
