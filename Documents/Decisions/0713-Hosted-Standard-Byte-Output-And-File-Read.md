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
- Refresh only the segmented WVO staging-producer family whose exact source
  closure includes the native lowerer. Retain the byte-identical compiler-image
  staging and canonical-transport families.
- Refresh only the WVB-to-WVO lowerer WVB and its paired host images. Retain the
  byte-identical Return-42 and independent-metadata WVB/WVO fixtures.
- Until the public publisher admits the refreshed capability catalog, have
  affected verification owners enter through the pinned raw native build front
  door, verify the exact build-driver and lowerer WVB identities, and package
  those verified intermediates through the existing development-cache bridge.
- Align the already-expanded database-storage owner at 54 declared cases and
  the WVDB Query capability owner at six declared cases on both hosts and in
  the verification registry; this corrects their stale 32- and five-case
  registry declarations without changing database or application semantics.

## Consequences

The canonical application WVB is 76,348 bytes with SHA-256
`4ef96f317c0ac0ca57d60c1c2b6533e6d51cc36b8adb5b481e8ec04b61b69a73`.
The current Windows application is 2,430,464 bytes with SHA-256
`16085cd263600822f693d1f57f14315f47fe4102b76b59a64e333bdcf98615b9`;
the Linux application is 2,428,928 bytes with SHA-256
`547c311b1f5398d7cc5f67d31782ccb992e98c02dd90edfe0a560b47de575beb`.

The refreshed staging-producer WVB is 531,428 bytes with SHA-256
`f91201a25fa18a673fa2e3c2df50f5c822b53b6e63849a7f18a4cea29480073f`.
Its Windows image is 7,749,120 bytes with SHA-256
`5a22020cd5000ede860cf069bcfe2054b630ad2aea4fda2ef668ef37ac5b973e`;
its Linux image is 7,749,632 bytes with SHA-256
`bd20ddf6fcd703a69b376f8cee0d05fa00c3cb0d4682b0156b02bacaed7d1475`.
The compiler-scale build-driver WVB is 1,155,121 bytes with SHA-256
`0cd519556a1cf59321b9418bfbf01643283e10e3dd111c8e2083ec0e51c4ce02`
and stages into 30,158,719 object bytes across 39 chunks plus its 492-byte
manifest.

The refreshed WVB-to-WVO lowerer is 522,025 bytes with SHA-256
`318717a608ba37360b9c39f53b9720944ab4463af4ab6a1ec9a267a6ceb85bf6`.
Its Windows image is 7,491,072 bytes with SHA-256
`85c07ef9f07b6b1351a5aa467c4e8f77de33099db9fce3c3adaf0a47191de0a3`;
its Linux image is 7,491,584 bytes with SHA-256
`deb75ead2af0d06d2357cdf88d8cf58fefd284bf4834e6489198b517f3a4908e`.

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
