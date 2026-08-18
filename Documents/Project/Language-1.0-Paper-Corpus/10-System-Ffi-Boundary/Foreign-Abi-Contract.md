# Workload 10 registered foreign ABI contract

## Identity and target

Canonical ABI-contract identity:
`windvale.paper.buffer_source.sysv_amd64_c_v1`.

Required target scope: `linux.x86_64.sysv_amd64_c_v1`, whose exact predicate is:

- environment identity Linux;
- architecture identity x86-64;
- ABI identity `sysv_amd64_c_v1`;
- 64-bit little-endian address/scalar witness; and
- registered no-unwind C scalar/pointer interface major 1.

The identity fixes the System V AMD64 calling convention, two's-complement
`i64`, unsigned `u64`, 8-bit `u8`, 64-bit pointer representation, argument and
return placement, stack alignment, callee-saved state, symbol lookup scope,
no-retain ownership, and no-unwind containment. It is not inferred from the
compiler host or the word `linux`.

## Exact symbol and semantic signature

External symbol: `wv_paper_buffer_source_read_v1`.

~~~text
unsafe foreign "windvale.paper.buffer_source.sysv_amd64_c_v1"
fn Readˉforeignˉrecord(
    Destination: Foreignˉpointer<u8, Bufferˉsourceˉabi>,
    Capacity: u64,
    Expectedˉgeneration: u64,
) -> i64 effects(ffi.call)
as "wv_paper_buffer_source_read_v1";
~~~

The C-facing shape is equivalent to a no-unwind
`int64_t (uint8_t*, uint64_t, uint64_t)` under this exact contract. That phrase
is explanatory; the registered identity and Windvale signature are normative.

`Destination` is non-null, 8-byte aligned, writable for exactly `Capacity`
bytes, exclusively borrowed for the call, initialized before the call, and not
retained. The caller always passes capacity 64. The callee may write within that
region only.

## Return contract

| Return | Meaning | Admitted bytes |
| ---: | --- | --- |
| `0..64` | complete | exactly the returned prefix is the record input |
| `-1` | rejected before semantic completion | scratch is not observed |
| `-2` | named foreign failure | scratch is not observed |
| `-3` | stale generation | bytes 0..7 contain observed generation as little-endian u64 |
| any other negative | invalid foreign status | scratch is not observed |
| above 64 | invalid foreign completion length | scratch is not observed |

The adapter never retries. Completion is local to this buffer-producing
interface and does not imply persistence, remote receipt, or an external commit.

## Unwind and memory-safety boundary

The contract forbids unwind across the symbol. An admitted recoverable foreign
condition is an i64 return. A forbidden unwind, write outside capacity, retained
pointer, use after return, stack/calling-convention violation, or corrupted
callee-saved state follows the target's terminal containment policy.

Those violations may have destroyed process integrity before Windvale regains
control. Converting them into `Result` would be a false safety claim. Tests use
an isolated shim/process to prove containment; ordinary workload cases inject
bad returned values and in-range bytes only.

## Authority

This paper symbol is an explicit build-bound System contract and grants no
filesystem, network, device, process, clock, entropy, or allocator authority.
A real foreign adapter that accesses one of those domains additionally declares
and receives the corresponding capability/system grant. `unsafe` never supplies
that grant.
