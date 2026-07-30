# Decision 0012: Windvale linker bootstrap prerequisites

- Date: 2026-07-30
- Status: Accepted, implemented, and cross-host qualified at `348c82a`

## Context

The accepted Windvale Linking 1 map contains a SHA-256 identity for the complete image and every ordered input object. The Windvale linker must also validate and rescan as many as 64 WVO inputs through several bounded passes. Seed has neither the bitwise operations needed to implement SHA-256 in source nor a general collection that can retain 64 independently sized byte values. Removing the map, reducing the input limit, or making the host calculate link evidence would produce a narrower linker than the qualified Stage 0 contract.

Repeated live file reads are not a deterministic substitute for collections: an input could change between validation, layout, relocation, reconstruction, and map generation. The existing 64-argument launcher limit is also three entries short of the smallest linker shell consisting of a base address, entry name, output resource, and 64 ordered input resources.

## Decision

- Advance the early-development bytecode format to WVB 1.6 and add pure opcode `0x7D`, `bytes.sha256_hex`.
- Expose that opcode as `Bytesˉsha256ˉhex(Value: bytes) -> text`. It hashes exactly the supplied immutable sequence or slice and returns 64 lowercase ASCII hexadecimal characters.
- Keep SHA-256 as a narrow artifact-identity primitive, not a general cryptography or configurable hashing API.
- Make `Referenceˉcapabilityˉhost` snapshot the first successful `file.read_bytes` result for each exact ordinal resource name in its hosted resource context. Repeated reads return that immutable snapshot without reinvoking the adapter.
- Bound a hosted resource context to 64 distinct successful file snapshots and reject a 65th with stable runtime trap `WVR3028`. Failed reads do not consume a snapshot slot.
- Increase the ordered launcher argument limit from 64 to 67 while preserving the existing per-argument and aggregate UTF-8 byte limits.
- Keep input collection, WVO parsing, symbol resolution, layout, relocation, image reconstruction, and map construction in Windvale source. These prerequisites may supply deterministic values but may not decide link semantics.

## Consequences

- The Windvale linker can reproduce the complete accepted map rather than delegating digests to C# or weakening the oracle comparison.
- A repeated-pass implementation observes one byte value per exact resource name even if the native file changes after its first successful read.
- The runtime retains at most 256 MiB of input snapshots in the theoretical worst case of 64 distinct maximum-sized resources; ordinary tools retain only the bytes they actually read.
- Two different resource names are distinct snapshot identities even when the native adapter maps them to one file. The portable program cannot infer or depend on native path aliasing.
- Every canonical WVB module digest changes with the 1.6 header. Current fixtures and both host reports must move together; no compatibility reader is added for obsolete development versions.
- SHA-256 may later move from a compiler-recognized intrinsic into a Windvale Foundation module after bitwise operations and bounded collections can express and qualify it without changing its result contract.

## Reconsider when

- Representative linker execution exceeds practical instruction, memory, or repeated-pass costs and a bounded collection or builder has measured justification.
- A streaming input contract can preserve one immutable logical snapshot while reducing retained memory.
- Windvale source can implement SHA-256 within accepted limits and match the intrinsic on Windows, Linux, and Windvale OS.
- A future tool needs more than 67 launcher arguments and a structured manifest is demonstrably simpler than another fixed-count increase.
