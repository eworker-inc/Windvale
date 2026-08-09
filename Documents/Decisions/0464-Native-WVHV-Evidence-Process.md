# Decision 0464: Native WVHV evidence process

- Status: Implemented current-host candidate; retained request-tool containers and dual-host promotion pending
- Date: 2026-08-09
- Advances: [Decision 0463](0463-Native-WVHV-Metadata-Request-Ownership.md), [Decision 0414](0414-Digest-Bound-Native-Hosted-Container-Composition.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contracts: [hosted metadata-request producer](../../Specifications/Windvale-Native-Hosted-Metadata-Request.md) and [verifier metadata request](../../Specifications/Windvale-Native-Hosted-Verifier-Metadata-Request.md)

## Context

Decision 0463 transferred pure `WVVR 1` projection, but its test still supplied
seven digests from C#. The existing native hosted metadata-request process
already protects immutable resource names, reads bounded chunks, streams
SHA-256 over logical regions, binds those regions to a publication plan, and
preserves output on rejection. Duplicating that trust boundary for the
verifier would create a parallel hashing path.

## Decision

- Add distinct 32-byte `WVVI 1` inputs for target, verifier profile 2, native
  entry, and zero reserved bytes.
- Extend the existing native metadata-request process to accept exact
  six-service publication geometry and seven immutable regions, then emit
  exact 352-byte `WVVE 1` evidence.
- Keep `WVVR` construction in the separate service-free Decision 0463 module.
  Add a small hosted wrapper that reads `WVVE`, invokes that constructor,
  validates its successful response, and writes only the 384-byte request.
- Preserve the established ten-service `WVMI` to `WVHM` behavior and repin its
  exact native-built WVB and deletion-bound Stage 0 package identities.
- Keep the verifier branch in the existing hashing-tool source temporarily.
  The native source composer rejects the otherwise valid extracted module at
  `Source_bindings`; extract it when that documented compiler limitation is
  removed instead of splitting the file into numbered fragments.

## Evidence and consequences

The extended hashing tool is an exact 68,641-byte native-built WVB at SHA-256
`c90fc5f817454a48c76b476d68fc4460426ba3bce9a787114b693600c4dbe784`.
Its retained Stage 0 packages are 1,100,800-byte Windows SHA-256
`cd22508c0f933d60cf1ed1850c2c45002fb0093c02b4a4befe778c5f040e07cf`
and 1,101,824-byte Linux SHA-256
`df370d3434a946784f337a4a7a18eb847a5ec694d7b6fe886c2e89f79b3b301e`.
The focused request wrapper is a 17,010-byte native-built WVB at SHA-256
`955907aa104c057d89071ee386d00913c05077bc4463c88a6d14547bd0539fad`.

One reviewed focused test executes both WVBs as native x64 for Windows and
Linux verifier resources. The first process hashes the real verifier fragment
and six service leaves into 352 bytes; the second produces the exact 384-byte
frozen-oracle `WVVR`. The established metadata-request test also passes with
its original mode and repinned PE/ELF identities.

This closes immutable verifier identity acquisition and request projection at
the native WVB/process boundary. Retained Windows/Linux containers for the
small request wrapper, six-service bundle request/materialization, startup,
outer container construction, independent Linux execution, and promotion
remain. No broad Seed, OS, Standard, Qualification, WebAssembly, or QEMU gate
ran.

## Reconsideration triggers

Extract the verifier evidence branch as soon as the native source composer
accepts the same graph. Change the evidence format only with a new authority or
immutable-resource requirement.
