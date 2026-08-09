# Decision 0475: Native WVVP metadata ownership

- Status: Implemented current-host candidate; native container integration pending
- Date: 2026-08-09
- Advances: [Decision 0474](0474-Native-WVHV-Application-Publication.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [native hosted-verifier publisher metadata](../../Specifications/Windvale-Native-Hosted-Verifier-Application-Publisher-Metadata.md)

## Context

Decision 0474 moved completed verifier admission and permanent publisher
execution into Windvale, but frozen C# still constructs the surrounding
publisher application. The first publisher-container-specific serialized value
is its 128-byte `WVVP` record. Leaving those fields implicit in the C# writer
would force later native layout work to rediscover a hidden contract.

The record includes the publisher WVB digest. Therefore its constructor cannot
be imported into that WVB without making the output digest depend on itself.

## Decision

- Define `WVPM 1`, an exact 112-byte request carrying target, fixed startup and
  native-function offsets, target startup digest, and publisher-module digest.
- Construct the exact `WVVP 1` record in a standalone portable Windvale module.
- Keep admission in a separate focused source file and require construction to
  admit its completed output before returning success.
- Pin the current publisher module, Windows/Linux startup, and private
  transaction-function evidence. A changed publisher requires an explicit new
  request identity and metadata repin.
- Keep this tool separate from the publisher module to avoid digest
  self-reference. Do not add a parallel hash implementation or new C# logic.

## Evidence and consequences

The native compiler builds a 10,441-byte service-free WVB with SHA-256
`208b2724a10f2e497ef13be51d254426e86afda99600c61dd937cdf4171d3bbd`.
One reviewed focused test passes 1/1 in 2.505 seconds after the incremental
build. Interpreter and native execution agree for both targets, and their exact
128-byte results equal the records embedded in the committed publisher
applications. Nine malformed request cases cover envelope, target, geometry,
reserved, startup-digest, and module-digest rejection.

This removes `WVVP` field ownership from the future native reconstruction gap;
the C# writer remains the frozen differential oracle and current Stage 0 outer
container constructor. Native discovery of `Main` and private transaction
symbols, admission/instantiation of the five WVO assets, publisher-specific
Windows imports and target layouts, final PE/ELF materialization, independent
Linux execution, and grouped promotion remain. No broad Seed, OS, Standard,
Qualification, WebAssembly, or QEMU gate ran.

## Reconsideration triggers

Version this boundary when the publisher module/startup identities, native
function offsets, capability count, snapshot limit, or publication transaction
version changes. Never import the constructor into a module whose digest it
must serialize.
