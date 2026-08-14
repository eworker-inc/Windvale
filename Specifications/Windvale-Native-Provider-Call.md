# Windvale native capability-provider call

## Status and scope

The first x86-64 capability-provider call emission and its separately
implemented structural verifier are implemented candidates. They define the
machine boundary that `storage.random_access_v1` will use, but they do not yet
publish native ABI 23, execution-context version 9, or a storage provider leaf.
ABI 22 and execution-context version 7 remain the only executable product
contract.

The call consumes one ordinal entry from the immutable [`WVPT 1` provider
table](Windvale-Native-Capability-Provider-Table.md). Generated code never
searches an identity, opens a path, obtains a host handle, or retains a provider
target or state address after the call.

## Planned context boundary

The successor context is version 9 and is append-only over version 7 plus the
version-8 allocator reservation from Decision 0151:

| Offset | Bytes | Field | Version-9 rule |
| ---: | ---: | --- | --- |
| 0 through 111 | 112 | retained context 7 | Unchanged |
| 112 | 8 | allocator-state pointer | Retains the version-8 reservation; zero until that provider is integrated |
| 120 | 8 | allocator-leaf pointer | Retains the version-8 reservation; zero until that provider is integrated |
| 128 | 8 | capability-provider table pointer | Nonzero when any call uses `WVPT 1`; otherwise zero |

The candidate size is 136 bytes. The separate [`WVXQ/WVXR 2`
constructor](Windvale-Native-Execution-Context-9-Construction.md) now validates
the pointer-presence relationship and exact initial bytes. Host table-lifetime
and WVB-identity agreement remain required before this becomes executable ABI.
No ABI-22 consumer may read offset 128.

## Exact x86-64 emission

The current emitter appends exactly 216 bytes for a five-parameter provider
call. It admits capability ordinals 0 through 31 and physical value slots 0
through 2,047. Any larger input fails without changing the supplied output.

The generated sequence:

1. Loads the immutable provider-table pointer from `[R15 + 128]`.
2. Loads the selected entry target and state from `32 + ordinal * 24`.
3. Reserves exactly 80 stack bytes and copies five complete 16-byte value cells
   from the verified function frame.
4. Supplies the caller-owned 16-byte result cell and exact argument count.
5. Calls the provider target, restores the stack, and branches to the ordinary
   runtime-service failure target when `EAX` is nonzero.

The call registers are:

| Register | Value |
| --- | --- |
| `RAX` | Provider target at call time; provider status on return |
| `R8` | Opaque, nonzero rights-limited provider-state pointer |
| `R9` | Pointer to the five copied argument cells |
| `RCX` | Pointer to the caller-owned result descriptor |
| `EDX` | Exact argument-cell count, `5` |
| `R10`, `R11`, `R15` | Instruction budget, call-depth budget, and execution context; provider must preserve all three |

The target may treat ordinary volatile registers as scratch. It must not change
the argument cells, provider table, generated frame outside the result cell, or
any execution resource not owned by its rights-limited state. Zero `EAX` means
the result cell contains a complete admitted response descriptor. Nonzero `EAX`
means the result cell is unpublished and generated code takes the existing
service-failure path.

## Random-access storage cells

For `storage.random_access_v1(u32,u64,u64,u32,bytes)->bytes`, cells retain their
ordinary ABI value representation:

| Cell | Meaning | Representation |
| ---: | --- | --- |
| 0 | operation | `u32` in the low dword; remaining bytes zero |
| 1 | expected generation | `u64` in the low machine word; remaining bytes zero |
| 2 | position | `u64` in the low machine word; remaining bytes zero |
| 3 | control | `u32` in the low dword; remaining bytes zero |
| 4 | payload | Complete borrowed-bytes descriptor: pointer, length, and generation |

The storage provider must revalidate the operation, generation, position,
control, payload range, response bound, revocation generation, and writer fence
before touching host I/O. A successful result is one `WVSA 1` response borrowed
from execution-owned provider scratch. Provider success does not reinterpret a
partial or indeterminate mutation as completion.

## Verification and current evidence

`Compiler/Windvale/Native-X64-Provider-Call.wv` owns emission.
`Tools/Windvale.Verify/Native-X64-Provider-Call-Verification.wv` independently
checks every opcode, displacement, copied source and destination cell, ordinal,
result address, count, call, stack restoration, and failure branch.

The focused self-test proves deterministic repetition, ordinal 31, slot 2,047,
unchanged output on invalid input, truncation rejection, and mutations to the
context load, entry load, first cell, and return path. It builds as a 20,798-byte
WVB with SHA-256
`f666fbdb0217b8eacf8ec57719feede8df1d2e2a949820ec79357f7eb0b840a2`
and a 194,819-byte verified test WVO with SHA-256
`41f580aff278d1e34e0def0cd80974a1dd92269833ffdfb6dd38e68a54f3b9a6`.
The 211,968-byte Windows package executes with result zero; the 212,992-byte
Linux package is constructed from the same fragment. The test treats emitted
provider-call bytes as data under ABI 22; it does not claim that ABI 22 executes
the successor call.

Publication still requires main-lowerer integration, fragment-verifier and host
admission, Windows and Linux provider leaves, and independent execution on both
hosts.
