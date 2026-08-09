# Decision 0466: Native WVHV request-container reconstruction

- Status: Implemented current-host candidate; independent Linux execution and promotion pending
- Date: 2026-08-09
- Advances: [Decision 0465](0465-Retained-Native-WVHV-Request-Containers.md), [Decision 0414](0414-Digest-Bound-Native-Hosted-Container-Composition.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [native hosted-container packaging](../../Specifications/Windvale-Native-Hosted-Container-Packaging.md)

## Context

Decision 0465 froze exact Windows and Linux containers for the small Windvale
`WVVE` to `WVVR` request process. Their construction still used the deletion-bound
`recovery-aot` writer. Windvale's existing native hosted-container pipeline could
already express the same compiler-profile application, but the request WVB and
its paired products were not retained in that pipeline's digest-bound toolset or
covered by its process-level reconstruction smoke.

## Decision

- Add the exact verifier-request WVB and its Windows/Linux containers to the
  native hosted-container candidate under the command name
  `wvhostverifierrequest`.
- Reconstruct both products through the existing compiler profile `1`. Do not
  add a verifier-specific launcher, binary decoder, service profile, or managed
  execution path.
- Extend the paired Windows/Linux packaging smokes to reconstruct the local-host
  product and the opposite-host product, require exact equality with the retained
  artifacts, and preserve the existing diagnostic, private-scratch, executable,
  and invalid-input checks.
- Keep the managed writers and `recovery-aot` targets until independent Linux
  evidence and the grouped retirement gate qualify deletion.

## Evidence and consequences

The candidate now binds 60 artifacts: 20 native-built WVBs and their paired
Windows and Linux applications. Its 5,728-byte `SHA256SUMS` has SHA-256
`7e06db3950f3f89edfff09afd7a081ac9fccff49844f184cbbc58771acda2379`;
all 60 entries match. Including the manifest and inventory, the candidate has
62 files totaling 16,046,685 bytes.

The reviewed Windows smoke passes 5/5 in 28.6 seconds. The new cases package
the retained 17,204-byte verifier-request WVB through the ordinary native
hosted-container composer and reproduce exactly:

| Target | Bytes | SHA-256 |
| --- | ---: | --- |
| Windows x64 | 187,904 | `dc42cd573e26ba8617a7323089f2c140f0488ec0cb3b9a6e4b77d5c4d7fbd4d5` |
| Linux x64 | 188,416 | `cfd0071c3d103ca0feedb33370b81bc3edb0b41e8bb95ee9744d1b53342fb6bd` |

No C# process participates in that reconstruction. The paired Linux script
passes shell syntax review but did not execute on this Windows host. Genuine
Linux execution, the six-service verifier bundle process, verifier startup and
final publication, grouped retirement qualification, ordinary-path promotion,
and recovery-source deletion remain. No broad Seed, OS, Standard,
Qualification, WebAssembly, or QEMU gate ran.

## Reconsideration triggers

Version and repin the candidate when the request WVB, fixed service leaves,
startup object, application profile, or container formats change. Delete the
managed request-container writers only after the grouped Windows/Linux gate has
qualified the native products and recovery provenance is archived.
