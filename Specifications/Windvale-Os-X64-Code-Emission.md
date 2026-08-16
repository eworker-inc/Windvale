# Windvale OS checked x86-64 code emission

## Status and purpose

This contract defines the first Windvale-owned construction primitive for
replacing the 46,678-byte reviewed Probe 40 process-machine fixture. It emits
bounded x86-64 byte sequences, records numeric local labels and relative
fixups, and resolves those fixups only during an explicit final build.

This is architecture-specific system tooling. It is not a source-language
semantic contract, a general assembler, executable publication authority, or
evidence that the current filesystem and network images are running.

## State and limits

`X64ˉemission` carries an immutable status, code bytes, label table, and fixup
table. A non-`Ready` state is terminal and later operations preserve it.

- Code is limited to 65,536 bytes.
- Numeric label identifier zero is invalid.
- At most 256 unique labels and 512 relative fixups are admitted.
- Duplicate labels and unresolved labels fail explicitly.
- Alignment is a power of two from 1 through 4,096 and uses x86 `NOP` bytes.
- Conditional branches accept only near-condition opcodes `0x80` through
  `0x8f` as a `u32`, avoiding host conversion or implicit truncation.

## Emission surface

The first surface owns exact byte append, little-endian `u32`/`u64`, `CALL`
and `JMP` placeholders, local near `CALL`, near `JMP`, near conditional branch,
RIP-relative `LEA RDX`, alignment, and final relative-fixup resolution. A
placeholder's displacement offset is the caller-observed code length plus one;
external WVO relocation ownership remains separate.

Relative displacements use the exact two's-complement `target - origin` bit
pattern, where origin is the first byte after the four-byte field. Final build
does not mutate the input state and fails before publishing code if any label
is missing.

## Verification

`Test-Os-X64-Code-Emission` covers forward and backward local fixups, exact
conditional and RIP-relative encodings, NOP alignment, scalar immediates,
external relocation placeholders, duplicate/zero/missing labels, invalid
conditions, invalid alignment, current-host execution, and deterministic
Windows/Linux console images. It also compiles and executes the first
source-owned process-machine consumer, which emits the exact 1,119-byte
coordinator entry and fixed three-record ready/wait dispatcher. Verification
pins its full fixture-equal SHA-256, exact entry displacement, coordinator
offset, bounded record validation, scan construction, and paired host images.

Ordered consumers now source-own the first 24,989 process-machine bytes and 120
external relocation fields through checked private construction, privileged
entry, timer activation, provider entry, provider-return/init transfer, and
init-return validation of the program, budget, and retained store/directory
backing records, followed by guarded client entry, checked return to init, and
the first reply's publication and client delivery, then transfer the exact
37-byte directory request to its isolated provider and publish its exact
3,096-byte reply, deliver it to the client, and validate and scrub the first
client's complete terminal IPC state, then revalidate all retained client state
before reclamation, then release generation 1 and prove same-root generation-2
allocation, then privately reconstruct the generation-2 client record and reuse
the exact checked paging constructor. The focused owner executes forty-two
projects and 252 cases with local results 50 through 91.

The module is a migration primitive. Coordinator relocation and channel/
endpoint initialization are separate consumers. Generation-2 construction,
syscall and exception handlers, context switching, and live QEMU application
execution remain before the reviewed process-machine fixture can be removed.
