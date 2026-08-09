# Windvale native hosted-verifier publisher object instantiation

## Status and scope

This contract instantiates the exact startup, publication-adapter, and shared
SHA-256 WVOs admitted by `WVPI 1`, using the target placements from `WVCR 1`
and ordered external addresses from `WVPT 1`. It is a controlled downstream
stage: callers must supply the objects and addresses produced by those prior
admission stages. It does not discover identities from paths and does not
materialize an outer PE or ELF.

## `WVIX 1` request

The little-endian request has a 48-byte header:

| Offset | Field |
| ---: | --- |
| 0 | magic `WVIX` (`0x58495657`) |
| 4 | version 1 |
| 8 | total request bytes |
| 12 | target: 1 Windows or 2 Linux |
| 16 | startup address, exactly 4,096 |
| 20 | adapter address |
| 24 | SHA address |
| 28 | ordered external-target count |
| 32 | startup WVO bytes |
| 36 | adapter WVO bytes |
| 40 | SHA WVO bytes, exactly 2,176 |
| 44 | reserved zero |

The header is followed by all `u32` `WVPT` addresses, startup WVO, adapter
WVO, and SHA WVO. The first address binds startup-run; remaining addresses
bind adapter imports in WVO symbol order. Windows uses 44 targets and
243,600/248,896 adapter/SHA addresses. Linux uses 27 targets and
142,929,920/142,933,296 addresses. A zero external address rejects.

## Instantiation rules

All admitted relocations are relative-i32 with addend `-4` and a four-byte
zero placeholder. The constructor computes `target - field-address - 4` with
checked signed-32-bit range. Local symbols use their section base and offset;
imports use the matching ordered external address.

Startup and adapter contain one code section. The SHA object contains 1,350
code bytes followed by two alignment bytes and 333 read-only-data bytes. Its
two relocations resolve internally, so its 1,685 output bytes are identical at
both admitted base addresses.

## `WVIO 1` response

Every response begins with a 64-byte little-endian header containing magic
`WVIO` (`0x4f495657`), version, total bytes, status, consumed request bytes,
target, component offsets and lengths, all three base addresses, and target
count. Status zero appends startup, adapter, and SHA bytes in that order.
Statuses 1 through 4 identify header, target/geometry, envelope, or object and
relocation rejection. A rejection is exactly the 64-byte header.

Canonical successful response sizes are 7,040 Windows bytes and 5,117 Linux
bytes. The payload identities are:

| Target | Component | Bytes | SHA-256 |
| --- | --- | ---: | --- |
| Windows | startup | 5 | `dbb15bae305f7eda414e935e3fcc8ef9ce9a25e9f3fa4d142814545d36fc9e9e` |
| Windows | adapter | 5,286 | `3f3f7c4230724bf6e2692f232ed3a904705174ed7ba1174012dc1d1ebfa1be93` |
| Linux | startup | 5 | `22ef5439e468626dc1b46c6c92fed269681b76d2b34325bba4bb1c13dc26b6d7` |
| Linux | adapter | 3,363 | `7cbae400e311d763170a77685e959caa630a590b8f2e8964e78dea53ea6d152c` |
| Both | SHA | 1,685 | `513d73834e2c6358adad022a31a386be59391874e73e4ad5bf74c70ec0b170ce` |

## Evidence and remaining work

The candidate WVB is built through the digest-bound native source front door.
The focused test checks its pinned identity, service-free entry shape,
interpreter/native equality, exact component equality with both canonical
publisher applications, and narrow malformed rejection.

The next slice must consume these bytes with `WVCR` and Decision 0475 metadata
to perform the exact Windows PE and Linux ELF mutations. The frozen C# writer
remains Stage 0 recovery/differential evidence until that materialization and
the broader dual-host retirement gates are complete.
