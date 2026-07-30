# Decision 0011: Deterministic flat-image linker

- Date: 2026-07-30
- Status: Accepted, implemented, and cross-host qualified at `9c4b9f5`

## Context

The qualified Windvale assembler now emits canonical WVO objects with unresolved imports and zero relocation placeholders. Extending the assembler to resolve symbols or choose addresses would collapse two independently useful tools and make layout policy part of WVA. Jumping directly to PE, ELF, or UEFI would mix portable link semantics with a large host or firmware container before the project has evidence for its ABI and loader requirements.

The next slice must expose the actual collection, resolution, checked-address, layout, and reporting pressure that a Windvale-written linker will face while producing a small artifact that later image adapters can consume.

## Decision

- Add `Linker/Windvale.Linker/` as a dependency-light C# Stage 0 oracle that consumes only verified WVO 1.0 bytes.
- Define Windvale Linking version 1 and the raw `flat-x86-64-v1` memory-image target in `Specifications/Windvale-Linking.md`.
- Make input order semantic and explicit; never derive it from host directory enumeration.
- Resolve imports only against one unique same-kind export. Keep local symbols object-private and require every import to resolve.
- Lay out contributions by WVO kind, input order, and source section order, aligning actual addresses and materializing padding plus BSS as zero bytes.
- Require one exported function entry and expose its address in the link result and canonical map rather than embedding loader policy in the raw image.
- Apply `absolute-u32` and `relative-i32` with checked integer arithmetic and exact WVO formulas.
- Reconstruct and compare the complete linked image through a separately implemented verifier before returning bytes.
- Emit a bounded ASCII/LF canonical map containing input digests, placements, resolutions, addresses, relocation values, and the image digest, but no paths or timestamps.
- Keep PE, ELF, UEFI, ABI, library search, dead stripping, and boot metadata outside this contract.

## Consequences

- The first linker exercises genuine multi-object resolution and both relocation kinds without pretending that a raw image is a host executable.
- Grouping by section kind exposes a simple memory policy and makes later permission regions possible, while explicit input order keeps ordering reproducible without premature archive or sorting rules.
- Materializing BSS increases raw-image bytes but makes the output an exact loadable memory snapshot and avoids an implicit loader-zeroing contract in the first target.
- The 4 MiB image and 1 MiB map limits fit current immutable byte values and bound the future Windvale implementation. Evidence from representative linker sources may justify a builder or a revised limit, but not silent unbounded allocation.
- The canonical map is reproducibility and diagnosis evidence, not debug information or a permanent executable symbol format.
- The required entry is target metadata. It does not establish a calling convention, process ABI, stack, or host interoperation promise.

## Reconsider when

- A native backend requires internal labels, RIP-relative data, 64-bit absolute addresses, section permissions, or a calling convention.
- A UEFI, PE, ELF, or Windvale OS image adapter needs metadata that cannot be derived from verified link evidence.
- Representative multi-object links exceed the bounded repeated-pass or map-construction model.
- Archive/library search or dead stripping becomes necessary for useful Foundation modules and can be specified deterministically.
