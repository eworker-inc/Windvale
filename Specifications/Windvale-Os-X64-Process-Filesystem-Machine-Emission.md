# Windvale OS x86-64 filesystem-machine emission

## Status and scope

This contract source-owns the generation-three filesystem provider's record,
page-table, image-copy, native-context, endpoint, and initial-thread
construction bytes. The current boot object links callable forms of the three
generated constructors and invokes them after an exact 85-page
generation-three allocation. The WVA transaction then publishes a durable
filesystem domain ledger, provider-side generation-two endpoint, fresh ready
thread, and ready process record. It does not bind a consumer capability, enter
the provider, or execute a request.

The machine reuses released process/object slot 2 as process reference
`196610` and closed resource-endpoint slot 0 as endpoint reference `131072`.
The record fixes process and thread identity 2, generation 3, role 2, runtime
profile 5, a 262,144-instruction candidate budget, depth 16, primary capability
slot 0 at endpoint generation 2 with provider rights 46 and capacity one, the
endpoint-table address at state offset 1,168, and the 195,657-byte service digest
`57f79e283a33c7e874761a9c3713ae753736731d6cbf477c90fd7caac231c8d6`.
Its configuration digest is SHA-256
`0e34a46dd568fdf97fb72c005d11bc626e9c2950b706fec73cb166521ccfecf4`,
the exact admitted 80-byte `WVPR 1` filesystem launch request. This is not
FAT32 media identity; boot integration must bind the selected media separately
before claiming a real read.

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

The raw record constructor is 462 bytes and has independent polynomial hash
pairs `58251/5347` using multipliers 251/257. It writes private state 0 at
`WVPROC17 + 0x10` and generation 3 at `WVPROC17 + 0x14`. The paging constructor is 3,342 bytes
and hashes to `23484/25185` using multipliers 263/269. The image/context
constructor is 58 bytes and hashes to `16263/35901` using multipliers 271/277.
The process-object adapter appends one `ret` opcode to each raw fragment, so the
callable image, paging, and record sections are 59, 3,343, and 463 bytes.

The image constructor has one external RIP-relative relocation at field 3. It
targets process-object symbol 7 with signed addend -4, copies exactly 195,657
bytes, and rejects zero or more than 196,608 bytes. The 956,321-byte process
object exports the image, paging, and record constructors at section addresses
780,192, 780,256, and 783,600 respectively, with SHA-256
`ea07c502f0b3f45e650284426c136c601c9fdacf8addfa9f99fd890cc2a535a1`.
The separately assembled 2,654-byte construction transaction has SHA-256
`51a7302bfe8f5565cb9e17522a4d042b618df2903944f2c567b46c9193d002d8`.

## Deterministic evidence and remaining boundary

The focused owner builds and executes three independent test roots on Windows
and packages matching Linux images. The record, paging, and image WVB identities
are respectively:

- `3f1c122df05e8c3d6a963846b8d97a4dbbe6ff692a205d8a6b4d19c2ceccf329`;
- `1e626b1775f34af1356a10287c23b04523f39cd9971e57f60bf1105c3ec6aeae`;
- `c2630d4100b2e3a8447f850ac0be9ffb21160431de199908584ed4b08c49a743`.

Their Windows executable identities are
`98f573b13a8ac2f4301078a1fd92a95348341e2711b30a002764044aef4826e3`,
`9fad8f8be8bfabaabd0d800ba3df4a8533ed7dc7df62a804d19d04a6a2e0db85`,
and `8b1d4296461f2e553ba1b4ed2f42bdfdcd4706cb4f88de74bfaa408b37b2d384`.
The packaged Linux identities are
`25c5c09a0aed29175c8745c09944831269266b3a8be9e74b91d3c50afb907604`,
`aebe8ae480c0e57ff1014030152d987091d463f047b8cdcc3c1a7ad83b887cd1`,
and `621ed9294cafae354b4b20099cb8bc0ef1f5fc1a5f1593b9efade98ce2aceee4`.
The native results are 50, 51, and 52.

The constructors and provider-side publication execute in deterministic
1,698,816-byte Probe 40 images, and pinned QEMU completes the normal and two
terminal fault paths with paging version 7 active. Exact validation of the
terminal directory endpoint permits its 64-byte state slot to become the live
filesystem domain record with committed use `1/81/1`. Endpoint `131072` names
generation 2, provider `196610`, client 0, and one empty retained channel. A
fresh generation-three thread and process are ready, but the boot path does not
dispatch them after publication. Integration still must bind a real surviving
consumer, finalize the endpoint for traffic, bind FAT32 media identity, enter
the service context, and execute one bounded request/reply lifecycle. Complete
rollback of the unpublished generation-three allocation after a
post-constructor validation failure also remains open.
