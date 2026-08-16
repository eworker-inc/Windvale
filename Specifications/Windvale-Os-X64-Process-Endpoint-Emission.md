# Windvale OS x86-64 process channel and endpoint emission

## Status and scope

This contract source-owns fixture offsets 1,428 through 1,871 of the current
Probe 40 process machine. The 444-byte slice initializes the fixed resource and
directory IPC channels and their endpoints after policy admission succeeds.

This is privileged construction evidence, not an application-visible IPC ABI,
dynamic endpoint allocation, provider publication, or evidence that the new
filesystem/network provider images are running.

## Exact construction

The constructor initializes two independent record pairs:

| Pair | Channel | Endpoint | Capability | Provider | Client |
| --- | ---: | ---: | ---: | ---: | ---: |
| Resource service | `0x420` | `0x490` | `0x00010000` | `0x00010001` | `0x00010002` |
| Directory service | `0x7f0` | `0x860` | `0x00010001` | `0x00010003` | `0x00010002` |

Each 112-byte `WVCHAN04` record is zeroed before publishing magic, version,
record size, and capacity one. Each 64-byte `WVEND01` record is likewise zeroed
before publishing version, size, open state, semantic reference, service kind,
capacity, provider/client generations, and its channel address.

The slice has no internal branch or external WVO relocation fields. Record
offsets and fixed references remain reconstruction evidence for the fixture,
not stable public values.

## Verification

`Test-Os-X64-Code-Emission` executes the constructor and pins all 444 bytes at
SHA-256
`92a53755d236709268d69b7b157ef7d2c8af345931e0dc06d2e2a77663b2104e`,
the exact fixture interval, deterministic Windows/Linux images, and local
result 53. Together with the preceding source-owned slices, Windvale now
reconstructs fixture offsets zero through 1,871 plus the eight explicit
coordinator relocation fields.

[Decision 0624](../Documents/Decisions/0624-First-Windvale-Owned-Init-Extent-Allocation.md)
now owns the first memory-object allocation beginning at offset 1,872. Complete
process-object replacement still requires record construction, paging,
user-copy, syscall, exception, timer, failure, and epilogue regions to compose
and pass pinned QEMU.
