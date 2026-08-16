# Decision 0713: Hosted standard byte output and file read

- Date: 2026-08-16
- Status: Implemented with paired hosted evidence
- Contract: [standard byte-output capability](../../Specifications/Standard-Byte-Output-Capability.md)
- Shell contract: [Windvale Shell 1](../../Specifications/Windvale-Shell-1.md)
- Builds on: [Decision 0704](0704-First-Portable-Standard-Byte-Output-Core.md)

## Context

The portable output core deliberately owned no host capability. Shell 1 could
therefore name `file-read` and map `cat` to it, but an ordinary application could
not preserve arbitrary bytes through line-oriented console output. Treating
stdout as a magic file name or exposing a host handle would have made the host
adapter, rather than Windvale, define the command semantics.

## Decision

- Add `standard_output.write_v1(bytes) -> bytes` to the exact Seed capability
  catalog and native ABI-23 provider-call subset.
- Define the fixed 32-byte `WVOW 1` response with exact progress, generation,
  pre-dispatch rejection, and post-dispatch uncertainty.
- Keep response decoding in a capability-free portable module and feed admitted
  responses through the existing portable output state machine.
- Bind one fixed rights-limited stdout provider in the hosted ABI-23 wrapper.
  Windows uses the admitted `WriteFile` target and private stdout handle; Linux
  uses the admitted private descriptor and `write(2)`. Neither adds text
  semantics or exposes its native resource.
- Implement `file-read <name>` as an ordinary Windvale application over one
  immutable-directory instance and standard byte output. Read at most 3,072
  bytes per directory call, append no terminator, and reject files larger than
  4 MiB before any output.
- Retain `cat` as the existing one-step Shell 1 alias to canonical identity
  `file-read`; do not duplicate the application.
- Register `file-read-application` as a 32-case focused owner: 20 hostile
  response-decoder cases and 12 real application cases, with deterministic
  Windows/Linux images and local execution on each host.

## Consequences

The canonical application WVB is 76,348 bytes with SHA-256
`4ef96f317c0ac0ca57d60c1c2b6533e6d51cc36b8adb5b481e8ec04b61b69a73`.
The current Windows application is 2,430,464 bytes with SHA-256
`16085cd263600822f693d1f57f14315f47fe4102b76b59a64e333bdcf98615b9`;
the Linux application is 2,428,928 bytes with SHA-256
`547c311b1f5398d7cc5f67d31782ccb992e98c02dd90edfe0a560b47de575beb`.

This proves the hosted provider and application, not active-generation package
publication, browser execution, terminal sessions, pipelines, or a Windvale OS
stdout provider. The pinned public build front door predates the new catalog
entry; the focused owner reconstructs a current temporary build driver and
lowerer from native source. Promoting those identities into the public front
door remains a separate product decision.

## Reconsideration triggers

Revisit the fixed provider generation when launch metadata can bind arbitrary
provider instances. Revisit chunk and lifetime limits only through an explicit
resource profile. Revisit hosted leaves when standard output becomes a general
terminal or pipeline endpoint, preserving `WVOW 1` completion semantics or
versioning the contract rather than inheriting host stream behavior.
