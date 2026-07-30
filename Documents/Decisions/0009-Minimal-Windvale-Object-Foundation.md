# Decision 0009: Minimal Windvale object foundation

- Date: 2026-07-29
- Status: Accepted and implemented; cross-host qualification pending

## Context

A Windvale-written assembler needs to construct bytes, preserve section ownership, name definitions and imports, and record fixups that cannot be resolved until layout. Writing PE or ELF directly would couple the assembler to a host image format, while adopting either as Windvale's semantic object model would import a large compatibility surface before the first instruction encoder exists. A completely architecture-neutral design would also be misleading because relocation widths and meanings are machine contracts.

Seed currently reads immutable bytes but cannot construct a dynamic byte result or persist it. Those are real prerequisites for an assembler, not general library speculation.

## Decision

- Define the compact versioned `WVO1` format in `Specifications/Windvale-Object-Format.md`.
- Identify x86-64 explicitly as the first architecture while keeping section and symbol concepts independent from PE and ELF.
- Support four derived section kinds, local/export/import symbols, function/data symbol kinds, and only `absolute-u32` plus `relative-i32` relocations.
- Require canonical record ordering, ASCII machine names, zero relocation placeholders, checked widths, strict limits, and complete malformed-input validation.
- Add a dependency-free C# object-model project as the temporary Stage 0 oracle and keep it separate from bytecode, compiler, assembler, and linker ownership.
- Add only the pure immutable byte-construction intrinsics demanded by the Windvale object writer: concatenation, fixed-width little-endian encoders, and strict UTF-8 encoding.
- Add an explicit bounded `file.write_bytes` hosted capability so Windvale-written tools can persist artifacts only when both declared and granted.
- Build a Windvale-written object core that encodes and validates the same representative object as the Stage 0 oracle, then compare exact bytes across Windows and Debian.
- Do not implement link layout or relocation application in the object writer; those belong to the linker phase.

## Consequences

- The assembler can target one small canonical object contract on every host, and later output adapters can translate linked images to PE, ELF, UEFI, or Windvale-native formats.
- The first relocation model is honestly x86-64 and four bytes wide. Future 64-bit absolute addressing requires a deliberate format revision or new relocation kind.
- Immutable byte concatenation is simple and auditable but may copy repeatedly. It is suitable for the bounded first writer; a streaming or builder abstraction should replace it only when measured assembler workloads demand one.
- Native file writing is an explicit externally visible capability. The resource name remains opaque to Windvale, while the selected host owns path interpretation and failure translation.
- WVO validation is not executable-image validation. A linker must still resolve imports, lay out sections, apply relocations, and validate final ranges.

## Reconsider when

- The first assembler needs additional relocation kinds or section metadata.
- Real object sizes make immutable concatenation an unacceptable bootstrap cost.
- PE/COFF, ELF, or UEFI output exposes a missing neutral linker concept rather than an adapter-only detail.
- Multiple architectures are implemented and justify versioned architecture-specific relocation catalogs.
