# Windvale OS x86-64 generation-2 client-record emission

This contract source-owns fixture offsets 19,742 through 20,240. It clears the
entire retained recyclable-client record and reconstructs its process, thread,
runtime, capability, resource, response, and generation metadata over the newly
allocated generation-2 physical root.

The 499-byte payload fixes process/thread identity 2, generation 2, runtime
profile 7, interpreter kind 2, 110 code pages, six stack pages, the 120-page
memory budget, the 189,137-instruction limit, retained resource and directory
endpoint bindings, pinned interpreter/program digests, and all private extent
addresses. It has no branch or external relocation fields. Its SHA-256 is
`9ec3a038c6580b02d5b76cf7e60fdcfc6cc4a4a03ba9f57c6ca3495d176224fd`.

The WVB is 2,246 bytes at
`408a51f39da581efc0ece5c54ba34207c553c82186cc218ae98c64e2a3b30030`;
Windows is 15,872 bytes at
`9ad20271c35b181ead51fd1ff3d84e3d8f83cf44183092c601c72f81a644b85c`;
Linux is 20,592 bytes at
`aa3700448dfb9d450afc48c64ed20d49f19ad780efe8ecd29031ecbdbba2c7b2`.
The focused owner validates exact payload geometry, four bounded hashes, both
host images, and result 90. Combined ownership reaches byte 20,240 with 120
external relocation fields.

This proves private generation-2 record reconstruction only. Its page tables,
images, resources, execution context, endpoint rebinding, and ready publication
remain separate transactions.
