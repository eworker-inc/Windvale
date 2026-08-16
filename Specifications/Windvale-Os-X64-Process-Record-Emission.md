# Windvale OS x86-64 init process-record emission

## Status and scope

This contract source-owns fixture offsets 1,971 through 2,432 for complete
initialization of the first protected-process record. It consumes two exact
32-byte digests and emits the retained 288-byte record construction without
depending on the retired managed bootstrap source.

The slice is privileged construction evidence. It is not a stable process ABI,
general dynamic process allocation, process publication, or proof that the
filesystem and network provider transactions are connected to the dispatcher.

## Construction

The constructor rejects either digest unless its length is exactly 32 bytes.
For valid input it clears the complete record and writes the retained magic,
version, size, ready process/thread states, identities, executable and program
digests, root, user code/stack/data addresses, resource budgets, capability
reference and rights, capacity-one endpoint, role, stack geometry, runtime
profile and kind, process generation, and service-response address.

The current init profile uses process/thread/generation 1, role 1, resource
owner profile 2, AOT-service kind 1, eight memory pages, 64 instructions, one
provider handle, nine syscalls, capability rights 46, two code pages, and one
stack page. Its user addresses derive from the already validated 12-page extent
at pages 4, 6, 7, and 11. The endpoint address derives from state offset 1,168.

Digest bytes are inputs to the constructor, not embedded semantic constants.
The self-test supplies the current fixture digests so that all 462 emitted bytes
remain exact while later image changes can pass different verified identities.
This slice adds no branch or call relocation fields; the combined source-owned
prefix therefore retains 13 fields.

## Verification

`Test-Os-X64-Code-Emission` checks input-length rejection, exact geometry, four
independent bounded hashes over the complete record slice, paired deterministic
host images, and local result 55. The exact 462-byte fixture slice has SHA-256
`2a5b757c6550a381ea3a22c0edbe9d6f24e6804274cd1cab8c28be721b448b65`.

The self-test WVB is 16,069 bytes at
`be44b1d300abd532a5689755f9ab9ed75b49e7e4954395d3626ee175b9b97e13`.
Its Windows executable is 236,032 bytes at
`693ce53db751bd537ade2933adc8f688ff42492aad6091e005ea9b6391d7ff16`;
the Linux image is 241,776 bytes at
`1ecaa2ac3dda959a632b88c753d4189ecd3213a2f04c69c886f5bc0f11db23c0`.

Together with preceding slices, Windvale source reconstructs fixture offsets
zero through 2,432 and all 13 branch/call relocation fields encountered there.
The next region copies the retained kernel paging tables and constructs the
init address space.
