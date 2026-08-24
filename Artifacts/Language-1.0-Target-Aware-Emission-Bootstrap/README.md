# Language 1.0 target-aware emission bootstrap

This directory retains three portable WVBs, not native executables. The
analyzer/emitter pair is the bounded split checkpoint for the fixed
`portable-wvb-optimized-v1` route. The third WVB is a narrowly retained WVIR
1.9 bridge emitter used only while reconstructing the current emitter across
the incompatible 32-byte to 28-byte operation-record transition.

Decision 0813 advances the pair atomically with ordinary WVIR 1.3 and
specialized WVIR 1.4. The cache family, complete cache keys, source adapters,
Project 2 identities, product sizes, and product digests are recorded in
`Manifest.json`. Dependencies are supplied as the declared root followed by
source paths sorted by ordinal manifest spelling.

`Wvir-1.9-Bridge.patch` is the exact 29,164-byte reader-only patch over baseline
`269294c0`. Applying it in a clean checkout reconstructs the three source files
whose hashes are bound by the manifest; the ordinary baseline writer remains
unchanged so the old pair can compile the bridge once.

The exact 992,412-byte analyzer, 895,787-byte emitter, and 1,146,083-byte bridge
emitter are independently WVB-verified. The Language 1.0 front-door gate
verifies every size and SHA-256 value before use. The original pair reconstructs
the current analyzer; the packaged bridge consumes that analyzer's compact WVIR
to reconstruct the current emitter. Every executable producer receives a
role-specific version-2 identity.

These are development bootstrap dependencies, not release or paired-host
qualification evidence. The bridge has no source compatibility decoder and is
not shipped as a second compiler. This directory must not grow into a second
compiler source tree or carry checked-in Windows/Linux applications. Remove the
bridge after a promoted compiler checkpoint natively consumes WVIR 1.9 or later;
reconsider retaining the original pair when that checkpoint provides an equally
bounded recovery path.
