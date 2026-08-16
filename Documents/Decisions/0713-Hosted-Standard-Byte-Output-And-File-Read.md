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
- Refresh the current compiler reconstruction candidate because the compiler's
  exact capability-catalog source closure changed. Preserve the seed compiler
  as the bootstrap input and require the refreshed WVB to converge exactly.
- Refresh the Symbols, Bindings, and WIR compiler-phase products derived through
  that fixed-point compiler; retain earlier phase products and seed-built WVB
  backend products when their exact bytes remain unchanged.
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

The seed first emits a transitional 947,975-byte Stage 1 WVB at SHA-256
`c929d5123078272e33a3c32288c770d6c20c2abc8f8800a3e0a32b8bda5c2fcb`.
That compiler emits the fixed-point 923,818-byte Stage 2 WVB at SHA-256
`49b5cbf040de4bcb22c071a5da9a4fbad47f4f0658ef910957a67b52c07607c2`;
Stage 3 is byte-identical to Stage 2. The fixed point lowers to a
27,647,511-byte canonical native image with SHA-256
`8f808c868da20fe8ffbe01a40cd8473f0cb570e825283bb938a2e37668470c8e`,
entry offset 51,356, and seven transport chunks. Its Windows image is
27,678,720 bytes with SHA-256
`6f266759e2d2524ad9ce2045cb21243538efc7bce35ab1f94a7da4009865eac8`;
its Linux image is 27,680,768 bytes with SHA-256
`7a81bc84a433bec0b2dcebd1ec3be82de120b11427687b9926ec13592231dc37`.
The fixed compiler refreshes the exact Symbols, Bindings, and WIR core products
to 443,101, 546,089, and 824,959 bytes respectively; their specifications own
the paired demo/tool identities and the Seed front-door owner verifies the
entire derived set on both hosts. Seed reconstruction also refreshes the WVB
backend core and demo products to 949,516 and 954,602 bytes while the
947,975-byte tool remains the distinct transitional Stage 1 compiler input.
Direct source reconstruction of the WVB Inspector refreshes to a 94,327-byte
WVB, while its qualified native-front-door artifact and locked package retain
their frozen 76,527-byte input pending a separate promotion decision.
The import-free WebAssembly interpreter retains its bytes and semantics while
the refreshed transitional compiler recalibrates its exact outer instruction
counts to 80,934,868 at guest instruction one, 82,361,677 and 82,362,870 at
the retained 1,229/1,230 boundary, and 176,224,036 at guest instruction
100,000. The same guest statuses and one-step boundary remain mandatory.
The compiler-capacity bundle now admits the fixed 923,818-byte compiler and
the 944,499-byte portable-memory compiler. Its six exact phase counts are
3,457,500,887, 1,833,665,843, 2,154,084,939, 2,935,778,319,
1,707,878,698, and 3,288,145,424 for the fixed compiler, and 3,497,297,850,
1,825,115,411, 1,283,949,057, 4,096,102,353, 1,665,016,802, and
3,358,791,985 for the portable-memory compiler. Every phase returns the same
one-byte admission result within the unsigned 32-bit meter.

This decision's initial staging-producer refresh produced a 531,428-byte WVB.
Subsequent segmented-storage and persistent-transaction lowering independently
advance the current producer to 532,490 bytes with SHA-256
`72d738268580584b967deca648bb12bc80bf3243d10600921dfc8ddf670be623`.
Its current Windows image is 7,756,800 bytes with SHA-256
`6cc939dc3f3e319f036d633626e867078c490564db83814add90b31936bc2bfd`;
its Linux image is 7,757,824 bytes with SHA-256
`7b9d1b1124b0d7cb09bc9b3d9bfd7c916e7272a40d3e029a39b444c788e1b758`.
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

The fixed-point compiler leaves the 927-byte Echo WVB and both hosted Echo
applications byte-identical. Recording the new compiler identity changes its
940-byte lock to SHA-256
`b2bd85d1f76d062c45a8f5ee8ce531b87aeaf499440f1fdf52a4a8a5aca317f3`,
its 454-byte provenance record to SHA-256
`ce1e48b24a462f632808b5678d1cb7ece0dd290e29cd6e611b426bb6ff3c3383`,
and its 17,009-byte bundle to SHA-256
`9abc97a4088ed60ba26015909ed4375ce92e27e9280fbe8be892c1b14ee7eb85`.
The resulting 793-byte approval has SHA-256
`cf2bf11b8b737466fad088e383004ee3fbdef45609ff046022fa6bf4a5c232b9`;
the Windows and Linux launch records have SHA-256
`39839a75c852c46eec896bfe47f8c43228d5e2fff650a722ea72f08f55e7a8b8`
and `1010e131f66c45dec68b29b2f2797bc6ef47c4c6c3b83554f1e0872949a670fb`.
Capability-aware package tooling also refreshes: the 284,755-byte writer is
SHA-256 `ccffc57e6a18b7a14b2aeecc0ff5ef38a0a9bd8206ea429ebf9d9b93c678296c`,
the 304,048-byte verifier is SHA-256
`1e37b48c182690b600d1310feb7d057ef337ebc4f962499eeb031116f22e64d8`,
and the 332,593-byte bundle self-test is SHA-256
`2c12fb139ebe89a2d206418a3ded6f73a948838b4b06d5df5de954214e4837ab`.
The historical WVDB Query and WVB Inspector bundles remain byte-identical.

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
