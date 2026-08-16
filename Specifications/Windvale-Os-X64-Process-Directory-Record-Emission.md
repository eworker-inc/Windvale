# Windvale OS x86-64 directory-provider record emission

## Status and scope

This contract source-owns fixture offsets 3,323 through 3,784. It completely
initializes the isolated directory provider's 288-byte process record from two
verified 32-byte identities before private paging or publication.

This slice is privileged construction evidence, not a stable process ABI or
proof that the provider is runnable. Its service and snapshot identities remain
constructor inputs rather than embedded kernel constants.

## Record construction

The constructor rejects either identity unless it is exactly 32 bytes. It
clears the complete record, then writes retained magic/version/size, ready
states, process/thread/generation 3/3/1, the verified service identity, root,
code pages 4–5, stack page 6, data page 7, six-page memory budget, instruction
budget 64, one provider handle, five syscalls, directory capability reference
`0x00010001`, rights 46, capacity one, directory endpoint at state offset 2,144,
directory role 3, one stack page, directory-owner profile 4, the verified
snapshot identity, two code pages, AOT-service runtime kind 1, and response
page 9.

The slice adds no branch, call, or data relocation fields.

## Verification

`Test-Os-X64-Code-Emission` validates identity-length rejection, the exact
462-byte record, four independent bounded hashes, paired deterministic host
images, and local result 60. The exact slice has SHA-256
`8f76ecc8d4d2b74a55c1cc26ffd78f4f1e4ec9bf53847bbf5034a489a33c1b60`.

The self-test WVB is 16,076 bytes at
`b549bbb7566023e09cb8dfa65ad774c6c99a6d4cb4b5f7239d0be317833d40b3`.
Its Windows executable is 236,032 bytes at
`865f82f369212f100f46d8e630bfef5a1aa5468e211ac8e15258bfe7c95f4b19`;
the Linux image is 241,776 bytes at
`b4c32f4820655131c2ba596f8003d78c3ffd16179a599c4f4fe77c9e36267e23`.

Together with preceding slices, Windvale source reconstructs fixture offsets
zero through 3,784 and all 29 relocation fields encountered there. The next
region copies retained kernel tables and constructs the directory provider's
private W^X mappings.
