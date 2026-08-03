# Decision 0119: Expanded WVA x86-64 foundation

- Date: 2026-08-02
- Status: Implemented; Windows qualification passed; cross-host qualification pending

## Context

WVA 1 began as a deliberately small semantic assembly contract for object, linker, native-stencil, and early kernel evidence. Its fixed instruction cases were sufficient for those gates but left ordinary scalar routines in separate Stage 0 emitters and the native compiler's direct byte construction. The object format already supports signed 32-bit PC-relative patches, so local control flow and RIP-relative symbol access do not require a WVO version change.

The useful next boundary is broader than adding isolated opcodes. WVA needs typed registers, one consistent REX/ModRM rule, local fixups, practical flag-producing operations, stack and indirect control, and position-independent data access. The independent C# reference implementation must remain an oracle rather than sharing the production encoder.

## Decision

- Retain WVA version 1 and WVO version 1.0.
- Admit all sixteen 32-bit and all sixteen 64-bit general-purpose register spellings.
- Extend immediate `move_i32` and `move_u32` to the eight extended 32-bit registers through REX.B.
- Add same-width register `move`, `add`, `subtract`, `and`, `or`, `xor`, `compare`, and `test` with derived REX.W/R/B and ModRM fields.
- Add definition-local `label`, `jump_label`, and all sixteen named near `branch` conditions. Local fixups always use `rel32`, are resolved by the assembler, and do not create WVO symbols or relocations.
- Add 64-bit register `push`, `pop`, `call_register`, and `jump_register`.
- Add typed RIP-relative `load_u32`, `load_u64`, `store_u32`, `store_u64`, and `load_address`. Their four-byte fields use existing `relative-i32` relocations with addend `-4`; loads and stores require data symbols, while address loading accepts either symbol kind.
- Keep source/object size limits, canonical declaration ordering, independent WVO verification, assembler/linker separation, and publish-after-success behavior unchanged.
- Keep general base/index/scale memory operands, arithmetic immediates, additional integer families, 8/16-bit registers, 64-bit immediates and absolute addresses, SIMD/floating point, macros, and raw executable-byte directives outside this change.

## Consequences

WVA can now express bounded scalar register routines, loops and conditional regions, register-managed stack/control transfers, and position-independent symbol access. Existing WVA sources remain byte-identical, and the canonical 218-byte WVO identity does not change. The expanded surface provides a credible input contract for later migration of named bootstrap emitters and for convergence with the production native backend.

The Windvale implementation still uses repeated immutable source passes. Local-label validation and resolution therefore add quadratic work within a definition, and the expanded differential fixture requires a bounded 50-million-instruction interpreter envelope. A bounded label table and byte builder remain explicit performance work rather than hidden limit increases.

The native compiler continues to emit its broader x86-64 subset directly. Sharing a typed production encoding layer remains a later architecture change; the C# assembler must remain independently implemented for differential value.

## Verification

Acceptance requires a zero-warning build; exact byte tests for low and extended REX/ModRM combinations; all sixteen condition encodings; forward and backward label fixups; stack and indirect control; RIP-relative relocation offsets and symbol kinds; malformed label, condition, width, target-kind, and section-context rejection; complete Windvale/C# object-byte agreement; independent WVO verification; and the repository's focused assembler gate. Cross-host qualification requires the same committed archive to pass on Windows and Debian with identical portable artifacts and normalized reports.

The 2026-08-02 Windows candidate passed the focused assembler gate (7/7), change-aware qualification scope (71/71), default Seed qualification (71/71), and OS suite (25/25), including zero-warning builds, native CLI publication, golden identities, and composed assembler WVB SHA-256 `dbdbae4fae2c19ec67e7f06824b91488ceccb65f1668520a759d62c7577521b7`. No Debian result is claimed by this decision yet.

## Reconsider when

- A real migrated emitter requires base/index/scale addressing, arithmetic immediates, another integer family, or `IRETQ`/system mechanics.
- Repeated label scans approach a normal hosted execution budget on representative sources.
- The native backend and WVA duplicate enough typed encodings to justify one production machine-instruction model without weakening the independent oracle.
- A required address cannot be represented safely by RIP-relative `i32` and forces an explicit WVO relocation extension.
