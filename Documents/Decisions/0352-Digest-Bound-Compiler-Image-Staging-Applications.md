# Decision 0352: Digest-bound compiler-image staging applications

- Status: Accepted current-host candidate; Linux execution, publisher-scale transfer, native container reconstruction, and grouped qualification pending
- Date: 2026-08-07
- Advances: [Decision 0351](0351-Immutable-Snapshot-Compiler-Image-Staging.md) and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [Windvale linking](../../Specifications/Windvale-Linking.md#hosted-immutable-snapshot-staging-boundary)

## Context

Decision 0351 proved the hosted Windvale tool through the reference capability
host and native-fragment verification, but it did not construct or execute a
real host process. The existing compiler-capacity containers already provide
the required 64 immutable file-input snapshots, bounded dynamic arena, process
arguments, output services, and Windows/Linux adapters.

The tool calls nine services. Existing WVB-to-WVO container metadata fixes ten
canonical service records, including a UTF-8 adapter slot used by that profile.
Changing generated Windvale code merely to call an otherwise unused service
would misstate its actual dependency. A focused package profile must validate
the nine-service fragment while retaining the established ten-record container
layout as infrastructure.

## Decision

- Add one focused native service-bundle builder for compiler-image staging. It
  accepts only the exact nine-service generated fragment and builds the
  established ten-placement hosted bundle with UTF-8 in its canonical slot.
  The extra placement is not a generated-code call or capability grant.
- Add a focused Stage 0 application contract and writer with target names
  `windows-x64-compiler-image-staging-v1` and
  `linux-x64-compiler-image-staging-v1`.
- Bind construction to the exact hosted module name, profile, six declared
  capabilities, nine generated services, one exported `Main`, complete tool
  WVB length/digest, complete application length/digest, entry offset, service
  bundle, and independent container verification.
- Retain the existing compiler-capacity runtime layout. Do not add another
  startup, service implementation, file parser, or platform-specific linker.
- Execute only the current-host candidate on the small eight-chunk fixture.
  Require every source file to remain exact, all four image chunks and `WVLI`
  to match, and no CLR, hostfxr, hostpolicy, or dotnet component to load.
- Keep the package writer as explicit Stage 0/recovery construction debt. These
  candidates do not qualify native PE/ELF reconstruction or ordinary target
  promotion.

## Exact identities

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Compiler-image staging WVB | 75,337 | `855983284c088cd795c119fe0c392308824066b10a9173dceb7cdc2daa219101` |
| Windows x86-64 application | 849,920 | `c6315f74f0a674e8d0cbb6e64e80c97d409a500551f51b6ce3d7fa618ca00f6e` |
| Linux x86-64 application | 851,968 | `f93db63052605ebb61ce934b351ad45fe7386d134325af8e1a8abb93bc64dd9f` |

## Evidence and consequences

After reviewing the final process test, the exact named selection passes 1/1
in 2.328 test seconds after a 7.94-second zero-warning Release build of the
affected test project. Both digest-bound containers pass independent structural
verification. The Windows candidate runs the complete small staging command,
preserves the `WVOP` and all eight source chunks, produces the four exact
linked chunks plus the 76-byte `WVLI` manifest, and loads no CLR component.

The first package attempt failed before construction because the general
WVB-to-WVO bundle correctly rejected the tool's narrower nine-service fragment.
The focused bundle profile resolved that mismatch without widening generated
authority. A measurement run then passed in 2.808 test seconds and established
the identities above; the final run replaced measurement-only construction
with the digest-bound package writer.

C# changes are limited to the temporary service-bundle/package construction
path and test orchestration. Linking, independent image verification, resource
policy, manifest construction, and process behavior execute in Windvale and
the existing fixed native services. No WebAssembly implementation or platform
assembly changed.

The 6,449,889-byte publisher WVO did not run through this process. Linux
execution, malformed/failure process cases, native file-identity binding,
durable publication, canonical map output, native host-container construction,
Development, Standard, Qualification, and the grouped retirement gate remain
deferred.

## Reconsideration triggers

Revisit the package profile if hosted metadata becomes variable-length, the
tool's actual service set changes, the compiler-capacity snapshot layout
changes, or a native package constructor replaces this Stage 0 writer. Do not
silently drop digest binding or treat structural Linux construction as Linux
execution evidence.
