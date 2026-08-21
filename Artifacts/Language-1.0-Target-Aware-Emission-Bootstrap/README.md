# Language 1.0 target-aware emission bootstrap

This directory retains two portable WVBs, not native executables. They are the
current bounded split analyzer/emitter checkpoint for the fixed
`portable-wvb-optimized-v1` route.

Decision 0813 advances the pair atomically with ordinary WVIR 1.3 and
specialized WVIR 1.4. The cache family, complete cache keys, source adapters,
Project 2 identities, product sizes, and product digests are recorded in
`Manifest.json`. Dependencies are supplied as the declared root followed by
source paths sorted by ordinal manifest spelling.

The exact 992,412-byte analyzer and 895,787-byte emitter are independently
WVB-verified. The Language 1.0 front-door gate verifies the emitter size and
SHA-256 value before use, reconstructs the analyzer from current source,
packages both active products under profile 7, and gives them role-specific
version-2 producer identities. The packaged pair reproduces the emitter WVB
byte for byte.

These are development bootstrap dependencies, not release or paired-host
qualification evidence. It must not grow into a second compiler source tree or
carry checked-in Windows/Linux applications. Reconsider retaining it after a
promoted compiler checkpoint provides an equally bounded recovery path.
