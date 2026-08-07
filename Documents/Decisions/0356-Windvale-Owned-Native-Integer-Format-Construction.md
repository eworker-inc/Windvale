# Decision 0356: Windvale-owned native integer-format construction

- Status: Accepted current-host ownership transfer; Linux execution and grouped qualification pending
- Date: 2026-08-07
- Advances: [Decision 0071](0071-Native-Text-Arena-And-Core-Text-Services.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale native execution context](../../Specifications/Windvale-Native-Execution-Context.md#dynamic-text-and-byte-arena)

## Context

Decision 0071 moved signed and unsigned invariant formatting out of managed
callbacks and into exact shared x86-64 leaves, but one C# method still emitted
both leaves for every normal service bundle. The compiler staging profile uses
the unsigned formatter, while inspector profiles use the same shared algorithm
for both signed and unsigned inputs. Moving only one variant would split one
construction invariant and retain most of the managed implementation.

The already qualified 225-byte and 191-byte leaves are descendant inputs to
many pinned bundles and applications. This transfer therefore preserves their
machine bytes rather than choosing new equivalent encodings.

## Decision

- Add `Runtime/Windvale/Native-X64-Integer-Format-Services.wv` as one focused
  portable generator for both variants. Its `Signed` input controls only the
  established sign-normalization and minus-prefix sequence; digit production,
  arena admission, copying, result publication, and failure handling remain
  shared.
- Construct relative branches from bounded recorded patches and named block
  positions. Encode backward displacements explicitly as two's-complement
  `u32` values, without adding a source-language conversion or another native
  assembler convention.
- Add a capability-free byte-result bridge that returns the signed leaf
  followed by the unsigned leaf, plus an ordinary Project 1 closure that the
  native source front door must reproduce exactly.
- Retain that bridge WVB in the runtime. The C# recovery wrapper may verify,
  lower, execute, split, and cache it, but no longer carries the shared integer
  formatter emission algorithm.
- Preserve the native ABI, service table, text-arena contract, failure detail,
  adapters, leaf identities, placement, and all descendant bundle/application
  identities.

## Exact identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Windvale integer-format core WVB | 11,611 | `6b5b5660392a9f927d046eff41aa3470bdbc616970a0e297c2c467b53d3f1fa2` |
| Retained paired bridge WVB | 11,598 | `851f6d8e01b62106763af518c15dc163a9af9ea30c14cdb01d62adf1538ae7f9` |
| `I32ˉformat` leaf | 225 | `c33758106e8d7cd31bbed8ef1e789a8e355c52736c119c75493154a4184fa41e` |
| `U32ˉformat` leaf | 191 | `b98f2d55e30bb7369e233f94e4ade5f3e8917a7730114446f1ebc81f353e1e43` |

## Evidence and consequences

Stage 0 first compiled the new closure for precise diagnostics. The ordinary
native source front door then published the same 11,598-byte WVB with the same
digest. After reviewing the focused test, the affected Release test project
built with zero warnings and errors in 20.62 seconds. The single named case
passed 1/1 in 2.213 seconds.

The test pins both source results, compares the retained WVB byte for byte,
rebuilds it through the native source front door, verifies both unchanged leaf
identities, and requires the reference interpreter and verified x64 backend to
return the same 416-byte pair. Existing dynamic-text coverage remains the
qualified semantic evidence for signed minimum, unsigned maximum, zero,
arena allocation, and failure behavior; it was not rerun for this exact-byte
ownership transfer.

The paired final qualification scripts now reproduce and inspect both modules
and compare the retained bridge exactly. Only their PowerShell and shell syntax
is checked in this slice. C# still constructs enum-name, concatenation, and
quoting leaves and still owns retained-WVB loading, native lowering, W^X
execution/publication, runtime arenas, bundle construction, and host-container
orchestration. Linux execution and the grouped broad gate remain deferred.

## Reconsideration triggers

Replace the retained bridge when native service-bundle construction can consume
the Windvale result without the managed loader/executor. Change either leaf
identity only through a new explicit runtime contract and complete descendant
qualification, not as an incidental generator refactor.
