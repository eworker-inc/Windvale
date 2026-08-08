# Decision 0363: Direct native enum-name leaf consumption

- Status: Accepted current-host normal-path loader reduction; Linux execution and grouped qualification pending
- Date: 2026-08-07
- Advances: [Decision 0359](0359-Windvale-Owned-Native-Enum-Name-Leaf.md), [Decision 0362](0362-Windvale-Owned-Segmented-Native-Enum-Metadata.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale native execution context](../../Specifications/Windvale-Native-Execution-Context.md#dynamic-text-and-byte-arena)

## Context

Decision 0359 made Windvale the source owner of the fixed 323-byte enum-name
leaf. The managed runtime nevertheless embedded its 592-byte generator WVB,
decoded and verified that module, lowered it through the Stage 0 x64 backend,
allocated a temporary executable image, invoked `Main() -> bytes`, copied the
result, and released the image the first time the service was needed.

That live construction is useful qualification evidence but unnecessary in a
normal runtime: the source is a fixed exact machine template and the generated
leaf already has a qualified immutable identity. Keeping the generator in the
ordinary path retains more managed compiler and publication machinery than the
service itself needs.

## Decision

- Retain the Windvale core source and 592-byte WVB as reproducible provenance
  and recovery evidence. They remain subject to exact source-front-door,
  interpreter, backend, and identity checks.
- Add the exact generated 323-byte leaf as a named runtime artifact with
  SHA-256 `fb05590c5b6e1791380ba288c4112387e791a18722428c90276796bd409d130a`.
- Embed that leaf, not the generator WVB, in the normal runtime assembly.
- Read the leaf once through the existing thread-safe cache, require exact size
  and digest before use, and append only the independently constructed and
  validated `WVEN` block.
- Remove normal enum-leaf calls to the managed WVB codec, x64 lowerer, temporary
  executable allocator, generator invocation, result copier, and teardown.
- Preserve the final service-bundle W^X publication, service-table placement,
  ABI, leaf bytes, metadata bytes, arena behavior, and runtime failure details.

## Evidence and consequences

The reviewed focused test still compiles the core and bridge through Stage 0,
pins both WVB identities, compares the retained provenance WVB, and reproduces
it through the ordinary native source front door. The reference interpreter
and verified x64 backend both return the same 323 bytes, and those bytes now
must equal the embedded leaf artifact and the prefix of a complete real enum
service bundle. All segmented metadata, malformed request, lexical worst-case,
and greater-than-4-MiB evidence remains in the same test.

The focused Release project built with zero warnings and errors in 9.57
seconds, and the single named test passed 1/1 in 4.277 seconds. A direct
manifest-resource inspection confirms the runtime assembly contains the `.bin`
leaf and does not contain the enum-name generator WVB. The qualification scripts
also pin the retained leaf's exact size and digest; only their syntax is checked
in this slice.

The normal enum-name leaf path no longer needs managed WVB loading, lowering,
or temporary execution. C# still projects and transports segmented enum
metadata, loads and executes that variable-input constructor WVB, validates and
assembles the complete service bundle, and owns final W^X publication. The
other fixed Windvale-owned service leaves still use their generator WVBs in the
normal managed runtime and can adopt this pattern in subsequent slices. Linux
execution and the grouped broad gate remain deferred.

## Reconsideration triggers

Regenerate the retained leaf only from the named Windvale source and update its
identity through an explicit decision. Replace the embedded leaf when native
packaging can derive and bind the same artifact without a managed assembly.
Keep generator execution as qualification/recovery evidence rather than
returning it to the normal runtime path.
