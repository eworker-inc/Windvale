# Language 1.0 target-aware emission bootstrap

This directory retains two portable WVBs, not native executables. They bridge
the last bounded split analyzer/emitter pair into the current fixed
`portable-wvb-optimized-v1` route without widening the native package or hosted
execution limits.

Both source closures use exact base commit
`49717f4dda27db2235033827ac164fac080ca623`. The only overlay changes the
hosted emission adapter's `Optimize` argument from `false` to `true`; the
analyzer is unmodified. All source and Project 2 identities are recorded in
`Manifest.json`. Dependencies are supplied as the declared root followed by
source paths sorted by ordinal manifest spelling.

The digest-pinned native reconstruction compiler produces the exact
949,355-byte analyzer and 746,557-byte emitter WVBs. The Language 1.0 front-door
gate verifies both sizes and SHA-256 values before use, packages them
independently on the active host, and gives them role-specific version-2
producer identities. It uses the pair only to analyze and emit the oversized
current compiler closure. Ordinary Language 1.0 fixtures use the separately
reconstructed current analyzer and current target-aware emitter.

These are development bootstrap dependencies, not release or paired-host
qualification evidence. It must not grow into a second compiler source tree or
carry checked-in Windows/Linux applications. Reconsider retaining it after a
promoted current compiler can reconstruct the split products within the same
bounded front door without this bridge.
