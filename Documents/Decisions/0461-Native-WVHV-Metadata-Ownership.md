# Decision 0461: Native WVHV metadata ownership

- Status: Implemented current-host candidate; native process composition and dual-host promotion pending
- Date: 2026-08-09
- Advances: [Decision 0460](0460-Native-Hosted-Enum-Directory-For-Variants.md), [Decision 0213](0213-Stage0-Semantic-Freeze-And-Native-Front-Door.md), and [Decision 0057](0057-Windvale-Native-Execution-And-Dotnet-Retirement.md)
- Contract: [native hosted-verifier metadata](../../Specifications/Windvale-Native-Hosted-Verifier-Metadata.md)
- Parent format: [hosted verifier application](../../Specifications/Windvale-Hosted-Verifier-Application.md)

## Context

Decision 0460 proved that the repinned hosted enum processes admit the enlarged
WVB 1.11 verifier, but also proved that compiler-family profile 2 cannot be used
for its final container. `WVHB` profile 2 is the build driver with six
capabilities and ten services. `WVHV` profile 2 is the read-only verifier with
five capabilities and six services. Treating those equal numeric values as one
profile produces a valid container for the wrong authority contract.

The first native transfer must therefore establish an explicit `WVHV` owner
without widening or redefining any compiler-family format.

## Decision

- Define `WVVR 1`, an exact 384-byte capability-free request containing target,
  profile 2, bundle/native extents, entry, native digest, and six ordered
  service placement/digest records.
- Construct the exact existing 1,024-byte `WVHV 1` metadata in portable
  Windvale, including its five capabilities, six services, format versions,
  arenas, profile, meter, adapters, flags, and reserved bytes.
- Put independent admission in a separate focused Windvale module and require
  the constructor to admit its own completed metadata before success.
- Keep the byte-input bridge separate and small. Do not add these rules to the
  compiler-family metadata file merely to reuse its numeric profile field.
- Make the focused differential test obtain production WVB through the native
  project front door. The frozen C# source compiler is not a prerequisite for
  this new source; C# remains only the post-WVB runtime/metadata oracle.

## Evidence and consequences

The native project front door builds 21 functions and 19,347 code bytes into a
21,566-byte WVB with SHA-256
`dc7c88f8ec9b6ddd77695b7890eeb6292314fcabd4939239c273908f3afa894b`.

One reviewed focused test passes. Windows and Linux requests execute identically
under the Windvale interpreter and native backend, and both successful metadata
values equal the established C# `WVHV` builder byte for byte and pass its
independent verifier. Fifteen malformed request cases cover the serialized
boundary and agree between both Windvale execution modes.

The first attempted test correctly demonstrated the Stage 0 freeze: the C#
source compiler rejected the new module's control-flow lowering while the
native Windvale compiler built it. The permanent test was corrected to use the
native compiler rather than changing valid Windvale source to accommodate the
frozen bootstrap compiler.

This closes portable `WVHV` metadata construction/admission only. Native request
production, the six-service bundle path, runtime header, verifier startup WVO,
layout/plan, platform bytes, segmentation/publication, independent Linux
execution, and artifact promotion remain. No broad Seed, OS, Standard,
Qualification, WebAssembly, or QEMU gate ran.

## Reconsideration triggers

Extend the request only when another named read-only profile is transferred and
its different service set is implemented. Do not infer verifier-family
authority from compiler-family profile numbers.
