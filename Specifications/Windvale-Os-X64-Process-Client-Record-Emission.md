# Windvale OS x86-64 recyclable-client record emission

## Status and scope

This contract source-owns fixture offsets 4,341 through 4,858. It selects the
retained recyclable-client record and root, clears the complete record, and
constructs the still-private process, thread, capability, execution, and
response metadata. It does not install page tables or publish the client.

## Bounded construction

The constructor accepts exactly two 32-byte digests: the bytecode interpreter
image identity and the admitted program identity. Any other size emits no
bytes. The record uses process/thread identity 2, generation 1, native runtime
profile 7, bytecode-interpreter kind 2, 110 code pages, six stack pages, a
120-page memory budget, 189,137-instruction budget, two-handle budget, and
four-syscall budget.

The primary resource capability is generation 1 with rights 17 and endpoint
state offset 1,168. The directory capability is encoded separately as slot and
generation value 65,537, rights 17, and endpoint state offset 2,144. Code,
stack, data, and response addresses remain relative to the retained private
122-page extent. This slice adds no relocation fields.

## Verification

`Test-Os-X64-Code-Emission` validates exact digest geometry, short-input
rejection, the exact 518-byte payload, four independent bounded hashes, paired
deterministic host images, and local result 63. The payload has SHA-256
`b7f96df2b0a39f201b1c1bbe83c2cefab455c0417be19892877078839965562e`.

The self-test WVB is 16,843 bytes at
`6182088b7f1ae89766d2a8cb20b2b022a4ca54571ba63312c7111379c1b15ef3`.
Its Windows executable is 251,392 bytes at
`2cbedd60fd226415ba274cffb121b7c39505fa74a6ed854fa628770d844d406b`;
the Linux image is 254,064 bytes at
`08911fe6297712035388dd9ae1baaa9e03ddb6d905fd82aba485a33dc192f484`.

Together with preceding slices, Windvale source reconstructs fixture offsets
zero through 4,858 and all 33 relocation fields encountered there. Client page
tables, interpreter/program copies, context, and final resource records remain
private construction work before readiness publication.
