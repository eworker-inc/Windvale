# Decision 0474: Native WVHV application publication

- Status: Implemented current-host candidate; independent Linux qualification pending
- Date: 2026-08-09
- Advances: [Decision 0473](0473-Native-WVHV-Startup-Admission.md), [Decision 0470](0470-Native-WVHV-Container-Composition.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [native hosted-verifier application publisher](../../Specifications/Windvale-Native-Hosted-Verifier-Application-Publisher.md)

## Context

The native composer already reconstructs both completed verifier applications
byte for byte and executes the current-host result without .NET. Durable release
installation still lacked a bounded admission seam, so normal promotion would
otherwise have to trust a pathname or retain managed application verification.

Repeating the complete PE/ELF, runtime, startup, bundle, and relocation verifier
inside the publisher exceeded the current native source-WIR binding capacity and
would have created a second structural oracle. The existing composer already owns
that proof and produces deterministic bytes.

## Decision

- Pin the exact completed Windows and Linux application length and SHA-256 in a
  focused portable Windvale admission module.
- Treat those values as qualified release identities derived from native composer
  output, not as hardcoded semantic examples or a replacement structural parser.
- Reuse the existing five-capability Windvale publisher module and native durable
  publication adapter after admission succeeds.
- Keep transaction bridge functions private because the frozen container writer
  binds their private native function symbols.
- Keep C# limited to the frozen Stage 0 publisher-container constructor and exact
  identity contract. Do not add new source-language or admission semantics there.
- Require an explicit composer requalification and repin whenever either completed
  verifier identity changes.

## Evidence and consequences

The completed verifier candidates are 1,004,032 Windows bytes with SHA-256
`aea110110300870cd4f8e3dfcae98de24d90678dd33bfc8584351f58028ff34a`
and 1,003,520 Linux bytes with SHA-256
`26a35ed3f0221968cee45b7cf5dc3fdad4b1e60c754b95928bd74559da65ec0b`.
The already-passing composer contract proves these exact bytes equal its native
format-4 construction outputs.

The native-built admission and publisher WVB identities are 18,091 and 29,170
bytes. Frozen recovery construction produces a 256,000-byte Windows publisher
and 254,917-byte Linux publisher. Digest-bound current-host launchers and the
native publisher-rejection suite make invalid admission permanent no-.NET
evidence. The focused Seed contract additionally owns successful durable
publication, installed-verifier execution, no-.NET module evidence, corruption
preservation, and zero scratch.

This closes standalone completed-application admission and current-host durable
publication. It does not claim native publisher-container construction,
independent Linux execution, grouped qualification, ordinary-path promotion, or
release completion. Broad Seed, OS, Standard, Qualification, WebAssembly, and
QEMU gates remain intentionally deferred to the final retirement goal gate.

## Reconsideration triggers

Reconsider this boundary when the verifier application identities change, the
shared publication transaction is versioned, the candidate snapshot limit no
longer covers the verifier, or a native publisher-container constructor replaces
the frozen Stage 0 writer.
