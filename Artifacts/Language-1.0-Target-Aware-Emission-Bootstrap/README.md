# Language 1.0 target-aware emission bootstrap

This directory retains two portable WVBs, not native executables. They are the
qualified current analyzer and emitter pair for the fixed
`portable-wvb-optimized-v1` route. Each product consumes the current compact
WVIR contract, so bootstrap no longer needs a transition emitter.

Decision 0896 promotes the pair atomically after the fixed point created at
commit `c02e6bd47554242b3be0a5fcd16fe9c178ab4d2d` and the paired-host evidence
closed on the `a32c9e4c` lineage. The source adapters, Project 2 identities,
product sizes, and product digests are recorded in `Manifest.json`.
Dependencies are supplied as the declared root followed by source paths sorted
by ordinal manifest spelling.

The exact 1,552,090-byte analyzer and 1,556,434-byte emitter are independently
WVB-verified. The Language 1.0 front-door gate verifies every size and SHA-256
value before use. The pair directly reconstructs both current compiler halves,
and every executable producer receives a role-specific version-2 identity.

These are bootstrap dependencies, not release artifacts or a second compiler
source tree. They do not grant source-admission authority and this directory
must not carry checked-in Windows/Linux applications. Decisions 0813 and 0846
remain historical provenance for the superseded pair and one-time compact-WVIR
bridge. On 2026-08-31 the rewritten 16-phase coordinator used an isolated empty
cache on Windows x64 and proved exact Stage 1/Stage 2 convergence from this
two-file inventory. The same path must pass on Linux before cross-host
convergence is claimed for the implementing commit.
